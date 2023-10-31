using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using Sigma.Core.RemoteHotelEntry;
using static Sigma.Core.DataStorage.OrganizationDataStorage;
using static Sigma.Core.DataStorage.StoreDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private ILogger<UserController> _logger;
        private StorageProvider _storageProvider;

        public UserController(ILogger<UserController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet(Name = "GetUser")]
        public UserEntity? Get()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetUser for connection {ConnectionID}", HttpContext.Connection.Id);

                UserClient? userClient = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id, true);

                if (userClient != null)
                {
                    return userClient.User;
                }
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            return null;
        }
    }
}
