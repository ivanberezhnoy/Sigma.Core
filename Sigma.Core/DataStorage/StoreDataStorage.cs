using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.Controllers;
using Sigma.Core.RemoteHotelEntry;

namespace Sigma.Core.DataStorage
{
    public class StoreDataStorage : BaseDataStorage
    {
        public class StoresDicrionary : Dictionary<string, StoreEntity> { };
        public StoresDicrionary? _stores;

        public StoreDataStorage(ILogger<StoreDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Stores = this;
        }

        private StoreEntity? fillStores(HotelManagerPortTypeClient session, string? storeID = null)
        {
            StoreEntity? result = null;

            if (_stores != null)
            {
                StoresList stores = session.getStoresList(storeID);

                if (stores.error != null && stores.error.Length > 0)
                {
                    _logger.LogError("Failed to load stores list. Error : {Error}, stores: {Organizations}", stores.error, stores);
                }

                foreach (var store in stores.data)
                {
                    StoreEntity newStore = new StoreEntity(store);
                    _stores[newStore.Id] = newStore;

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
            if (_stores == null)
            {
                _stores = new StoresDicrionary();

                fillStores(session);
            }
            return _stores;
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

                result = fillStores(session, storeID);
            }

            return result;
        }

    }
}
