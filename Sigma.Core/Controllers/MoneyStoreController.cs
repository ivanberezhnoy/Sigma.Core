using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using Sigma.Core.Utils;
using System.Net;
using static Sigma.Core.DataStorage.MoneyStoreDataStorage;
using static Sigma.Core.DataStorage.OrganizationDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
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
        public MoneyStoresDicrionary? GetMoneyStores()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetDataStores for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.MoneyStores.GetMoneyStores(session.Client);
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return null;
        }

        [HttpGet]
        public RequestResult GetBalance(string moneyStoreID)
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("Get money store balance {MoneyStoreID}", moneyStoreID);
                MoneyStoreBalance balance = _storageProvider.MoneyStores.GetBalance(session.Client, moneyStoreID);
                return new RequestResult(null, balance, false);
            }

            return new RequestResult(ErrorCode.NotAuthorizied, "setDocuments: user not authorized");
        }
    }
}
