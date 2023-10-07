using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class OrganizationEntity
    {
        public OrganizationEntity(Organization organization)
        {
            Id = organization.Id;
            Name = organization.Name;
        }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
