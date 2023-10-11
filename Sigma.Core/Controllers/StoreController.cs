using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.RemoteHotelEntry;
using static Sigma.Core.Controllers.OrganizationController;
using static Sigma.Core.Controllers.ProductController;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreController : Controller
    {
        public class StoresDicrionary : Dictionary<string, StoreEntity> { };
        public StoresDicrionary? _stores;

        private ILogger<StoreController> _logger;
        private SOAP1CCleintProviderController _clientProvider;
        private IHttpContextAccessor _httpContextAccessor;

        public StoreController(ILogger<StoreController> logger, SOAP1CCleintProviderController clientProvider, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _clientProvider = clientProvider;
            _httpContextAccessor = httpContextAccessor;
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
        private StoresDicrionary getStores(HotelManagerPortTypeClient session)
        {
            if (_stores == null)
            {
                _stores = new StoresDicrionary();

                fillStores(session);
            }
            return _stores;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public StoreEntity? GetStore(HotelManagerPortTypeClient session, string? storeID)
        {
            StoreEntity? result = null;

            if (storeID == null || storeID.Length == 0)
            {
                return result;
            }

            StoresDicrionary stores = getStores(session);

            if (!stores.TryGetValue(storeID, out result))
            {
                _logger.LogInformation("Loading store with ID {StoreID}", storeID);

                result = fillStores(session, storeID);
            }

            return result;
        }

    }
}
