using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using System.Net;

namespace Sigma.Core.Controllers
{
    public abstract class BaseController : Controller
    {
        protected ILogger<BaseController> _logger;
        protected StorageProvider _storageProvider;

        public BaseController(ILogger<BaseController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        protected UserClient? GetClient()
        {
            var userClient = _storageProvider.Sessions.GetClientForHttpContext(HttpContext);

            if (userClient == null || userClient.Client == null)
            {
                if (userClient == null)
                {
                    _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);
                }
                else
                {
                    _logger.LogWarning("User: {UserName} is not connected", userClient.User.Name);
                }

                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return null;
            }

            return userClient;
        }
    }
}
