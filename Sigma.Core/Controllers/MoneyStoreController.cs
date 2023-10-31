using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using static Sigma.Core.DataStorage.MoneyStoreDataStorage;
using static Sigma.Core.DataStorage.OrganizationDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoneyStoreController : Controller
    {
        private ILogger<MoneyStoreController> _logger;
        private StorageProvider _storageProvider;

        public MoneyStoreController(ILogger<MoneyStoreController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet(Name = "GetMoneyStores")]
        public MoneyStoresDicrionary? Get()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetDataStores for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.MoneyStores.GetMoneyStores(session.Client);
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            return null;
        }
    }
}
