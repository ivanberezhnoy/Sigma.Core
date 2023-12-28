using HotelManager;
using MySqlX.XDevAPI;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyTransferDocumentEntity: DocumentEntity
    {
        public MoneyStoreEntity MoneyStoreFrom { get; set; }
        public MoneyStoreEntity MoneyStoreTo { get; set; }

        public void Fill(OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user,
            bool isActive, bool isDeleted, MoneyStoreEntity moneyStoreFrom, MoneyStoreEntity moneyStoreTo)
        {
            base.Fill(organization, date, money, comment, user, isActive, isDeleted);

            MoneyStoreFrom = moneyStoreFrom;
            MoneyStoreTo = moneyStoreTo;
        }

        public MoneyTransferDocumentEntity(string id, OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user,
            bool isActive, bool isDeleted, MoneyStoreEntity moneyStoreFrom, MoneyStoreEntity moneyStoreTo)
            : base(id, organization, date, money, comment, user, DocumentType.MoneyTransfer, isActive, isDeleted)
        {
            MoneyStoreFrom = moneyStoreFrom;
            MoneyStoreTo = moneyStoreTo;
        }
    }
}
