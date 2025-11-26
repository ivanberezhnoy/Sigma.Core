using HotelManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using Sigma.Core.DataStorage;
using Sigma.Core.RemoteHotelEntry;
using System.Net;
using static Sigma.Core.DataStorage.OrganizationDataStorage;
using static Sigma.Core.DataStorage.StoreDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : BaseController
    {
        public UserController(ILogger<UserController> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
        }

        [HttpGet]
        [Authorize]
        public Dictionary<string, UserEntity>? GetUsers()
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return null;
            }

            _logger.LogInformation("GetUsers for connection {ConnectionID}", HttpContext.Connection.Id);

            var users = _storageProvider.Sessions.GetUsers(userClient.Client);

            Dictionary<string, UserEntity> result = new Dictionary<string, UserEntity>();
            foreach (var user in users.Values)
            {
                result[user.Id] = user.Id == userClient.User.Id ? user : user.MakeNakedCopy();
                if (user.Id == userClient.User.Id)
                { }
            }

            return result;
        }
    }

}
