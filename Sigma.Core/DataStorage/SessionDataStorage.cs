using HotelManager;
using System.Collections.Generic;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;

namespace Sigma.Core.DataStorage
{
    using UserName = System.String;

    public class UserClient
    {
        public UserEntity User { get; set; }

        public HotelManagerPortTypeClient? Client { get; set; }

        public UserClient(UserEntity user, HotelManagerPortTypeClient? client)
        {
            User = user;
            Client = client;
        }
    }

    public class SessionDataStorage : BaseDataStorage
    {
        public SessionDataStorage(ILogger<SessionDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _usersClient = new Dictionary<UserName, UserClient>();

            _storageProvider.Sessions = this;
        }

        // Users by name
        private Dictionary<UserName, UserClient> _usersClient;
        // Users by ID
        private Dictionary<string, UserEntity>? _usersDictionary;

        private Dictionary<UserName, UserEntity> reloadUsers(HotelManagerPortTypeClient client)
        {
            if (_usersDictionary == null)
            {
                _usersDictionary = new Dictionary<UserName, UserEntity>();
            }

            _logger.LogInformation("Reload all users");

            UsersList users = client.getUsers();

            foreach (var user in users.data)
            {
                if (!_usersDictionary.ContainsKey(user.Id))
                {
                    UserEntity newUser = new UserEntity(user.Name, null, user.Id);

                    if (_usersClient.ContainsKey(newUser.Name))
                    {
                        _logger.LogError("Inconsistensy for users storage for user with name {UserName}", newUser.Name);
                    }

                    _usersClient[newUser.Id.Trim()] = new UserClient(newUser, null);

                    _usersDictionary[newUser.Id.Trim()] = newUser;
                }
            }

            return _usersDictionary;
        }
        public Dictionary<string, UserEntity> GetUsers(HotelManagerPortTypeClient client)
        {
            if (_usersDictionary == null)
            {
                return reloadUsers(client);
            }

            return _usersDictionary;
        }
        public UserEntity? GetUserByID(HotelManagerPortTypeClient client, string userID)
        {
            UserEntity? result = null;
            bool usersReloaded = false;

            Dictionary<UserName, UserEntity>? usersDictionary = _usersDictionary;

            userID = userID.Trim();

            if (usersDictionary == null)
            {
                usersDictionary = reloadUsers(client);

                usersReloaded = true;
            }

            if (usersDictionary.TryGetValue(userID, out result))
            {
                return result;
            }

            if (!usersReloaded)
            {
                reloadUsers(client);

                if (!usersDictionary.TryGetValue(userID, out result))
                {
                    _logger.LogWarning("Unable find user with UserID: {UserID}", userID);
                }
            }

            return result;
        }

        public bool ConnectClient(CredentionalInfo userCredential)
        {
            _logger.LogInformation("Try to connect user: {UserName}", userCredential.UserName);

            UserClient? userClient = GetClientForUserWithName(userCredential.UserName, false);

            if (userClient != null && userClient.Client != null)
            {
                if (userClient.User.Password == null)
                {
                    _logger.LogError("Unable to connect user with username {UserName}, Empty password detected for connected client", userCredential.UserName);

                    return false;
                }

                if (userClient.User.Password == userCredential.Password)
                {
                    return true;
                }
            }

            UserEntity? user = userClient?.User;

            if (userClient != null && userClient.Client != null && userClient.User.Password != userCredential.Password)
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
            hotelManagerBinding.SendTimeout = timeout;
            hotelManagerBinding.OpenTimeout = timeout;
            hotelManagerBinding.ReceiveTimeout = timeout;
            hotelManagerBinding.CloseTimeout = timeout;
#endif

            //var hotelManagerEndpoint = new System.ServiceModel.EndpointAddress("http://192.168.1.152/ApartmentDeveloper/ws/ws2.1cws");
            var hotelManagerEndpoint = new System.ServiceModel.EndpointAddress("http://192.168.1.152/HotelManager/ws/ws2.1cws");

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

            if (userClient != null && userClient.Client != null)
            {
                _logger.LogWarning("User: {User} password was changed", userCredential.UserName);

                userClient.Client.CloseAsync();
                userClient.Client = null;
            }

            // Get user from cache or create new in cache
            user = GetUserByID(client, userSettings.UserID.Trim());
            userClient = GetClientForUserWithName(userCredential.UserName, true);

            if (user == null || userClient == null)
            {
                _logger.LogError("Unable to get user with ID: {UserID} and name: {}", userSettings.UserID, userCredential.UserName);

                return false;
            }

            OrganizationDataStorage OrganizationDataStorage = _storageProvider.Organizations;
            MoneyStoreDataStorage moneyStoreDataStorage = _storageProvider.MoneyStores;
            StoreDataStorage StoreDataStorage = _storageProvider.Stores;
            ClientDataStorage ClientDataStorage = _storageProvider.Clients;

            user.Name = userCredential.UserName;
            user.Password = userCredential.Password;

            user.DefaultOrganization = OrganizationDataStorage.GetOrganization(client, userSettings.DefaultOrganizationId);
            user.DefaultMoneyStore = moneyStoreDataStorage.GetMoneyStore(client, userSettings.DefaultMoneyStoreId);
            user.DefaultMoneyStoreCash = moneyStoreDataStorage.GetMoneyStore(client, userSettings.DefaultMoneyStoreCashId);
            user.DefaultMoneyStoreTransfer = moneyStoreDataStorage.GetMoneyStore(client, userSettings.DefaultMoneyStoreTransferId);
            user.DefaultMoneyStoreTerminal = moneyStoreDataStorage.GetMoneyStore(client, userSettings.DefaultMoneyStoreTerminalId);
            user.DefaultStore = StoreDataStorage.GetStore(client, userSettings.DefaultStoreId);
            user.DefaultClient = ClientDataStorage.GetClient(client, userSettings.DefaultClientId);

            userClient.Client = client;

             _logger.LogInformation("New user {UserName} session", userCredential.UserName);

            return true;
        }

        static public UserName? GetCurrentUserName(HttpContext context)
        {
            return context.User.Identity?.Name
                   ?? context.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        public UserClient? GetClientForUserWithName(String? userName, bool logWarning = true)
        {
            if (userName == null)
            {
                _logger.LogWarning("Unauthorized user");

                return null;
            }
            UserClient? result;

            _usersClient.TryGetValue(userName, out result);

            if (result == null && logWarning)
            {
                _logger.LogWarning("Unable to find client for User with name: {}", userName);
            }

            return result;
        }

    }
}
