using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Security.Claims;

public class ProductsDicrionary : Dictionary<string, ProductEntity> { };

namespace Sigma.Core.DataStorage
{
    public class ProductDataStorage : BaseDataStorage
    {
        private ProductsDicrionary? _products;

        public ProductDataStorage(ILogger<ProductDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Products = this;
        }

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
        public ProductsDicrionary GetProducts(HotelManagerPortTypeClient session)
        {

            if (_products == null)
            {
                _logger.LogInformation("Reloading all products list started");
                _products = new ProductsDicrionary();

                fillProducts(session);

                _logger.LogInformation("Reloading all products list finished");
            }

            return _products;
        }

        public ProductEntity? GetProduct(HotelManagerPortTypeClient session, string? productID)
        {
            ProductEntity? result = null;

            if (productID == null || productID.Length == 0)
            {
                return result;
            }

            ProductsDicrionary products = GetProducts(session);

            if (!products.TryGetValue(productID, out result))
            {
                _logger.LogInformation("Loading product with ID {OrganizationID}", productID);

                result = fillProducts(session, productID);
            }

            return result;
        }
    }
}
