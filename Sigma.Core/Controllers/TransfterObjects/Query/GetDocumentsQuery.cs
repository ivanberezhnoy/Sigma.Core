using Sigma.Core.DataStorage;

namespace Sigma.Core.Controllers.Query
{
    public class GetDocumentsQuery
    {
        public EntityFilterDocument? Filter {get; set;}
        public int? PageIndex {get; set;}
        public int? PageSize {get; set;}
    }
}
