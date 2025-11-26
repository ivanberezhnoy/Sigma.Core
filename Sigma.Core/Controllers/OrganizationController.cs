using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using Sigma.Core.Utils;
using System.Net;
using static Sigma.Core.DataStorage.OrganizationDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrganizationController : BaseController
    {
        public OrganizationController(ILogger<OrganizationController> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
        }

        [HttpGet(Name = "GetOrganizations")]
        [Authorize]
        public OrganizationsDicrionary? Get()
        {
            UserClient? userClient = GetClient();

            if (userClient?.Client == null)
            {
                return null;
            }

            _logger.LogInformation("GetOrganizations for connection {ConnectionID}", HttpContext.Connection.Id);
            return _storageProvider.Organizations.GetOrganizations(userClient.Client);
        }
    }
}
