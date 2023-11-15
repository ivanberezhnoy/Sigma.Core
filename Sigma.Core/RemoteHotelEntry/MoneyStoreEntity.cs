using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyStoreEntity: Entity
    {
        public MoneyStoreEntity(string id, string name, OrganizationEntity organization): base(id, name)
        {
            Organization = organization;
        }

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
