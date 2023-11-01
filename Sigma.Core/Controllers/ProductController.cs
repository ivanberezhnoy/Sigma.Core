using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using System.Net;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController: Controller
    {
        private ILogger<ProductController> _logger;
        private StorageProvider _storageProvider;

        public ProductController(ILogger<ProductController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet(Name = "GetProducts")]
        public ProductsDicrionary? Get()
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
    }
}
