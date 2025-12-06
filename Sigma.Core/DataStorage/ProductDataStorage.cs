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
        private readonly Dictionary<EndpointType, ProductsDicrionary> _productsByEndpoint;

        public ProductDataStorage(ILogger<ProductDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Products = this;
            _productsByEndpoint = new Dictionary<EndpointType, ProductsDicrionary>();
        }

        public void ReloadProduct(HotelManagerPortTypeClient session, ProductEntity product)
        {
            var products = GetProducts(session);
            if (products != null)
            {
                if (!products.ContainsKey(product.Id))
                {
                    _logger.LogError("ReloadProduct. Products container does not contain product with ID: {ProductID}", product.Id);
                }
                fillProducts(session, products, product.Id);
            }
            else
            {
                _logger.LogCritical("Unable to reload product. Products container is empty");
            }
        }

        private ProductEntity? fillProducts(HotelManagerPortTypeClient session, ProductsDicrionary products, string? productID = null, bool fullReload = false)
        {
            ProductEntity? firstProduct = null;

            if (products != null)
            {
                ProductsList products = session.getProducts(productID);

                if (products != null && products.data != null)
                {
                    foreach (Product product in products.data)
                    {
                        ProductEntity? productEntity = null;

                        if (!fullReload && products.TryGetValue(product.Id, out productEntity))
                        {
                            productEntity.Reload(product);
                        }

                        if (productEntity == null)
                        {
                            productEntity = new ProductEntity(product);
                            products[product.Id] = productEntity;
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
            var endpoint = GetEndpoint(session);
            if (!_productsByEndpoint.TryGetValue(endpoint, out var products))
            {
                _logger.LogInformation("Reloading all products list started");
                products = new ProductsDicrionary();
                _productsByEndpoint[endpoint] = products;

                fillProducts(session, products);

                _logger.LogInformation("Reloading all products list finished");
            }

            return products;
        }

        public RequestResult BingCharacteristic(
            HotelManagerPortTypeClient session,
            string productId,
            string characteristicId,
            string easyMSRoomId)
        {
            // 1. Находим продукт в кэше
            ProductEntity? product = GetProduct(session, productId);
            if (product == null)
            {
                return new RequestResult(
                    new Sigma.Core.Utils.Error(
                        ErrorCode.UnableToFindObjectWithId,
                        "Unable to find product with Id: " + productId),
                    null,
                    false);
            }

            // 2. Находим характеристику в кэше
            if (!product.Characteristics.TryGetValue(characteristicId, out var characteristicEntity) ||
                characteristicEntity == null)
            {
                return new RequestResult(
                    new Sigma.Core.Utils.Error(
                        ErrorCode.UnableToFindObjectWithId,
                        "Unable to find characteristic with Id: " + characteristicId),
                    null,
                    false);
            }

            // 3. Вызываем 1С-метод, который меняет значение в базе
            //    Логика уникальности реализована там.
            RequestResult result = new RequestResult(
                session.setCharacteristicEasyMSRoomId(characteristicId, easyMSRoomId));

            if (result.HasError)
            {
                return result;
            }

            // 4. Синхронизируем кэш с логикой на стороне 1С

            // Получаем весь словарь продуктов для текущей сессии
            ProductsDicrionary products = GetProducts(session);

            // Нормализуем "пустое" значение (как у тебя заведено — null или "")
            // Предположим, что null в кэше = "не привязано"
            bool isEmpty = string.IsNullOrEmpty(easyMSRoomId);

            if (!isEmpty)
            {
                // 4.1. Очистить easyMSRoomID у ВСЕХ других характеристик,
                //      которые имеют такой же easyMSRoomId
                foreach (var kvpProduct in products)
                {
                    var otherProduct = kvpProduct.Value;
                    if (otherProduct?.Characteristics == null)
                        continue;

                    foreach (var kvpChar in otherProduct.Characteristics)
                    {
                        var otherCharId = kvpChar.Key;
                        var otherChar = kvpChar.Value;

                        if (otherChar == null)
                            continue;

                        // пропускаем текущую характеристику
                        if (otherProduct.Id == productId && otherCharId == characteristicId)
                            continue;

                        if (string.Equals(otherChar.easyMSRoomID, easyMSRoomId, StringComparison.Ordinal))
                        {
                            // "Отвязываем" комнату от других характеристик
                            otherChar.easyMSRoomID = null; // или "" — под твой стиль хранения "пусто"
                        }
                    }
                }
            }

            // 4.2. Обновляем текущую характеристику в кэше
            //       Если значение не поменялось — просто присвоим то же самое, это безопасно.
            characteristicEntity.easyMSRoomID = isEmpty ? null : easyMSRoomId;

            return result;
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

                result = fillProducts(session, products, productID);
            }

            return result;
        }
    }
}
