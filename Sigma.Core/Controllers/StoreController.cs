using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using System.Net;
using static Sigma.Core.DataStorage.OrganizationDataStorage;
using static Sigma.Core.DataStorage.StoreDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreController : Controller
    {
        private ILogger<StoreController> _logger;
        private StorageProvider _storageProvider;

        public StoreController(ILogger<StoreController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet(Name = "GetStorages")]
        public StoresDicrionary? Get()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetStorages for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.Stores.GetStores(session.Client);
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return null;
        }
    }
}
