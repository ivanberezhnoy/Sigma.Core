using Microsoft.AspNetCore.Mvc;
using Sigma.Core.Controllers.Query;
using Sigma.Core.DataStorage;
using Sigma.Core.RemoteHotelEntry;
using System.Net;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : Controller
    {
        private ILogger<DocumentController> _logger;
        private StorageProvider _storageProvider;

        public DocumentController(ILogger<DocumentController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet(Name = "GetDocuments")]
        public List<DocumentEntity> Get(GetDocumentsQuery? query)
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetDocuments for connection {ConnectionID}", HttpContext.Connection.Id);
                List<DocumentEntity> documents = _storageProvider.Documents.GetSortedDocuments(session.Client, query?.Filter);

                if (query?.PageIndex != null && query?.PageSize != null)
                {
                    int startIndex = (int)(query.PageIndex * query.PageSize);

                    if (startIndex >= documents.Count)
                    {
                        documents = new DocumentsArray();
                    }
                    else 
                    {
                        documents = documents.GetRange(startIndex, Math.Min((int)query.PageSize, documents.Count - startIndex));
                    }
                }

                return documents;
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return null;
        }
    }
}
