using HotelManager;
using Sigma.Core.DataStorage;
using System.Numerics;

namespace Sigma.Core.RemoteHotelEntry
{
    using ProductID = String;
    public class ProductsSales : Dictionary<string, ProductSaleEntity> { }

    public class ProductDocumentEntity : ClientDocumentEntity
    {
        public void Fill(OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, bool isActive, ClientEntity client, AgreementEntity agreement, StoreEntity store, ProductsSales sales, string easyMSBookingId) 
        {
            base.Fill(organization, date, money, comment, user, isActive, client, agreement, easyMSBookingId);

            Sales = sales;
            Store = store;
        }

        public ProductDocumentEntity(string id, OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user,
            DocumentType documentType, bool isActive, ClientEntity client, AgreementEntity agreement, StoreEntity store, ProductsSales sales, string easyMSBookingId) 
            : base(id, organization, date, money, comment, user, documentType, isActive, client, agreement, easyMSBookingId)
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

        public override bool filterMatch(EntityFilterDocument filter, out bool completeMismatch)
        {
            if (base.filterMatch(filter, out completeMismatch))
            {
                return true;
            }
            else if (completeMismatch)
            {
                return false;
            }

            foreach (var sale in Sales.Values)
            {
                if (sale.filterMatch(filter, out completeMismatch))
                {
                    return true;
                }
                else if (completeMismatch)
                {
                    return false;
                }
            }

            return false;
        }

        public ProductsSales Sales { get; set; }

        public StoreEntity Store { get; set; }
    }
}
