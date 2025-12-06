using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;

namespace Sigma.Core.DataStorage
{
    public class MoneyStoreDataStorage : BaseDataStorage
    {
        public class MoneyStoresDicrionary : Dictionary<string, MoneyStoreEntity> { };
        private readonly Dictionary<EndpointType, MoneyStoresDicrionary> _moneyStoresByEndpoint;

        public MoneyStoreDataStorage(ILogger<MoneyStoreDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.MoneyStores = this;
            _moneyStoresByEndpoint = new Dictionary<EndpointType, MoneyStoresDicrionary>();
        }

        private MoneyStoreEntity? fillMoneyStores(HotelManagerPortTypeClient session, MoneyStoresDicrionary moneyStoresCache, string? moneyStoreID = null)
        {
            MoneyStoreEntity? result = null;

            MoneyStoresList moneyStores = session.getMoneyStoresList(moneyStoreID);

            if (moneyStoresCache != null)
            {
                if (moneyStores.error != null && moneyStores.error.Length > 0)
                {
                    _logger.LogError("Failed to load money stores list. Error : {Error}, stores: {Organizations}", moneyStores.error, moneyStores);
                }

                OrganizationDataStorage OrganizationDataStorage = _storageProvider.Organizations;

                foreach (var moneyStore in moneyStores.data)
                {
                    OrganizationEntity? organization = OrganizationDataStorage.GetOrganization(session, moneyStore.OrganizationId);

                    if (organization != null)
                    {
                        MoneyStoreEntity newMoneyStore = new MoneyStoreEntity(moneyStore.Id, moneyStore.Name, organization, moneyStore.IsDeleted);
                        moneyStoresCache[newMoneyStore.Id] = newMoneyStore;

                        result = newMoneyStore;
                    }
                }
            }
            else
            {
                _logger.LogError("Error moneyStores is NULL");
            }

            return result;
        }

        public MoneyStoresDicrionary GetMoneyStores(HotelManagerPortTypeClient session)
        {
            var endpoint = GetEndpoint(session);
            if (!_moneyStoresByEndpoint.TryGetValue(endpoint, out var moneyStores))
            {
                moneyStores = new MoneyStoresDicrionary();
                _moneyStoresByEndpoint[endpoint] = moneyStores;

                fillMoneyStores(session, moneyStores);
            }

            return moneyStores;
        }


        public MoneyStoreEntity? GetMoneyStore(HotelManagerPortTypeClient session, string? moneyStoreID)
        {
            MoneyStoreEntity? result = null;

            if (moneyStoreID == null || moneyStoreID.Length == 0)
            {
                return result;
            }

            MoneyStoresDicrionary moneyStores = GetMoneyStores(session);

            if (!moneyStores.TryGetValue(moneyStoreID, out result))
            {
                _logger.LogInformation("Loading money store with ID {MoenyStoreID}", moneyStoreID);
                result = fillMoneyStores(session, moneyStores, moneyStoreID);
            }

            return result;
        }

        public MoneyStoreBalance GetBalance(HotelManagerPortTypeClient session, string moneyStoreID)
        {
            return session.getMoneyStoreBalance(moneyStoreID);
        }
    }
}
