namespace Sigma.Core.DataStorage
{
    public class StorageProvider
    {
        public AgreementDataStorage Agreements { get; set; }
        public ClientDataStorage Clients { get; set; }
        public DocumentDataStorage Documents { get; set; }
        public MoneyStoreDataStorage MoneyStores { get; set; }
        public OrganizationDataStorage Organizations { get; set; }
        public ProductDataStorage Products { get; set; }
        public SessionDataStorage Sessions { get; set; }
        public StoreDataStorage Stores { get; set; }

        public StorageProvider()
        {

        }
        /*public StorageProvider(AgreementDataStorage agreements, ClientDataStorage clients,
            DocumentDataStorage documents, MoneyStoreDataStorage moneyStores, OrganizationDataStorage organizations,
            ProductDataStorage products, SessionDataStorage sessions, StoreDataStorage stores)
        {
            Agreements = agreements;
            Clients = clients;
            Documents = documents;
            MoneyStores = moneyStores;
            Organizations = organizations;
            Products = products;
            Sessions = sessions;
            Stores = stores;
        }*/
    }
}
