using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Sigma.Core.Utils;
using Sigma.Core.DataStorage;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Sigma.Core.DB;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController: Controller
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtOptions _jwtOptions;
        private readonly SessionDataStorage _clientProvier;
        private readonly ILogger<LoginController> _logger;

        public LoginController(SessionDataStorage clientProvier, IOptions<JwtOptions> jwtOptions, IRefreshTokenRepository refreshTokenRepository, ILogger<LoginController> logger)
        {
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
            _clientProvier = clientProvier;
            _refreshTokenRepository = refreshTokenRepository;
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(randomBytes);
        }

        private static string ComputeHash(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(bytes);
        }

        private string CreateJwtToken(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                // сюда можно добавить роли, id клиента и т.п.
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1200);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost(Name = "Login")]
        public IResult Post([FromBody] CredentionalInfo userCredentional)
        {
            if (userCredentional == null ||
                string.IsNullOrWhiteSpace(userCredentional.UserName) ||
                string.IsNullOrWhiteSpace(userCredentional.Password))
            {
                return Results.Unauthorized();
            }

            bool connectionResult = _clientProvier.ConnectClient(userCredentional);

            if (!connectionResult)
            {
                return Results.Unauthorized();
            }

            // 1) Access token (твой CreateJwtToken)
            var accessToken = CreateJwtToken(userCredentional.UserName);

            // 2) Refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = ComputeHash(refreshToken);

            // 3) Сохраняем refresh в БД
            var entity = new Sigma.Core.DB.Entities.RefreshTokenDbEntity
            {
                UserName = userCredentional.UserName,
                TokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _refreshTokenRepository.Save(entity);

            return Results.Ok(new
            {
                accessToken = accessToken,
                refreshToken = refreshToken,
                userName = userCredentional.UserName
            });
        }

        public record RefreshRequest(string RefreshToken);

        [HttpPost("refresh")]
        public IResult Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Results.Unauthorized();

            var hash = ComputeHash(request.RefreshToken);
            var tokenEntity = _refreshTokenRepository.FindByHash(hash);

            if (tokenEntity == null ||
                tokenEntity.ExpiresAt < DateTime.UtcNow ||
                tokenEntity.RevokedAt != null)
            {
                return Results.Unauthorized();
            }

            var accessToken = CreateJwtToken(tokenEntity.UserName);

            return Results.Ok(new
            {
                accessToken
            });
        }

        [HttpPost("logout")]
        [Authorize] // опционально, но лучше включить
        public IResult Logout([FromBody] RefreshRequest request)
        {
            // Даже если клиент не прислал токен — просто "успешно"
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Results.Ok();

            var hash = ComputeHash(request.RefreshToken);
            var tokenEntity = _refreshTokenRepository.FindByHash(hash);

            if (tokenEntity == null)
            {
                // Токен уже удалён/неизвестен. Логически — уже разлогинен.
                return Results.Ok();
            }

            // (не обязательно, но можно: проверка, что это токен именно текущего пользователя)
            // var userName = HttpContext.User.Identity?.Name;
            // if (userName != null && !string.Equals(userName, tokenEntity.UserName, StringComparison.OrdinalIgnoreCase))
            //     return Results.Unauthorized();

            tokenEntity.RevokedAt = DateTime.UtcNow;
            _refreshTokenRepository.Revoke(tokenEntity);

            return Results.Ok();
        }

        [HttpPost("logout-all")]
        [Authorize]
        public IResult LogoutAll()
        {
            var userName = HttpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Results.Unauthorized();

            _refreshTokenRepository.RevokeAllForUser(userName);

            return Results.Ok();
        }

    }
}
