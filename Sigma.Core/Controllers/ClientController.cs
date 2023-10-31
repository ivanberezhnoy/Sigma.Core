using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using static Sigma.Core.DataStorage.ClientDataStorage;
using static Sigma.Core.DataStorage.OrganizationDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : Controller
    {
        private ILogger<ClientController> _logger;
        private StorageProvider _storageProvider;

        public ClientController(ILogger<ClientController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet(Name = "GetClients")]
        public ClientDicrionary? Get()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetClients for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.Clients.GetClients(session.Client);
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            return null;
        }
    }
}
