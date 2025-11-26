using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using System.Net;
using static Sigma.Core.DataStorage.ClientDataStorage;
using static Sigma.Core.DataStorage.OrganizationDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : BaseController
    {
        public ClientController(ILogger<ClientController> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
        }

        [HttpGet(Name = "GetClients")]
        [Authorize]
        public ClientDicrionary? Get()
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return null;
            }

            _logger.LogInformation("GetClients for connection {ConnectionID}", HttpContext.Connection.Id);
            return _storageProvider.Clients.GetClients(userClient.Client);
        }
    }
}
