using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class OrganizationEntity: Entity
    {
        public OrganizationEntity(Organization organization): base(organization.Id, organization.Name, organization.IsDeleted)
        {
        }
    }
}
