using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DataStorage;
using static Sigma.Core.DataStorage.OrganizationDataStorage;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrganizationController : Controller
    {
        private ILogger<OrganizationController> _logger;
        private StorageProvider _storageProvider;

        public OrganizationController(ILogger<OrganizationController> logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        [HttpGet(Name = "GetOrganizations")]
        public OrganizationsDicrionary? Get()
        {
            var session = _storageProvider.Sessions.GetClentForConnectionID(HttpContext.Connection.Id);

            if (session != null)
            {
                _logger.LogInformation("GetOrganizations for connection {ConnectionID}", HttpContext.Connection.Id);
                return _storageProvider.Organizations.GetOrganizations(session.Client);
            }

            _logger.LogWarning("Unable to find user with connection ID {ConnectionID}", HttpContext.Connection.Id);

            return null;
        }
    }
}
