using HotelManager;
using System.Diagnostics;

namespace Sigma.Core.RemoteHotelEntry
{
    public enum DocumentEntityType
    {
        Sell,
        Buy,
        ReturnFromClient,
        ReturnToClient
    }

    public class DocumentsSet : HashSet<DocumentEntity>{}

    public class DocumentEntity
    {
        public static DocumentEntityType ConvertDocumentType(DocumentType type)
        {
            switch(type)
            {
                case HotelManager.DocumentType.Sell:
                        return DocumentEntityType.Sell;
                case HotelManager.DocumentType.Buy:
                    return DocumentEntityType.Sell;
                case HotelManager.DocumentType.ReturnToClient:
                    return DocumentEntityType.ReturnToClient;
                case HotelManager.DocumentType.ReturnFromClient:
                    return DocumentEntityType.ReturnFromClient;
                default:
                    return DocumentEntityType.Sell;
            }

        }
        public void Fill(OrganizationEntity organization, ClientEntity client, DateTime? date, string? comment, UserEntity? user, bool isActive, AgreementEntity agreement)
        {
            Organization = organization;
            Client = client;
            Date = date;
            Comment = comment;
            User = user;
            IsActive = isActive;
            Agreement = agreement;
        }

        public void SetParentDocument(DocumentEntity? parentDocument)
        {
            if (parentDocument != ParentDocument)
            {
                ParentDocument = parentDocument;
            }
        }

        public void SetChildDocuments(DocumentsSet newChildDocuments)
        {
            if (!newChildDocuments.Equals(ChildDocuments))
            {
                foreach(DocumentEntity oldChild in ChildDocuments)
                {
                    if (!newChildDocuments.Contains(oldChild))
                    {
                        if (oldChild.ParentDocument == this)
                        {
                            oldChild.SetParentDocument(null);
                        }
                    }
                }

                foreach (DocumentEntity newChild in newChildDocuments)
                {
                    if (newChild.ParentDocument != this)
                    {
                        newChild.SetParentDocument(this);
                    }
                }

                ChildDocuments = newChildDocuments;
            }
        }

        public DocumentEntity(string id, OrganizationEntity organization, ClientEntity client, DateTime? date, string? comment, UserEntity? user, DocumentEntityType documentType, bool isActive, AgreementEntity agreement)
        {
            Id = id;
            ChildDocuments = new DocumentsSet();
            DocumentType = documentType;

            Organization = organization;
            Client = client;
            Date = date;
            Comment = comment;
            User = user;
            IsActive = isActive;
            Agreement = agreement;

            //Fill(organization, client, date, comment, user, isActive);
        }

        public string Id { get; set; }
        public OrganizationEntity Organization { get; set; }
        public ClientEntity Client { get; set; }

        public DateTime? Date { get; set; }

        public string? Comment { get; set; }

        public UserEntity? User { get; set; }

        public DocumentEntityType DocumentType { get; set; }

        public bool IsActive { get; set; }

        public DocumentsSet ChildDocuments { get; set; }

        public DocumentEntity? ParentDocument { get; set; }

        public AgreementEntity Agreement { get; set; }
    }
}
