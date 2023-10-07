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
    using UserSessions = Tuple<UserEntity, HotelManagerPortTypeClient, HashSet<ConnectionID>>;
    using UserClient = Tuple<UserEntity, HotelManagerPortTypeClient>;
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
        public bool ConnectClient(CredentionalInfo userCredential, ConnectionID connectionID)
        {
            _logger.LogInformation("Try to connect user: {UserName}", userCredential.UserName);

            HotelManagerPortTypeClient? client = GetClentForConnectionID(connectionID, false);

            if (client != null)
            {
                return true;
            }

            UserSessions? connectedUserInfo;
            _connectedUsersSessions.TryGetValue(userCredential.UserName, out connectedUserInfo);

            UserEntity? user;
            if (connectedUserInfo == null || connectedUserInfo.Item1.Credentional.Password != userCredential.Password)
            {
                if (connectedUserInfo != null && connectedUserInfo.Item1.Credentional.Password != userCredential.Password)
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

                var hotelManagerEndpoint = new System.ServiceModel.EndpointAddress("http://192.168.1.152/ApartmentDeveloper/ws/ws2.1cws");

                client = new HotelManagerPortTypeClient(hotelManagerBinding, hotelManagerEndpoint);
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
                    _logger.LogError("Unable to connect with username {UserName}", userCredential.UserName);
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

                    user = new UserEntity(userCredential);
                    user.UserId = userSettings.UserID;
                    user.DefaultOrganization = organizationController.GetOrganization(client, userSettings.DefaultOrganizationId);
                    user.DefaultMoneyStore = moneyStoreController.GetMoneyStore(client, userSettings.DefaultMoneyStoreId);
                    user.DefaultStore = storeController.GetStore(client, userSettings.DefaultStoreId);

                    if (connectedUserInfo != null)
                    {
                        _logger.LogWarning("User: {User} password was changed", userCredential.UserName);

                        connectedUserInfo.Item2.CloseAsync();

                        foreach (ConnectionID oldSession in connectedUserInfo.Item3)
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
                user = connectedUserInfo.Item1;
            }

            _logger.LogInformation("New user {UserName} session establiched {SessionID}", userCredential.UserName, connectionID);

            connectedUserInfo.Item3.Add(connectionID);
            _usersConnections[connectionID] = new UserClient(user, connectedUserInfo.Item2);

            return true;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public HotelManagerPortTypeClient? GetClentForConnectionID(ConnectionID? connectionID, bool logWarning = true)
        {
            HotelManagerPortTypeClient? result = null;

            if (connectionID == null || connectionID.Length == 0)
            {
                return result;
            }

            UserClient? userClient;

            if (_usersConnections.TryGetValue(connectionID, out userClient))
            {
                result = userClient.Item2;
            }

#if DEBUG
            if (logWarning && ConnectClient(new CredentionalInfo("Service", "yandex"), connectionID))
            {
                if (_usersConnections.TryGetValue(connectionID, out userClient))
                {
                    result = userClient.Item2;
                }
            }
#endif

            if (result == null && logWarning)
            {
                _logger.LogWarning("Unable to find session with ID: {}", connectionID);
            }

            return result;
        }

    }
}
