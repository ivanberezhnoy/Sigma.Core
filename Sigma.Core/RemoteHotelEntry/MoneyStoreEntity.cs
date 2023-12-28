using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyStoreEntity: Entity
    {
        public MoneyStoreEntity(string id, string name, OrganizationEntity organization, bool isDeleted): base(id, name, isDeleted)
        {
            Organization = organization;
        }

        public OrganizationEntity Organization { get; set; }
    }
}
