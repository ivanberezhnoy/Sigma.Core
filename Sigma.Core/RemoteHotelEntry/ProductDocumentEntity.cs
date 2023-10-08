namespace Sigma.Core.RemoteHotelEntry
{
    using ProductID = String;
    public class ProductsSales : Dictionary<ProductID, ProductSaleEntity> { }

    public class ProductDocumentEntity : DocumentEntity
    {
        public ProductDocumentEntity(string id, OrganizationEntity organization, ClientEntity client, ProductsSales products, DateTime date, string? comment, UserEntity user, StoreEntity store, DocumentType documentType, bool isActive) 
            : base(id, organization, client, date, comment, user, documentType, isActive)
        {
            Products = products;
            Store = store;
        }

        public ProductsSales Products { get; set; }

        public StoreEntity Store { get; set; }
    }
}
