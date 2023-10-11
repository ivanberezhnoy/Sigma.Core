using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyStoreDocumentEntity : DocumentEntity
    {
        public float Money { get; set; }
        public MoneyStoreEntity MoneyStore { get; set; }

        public void Fill(OrganizationEntity organization, ClientEntity client, DateTime? date, string? comment, UserEntity? user, bool isActive, AgreementEntity agreement, 
            MoneyStoreEntity moneystore, float money)
        {
            base.Fill(organization, client, date, comment, user, isActive, agreement);

            MoneyStore = moneystore;
            Money = money;
        }

        public MoneyStoreDocumentEntity(string id, OrganizationEntity organization, ClientEntity client, DateTime? date, string? comment, UserEntity? user, DocumentEntityType documentType,
            bool isActive, AgreementEntity agreement, MoneyStoreEntity moneyStore, float money)
            : base(id, organization, client, date, comment, user, documentType, isActive, agreement)
        {
            MoneyStore = moneyStore;
            Money = money;
        }
    }
}
