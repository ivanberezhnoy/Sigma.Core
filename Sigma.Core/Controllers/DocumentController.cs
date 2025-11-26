using HotelManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Sigma.Core.Controllers.Query;
using Sigma.Core.DataStorage;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Net;
using System.Reflection.PortableExecutable;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DocumentController : BaseController
    {
        public DocumentController(ILogger<DocumentController> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
        }

        [HttpPost]
        [Authorize]
        public List<DocumentEntity>? documents(GetDocumentsQuery? query)
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return null;
            }

            _logger.LogInformation("GetDocuments for connection {ConnectionID}", HttpContext.Connection.Id);
            List<DocumentEntity> documents = _storageProvider.Documents.GetSortedDocuments(userClient.Client, query?.Filter);

            if (query?.PageIndex != null && query?.PageSize != null)
            {
                int startIndex = (int)(query.PageIndex * query.PageSize);

                if (startIndex >= documents.Count)
                {
                    documents = new List<DocumentEntity>();
                }
                else
                {
                    documents = documents.GetRange(startIndex, Math.Min((int)query.PageSize, documents.Count - startIndex));
                }
            }

            return documents;
        }

        [HttpPost]
        [Authorize]
        public RequestResult setDocuments(HotelManager.Document[] documents)
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return new RequestResult(ErrorCode.NotAuthorizied, "setDocuments: user not authorized"); ;
            }

            _logger.LogInformation("BingCharacteristic for connection {ConnectionID}", HttpContext.Connection.Id);
            return _storageProvider.Documents.setDocuments(userClient.Client, documents);
        }

        [HttpGet]
        [Authorize]
        public RequestResult updateDocument(string documentId)
        {
            _storageProvider.Documents.UpdateDocument(documentId);

            return new RequestResult(null, null, true);
        }
    }

}
