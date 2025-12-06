using Microsoft.Extensions.Logging;
using Sigma.Core.Utils;

namespace Sigma.Core.DataStorage
{
    public class BaseDataStorage
    {
        protected ILogger _logger;
        protected StorageProvider _storageProvider;

        protected BaseDataStorage(ILogger logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        protected EndpointType GetEndpoint(HotelManager.HotelManagerPortTypeClient session)
        {
            return EndpointResolver.GetEndpointTypeFromUrl(session.Endpoint.Address.Uri.AbsoluteUri);
        }
    }
}
