using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;

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
                return _storageProvider.Products.GetProducts(session.Client);
            }
            return null;
        }
    }
}
