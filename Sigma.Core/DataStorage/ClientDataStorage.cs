﻿using HotelManager;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using Sigma.Core.Controllers;
using Sigma.Core.RemoteHotelEntry;

namespace Sigma.Core.DataStorage
{
    public class ClientDataStorage : BaseDataStorage
    {
        public class ClientDicrionary : Dictionary<string, ClientEntity> { };
        public ClientDicrionary? _clients;

        public ClientDataStorage(ILogger<ClientDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Clients = this;
        }

        private ClientEntity? fillClients(HotelManagerPortTypeClient session, string? clientID = null)
        {
            ClientEntity? result = null;

            AgreementDataStorage AgreementDataStorage = _storageProvider.Agreements;

            if (_clients != null)
            {
                ClientsList clientsList = session.getClients(clientID);

                if (clientsList.error != null && clientsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load clients list. Error : {Error}, clients: {Organizations}, clientID: {ClientID}", clientsList.error, clientsList, clientID != null ? clientID : "null");
                }

                foreach (var client in clientsList.data)
                {
                    Agreements agreements = new Agreements();
                    AgreementEntity? mainAgreement = null;
                    foreach (var agreement in client.AgreementsId)
                    {
                        var agreementEntity = AgreementDataStorage.GetAgreement(session, agreement);
                        if (agreementEntity != null)
                        {
                            agreements[agreementEntity.Id] = agreementEntity;
                            if (mainAgreement == null)
                            {
                                mainAgreement = agreementEntity;
                            }
                        }
                    }

                    if (!_clients.TryGetValue(client.Id, out result))
                    {
                        result = new ClientEntity(client, agreements, mainAgreement);
                        _clients[client.Id] = result;
                    }
                    else
                    {
                        result.FillClient(client, agreements, mainAgreement);
                    }
                }
            }

            return result;
        }

        public AgreementEntity? GetAgreement(HotelManagerPortTypeClient session, string clientID, string agreemntID)
        {
            AgreementEntity? result = null;

            var client = GetClient(session, clientID);

            if (client == null)
            {
                _logger.LogError("Unable to find client with ID {ClientID}", clientID);
                return result;
            }

            if (!client.Agreements.TryGetValue(agreemntID, out result))
            {
                _logger.LogInformation("Unable to find agreement with ID: {AgreementID}. Try to reload client with ID: {ClientID}", agreemntID, clientID);
                client = fillClients(session, clientID);
            }

            if (client != null)
            {
                if (!client.Agreements.TryGetValue(agreemntID, out result))
                {
                    _logger.LogInformation("Unable to find agreement with ID: {AgreementID}. For client with ID: {ClientID}", agreemntID, clientID);
                }
            }

            return result;
        }

        private ClientDicrionary getClients(HotelManagerPortTypeClient session)
        {
            if (_clients == null)
            {
                _clients = new ClientDicrionary();

                _logger.LogInformation("Reloading clients list");

                fillClients(session);
            }

            return _clients;
        }

        public ClientEntity? GetClient(HotelManagerPortTypeClient session, string? clientID)
        {
            ClientEntity? result = null;

            if (clientID == null || clientID.Length == 0)
            {
                return result;
            }

            ClientDicrionary clients = getClients(session);

            if (!clients.TryGetValue(clientID, out result))
            {
                _logger.LogInformation("Loading client with ID {ClientID}", clientID);

                result = fillClients(session, clientID);
            }

            return result;
        }
    }
}
