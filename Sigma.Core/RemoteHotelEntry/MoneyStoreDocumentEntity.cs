using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class MoneyStoreDocumentEntity : ClientDocumentEntity
    {
        public MoneyStoreEntity MoneyStore { get; set; }

        public void Fill(OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, bool isActive, bool isDeleted, ClientEntity client, AgreementEntity agreement, 
            MoneyStoreEntity moneystore, string easyMSBookingId)
        {
            base.Fill(organization, date, money, comment, user, isActive, isDeleted, client, agreement, easyMSBookingId);

            MoneyStore = moneystore;
        }

        public MoneyStoreDocumentEntity(string id, OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, DocumentType documentType,
            bool isActive, bool isDeleted, ClientEntity client, AgreementEntity agreement, MoneyStoreEntity moneyStore, string easyMSBookingId)
            : base(id, organization, date, money, comment, user, documentType, isActive, isDeleted, client, agreement, easyMSBookingId)
        {
            MoneyStore = moneyStore;
        }
    }
}
