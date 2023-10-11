using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Security.Claims;
using static Sigma.Core.Controllers.OrganizationController;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        public class ProductsDicrionary : Dictionary<string, ProductEntity> { };
        private ProductsDicrionary? _products;

        private ILogger<ProductController> _logger;
        private SOAP1CCleintProviderController _clientProvider;
        IHttpContextAccessor _httpContextAccessor;

        public ProductController(ILogger<ProductController> logger, SOAP1CCleintProviderController clientProvider, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _clientProvider = clientProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet(Name = "GetProducts")]
        public ProductsDicrionary? Get()
        {
            var session = _clientProvider.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                return getProducts(session.Client);
            }
            return null;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public void ReloadProduct(HotelManagerPortTypeClient session, ProductEntity product)
        {
            if (_products != null)
            {
                if (!_products.ContainsKey(product.Id))
                {
                    _logger.LogError("ReloadProduct. Products container does not contain product with ID: {ProductID}", product.Id);
                }
                fillProducts(session, product.Id);
            }
            else
            {
                _logger.LogCritical("Unable to reload product. Products container is empty");
            }
        }

        private ProductEntity? fillProducts(HotelManagerPortTypeClient session, string? productID = null, bool fullReload = false)
        {
            ProductEntity? firstProduct = null;

            if (_products != null)
            {
                ProductsList products = session.getProducts(productID);

                if (products != null && products.data != null)
                {
                    foreach (Product product in products.data)
                    {
                        ProductEntity? productEntity = null;

                        if (!fullReload && _products.TryGetValue(product.Id, out productEntity))
                        {
                            productEntity.Reload(product);
                        }

                        if (productEntity == null)
                        {
                            productEntity = new ProductEntity(product);
                            _products[product.Id] = productEntity;
                        }

                        if (firstProduct == null)
                        {
                            firstProduct = productEntity;
                        }
                    }
                }
            }

            return firstProduct;
        }
        private ProductsDicrionary getProducts(HotelManagerPortTypeClient session)
        {

            if (_products == null)
            {
                _products = new ProductsDicrionary();

                fillProducts(session);
            }

            return _products;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public ProductEntity? GetProduct(HotelManagerPortTypeClient session, string? productID)
        {
            ProductEntity? result = null;

            if (productID == null || productID.Length == 0)
            {
                return result;
            }

            ProductsDicrionary products = getProducts(session);

            if (!products.TryGetValue(productID, out result))
            {
                _logger.LogInformation("Loading product with ID {OrganizationID}", productID);

                result = fillProducts(session, productID);
            }

            return result;
        }
    }
}
