using HotelManager;
using MySqlX.XDevAPI;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyTransferDocumentEntity: DocumentEntity
    {
        public float Money { get; set; }
        public MoneyStoreEntity MoneyStoreFrom { get; set; }
        public MoneyStoreEntity MoneyStoreTo { get; set; }

        public void Fill(OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user,
            bool isActive, MoneyStoreEntity moneyStoreFrom, MoneyStoreEntity moneyStoreTo, float money)
        {
            base.Fill(organization, date, comment, user, isActive);

            MoneyStoreFrom = moneyStoreFrom;
            MoneyStoreTo = moneyStoreTo;
            Money = money;
        }

        public MoneyTransferDocumentEntity(string id, OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user,
            bool isActive, MoneyStoreEntity moneyStoreFrom, MoneyStoreEntity moneyStoreTo, float money)
            : base(id, organization, date, comment, user, DocumentType.MoneyTransfer, isActive)
        {
            MoneyStoreFrom = moneyStoreFrom;
            MoneyStoreTo = moneyStoreTo;
            Money = money;
        }
    }
}
