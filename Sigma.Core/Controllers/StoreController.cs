using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sigma.Core.DataStorage;
using System.Net;
using static Sigma.Core.DataStorage.OrganizationDataStorage;
using static Sigma.Core.DataStorage.StoreDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreController : BaseController
    {
        public StoreController(ILogger<StoreController> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
        }

        [HttpGet(Name = "GetStorages")]
        public StoresDicrionary? Get()
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return null;
            }

            _logger.LogInformation("GetStorages for connection {ConnectionID}", HttpContext.Connection.Id);
            return _storageProvider.Stores.GetStores(userClient.Client);
        }
    }
}
