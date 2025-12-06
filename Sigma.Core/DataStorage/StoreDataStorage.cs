using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sigma.Core.Controllers;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;

namespace Sigma.Core.DataStorage
{
    public class StoreDataStorage : BaseDataStorage
    {
        public class StoresDicrionary : Dictionary<string, StoreEntity> { };
        private readonly Dictionary<EndpointType, StoresDicrionary> _storesByEndpoint;

        public StoreDataStorage(ILogger<StoreDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Stores = this;
            _storesByEndpoint = new Dictionary<EndpointType, StoresDicrionary>();
        }

        private StoreEntity? fillStores(HotelManagerPortTypeClient session, StoresDicrionary storesCache, string? storeID = null)
        {
            StoreEntity? result = null;

            if (storesCache != null)
            {
                StoresList stores = session.getStoresList(storeID);

                if (stores.error != null && stores.error.Length > 0)
                {
                    _logger.LogError("Failed to load stores list. Error : {Error}, stores: {Organizations}", stores.error, stores);
                }

                foreach (var store in stores.data)
                {
                    StoreEntity newStore = new StoreEntity(store);
                    storesCache[newStore.Id] = newStore;

                    if (result == null)
                    {
                        result = newStore;
                    }
                }
            }

            return result;
        }
        public StoresDicrionary GetStores(HotelManagerPortTypeClient session)
        {
            var endpoint = GetEndpoint(session);
            if (!_storesByEndpoint.TryGetValue(endpoint, out var stores))
            {
                stores = new StoresDicrionary();
                _storesByEndpoint[endpoint] = stores;

                fillStores(session, stores);
            }
            return stores;
        }

        public StoreEntity? GetStore(HotelManagerPortTypeClient session, string? storeID)
        {
            StoreEntity? result = null;

            if (storeID == null || storeID.Length == 0)
            {
                return result;
            }

            StoresDicrionary stores = GetStores(session);

            if (!stores.TryGetValue(storeID, out result))
            {
                _logger.LogInformation("Loading store with ID {StoreID}", storeID);

                result = fillStores(session, stores, storeID);
            }

            return result;
        }

    }
}
