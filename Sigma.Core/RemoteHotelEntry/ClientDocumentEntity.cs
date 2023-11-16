using HotelManager;
using Sigma.Core.DataStorage;

namespace Sigma.Core.RemoteHotelEntry
{
    public class ClientDocumentEntity: DocumentEntity
    {
        public AgreementEntity Agreement { get; set; }

        public ClientEntity Client { get; set; }

        public void Fill(OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, bool isActive, ClientEntity client, AgreementEntity agreement)
        {
            base.Fill(organization, date, money, comment, user, isActive);

            Client = Client;
            Agreement = agreement;
        }

        public ClientDocumentEntity(string id, OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, DocumentType documentType, bool isActive, ClientEntity client, AgreementEntity agreement)
            : base(id, organization, date, money, comment, user, documentType, isActive)
        {
            Children = new DocumentsDictionary();
            Type = documentType;

            Organization = organization;
            Client = client;
            Date = date;
            Comment = comment;
            User = user;
            IsActive = isActive;
            Agreement = agreement;

            //Fill(organization, client, date, comment, user, isActive);
        }
    }
}
