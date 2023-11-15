using Microsoft.AspNetCore.Mvc;
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
        public List<DocumentEntity> Get(EntityFilterDocument? filter, int? pageIndex, int? pageSize)
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetDocuments for connection {ConnectionID}", HttpContext.Connection.Id);
                List<DocumentEntity> documents = (List<DocumentEntity>)_storageProvider.Documents.GetSortedDocuments(session.Client, filter);

                if (pageIndex != null && pageSize != null)
                {
                    int startIndex = (int)(pageIndex * pageSize);

                    if (startIndex >= documents.Count)
                    {
                        documents = new DocumentsArray();
                    }
                    else 
                    {
                        documents = documents.GetRange(startIndex, Math.Min((int)pageSize, documents.Count - startIndex));
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
