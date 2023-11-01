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
        public DocumentsDictionary? Get()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetDocuments for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.Documents.GetDocuments(session.Client);
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return null;
        }
    }
}
