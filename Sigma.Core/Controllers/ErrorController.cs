using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using static Sigma.Core.DataStorage.ClientDataStorage;
using System.Net;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorController: Controller
    {
        private ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "PostError")]
        public void Post(String errorText)
        {
            _logger.LogError(errorText);
        }
    }
}
