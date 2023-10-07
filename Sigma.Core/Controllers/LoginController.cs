using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Sigma.Core.Utils;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController: Controller
    { 

        private SOAP1CCleintProviderController _clientProvier;
        public LoginController(SOAP1CCleintProviderController clientProvier)
        {
            _clientProvier = clientProvier;
        }

        [HttpPost(Name = "Login")]
        public IResult Post(CredentionalInfo userCredentional)
        {
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

            

            return Results.Unauthorized();

        }
    }
}
