﻿using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    using ProductID = String;
    public class ProductsSales : List<ProductSaleEntity> { }

    public class ProductDocumentEntity : DocumentEntity
    {
        public void Fill(OrganizationEntity organization, ClientEntity client, DateTime? date, string? comment, UserEntity? user, bool isActive, AgreementEntity agreement, StoreEntity store, ProductsSales products) 
        {
            base.Fill(organization, client, date, comment, user, isActive, agreement);

            Products = products;
        }
        public ProductDocumentEntity(string id, OrganizationEntity organization, ClientEntity client, DateTime? date, string? comment, UserEntity? user,
            DocumentEntityType documentType, bool isActive, AgreementEntity agreement, StoreEntity store, ProductsSales products) 
            : base(id, organization, client, date, comment, user, documentType, isActive, agreement)
        {
            Products = products;
            Store = store;
        }
        
        public static bool IsProductDocument(DocumentType documentType)
        {
            return documentType == HotelManager.DocumentType.Sell || documentType == HotelManager.DocumentType.Buy ||
                documentType == HotelManager.DocumentType.ReturnToClient || documentType == HotelManager.DocumentType.ReturnFromClient;
        }

        public static bool IsMoneyDocument(DocumentType documentType)
        {
            return documentType == HotelManager.DocumentType.MoneyPut || documentType == HotelManager.DocumentType.MoneyGet || 
                documentType == HotelManager.DocumentType.MoneyReturnToClient || documentType == HotelManager.DocumentType.MoneyReturnFromClient;
        }

        public ProductsSales Products { get; set; }

        public StoreEntity Store { get; set; }
    }
}
