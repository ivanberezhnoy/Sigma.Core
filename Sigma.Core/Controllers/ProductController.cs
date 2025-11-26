using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using Sigma.Core.Utils;
using System.Net;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProductController: BaseController
    {
        public ProductController(ILogger<ProductController> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
        }

        [HttpGet]
        [Authorize]
        public ProductsDicrionary? Products()
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return null;
            }

            _logger.LogInformation("GetProducts for connection {ConnectionID}", HttpContext.Connection.Id);
            return _storageProvider.Products.GetProducts(userClient.Client);
        }

        [HttpPost]
        [Authorize]
        public RequestResult BindCharacteristic(string productId, string characteristicId, string easyMSRoomId)
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return new RequestResult(ErrorCode.NotAuthorizied, "BingCharacteristic: user not authorized"); ;
            }

            _logger.LogInformation("BingCharacteristic for connection {ConnectionID}", HttpContext.Connection.Id);
            return _storageProvider.Products.BingCharacteristic(userClient.Client, productId, characteristicId, easyMSRoomId);
        }
    }
}
