using HotelManager;
using System.Diagnostics;

namespace Sigma.Core.RemoteHotelEntry
{
    public enum DocumentEntityType
    {
        Sell,
        Buy,
        ReturnFromClient,
        ReturnToClient,
        MoneyGet,
        MoneyPut,
        MoneyReturnToClient,
        MoneyReturnFromClient
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

                case HotelManager.DocumentType.MoneyGet:
                    return DocumentEntityType.MoneyGet;
                case HotelManager.DocumentType.MoneyPut:
                    return DocumentEntityType.MoneyPut;
                case HotelManager.DocumentType.MoneyReturnToClient:
                    return DocumentEntityType.MoneyReturnToClient;
                case HotelManager.DocumentType.MoneyReturnFromClient:
                    return DocumentEntityType.MoneyReturnFromClient;

                default:
                    return DocumentEntityType.Sell;
            }

        }

        public static string DocumentTypeToString(DocumentEntityType documentType)
        {
            switch(documentType)
            {
                case DocumentEntityType.Sell:
                    return "Реализация товаров и услуг";
                case DocumentEntityType.Buy:
                    return "Поступление товаров и услуг";
                case DocumentEntityType.ReturnFromClient:
                    return "Возврат товаров от покупателя";
                case DocumentEntityType.ReturnToClient:
                    return "Поступление товаров и услуг";
                case DocumentEntityType.MoneyGet:
                    return "Приходный кассовый ордер";
                case DocumentEntityType.MoneyPut:
                    return "Расходный кассовый ордер";
                case DocumentEntityType.MoneyReturnToClient:
                    return "Возврат денег покупателю";
                case DocumentEntityType.MoneyReturnFromClient:
                    return "Возврат денег от продавца";

            }

            return "Unknown";
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

        public void SetParentDocumentID(string? parentDocumentID)
        {
            if (parentDocumentID != ParentID)
            {
                ParentID = parentDocumentID;
            }
        }

        public void SetChildDocuments(DocumentsSet newChildDocuments)
        {
            if (!newChildDocuments.Equals(Children))
            {
                foreach(DocumentEntity oldChild in Children)
                {
                    if (!newChildDocuments.Contains(oldChild))
                    {
                        if (oldChild.ParentID == this.Id)
                        {
                            oldChild.SetParentDocumentID(null);
                        }
                    }
                }

                foreach (DocumentEntity newChild in newChildDocuments)
                {
                    if (newChild.ParentID != this.Id)
                    {
                        newChild.SetParentDocumentID(this.Id);
                    }
                }

                Children = newChildDocuments;
            }
        }

        public DocumentEntity(string id, OrganizationEntity organization, ClientEntity client, DateTime? date, string? comment, UserEntity? user, DocumentEntityType documentType, bool isActive, AgreementEntity agreement)
        {
            Id = id;
            Children = new DocumentsSet();
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

        public string Id { get; set; }
        public string Name 
        {
            get
            { 
                return DocumentTypeToString(Type) + " " + Id + " " + Date.ToString();
            }
        }
        public OrganizationEntity Organization { get; set; }
        public ClientEntity Client { get; set; }

        public DateTime? Date { get; set; }

        public string? Comment { get; set; }

        public UserEntity? User { get; set; }

        public DocumentEntityType Type { get; set; }

        public bool IsActive { get; set; }

        public DocumentsSet Children { get; set; }

        public String? ParentID { get; set; }

        public AgreementEntity Agreement { get; set; }
    }
}
