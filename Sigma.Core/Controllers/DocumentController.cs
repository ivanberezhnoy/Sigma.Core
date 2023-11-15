using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
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
        public DocumentsDictionary? Get(DateTime? minDate, DateTime? maxDate)
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetDocuments for connection {ConnectionID}", HttpContext.Connection.Id);
                DocumentsDictionary documents = _storageProvider.Documents.GetDocuments(session.Client);

                if (minDate == null && maxDate == null)
                {
                    return documents;
                }
                DocumentsDictionary result = new DocumentsDictionary();
                foreach (var document in documents)
                {
                    if (minDate != null && document.Value.Date < minDate)
                    {
                        continue;
                    }

                    if (maxDate != null && document.Value.Date > maxDate)
                    {
                        continue;
                    }

                    result[document.Key] = document.Value;
                }

                return result;
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return null;
        }
    }
}
