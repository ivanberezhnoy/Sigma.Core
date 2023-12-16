﻿using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Mysqlx.Expr;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Crypto.Agreement;
using Sigma.Core.Controllers;
using Sigma.Core.Controllers.TransfterObjects;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Linq;
using System.Reflection.Metadata;

namespace Sigma.Core.DataStorage
{

    public class DocumentsDictionary : Dictionary<string, DocumentEntity> { }

    public class DocumentDataStorage : BaseDataStorage
    {
        private DocumentsDictionary? _documents;
        private Dictionary<EntityFilterDocument, List<DocumentEntity>> _cashedDocumentFilters;

        public DocumentDataStorage(ILogger<DocumentDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            storageProvider.Documents = this;
            _cashedDocumentFilters = new Dictionary<EntityFilterDocument, List<DocumentEntity>>();
        }

        private DocumentEntity? fillDocuments(HotelManagerPortTypeClient session, HotelManager.Document[] documents, string? documentID = null, Dictionary<string, DocumentEntity>? newDocuments = null)
        {
            OrganizationDataStorage organizationDataStorage = _storageProvider.Organizations;
            MoneyStoreDataStorage moneyStoreDataStorage = _storageProvider.MoneyStores;
            StoreDataStorage storeDataStorage = _storageProvider.Stores;
            ClientDataStorage clientDataStorage = _storageProvider.Clients;
            ProductDataStorage productDataStorage = _storageProvider.Products;
            SessionDataStorage sessionDataStorage = _storageProvider.Sessions;

            DocumentsDictionary newDocumentsList = new DocumentsDictionary();

            DocumentEntity? result = null;

            if (_documents == null)
            {
                _documents = new DocumentsDictionary();
            }

            foreach (var document in documents)
            {
                DocumentEntity? existingDocument = null;
                bool documentExists = _documents.TryGetValue(document.Id, out existingDocument);

                OrganizationEntity? organization = organizationDataStorage.GetOrganization(session, document.OrganizationId);
                if (organization == null)
                {
                    _logger.LogError("Failed to load organization with ID : {OrganizationID} for document with ID: {DocumentID}", document.OrganizationId, document.Id);
                    continue;
                }

                UserEntity? user = sessionDataStorage.GetUserByID(session, document.UserId);
                if (user == null)
                {
                    _logger.LogError("Failed to load user with ID : {UserID} for document with ID: {DocumentID}", document.UserId, document.Id);
                }

                DocumentType documentType = document.DocumentType;
                if (ProductDocumentEntity.IsClientDocument(documentType))
                {
                    AgreementEntity? agreement = clientDataStorage.GetAgreement(session, document.ClientId, document.AgreementId);
                    ClientEntity? client = clientDataStorage.GetClient(session, document.ClientId);

                    if (client == null)
                    {
                        _logger.LogError("Failed to load client with ID : {ClientID} for document with ID: {ClientID}", document.ClientId, document.Id);
                        continue;
                    }
                    if (agreement == null)
                    {
                        _logger.LogError("Failed to load agreement with ID : {AgreementID} for document with ID: {DocumentID}", document.AgreementId, document.Id);
                        continue;
                    }


                    if (ProductDocumentEntity.IsProductDocument(documentType))
                    {
                        StoreEntity? store = storeDataStorage.GetStore(session, document.StoreId);

                        if (store == null)
                        {
                            _logger.LogError("Failed to load store with ID : {StoreID} for document with ID: {DocumentID}", document.StoreId, document.Id);
                            continue;
                        }

                        ProductsSales sales = new ProductsSales();

                        if (document.ProductItems != null)
                        {
                            uint index = 0;
                            foreach (ProductItem productItem in document.ProductItems)
                            {
                                ProductEntity? productEntity = productDataStorage.GetProduct(session, productItem.ProductId);

                                if (productEntity == null)
                                {
                                    _logger.LogError("Unable to load product sale with product ID: {ProductID} document ID: {DocumentID}", productItem.ProductId, document.Id);
                                    continue;
                                }

                                UnitEntity? unit = productEntity.GetUnitWithID(productItem.UnitId);
                                CharacteristicEntity? characteristic = null;
                                if (productItem.CharacteristicId != null)
                                {
                                    characteristic = productEntity.GetCharacteristicWithID(productItem.CharacteristicId);
                                    if (unit == null)
                                    {
                                        _logger.LogError("Try to reload unit with unit ID: {UnitID} document ID: {DocumentID} product ID: {ProductID}", productItem.UnitId, document.Id, productItem.ProductId);
                                    }
                                    if (characteristic == null)
                                    {
                                        _logger.LogError("Try to reload characteristic with characteristic ID: {CharacteristicID} document ID: {DocumentID} product ID: {ProductID}", productItem.CharacteristicId, document.Id, productItem.ProductId);
                                    }
                                }

                                if (unit == null || characteristic == null && productItem.CharacteristicId != null)
                                {
                                    productDataStorage.ReloadProduct(session, productEntity);
                                    unit = productEntity.GetUnitWithID(productItem.UnitId);
                                    if (unit == null)
                                    {
                                        _logger.LogError("Filed to reload unit with unit ID: {UnitID} document ID: {DocumentID} product ID: {ProductID}", productItem.UnitId, document.Id, productItem.ProductId);
                                        continue;
                                    }
                                    if (productItem.CharacteristicId != null)
                                    {
                                        characteristic = productEntity.GetCharacteristicWithID(productItem.CharacteristicId);
                                        if (characteristic == null)
                                        {
                                            _logger.LogError("Filed to reload characteristic with characteristic ID: {CharacteristicID} document ID: {DocumentID} product ID: {ProductID}", productItem.CharacteristicId, document.Id, productItem.ProductId);                                            continue;
                                        }
                                    }
                                }

                                ProductSaleEntity saleEntity = new ProductSaleEntity(index.ToString(), productEntity, productItem.Quantity, productItem.Price, unit, characteristic);
                                sales[index.ToString()] = saleEntity;
                                ++index;
                            }
                        }

                        if (existingDocument == null)
                        {
                            existingDocument = new ProductDocumentEntity(document.Id, organization, document.Date, document.Sum, document.Comment, user, documentType, document.IsActive, client, agreement, store, sales, document.easyMSBookingId);
                            _documents[document.Id] = existingDocument;
                        }
                        else
                        {
                            ((ProductDocumentEntity)existingDocument).Fill(organization, document.Date, document.Sum, document.Comment, user, document.IsActive, client, agreement, store, sales, document.easyMSBookingId);
                        }
                    }
                    else if (ProductDocumentEntity.IsMoneyStoreDocument(documentType))
                    {
                        MoneyStoreEntity? moneyStore = moneyStoreDataStorage.GetMoneyStore(session, document.MoneyStoreId);

                        if (moneyStore == null)
                        {
                            _logger.LogError("Failed to load money store with ID : {MoneyStoreID} for document with ID: {DocumentID}", document.MoneyStoreId, document.Id);
                            continue;
                        }

                        if (existingDocument == null)
                        {
                            existingDocument = new MoneyStoreDocumentEntity(document.Id, organization, document.Date, document.Sum, document.Comment, user, documentType,
                                document.IsActive, client, agreement, moneyStore, document.easyMSBookingId);
                        }
                        else
                        {
                            ((MoneyStoreDocumentEntity)existingDocument).Fill(organization, document.Date, document.Sum, document.Comment, user, document.IsActive, client, agreement, moneyStore, document.easyMSBookingId);
                        }
                    }
                }
                else if (documentType == DocumentType.MoneyTransfer)
                {
                    MoneyStoreEntity? moneyStoreFrom = moneyStoreDataStorage.GetMoneyStore(session, document.MoneyStoreToId);

                    if (moneyStoreFrom == null)
                    {
                        _logger.LogError("Failed to load source money store with ID : {MoneyStoreID} for document with ID: {DocumentID}", document.MoneyStoreId, document.Id);
                        continue;
                    }

                    MoneyStoreEntity? moneyStoreTo = moneyStoreDataStorage.GetMoneyStore(session, document.MoneyStoreToId);

                    if (moneyStoreTo == null)
                    {
                        _logger.LogError("Failed to load target money store with ID : {MoneyStoreID} for document with ID: {DocumentID}", document.MoneyStoreId, document.Id);
                        continue;
                    }

                    if (existingDocument == null)
                    {
                        existingDocument = new MoneyTransferDocumentEntity(document.Id, organization, document.Date, document.Sum, document.Comment, user,
                            document.IsActive, moneyStoreFrom, moneyStoreTo);
                    }
                    else
                    {
                        ((MoneyTransferDocumentEntity)existingDocument).Fill(organization, document.Date, document.Sum, document.Comment, user, document.IsActive, moneyStoreFrom, moneyStoreTo);
                    }
                }

                if (existingDocument != null)
                {
                    newDocumentsList[existingDocument.Id] = existingDocument;

                    if (documentID != null && documentID == existingDocument.Id)
                    {
                        result = existingDocument;
                    }

                    if (!documentExists && newDocuments != null)
                    {
                        newDocuments[existingDocument.Id] = existingDocument;
                    }
                }
                else
                {
                    _logger.LogError("Unknown document type for document with ID: {DocumentID}", document.Id);
                }
            }

            foreach (var document in documents)
            {
                DocumentEntity? existingDocument = null;
                if (newDocumentsList.TryGetValue(document.Id, out existingDocument))
                {
                    DocumentsDictionary newChildDocuments = new DocumentsDictionary();
                    foreach (string childDocumentID in document.ChildDocumentsId)
                    {
                        DocumentEntity? existingChildDocument;
                        if (newDocumentsList.TryGetValue(childDocumentID, out existingChildDocument))
                        {
                            newChildDocuments[childDocumentID] = existingChildDocument;
                        }
                        else
                        {
                            _logger.LogError("Unable to find child document with ID: {DocumentID}, for parent document with ID: {ParentDocumentID}", childDocumentID, document.Id);
                        }
                    }

                    existingDocument.SetChildDocuments(newChildDocuments);
                }
            }

            _documents = newDocumentsList;

            return result;
        }

        private DocumentEntity? fillDocuments(HotelManagerPortTypeClient session, string? documentID = null)
        {
            DocumentEntity? result = null;

            if (_documents != null)
            {
                _cashedDocumentFilters.Clear();

                DocumentsList documentsList = session.getDocuments(documentID);

                if (documentsList.error != null && documentsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load documents list. Error : {Error}, organizations: {Organizations} documentID: {DocumentID}", documentsList.error, documentsList, documentID != null ? documentID : "null");
                }

                result = fillDocuments(session, documentsList.data, documentID);
            }

            return result;
        }

        public List<DocumentEntity> GetSortedDocuments(HotelManagerPortTypeClient session, EntityFilterDocument? filter)
        {
            EntityFilterDocument targetFilter = filter ?? new EntityFilterDocument();

            List<DocumentEntity>? result;
            if (_cashedDocumentFilters.TryGetValue(targetFilter, out result))
            {
                return result;
            }

            DocumentsDictionary documents = this.GetDocuments(session);
            bool completeMismatch = false;
            var filteredValues = !targetFilter.IsEmpty() ? documents.Values.Where(document => document.filterMatch(targetFilter, out completeMismatch)).ToList() : documents.Values.ToList();

            filteredValues.Sort((document1, document2) => 
            {
                if (document1.Date == null || document2.Date == null)
                {
                    if (document1.Date == null && document2.Date == null)
                    {
                        return 0;
                    }

                    return document1.Date == null ? -1 : 1;
                }

                return DateTime.Compare((DateTime)document2.Date, (DateTime)document1.Date);
            });

            _cashedDocumentFilters[targetFilter] = filteredValues;

            return filteredValues;
        }

        public DocumentsDictionary GetDocuments(HotelManagerPortTypeClient session)
        {
            if (_documents == null)
            {
                _documents = new DocumentsDictionary();

                _logger.LogInformation("Reloading documents list started");

                fillDocuments(session);

                _logger.LogInformation("Reloading documents list finished");
            }

            return _documents;
        }

        DocumentEntity? GetDocument(HotelManagerPortTypeClient session, string documentId, bool logError = false)
        {
            DocumentsDictionary documents = GetDocuments(session);
            DocumentEntity? result;

            if (!documents.TryGetValue(documentId, out result))
            {
                if (logError)
                {
                    _logger.LogError("GetDocument: Unable to find document with ID: {DocumentID}", documentId);
                }
            }

            return result;
        }

        public RequestResult setDocuments(HotelManagerPortTypeClient session, HotelManager.Document[] documents)
        {
            HotelManager.DocumentsList documentsList = new DocumentsList();
            documentsList.data = documents;

            HotelManager.Error[] errors = session.setDocuments(documentsList);

            foreach (HotelManager.Error error in errors)
            {
                _logger.LogError("setDocuments errorCode: {ErrorCode}, desciption: {ErrorDescription}", error.errorCode, error.errorDescription);
            }

            Dictionary<string, DocumentEntity> newDocuments = new Dictionary<string, DocumentEntity>();
            fillDocuments(session, documents, null, newDocuments);

            foreach (DocumentEntity document in newDocuments.Values)
            { 

            }

            RequestResult result = new RequestResult(errors);

            return result;
        }
    }
}
