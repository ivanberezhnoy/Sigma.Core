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
    }
}
