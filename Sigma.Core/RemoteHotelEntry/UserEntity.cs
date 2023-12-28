namespace Sigma.Core.RemoteHotelEntry
{
    using Sigma.Core.Utils;
    using System.Drawing;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    public class UserEntity: Entity
    {
        public UserEntity(string userName, string? userPassword, string userID): base(userID, userName, false)
        {
            Password = userPassword;
        }

        public string? Password { get; set; }

        public OrganizationEntity? DefaultOrganization { get; set; } = null;
        public StoreEntity? DefaultStore { get; set; } = null;

        public MoneyStoreEntity? DefaultMoneyStore { get; set; } = null;
        public MoneyStoreEntity? DefaultMoneyStoreCash { get; set; } = null;
        public MoneyStoreEntity? DefaultMoneyStoreTerminal { get; set; } = null;
        public MoneyStoreEntity? DefaultMoneyStoreTransfer { get; set; } = null;

        public ClientEntity? DefaultClient { get; set; } = null;

        public UserEntity MakeNakedCopy()
        {
            UserEntity copy = new UserEntity(Name, null, Id);
            return copy;
        }
    }
}
