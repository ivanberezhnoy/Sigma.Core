namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyStoreEntity
    {
        public MoneyStoreEntity(string id, string name, OrganizationEntity organization)
        {
            Id = id;
            Name = name;
            Organization = organization;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public OrganizationEntity Organization { get; set; }
    }
}
