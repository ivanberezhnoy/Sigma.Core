using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Security.Claims;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        public class ProductsDicrionary : Dictionary<string, ProductEntry> { };
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
            HotelManagerPortTypeClient? session = _clientProvider.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                return getProducts(session);
            }
            return null;
        }

        private ProductEntry? fillProducts(HotelManagerPortTypeClient session, string? productID = null, bool fullReload = false)
        {
            ProductEntry? firstProduct = null;

            if (_products != null)
            {
                ProductsList products = session.getProducts(productID);

                if (products != null && products.data != null)
                {
                    foreach (Product product in products.data)
                    {
                        ProductEntry? productEntity = null;

                        if (!fullReload && _products.TryGetValue(product.Id, out productEntity))
                        {
                            productEntity.Reload(product);
                        }

                        if (productEntity == null)
                        {
                            productEntity = new ProductEntry(product);
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
    }
}
