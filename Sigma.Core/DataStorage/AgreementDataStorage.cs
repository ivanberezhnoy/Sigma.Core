using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.RemoteHotelEntry;

namespace Sigma.Core.DataStorage
{
    public class AgreementDataStorage : BaseDataStorage
    {
        public class AgreementsDicrionary : Dictionary<string, AgreementEntity> { };
        public AgreementsDicrionary? _agreements;

        public AgreementDataStorage(ILogger<AgreementDataStorage> logger, StorageProvider storageProvider) : base(logger, storageProvider)
        {
            _storageProvider.Agreements = this;
        }

        private AgreementEntity? fillAgreements(HotelManagerPortTypeClient session, string? agreementID = null)
        {
            AgreementEntity? result = null;

            if (_agreements != null)
            {
                AgreementsList agreements = session.getAgrementsList(agreementID);

                if (agreements.error != null && agreements.error.Length > 0)
                {
                    _logger.LogError("Failed to load agreements list. Error : {Error}, stores: {Agreements}", agreements.error, agreements);
                }

                foreach (var agreement in agreements.data)
                {
                    AgreementEntity newAgreement = new AgreementEntity(agreement);
                    _agreements[newAgreement.Id] = newAgreement;

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
            if (_agreements == null)
            {
                _agreements = new AgreementsDicrionary();

                fillAgreements(session);
            }
            return _agreements;
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

                result = fillAgreements(session, agreementID);
            }

            return result;
        }
    }
}
