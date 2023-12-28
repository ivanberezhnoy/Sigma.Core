using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using Sigma.Core.RemoteHotelEntry;
using System.Net;
using static Sigma.Core.DataStorage.OrganizationDataStorage;
using static Sigma.Core.DataStorage.StoreDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        private ILogger<UserController> _logger;
        private StorageProvider _storageProvider;

        public UserController(ILogger<UserController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet]
        public Dictionary<string, UserEntity>? GetUsers()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetUsers for connection {ConnectionID}", HttpContext.Connection.Id);

                var users = _storageProvider.Sessions.GetUsers(session.Client);

                Dictionary<string, UserEntity> result = new Dictionary<string, UserEntity>();
                foreach (var user in users.Values)
                {
                    result[user.Id] = user.Id == session.User.Id ? user : user.MakeNakedCopy();
                    if (user.Id == session.User.Id)
                    { }
                }
                
                return result;
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
    }

}
