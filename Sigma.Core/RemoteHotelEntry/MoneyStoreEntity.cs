using HotelManager;

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

        private float? moneyValue { get; set; }
        private DateTime? moneyValueTimeStamp { get; set; }
        public void InvalidateMoneyValue()
        { 
        }

        public float? GetMoneyValue(HotelManagerPortTypeClient client)
        {
            if (moneyValue == null || moneyValueTimeStamp == null || (DateTime.Now - moneyValueTimeStamp).Value.TotalSeconds > 60)
            {
                moneyValue = client.getMoneyValue(Id);
                moneyValueTimeStamp = DateTime.Now;
            }

            return moneyValue;
        }
    }
}
