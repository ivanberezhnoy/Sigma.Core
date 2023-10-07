using HotelManager;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using Sigma.Core.RemoteHotelEntry;
using static Sigma.Core.Controllers.OrganizationController;
using static Sigma.Core.Controllers.StoreController;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoneyStoreController : Controller
    {
        public class MoneyStoresDicrionary : Dictionary<string, MoneyStoreEntity> { };
        public MoneyStoresDicrionary? _moneyStores;

        private ILogger<MoneyStoreController> _logger;
        private SOAP1CCleintProviderController _clientProvider;
        private IHttpContextAccessor _httpContextAccessor;

        public MoneyStoreController(ILogger<MoneyStoreController> logger, SOAP1CCleintProviderController clientProvider, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _clientProvider = clientProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        private MoneyStoreEntity? fillMoneyStores(HotelManagerPortTypeClient session, string? moneyStoreID = null)
        {
            MoneyStoreEntity? result = null;

            MoneyStoresList moneyStores = session.getMoneyStoresList(moneyStoreID);

            if (_moneyStores != null)
            {
                if (moneyStores.error.Length > 0)
                {
                    _logger.LogError("Failed to load money stores list. Error : {Error}, stores: {Organizations}", moneyStores.error, moneyStores);
                }

                if (_httpContextAccessor == null)
                {
                    _logger.LogError("Unable to find HTTP Context");

                    return result;
                }

                OrganizationController? organizationController = _httpContextAccessor.HttpContext.RequestServices.GetService<OrganizationController>();

                if (organizationController != null)
                {
                    foreach (var moneyStore in moneyStores.data)
                    {
                        OrganizationEntity? organization = organizationController.GetOrganization(session, moneyStore.OrganizationId);

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
                    _logger.LogError("Unable to find OrganizationController");
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

        [ApiExplorerSettings(IgnoreApi = true)]
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
