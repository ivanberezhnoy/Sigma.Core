using HotelManager;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace Sigma.Core.DataStorage
{
    public class EntityFilterDocument: EntityFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DocumentType? DocumentType { get; set; }

        public string? CreatorID { get; set; }

        public EntityFilterDocument() : base()
        { 

        }

        public override bool IsEmpty()
        { 
            return StartDate == null && EndDate == null && DocumentType == null && CreatorID == null && base.IsEmpty();
        }

        public EntityFilterDocument(DateTime? startDate, DateTime? endDate, DocumentType? documentType, string? stringFilter, string? creatorID): base(stringFilter)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.DocumentType = documentType;
            this.CreatorID = creatorID;
        }
    }

    class EntityFilterDocumentComparer : IEqualityComparer<EntityFilterDocument>
    {
        public bool Equals(EntityFilterDocument? documentFilter1, EntityFilterDocument? documentFilter2)
        {
            if (documentFilter1 == null && documentFilter2 == null)
                return true;
            else if (documentFilter1 == null || documentFilter2 == null)
                return false;

            return Equals(documentFilter1.StartDate, documentFilter2.StartDate) &&
                Equals(documentFilter1.EndDate, documentFilter2.EndDate) &&
                documentFilter1.DocumentType == documentFilter2.DocumentType &&
                Equals(documentFilter1.StringFilter, documentFilter2.StringFilter) &&
                Equals(documentFilter1.CreatorID, documentFilter2.CreatorID);
        }

        public int GetHashCode(EntityFilterDocument documentFilter)
        {
            return HashCode.Combine(documentFilter.StartDate, documentFilter.EndDate, documentFilter.DocumentType, documentFilter.StringFilter, documentFilter.CreatorID);
        }
    }
}
