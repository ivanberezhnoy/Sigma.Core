using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.RemoteHotelEntry;
using static Sigma.Core.Controllers.OrganizationController;
using static Sigma.Core.Controllers.ProductController;

namespace Sigma.Core.Controllers
{

    public class DocumentsDictionary : Dictionary<string, DocumentEntity> {}

    [ApiController]
    [Route("[controller]")]
    public class DocumentController: Controller
    {
        private DocumentsDictionary? _documents;

        private ILogger<DocumentController> _logger;
        private SOAP1CCleintProviderController _clientProvider;
        IHttpContextAccessor _httpContextAccessor;

        public DocumentController(ILogger<DocumentController> logger, SOAP1CCleintProviderController clientProvider, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _clientProvider = clientProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet(Name = "GetDocuemnts")]
        public DocumentsDictionary? Get()
        {
            var session = _clientProvider.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                return getDocuments(session.Client);
            }
            return null;
        }

        private DocumentEntity? fillDocuments(HotelManagerPortTypeClient session, string? documentID = null)
        {
            DocumentEntity? result = null;


            if (_httpContextAccessor.HttpContext == null)
            {
                _logger.LogError("Unable to find HTTP Context");

                return result;
            }

            OrganizationController? organizationController = _httpContextAccessor.HttpContext.RequestServices.GetService<OrganizationController>();
            MoneyStoreController? moneyStoreController = _httpContextAccessor.HttpContext.RequestServices.GetService<MoneyStoreController>();
            StoreController? storeController = _httpContextAccessor.HttpContext.RequestServices.GetService<StoreController>();
            ClientController? clientController = _httpContextAccessor.HttpContext.RequestServices.GetService<ClientController>();
            ProductController? productController = _httpContextAccessor.HttpContext.RequestServices.GetService<ProductController>();

            if (organizationController == null || moneyStoreController == null || storeController == null || clientController == null || productController == null)
            {
                _logger.LogCritical("Unable to filend controller");
                return result;
            }

            if (_documents != null)
            {
                DocumentsList documentsList = session.getDocuments(documentID);
                DocumentsDictionary newDocumentsList = new DocumentsDictionary();

                if (documentsList.error != null && documentsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load documents list. Error : {Error}, organizations: {Organizations} documentID: {DocumentID}", documentsList.error, documentsList, documentID != null ? documentID : "null") ;
                }

                foreach (var document in documentsList.data)
                {
                    DocumentEntity? existingDocument = null;
                    _documents.TryGetValue(document.Id, out existingDocument);

                    OrganizationEntity? organization = organizationController.GetOrganization(session, document.OrganizationId);
                    ClientEntity? client = clientController.GetClient(session, document.ClientId);
                    DocumentEntityType documentType = DocumentEntity.ConvertDocumentType(document.DocumentType);
                    AgreementEntity? agreement = clientController.GetAgreement(session, document.ClientId, document.AgreementId);

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


                    UserEntity? user = _clientProvider.GetUserByID(session, document.UserId);

                    if (user == null)
                    {
                        _logger.LogError("Failed to load user with ID : {UserID} for document with ID: {DocumentID}", document.UserId, document.Id);
                    }


                    if (ProductDocumentEntity.IsProductDocument(document.DocumentType))
                    {
                        StoreEntity? store = storeController.GetStore(session, document.StoreId);

                        if (store == null)
                        {
                            _logger.LogError("Failed to load store with ID : {StoreID} for document with ID: {DocumentID}", document.StoreId, document.Id);
                            continue;
                        }

                        ProductsSales sales = new ProductsSales();

                        uint index = 0;

                        if (document.ProductItems != null)
                        {
                            foreach (ProductItem productItem in document.ProductItems)
                            {
                                ProductEntity? productEntity = productController.GetProduct(session, productItem.ProductId);

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

                                if (unit == null || (characteristic == null && productItem.CharacteristicID != null))
                                {
                                    productController.ReloadProduct(session, productEntity);
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

                                ProductSaleEntity saleEntity = new ProductSaleEntity(index, productEntity, productItem.Quantity, productItem.Price, unit, characteristic);
                                sales.Add(saleEntity);
                                ++index;
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
                        MoneyStoreEntity? moneyStore = moneyStoreController.GetMoneyStore(session, document.MoneyStoreId);

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
                        _logger.LogError("Unknown document type for document with ID: {DocumentID}", existingDocument.Id);
                    }
                }

                foreach (var document in documentsList.data)
                {
                    DocumentEntity? existingDocument = null;
                    if (newDocumentsList.TryGetValue(document.Id, out existingDocument))
                    {
                        DocumentsSet newChildDocuments = new DocumentsSet();
                        foreach(string childDocumentID in document.ChildDocumentsId)
                        {
                            DocumentEntity? existingChildDocument = null;
                            if (newDocumentsList.TryGetValue(childDocumentID, out existingChildDocument))
                            {
                                newChildDocuments.Add(existingChildDocument);
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

        private DocumentsDictionary getDocuments(HotelManagerPortTypeClient session)
        {
            if (_documents == null)
            {
                _documents = new DocumentsDictionary();

                _logger.LogInformation("Reloading organizations list");

                fillDocuments(session);
            }

            return _documents;
        }
    }
}
