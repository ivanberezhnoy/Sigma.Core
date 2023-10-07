using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.RemoteHotelEntry;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrganizationController : Controller
    {
        public class OrganizationsDicrionary : Dictionary<string, OrganizationEntity> { };
        public OrganizationsDicrionary? _organizations;

        private ILogger<OrganizationController> _logger;
        private SOAP1CCleintProviderController _clientProvider;
        IHttpContextAccessor _httpContextAccessor;

        public OrganizationController(ILogger<OrganizationController> logger, SOAP1CCleintProviderController clientProvider, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _clientProvider = clientProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        private OrganizationEntity? fillOrganizations(HotelManagerPortTypeClient session, string? organizationID = null)
        {
            OrganizationEntity? result = null;

            if (_organizations != null)
            {
                OrganizationsList organizationsList = session.getOrganizationsList(null);

                if (organizationsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load organizations list. Error : {Error}, organizations: {Organizations}", organizationsList.error, organizationsList);
                }

                foreach (var organization in organizationsList.data)
                {
                    OrganizationEntity newOrganization = new OrganizationEntity(organization);
                    _organizations[newOrganization.Id] = newOrganization;

                    result = newOrganization;
                }
            }

            return result;
        }

        private OrganizationsDicrionary getOrganizations(HotelManagerPortTypeClient session)
        {
            if (_organizations == null)
            {
                _organizations = new OrganizationsDicrionary();

                _logger.LogInformation("Reloading organizations list");

                fillOrganizations(session);
            }

            return _organizations;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public OrganizationEntity? GetOrganization(HotelManagerPortTypeClient session, string? organizationID)
        {
            OrganizationEntity? result = null;

            if (organizationID  == null || organizationID.Length == 0)
            {
                return result;
            }

            OrganizationsDicrionary organizations = getOrganizations(session);

            if (!organizations.TryGetValue(organizationID, out result))
            {
                _logger.LogInformation("Loading organization with ID {OrganizationID}", organizationID);

                result = fillOrganizations(session, organizationID);
            }

            return result;
        }
    }
}
