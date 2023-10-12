using HotelManager;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using Sigma.Core.RemoteHotelEntry;

namespace Sigma.Core.DataStorage
{
    public class MoneyStoreDataStorage : BaseDataStorage
    {
        public class MoneyStoresDicrionary : Dictionary<string, MoneyStoreEntity> { };
        public MoneyStoresDicrionary? _moneyStores;

        public MoneyStoreDataStorage(ILogger<MoneyStoreDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.MoneyStores = this;
        }

        /*[HttpGet(Name = "GetMyMoneyValue")]
        public float? Get()
        {
            var session = _clientProvider.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                if (session.User.DefaultMoneyStore != null)
                {
                    return session.User.DefaultMoneyStore.GetMoneyValue(session.Client);
                }
            }
            return null;
        }*/

        private MoneyStoreEntity? fillMoneyStores(HotelManagerPortTypeClient session, string? moneyStoreID = null)
        {
            MoneyStoreEntity? result = null;

            MoneyStoresList moneyStores = session.getMoneyStoresList(moneyStoreID);

            if (_moneyStores != null)
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
                        MoneyStoreEntity newMoneyStore = new MoneyStoreEntity(moneyStore.Id, moneyStore.Name, organization);
                        _moneyStores[newMoneyStore.Id] = newMoneyStore;

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

        private MoneyStoresDicrionary getMoneyStores(HotelManagerPortTypeClient session)
        {
            if (_moneyStores == null)
            {
                _moneyStores = new MoneyStoresDicrionary();

                fillMoneyStores(session);
            }
            return _moneyStores;
        }


        public MoneyStoreEntity? GetMoneyStore(HotelManagerPortTypeClient session, string? moneyStoreID)
        {
            MoneyStoreEntity? result = null;

            if (moneyStoreID == null || moneyStoreID.Length == 0)
            {
                return result;
            }

            MoneyStoresDicrionary monetStores = getMoneyStores(session);

            if (!monetStores.TryGetValue(moneyStoreID, out result))
            {
                _logger.LogInformation("Loading money store with ID {MoenyStoreID}", moneyStoreID);
                result = fillMoneyStores(session, moneyStoreID);
            }

            return result;
        }
    }
}
