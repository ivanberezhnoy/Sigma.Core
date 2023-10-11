using HotelManager;
using System.Collections.Generic;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using MySqlX.XDevAPI;
using Org.BouncyCastle.Utilities.Collections;

using ConnectionID = System.String;
using Microsoft.AspNetCore.Mvc;

namespace Sigma.Core.Controllers
{
    using UserName = ConnectionID;
    //using UserSessions = Tuple<UserEntity, HotelManagerPortTypeClient, HashSet<ConnectionID>>;

    public class UserSessions
    {
        public UserSessions(UserEntity user, HotelManagerPortTypeClient? client, HashSet<ConnectionID> connections)
        {
            User = user;
            Client = client;
            Connections = connections;
        }
        public UserEntity User { get; set; }
        public HotelManagerPortTypeClient? Client;
        public HashSet<ConnectionID> Connections { get; set; }
    }

    public class UserClient
    {
        public UserEntity User { get; set; }

        public HotelManagerPortTypeClient Client {get;set;}

        public UserClient(UserEntity user, HotelManagerPortTypeClient client)
        {
            User = user;
            Client = client;
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class SOAP1CCleintProviderController: Controller
    {
        public SOAP1CCleintProviderController(ILogger<SOAP1CCleintProviderController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _connectedUsersSessions = new Dictionary<UserName, UserSessions>();
            _usersConnections = new Dictionary<ConnectionID, UserClient>();

            _httpContextAccessor = httpContextAccessor;

            _logger = logger;
        }

        private Dictionary<UserName, UserSessions> _connectedUsersSessions;
        private Dictionary<ConnectionID, UserClient> _usersConnections;
        private IHttpContextAccessor _httpContextAccessor;

        private ILogger<SOAP1CCleintProviderController> _logger;


        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public UserEntity? GetUserByID(HotelManagerPortTypeClient client, string userID)
        {
            UserEntity? result = null;
            if (userID != null && userID.Length > 0)
            {
                HashSet<string> existingUserIds = new HashSet<UserName>();
                foreach(var userSession in _connectedUsersSessions.Values)
                {
                    existingUserIds.Add(userSession.User.UserId);
                    
                    if (userSession.User.UserId == userID)
                    {
                        return userSession.User;
                    }
                }

                UsersList users = client.getUsers();

                foreach(var user in users.data)
                {
                    if (!existingUserIds.Contains(user.Id))
                    {
                        result = new UserEntity(new CredentionalInfo(user.Name, null), user.Id);
                        _connectedUsersSessions[result.Credentional.UserName] = new UserSessions(result, null, new HashSet<UserName>());
                    }
                }
            }
            else 
            {
                _logger.LogWarning("Unable find user with empty UserID");
            }

            return result;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public bool ConnectClient(CredentionalInfo userCredential, ConnectionID connectionID)
        {
            _logger.LogInformation("Try to connect user: {UserName}", userCredential.UserName);

            UserClient? userClient = GetClentForConnectionID(connectionID, false);

            if (userClient != null)
            {
                return true;
            }

            UserSessions? connectedUserInfo;
            _connectedUsersSessions.TryGetValue(userCredential.UserName, out connectedUserInfo);

            UserEntity? user;
            if (connectedUserInfo == null || connectedUserInfo.Client == null || connectedUserInfo.User.Credentional.Password != userCredential.Password)
            {
                if (connectedUserInfo != null && connectedUserInfo.User.Credentional.Password != userCredential.Password)
                {
                    _logger.LogWarning("User: {User} try to connect with new password", userCredential.UserName);
                }

                System.ServiceModel.BasicHttpBinding hotelManagerBinding = new System.ServiceModel.BasicHttpBinding();
                hotelManagerBinding.MaxBufferSize = int.MaxValue;
                hotelManagerBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                hotelManagerBinding.MaxReceivedMessageSize = int.MaxValue;
                hotelManagerBinding.AllowCookies = true;
                hotelManagerBinding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                hotelManagerBinding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;
#if DEBUG
                var timeout = new TimeSpan(0, 30, 0);
                hotelManagerBinding.OpenTimeout = timeout;
                hotelManagerBinding.ReceiveTimeout = timeout;
                hotelManagerBinding.CloseTimeout = timeout;
#endif

                var hotelManagerEndpoint = new System.ServiceModel.EndpointAddress("http://192.168.1.152/ApartmentDeveloper/ws/ws2.1cws");

                HotelManagerPortTypeClient client = new HotelManagerPortTypeClient(hotelManagerBinding, hotelManagerEndpoint);
                client.ClientCredentials.UserName.UserName = userCredential.UserName;
                client.ClientCredentials.UserName.Password = userCredential.Password;

                client.OpenAsync().Wait();


                UserSettings userSettings;
                try
                {
                    userSettings = client.getUserSettings(null);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unable to connect with username {UserName}, Exception: {Exception}", userCredential.UserName, ex.ToString());
                    return false;
                }

                if (userSettings.error != null)
                {
                    _logger.LogError("Error on connect user: {UserName}. Error: {Error}", userCredential.UserName, userSettings.error);
                }

                if (_httpContextAccessor.HttpContext == null)
                {
                    _logger.LogError("Unable to find HTTP Context");

                    return false;
                }

                OrganizationController? organizationController = _httpContextAccessor.HttpContext.RequestServices.GetService<OrganizationController>();
                MoneyStoreController? moneyStoreController = _httpContextAccessor.HttpContext.RequestServices.GetService<MoneyStoreController>();
                StoreController? storeController = _httpContextAccessor.HttpContext.RequestServices.GetService<StoreController>();

                if (organizationController != null && storeController != null && moneyStoreController != null)
                {

                    user = new UserEntity(userCredential, userSettings.UserID);

                    user.DefaultOrganization = organizationController.GetOrganization(client, userSettings.DefaultOrganizationId);
                    user.DefaultMoneyStore = moneyStoreController.GetMoneyStore(client, userSettings.DefaultMoneyStoreId);
                    user.DefaultStore = storeController.GetStore(client, userSettings.DefaultStoreId);

                    if (connectedUserInfo != null && connectedUserInfo.Client != null)
                    {
                        _logger.LogWarning("User: {User} password was changed", userCredential.UserName);

                        connectedUserInfo.Client.CloseAsync();

                        foreach (ConnectionID oldSession in connectedUserInfo.Connections)
                        {
                            _usersConnections.Remove(oldSession);
                        }
                    }
                    connectedUserInfo = new UserSessions(user, client, new HashSet<ConnectionID>());
                    _connectedUsersSessions[user.Credentional.UserName] = connectedUserInfo;
                }
                else 
                {
                    _logger.LogCritical("Unable to get controllers");
                    return false;
                }
            }
            else
            {
                user = connectedUserInfo.User;
            }

            _logger.LogInformation("New user {UserName} session establiched {SessionID}", userCredential.UserName, connectionID);

            connectedUserInfo.Connections.Add(connectionID);

            if (connectedUserInfo.Client != null)
            {
                _usersConnections[connectionID] = new UserClient(user, connectedUserInfo.Client);
            }

            return true;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public UserClient? GetClentForConnectionID(ConnectionID? connectionID, bool logWarning = true)
        {
            UserClient? result = null;

            if (connectionID == null || connectionID.Length == 0)
            {
                return result;
            }

            if (!_usersConnections.TryGetValue(connectionID, out result))
            {
#if DEBUG
                if (logWarning && ConnectClient(new CredentionalInfo("Іван Бережний", "Bi34#802"), connectionID))
                {
                    _usersConnections.TryGetValue(connectionID, out result);
                }
#endif
            }

            if (result == null && logWarning)
            {
                _logger.LogWarning("Unable to find session with ID: {}", connectionID);
            }

            return result;
        }

    }
}
