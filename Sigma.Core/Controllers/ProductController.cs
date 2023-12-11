using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using Sigma.Core.Utils;
using System.Net;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProductController: Controller
    {
        private ILogger<ProductController> _logger;
        private StorageProvider _storageProvider;

        public ProductController(ILogger<ProductController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet]
        public ProductsDicrionary? Products()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetProducts for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.Products.GetProducts(session.Client);
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return null;
        }

        [HttpPost]
        public RequestResult BindCharacteristic(string productId, string characteristicId, string easyMSRoomId)
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("BingCharacteristic for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.Products.BingCharacteristic(session.Client, productId, characteristicId, easyMSRoomId);
            }

            return new RequestResult(ErrorCode.NotAuthorizied, "BingCharacteristic: user not authorized");
        }
    }
}
