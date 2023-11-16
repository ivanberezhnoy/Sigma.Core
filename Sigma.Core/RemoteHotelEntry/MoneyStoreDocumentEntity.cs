using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyStoreDocumentEntity : ClientDocumentEntity
    {
        public MoneyStoreEntity MoneyStore { get; set; }

        public void Fill(OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, bool isActive, ClientEntity client, AgreementEntity agreement, 
            MoneyStoreEntity moneystore)
        {
            base.Fill(organization, date, money, comment, user, isActive, client, agreement);

            MoneyStore = moneystore;
        }

        public MoneyStoreDocumentEntity(string id, OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, DocumentType documentType,
            bool isActive, ClientEntity client, AgreementEntity agreement, MoneyStoreEntity moneyStore)
            : base(id, organization, date, money, comment, user, documentType, isActive, client, agreement)
        {
            MoneyStore = moneyStore;
        }
    }
}
