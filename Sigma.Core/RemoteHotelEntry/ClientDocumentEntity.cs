using HotelManager;
using Sigma.Core.DataStorage;

namespace Sigma.Core.RemoteHotelEntry
{
    public class ClientDocumentEntity: DocumentEntity
    {
        public AgreementEntity Agreement { get; set; }

        public ClientEntity Client { get; set; }
        public string? EasyMSBookingId { get; set; }

        public void Fill(OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, 
            bool isActive, ClientEntity client, AgreementEntity agreement, string easyMSBookingId)
        {
            Fill(organization, date, money, comment, user, isActive);

            Client = client;
            Agreement = agreement;
            EasyMSBookingId = easyMSBookingId;
        }

        public ClientDocumentEntity(string id, OrganizationEntity organization, DateTime? date, float money, string? comment, 
            UserEntity? user, DocumentType documentType, bool isActive, ClientEntity client, AgreementEntity agreement, string easyMSBookingId)
            : base(id, organization, date, money, comment, user, documentType, isActive)
        {
            Fill(organization, date, money, comment, user, isActive, client, agreement, easyMSBookingId);
        }
    }
}
