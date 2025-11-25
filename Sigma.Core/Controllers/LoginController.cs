using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Sigma.Core.Utils;
using Sigma.Core.DataStorage;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController: Controller
    {

        private readonly JwtOptions _jwtOptions;
        private SessionDataStorage _clientProvier;
        public LoginController(SessionDataStorage clientProvier, IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            _clientProvier = clientProvier;
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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userCredentional.UserName),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)); // секрет в конфиге

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1200);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new
            {
                accessToken = tokenString,
                expiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
                userName = userCredentional.UserName
            });
        }
    }
}
