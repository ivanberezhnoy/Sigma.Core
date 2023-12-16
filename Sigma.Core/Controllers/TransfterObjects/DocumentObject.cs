namespace Sigma.Core.Controllers.TransfterObjects
{

    // Remove
    public class DocumentObject
    {
        public string Id { get; set; } = "";
        public DateTime CreationDate { get; set; }
        public string? StoreId { get; set; }
        public string? SourceMoneyStoreId { get; set; }
        public string? TargetMoneyStoreId { get; set; }

        public string? ParentDocumentId;
        public string? comment;
        public SaleProductObject[] ProductSales;
        public DocumentObject[] ChildDocuments;

        HotelManager.Document getHotelManagerDocument()
        {
            HotelManager.Document result = new HotelManager.Document();
            result.Id = Id;
            result.ParentDocumentId = ParentDocumentId;
            result.Comment = "";

            return result;
        }
    }
}
