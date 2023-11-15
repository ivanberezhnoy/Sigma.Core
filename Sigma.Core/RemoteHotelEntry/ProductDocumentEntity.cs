using HotelManager;
using Sigma.Core.DataStorage;

namespace Sigma.Core.RemoteHotelEntry
{
    using ProductID = String;
    public class ProductsSales : Dictionary<string, ProductSaleEntity> { }

    public class ProductDocumentEntity : ClientDocumentEntity
    {
        public void Fill(OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user, bool isActive, ClientEntity client, AgreementEntity agreement, StoreEntity store, ProductsSales sales) 
        {
            base.Fill(organization, date, comment, user, isActive, client, agreement);

            Sales = sales;
        }

        public ProductDocumentEntity(string id, OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user,
            DocumentType documentType, bool isActive, ClientEntity client, AgreementEntity agreement, StoreEntity store, ProductsSales sales) 
            : base(id, organization, date, comment, user, documentType, isActive, client, agreement)
        {
            Sales = sales;
            Store = store;
        }

        public static bool IsClientDocument(DocumentType documentType)
        { 
            return IsProductDocument(documentType) || IsMoneyStoreDocument(documentType);
        }

        public static bool IsProductDocument(DocumentType documentType)
        {
            return documentType == HotelManager.DocumentType.Sell || documentType == HotelManager.DocumentType.Buy ||
                documentType == HotelManager.DocumentType.ReturnToClient || documentType == HotelManager.DocumentType.ReturnFromClient;
        }

        public static bool IsMoneyStoreDocument(DocumentType documentType)
        {
            return documentType == HotelManager.DocumentType.MoneyPut || documentType == HotelManager.DocumentType.MoneyGet || 
                documentType == HotelManager.DocumentType.MoneyReturnToClient || documentType == HotelManager.DocumentType.MoneyReturnFromClient;
        }

        public override bool filterMatch(EntityFilterDocument filter)
        {
            if (base.filterMatch(filter))
            {
                return true;
            }

            foreach (var sale in Sales.Values)
            {
                if (sale.filterMatch(filter))
                {
                    return true;
                }
            }

            return false;
        }

        public ProductsSales Sales { get; set; }

        public StoreEntity Store { get; set; }
    }
}
