using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sigma.Core.RemoteHotelEntry;
using Sigma.Core.Utils;

namespace Sigma.Core.DataStorage
{
    public class AgreementDataStorage : BaseDataStorage
    {
        public class AgreementsDicrionary : Dictionary<string, AgreementEntity> { };
        private readonly Dictionary<EndpointType, AgreementsDicrionary> _agreementsByEndpoint;

        public AgreementDataStorage(ILogger<AgreementDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Agreements = this;
            _agreementsByEndpoint = new Dictionary<EndpointType, AgreementsDicrionary>();
        }

        private AgreementEntity? fillAgreements(HotelManagerPortTypeClient session, AgreementsDicrionary agreements, string? agreementID = null)
        {
            AgreementEntity? result = null;

            if (agreements != null)
            {
                AgreementsList agreementsList = session.getAgrementsList(agreementID);

                if (agreementsList.error != null && agreementsList.error.Length > 0)
                {
                    _logger.LogError("Failed to load agreements list. Error : {Error}, stores: {Agreements}", agreementsList.error, agreementsList);
                }

                foreach (var agreement in agreementsList.data)
                {
                    AgreementEntity newAgreement = new AgreementEntity(agreement);
                    agreements[newAgreement.Id] = newAgreement;

                    if (result == null)
                    {
                        result = newAgreement;
                    }
                }
            }

            return result;
        }

        private AgreementsDicrionary getAgreements(HotelManagerPortTypeClient session)
        {
            var endpoint = GetEndpoint(session);
            if (!_agreementsByEndpoint.TryGetValue(endpoint, out var agreements))
            {
                agreements = new AgreementsDicrionary();
                _agreementsByEndpoint[endpoint] = agreements;

                fillAgreements(session, agreements);
            }

            return agreements;
        }

        public AgreementEntity? GetAgreement(HotelManagerPortTypeClient session, string? agreementID)
        {
            AgreementEntity? result = null;

            if (agreementID == null || agreementID.Length == 0)
            {
                return result;
            }

            AgreementsDicrionary agreements = getAgreements(session);

            if (!agreements.TryGetValue(agreementID, out result))
            {
                _logger.LogInformation("Loading agreement with ID {AgreementID}", agreementID);

                result = fillAgreements(session, agreements, agreementID);
            }

            return result;
        }
    }
}
