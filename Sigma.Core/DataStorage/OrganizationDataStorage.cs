using HotelManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.RemoteHotelEntry;
using System.Reflection.Metadata;

namespace Sigma.Core.DataStorage
{
    public class OrganizationDataStorage : BaseDataStorage
    {
        public class OrganizationsDicrionary : Dictionary<string, OrganizationEntity> { };
        public OrganizationsDicrionary? _organizations;

        public OrganizationDataStorage(ILogger<OrganizationDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Organizations = this;
        }

        private OrganizationEntity? fillOrganizations(HotelManagerPortTypeClient session, string? organizationID = null)
        {
            OrganizationEntity? result = null;

            if (_organizations != null)
            {
                OrganizationsList organizationsList = session.getOrganizationsList(organizationID);

                if (organizationsList.error != null && organizationsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load organizations list. Error : {Error}, organizations: {Organizations}, organizationID: {OrganizationID}", organizationsList.error, organizationsList, organizationID != null ? organizationID : "null");
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

        public OrganizationsDicrionary GetOrganizations(HotelManagerPortTypeClient session)
        {
            if (_organizations == null)
            {
                _organizations = new OrganizationsDicrionary();

                _logger.LogInformation("Reloading organizations list");

                fillOrganizations(session);
            }

            return _organizations;
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

                result = fillOrganizations(session, organizationID);
            }

            return result;
        }
    }
}
