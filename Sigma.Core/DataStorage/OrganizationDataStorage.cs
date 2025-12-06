using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;
using System.Reflection.Metadata;

namespace Sigma.Core.DataStorage
{
    public class OrganizationDataStorage : BaseDataStorage
    {
        public class OrganizationsDicrionary : Dictionary<string, OrganizationEntity> { };
        private readonly Dictionary<EndpointType, OrganizationsDicrionary> _organizationsByEndpoint;

        public OrganizationDataStorage(ILogger<OrganizationDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Organizations = this;
            _organizationsByEndpoint = new Dictionary<EndpointType, OrganizationsDicrionary>();
        }

        private OrganizationEntity? fillOrganizations(HotelManagerPortTypeClient session, OrganizationsDicrionary organizations, string? organizationID = null)
        {
            OrganizationEntity? result = null;

            if (organizations != null)
            {
                OrganizationsList organizationsList = session.getOrganizationsList(organizationID);

                if (organizationsList.error != null && organizationsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load organizations list. Error : {Error}, organizations: {Organizations}, organizationID: {OrganizationID}", organizationsList.error, organizationsList, organizationID != null ? organizationID : "null");
                }

                foreach (var organization in organizationsList.data)
                {
                    OrganizationEntity newOrganization = new OrganizationEntity(organization);
                    organizations[newOrganization.Id] = newOrganization;

                    result = newOrganization;
                }
            }

            return result;
        }

        public OrganizationsDicrionary GetOrganizations(HotelManagerPortTypeClient session)
        {
            var endpoint = GetEndpoint(session);
            if (!_organizationsByEndpoint.TryGetValue(endpoint, out var organizations))
            {
                organizations = new OrganizationsDicrionary();
                _organizationsByEndpoint[endpoint] = organizations;

                _logger.LogInformation("Reloading organizations list");

                fillOrganizations(session, organizations);
            }

            return organizations;
        }

        public OrganizationEntity? GetOrganization(HotelManagerPortTypeClient session, string? organizationID)
        {
            OrganizationEntity? result = null;

            if (organizationID == null || organizationID.Length == 0)
            {
                return result;
            }

            OrganizationsDicrionary organizations = GetOrganizations(session);

            if (!organizations.TryGetValue(organizationID, out result))
            {
                _logger.LogInformation("Loading organization with ID {OrganizationID}", organizationID);

                result = fillOrganizations(session, organizations, organizationID);
            }

            return result;
        }
    }
}
