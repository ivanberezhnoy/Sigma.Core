using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.Controllers;
using Sigma.Core.RemoteHotelEntry;

namespace Sigma.Core.DataStorage
{

    public class DocumentsDictionary : Dictionary<string, DocumentEntity> { }

    public class DocumentDataStorage : BaseDataStorage
    {
        private DocumentsDictionary? _documents;

        public DocumentDataStorage(ILogger<DocumentDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            storageProvider.Documents = this;
        }

        /*[HttpGet(Name = "GetDocuemnts")]
        public DocumentsDictionary? Get()
        {
            var session = _clientProvider.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                return getDocuments(session.Client);
            }
            return null;
        }*/

        private DocumentEntity? fillDocuments(HotelManagerPortTypeClient session, string? documentID = null)
        {
            DocumentEntity? result = null;

            OrganizationDataStorage organizationDataStorage = _storageProvider.Organizations;
            MoneyStoreDataStorage moneyStoreDataStorage = _storageProvider.MoneyStores;
            StoreDataStorage storeDataStorage = _storageProvider.Stores;
            ClientDataStorage clientDataStorage = _storageProvider.Clients;
            ProductDataStorage productDataStorage = _storageProvider.Products;
            SessionDataStorage sessionDataStorage = _storageProvider.Sessions;

            if (_documents != null)
            {
                DocumentsList documentsList = session.getDocuments(documentID);
                DocumentsDictionary newDocumentsList = new DocumentsDictionary();

                if (documentsList.error != null && documentsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load documents list. Error : {Error}, organizations: {Organizations} documentID: {DocumentID}", documentsList.error, documentsList, documentID != null ? documentID : "null");
                }

                foreach (var document in documentsList.data)
                {
                    DocumentEntity? existingDocument = null;
                    _documents.TryGetValue(document.Id, out existingDocument);

                    OrganizationEntity? organization = organizationDataStorage.GetOrganization(session, document.OrganizationId);
                    ClientEntity? client = clientDataStorage.GetClient(session, document.ClientId);
                    DocumentEntityType documentType = DocumentEntity.ConvertDocumentType(document.DocumentType);
                    AgreementEntity? agreement = clientDataStorage.GetAgreement(session, document.ClientId, document.AgreementId);

                    if (client == null)
                    {
                        _logger.LogError("Failed to load client with ID : {ClientID} for document with ID: {ClientID}", document.ClientId, document.Id);
                        continue;
                    }
                    if (organization == null)
                    {
                        _logger.LogError("Failed to load organization with ID : {OrganizationID} for document with ID: {DocumentID}", document.OrganizationId, document.Id);
                        continue;
                    }
                    if (agreement == null)
                    {
                        _logger.LogError("Failed to load agreement with ID : {AgreementID} for document with ID: {DocumentID}", document.AgreementId, document.Id);
                        continue;
                    }


                    UserEntity? user = sessionDataStorage.GetUserByID(session, document.UserId);

                    if (user == null)
                    {
                        _logger.LogError("Failed to load user with ID : {UserID} for document with ID: {DocumentID}", document.UserId, document.Id);
                    }


                    if (ProductDocumentEntity.IsProductDocument(document.DocumentType))
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
                            foreach (ProductItem productItem in document.ProductItems)
                            {
                                ProductEntity? productEntity = productDataStorage.GetProduct(session, productItem.ProductId);

                                if (productEntity == null)
                                {
                                    _logger.LogError("Unable to load product sale with product ID: {ProductID} document ID: {DocumentID}", productItem.ProductId, document.Id);
                                    continue;
                                }

                                UnitEntity? unit = productEntity.GetUnitWithID(productItem.UnitID);
                                CharacteristicEntity? characteristic = null;
                                if (productItem.CharacteristicID != null)
                                {
                                    characteristic = productEntity.GetCharacteristicWithID(productItem.CharacteristicID);
                                    if (unit == null)
                                    {
                                        _logger.LogError("Try to reload unit with unit ID: {UnitID} document ID: {DocumentID} product ID: {ProductID}", productItem.UnitID, document.Id, productItem.ProductId);
                                    }
                                    if (characteristic == null)
                                    {
                                        _logger.LogError("Try to reload characteristic with characteristic ID: {CharacteristicID} document ID: {DocumentID} product ID: {ProductID}", productItem.CharacteristicID, document.Id, productItem.ProductId);
                                    }
                                }

                                if (unit == null || characteristic == null && productItem.CharacteristicID != null)
                                {
                                    productDataStorage.ReloadProduct(session, productEntity);
                                    unit = productEntity.GetUnitWithID(productItem.UnitID);
                                    if (unit == null)
                                    {
                                        _logger.LogError("Filed to reload unit with unit ID: {UnitID} document ID: {DocumentID} product ID: {ProductID}", productItem.UnitID, document.Id, productItem.ProductId);
                                        continue;
                                    }
                                    if (productItem.CharacteristicID != null)
                                    {
                                        characteristic = productEntity.GetCharacteristicWithID(productItem.CharacteristicID);
                                        if (characteristic == null)
                                        {
                                            _logger.LogError("Filed to reload characteristic with characteristic ID: {CharacteristicID} document ID: {DocumentID} product ID: {ProductID}", productItem.CharacteristicID, document.Id, productItem.ProductId);
                                            continue;
                                        }
                                    }
                                }

                                ProductSaleEntity saleEntity = new ProductSaleEntity(productEntity, productItem.Quantity, productItem.Price, unit, characteristic);
                                sales.Add(saleEntity);
                            }
                        }

                        if (existingDocument == null)
                        {
                            existingDocument = new ProductDocumentEntity(document.Id, organization, client, document.Date, document.Comment, user, documentType, document.IsActive, agreement, store, sales);
                            _documents[document.Id] = existingDocument;
                        }
                        else
                        {
                            ((ProductDocumentEntity)existingDocument).Fill(organization, client, document.Date, document.Comment, user, document.IsActive, agreement, store, sales);
                        }
                    }
                    else if (ProductDocumentEntity.IsMoneyDocument(document.DocumentType))
                    {
                        MoneyStoreEntity? moneyStore = moneyStoreDataStorage.GetMoneyStore(session, document.MoneyStoreId);

                        if (moneyStore == null)
                        {
                            _logger.LogError("Failed to load money store with ID : {MoneyStoreID} for document with ID: {DocumentID}", document.MoneyStoreId, document.Id);
                            continue;
                        }

                        if (existingDocument == null)
                        {
                            existingDocument = new MoneyStoreDocumentEntity(document.Id, organization, client, document.Date, document.Comment, user, documentType,
                                document.IsActive, agreement, moneyStore, document.Sum);
                        }
                        else
                        {
                            ((MoneyStoreDocumentEntity)existingDocument).Fill(organization, client, document.Date, document.Comment, user, document.IsActive, agreement, moneyStore, document.Sum);
                        }
                    }

                    if (existingDocument != null)
                    {
                        newDocumentsList[existingDocument.Id] = existingDocument;

                        if (documentID != null && documentID == existingDocument.Id)
                        {
                            result = existingDocument;
                        }
                    }
                    else
                    {
                        _logger.LogError("Unknown document type for document with ID: {DocumentID}", document.Id);
                    }
                }

                foreach (var document in documentsList.data)
                {
                    DocumentEntity? existingDocument = null;
                    if (newDocumentsList.TryGetValue(document.Id, out existingDocument))
                    {
                        DocumentsDictionary newChildDocuments = new DocumentsDictionary();
                        foreach (string childDocumentID in document.ChildDocumentsId)
                        {
                            DocumentEntity? existingChildDocument = null;
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
            }

            return result;
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
    }
}
