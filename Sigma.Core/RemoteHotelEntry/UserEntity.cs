namespace Sigma.Core.RemoteHotelEntry
{
    using Sigma.Core.Utils;
    using System.Drawing;
    using System.ServiceModel.Security;
    public class UserEntity
    {
        public UserEntity(CredentionalInfo credentional, string userID)
        {
            Name = credentional.UserName;
            Password = credentional.Password;
            Id = userID;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string? Password { get; set; }

        public OrganizationEntity? DefaultOrganization { get; set; } = null;
        public StoreEntity? DefaultStore { get; set; } = null;

        public MoneyStoreEntity? DefaultMoneyStore { get; set; } = null;
    }
}
