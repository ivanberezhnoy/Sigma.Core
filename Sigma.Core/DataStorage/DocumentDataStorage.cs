using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mysqlx;
using Mysqlx.Expr;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Crypto.Agreement;
using Sigma.Core.Controllers;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace Sigma.Core.DataStorage
{

    public class DocumentsDictionary : Dictionary<string, DocumentEntity> { }

    public class DocumentDataStorage : BaseDataStorage
    {
        private class DocumentCacheState
        {
            public DocumentsDictionary Documents { get; } = new DocumentsDictionary();
            public Dictionary<EntityFilterDocument, List<DocumentEntity>> CachedFilters { get; } = new Dictionary<EntityFilterDocument, List<DocumentEntity>>(new EntityFilterDocumentComparer());
            public HashSet<string> DocumentsToUpdate { get; } = new HashSet<string>();
        }

        private readonly Dictionary<EndpointType, DocumentCacheState> _documentsByEndpoint;

        public DocumentDataStorage(ILogger<DocumentDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            storageProvider.Documents = this;
            _documentsByEndpoint = new Dictionary<EndpointType, DocumentCacheState>();
        }

        private DocumentCacheState GetState(HotelManagerPortTypeClient session)
        {
            var endpoint = GetEndpoint(session);
            if (!_documentsByEndpoint.TryGetValue(endpoint, out var state))
            {
                state = new DocumentCacheState();
                _documentsByEndpoint[endpoint] = state;
            }

            return state;
        }

        private DocumentsDictionary? fillDocuments(HotelManagerPortTypeClient session, HotelManager.Document[] documents, HashSet<string>? documentsToUpdate, DocumentsDictionary? createdDocuments = null,
            bool foolReload = true)
        {
            var state = GetState(session);
            OrganizationDataStorage organizationDataStorage = _storageProvider.Organizations;
            MoneyStoreDataStorage moneyStoreDataStorage = _storageProvider.MoneyStores;
            StoreDataStorage storeDataStorage = _storageProvider.Stores;
            ClientDataStorage clientDataStorage = _storageProvider.Clients;
            ProductDataStorage productDataStorage = _storageProvider.Products;
            SessionDataStorage sessionDataStorage = _storageProvider.Sessions;

            DocumentsDictionary newDocuments = createdDocuments ?? new DocumentsDictionary();
            DocumentsDictionary documentsList = new DocumentsDictionary();

            DocumentsDictionary? result = null;

            if (documentsToUpdate != null)
            {
                result = new DocumentsDictionary();
            }

            foreach (var document in documents)
            {
                DocumentEntity? existingDocument = null;
                bool documentExists = state.Documents.TryGetValue(document.Id, out existingDocument);

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
                            existingDocument = new ProductDocumentEntity(document.Id, organization, document.Date, document.Sum, document.Comment, user, documentType, document.IsActive, document.IsDeleted, client, agreement, store, sales, document.easyMSBookingId);
                            state.Documents[document.Id] = existingDocument;
                        }
                        else
                        {
                            ((ProductDocumentEntity)existingDocument).Fill(organization, document.Date, document.Sum, document.Comment, user, document.IsActive, document.IsDeleted, client, agreement, store, sales, document.easyMSBookingId);
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
                                document.IsActive, document.IsDeleted, client, agreement, moneyStore, document.easyMSBookingId);
                        }
                        else
                        {
                            ((MoneyStoreDocumentEntity)existingDocument).Fill(organization, document.Date, document.Sum, document.Comment, user, document.IsActive, document.IsDeleted, client, agreement, moneyStore, document.easyMSBookingId);
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
                            document.IsActive, document.IsDeleted, moneyStoreFrom, moneyStoreTo);
                    }
                    else
                    {
                        ((MoneyTransferDocumentEntity)existingDocument).Fill(organization, document.Date, document.Sum, document.Comment, user, document.IsActive, document.IsDeleted, moneyStoreFrom, moneyStoreTo);
                    }
                }

                if (existingDocument != null)
                {
                    documentsList[existingDocument.Id] = existingDocument;

                    if (result != null && documentsToUpdate != null && documentsToUpdate.Contains(existingDocument.Id))
                    {
                        result[existingDocument.Id] = existingDocument;
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
                if (documentsList.TryGetValue(document.Id, out existingDocument))
                {
                    DocumentsDictionary newChildDocuments = new DocumentsDictionary();
                    foreach (string childDocumentID in document.ChildDocumentsId)
                    {
                        DocumentEntity? existingChildDocument;
                        if (documentsList.TryGetValue(childDocumentID, out existingChildDocument))
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

            if (foolReload)
            {
                state.Documents.Clear();
                foreach (var documentInfo in documentsList)
                {
                    state.Documents[documentInfo.Key] = documentInfo.Value;
                }
            }
            else
            {
                foreach (var documentInfo in newDocuments)
                {
                    state.Documents[documentInfo.Key] = documentInfo.Value;
                }
            }

            return result;
        }

        private DocumentsDictionary? fillDocuments(HotelManagerPortTypeClient session, HashSet<String>? documentsToUpdate = null)
        {
            var state = GetState(session);
            DocumentsDictionary? result = null;

            if (state.Documents != null)
            {
                state.CachedFilters.Clear();

                DocumentsList documentsList = session.getDocuments(documentsToUpdate != null ? documentsToUpdate.ToArray(): null);

                if (documentsList.error != null && documentsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load documents list. Error : {Error}, organizations: {Organizations} documentID: {DocumentID}", documentsList.error, documentsList, documentsToUpdate.ToString());
                }

                result = fillDocuments(session, documentsList.data, documentsToUpdate, null, documentsToUpdate == null);
            }

            return result;
        }

        public List<DocumentEntity> GetSortedDocuments(HotelManagerPortTypeClient session, EntityFilterDocument? filter)
        {
            var state = GetState(session);
            updateChangedDocuments(session);

            EntityFilterDocument targetFilter = filter ?? new EntityFilterDocument();

            List<DocumentEntity>? result;
            if (state.CachedFilters.TryGetValue(targetFilter, out result))
            {
                return result;
            }

            DocumentsDictionary documents = GetDocuments(session);
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

            state.CachedFilters[targetFilter] = filteredValues;

            return filteredValues;
        }

        public DocumentsDictionary GetDocuments(HotelManagerPortTypeClient session)
        {
            var state = GetState(session);

            if (state.Documents.Count == 0)
            {
                _logger.LogInformation("Reloading documents list started");

                state.DocumentsToUpdate.Clear();

                fillDocuments(session);

                _logger.LogInformation("Reloading documents list finished");
            }

            updateChangedDocuments(session);

            return state.Documents;
        }

        private void updateChangedDocuments(HotelManagerPortTypeClient session)
        {
            var state = GetState(session);

            if (state.DocumentsToUpdate.Count > 0)
            {
                DocumentsDictionary? changedDocuments = fillDocuments(session, state.DocumentsToUpdate);

                if (changedDocuments != null)
                {
                    updateCashedDocumentsFilters(state, changedDocuments);
                }
                state.DocumentsToUpdate.Clear();
            }
        }

        private void updateCashedDocumentsFilters(DocumentCacheState state, DocumentsDictionary changedDocuments)
        {
            foreach (KeyValuePair<EntityFilterDocument, List<DocumentEntity>> filter in state.CachedFilters)
            {
                bool isFilterSortChanged = false;
                foreach (DocumentEntity document in changedDocuments.Values)
                {
                    bool completeMismatch = false;
                    //TODO: use seak in sorted array
                    bool containsDocument = filter.Value.Contains(document);
                    bool shouldContain = document.filterMatch(filter.Key, out completeMismatch);

                    if (!containsDocument && shouldContain)
                    {
                        //TODO: insert in sorted array
                        filter.Value.Add(document);
                        isFilterSortChanged = true;
                    }
                    else if (containsDocument && !shouldContain)
                    {
                        filter.Value.Remove(document);
                    }
                }

                if (isFilterSortChanged)
                {
                    filter.Value.Sort((document1, document2) =>
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
                }
            }
        }

        public void UpdateDocument(HotelManagerPortTypeClient session, string documentId)
        {
            var state = GetState(session);
            state.DocumentsToUpdate.Add(documentId);
        }

        public RequestResult setDocuments(HotelManagerPortTypeClient session, HotelManager.Document[] documents)
        {
            var state = GetState(session);
            HotelManager.DocumentsList documentsList = new DocumentsList();
            documentsList.data = documents;

            HotelManager.Result result = session.setDocuments(documentsList);

            if (result.errors != null)
            {
                foreach (HotelManager.Error error in result.errors)
                {
                    _logger.LogError("setDocuments errorCode: {ErrorCode}, desciption: {ErrorDescription}", error.errorCode, error.errorDescription);
                }
            }

            if (result.documetsMapping != null)
            {
                Dictionary<string, string> documentsMapping = new Dictionary<string, string>();
                foreach (HotelManager.DocumentMapping mapping in result.documetsMapping)
                {
                    documentsMapping[mapping.guidID] = mapping.number;
                }

                foreach (HotelManager.Document document in documents)
                {
                    if (documentsMapping.TryGetValue(document.Id, out string? mappedId))
                    {
                        document.Id = mappedId;
                    }
                }
            }

            DocumentsDictionary newDocuments = new DocumentsDictionary();
            fillDocuments(session, documents, null, newDocuments, false);
            updateCashedDocumentsFilters(state, newDocuments);

            return new RequestResult(result.errors, newDocuments);
        }
    }
}
