using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Sigma.Core.Utils;
using Sigma.Core.DataStorage;
using System.Net;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController: Controller
    { 

        private SessionDataStorage _clientProvier;
        public LoginController(SessionDataStorage clientProvier)
        {
            _clientProvier = clientProvier;
        }

        [HttpPost(Name = "Login")]
        public IResult Post(CredentionalInfo userCredentional)
        {
            if (userCredentional == null || userCredentional.UserName == null || userCredentional.Password == null)
            {
                return Results.Unauthorized();
            }
            if (userCredentional.UserName.Length != 0 && userCredentional.Password.Length != 0)
            {
                bool connectionResult = _clientProvier.ConnectClient(userCredentional, HttpContext.Connection.Id);

                if (connectionResult)
                {
                    var claims = new List<Claim>
                    {
                        new (ClaimTypes.Name, userCredentional.UserName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var newClaimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    HttpContext.SignInAsync(newClaimsPrincipal).Wait();

                    return Results.Accepted();
                }
            }

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Results.Unauthorized();

        }
    }
}
