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
    public class MoneyStoreController : BaseController
    {
        public MoneyStoreController(ILogger<MoneyStoreController> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
        }

        [HttpGet(Name = "GetMoneyStores")]
        public MoneyStoresDicrionary? GetMoneyStores()
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return null;
            }

            _logger.LogInformation("GetDataStores for connection {ConnectionID}", HttpContext.Connection.Id);
            return _storageProvider.MoneyStores.GetMoneyStores(userClient.Client);
        }

        [HttpGet]
        public RequestResult GetBalance(string moneyStoreID)
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return new RequestResult(ErrorCode.NotAuthorizied, "setDocuments: user not authorized");
            }

            _logger.LogInformation("Get money store balance {MoneyStoreID}", moneyStoreID);
            MoneyStoreBalance balance = _storageProvider.MoneyStores.GetBalance(userClient.Client, moneyStoreID);
            return new RequestResult(null, balance, false);
        }
    }
}
