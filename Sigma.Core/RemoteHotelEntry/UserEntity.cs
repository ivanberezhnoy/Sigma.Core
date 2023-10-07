namespace Sigma.Core.RemoteHotelEntry
{
    using Sigma.Core.Utils;
    using System.Drawing;
    using System.ServiceModel.Security;
    public class UserEntity
    {
        public UserEntity(CredentionalInfo credentional)
        {
            Credentional = credentional;
        }

        public CredentionalInfo Credentional { get; set; }

        public string? UserId { get; set; }

        public OrganizationEntity? DefaultOrganization { get; set; } = null;
        public StoreEntity? DefaultStore { get; set; } = null;

        public MoneyStoreEntity? DefaultMoneyStore { get; set; } = null;
    }
}
