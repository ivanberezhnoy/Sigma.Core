using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyStoreDocumentEntity : ClientDocumentEntity
    {
        public float Money { get; set; }
        public MoneyStoreEntity MoneyStore { get; set; }

        public void Fill(OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user, bool isActive, ClientEntity client, AgreementEntity agreement, 
            MoneyStoreEntity moneystore, float money)
        {
            base.Fill(organization, date, comment, user, isActive, client, agreement);

            MoneyStore = moneystore;
            Money = money;
        }

        public MoneyStoreDocumentEntity(string id, OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user, DocumentType documentType,
            bool isActive, ClientEntity client, AgreementEntity agreement, MoneyStoreEntity moneyStore, float money)
            : base(id, organization, date, comment, user, documentType, isActive, client, agreement)
        {
            MoneyStore = moneyStore;
            Money = money;
        }
    }
}
