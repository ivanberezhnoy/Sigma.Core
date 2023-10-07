namespace Sigma.Core.RemoteHotelEntry
{
    using ProductID = String;
    public class ProductsSales : Dictionary<ProductID, ProductSaleEntity> { }
    public class SaleEntity
    {
        public SaleEntity(string id, OrganizationEntity organization, ClientEntity client, ProductsSales products, DateTime saleDate, string? comment, UserEntity user, StoreEntity store)
        {
            Id = id;
            Organization = organization;
            Client = client;
            Products = products;
            SaleDate = saleDate;
            Comment = comment;
            User = user;
            Store = store;
        }

        public string Id { get; set; }
        public OrganizationEntity Organization { get; set; }
        public ClientEntity Client { get; set; }

        public ProductsSales Products { get; set; }

        public DateTime SaleDate { get; set; }

        public string? Comment { get; set; }

        public UserEntity User { get; set; }

        public StoreEntity Store { get; set; }
    }
}
