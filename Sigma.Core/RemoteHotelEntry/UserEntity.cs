namespace Sigma.Core.RemoteHotelEntry
{
    using Sigma.Core.Utils;
    using System.Drawing;
    using System.ServiceModel.Security;
    public class UserEntity
    {
        public UserEntity(string userName, string? userPassword, string userID)
        {
            Name = userName;
            Password = userPassword;
            Id = userID;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string? Password { get; set; }

        public OrganizationEntity? DefaultOrganization { get; set; } = null;
        public StoreEntity? DefaultStore { get; set; } = null;

        public MoneyStoreEntity? DefaultMoneyStore { get; set; } = null;

        public UserEntity MakeNakedCopy()
        {
            UserEntity copy = new UserEntity(Name, null, Id);
            return copy;
        }
    }
}
