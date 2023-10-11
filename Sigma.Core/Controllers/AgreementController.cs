using HotelManager;
using Microsoft.AspNetCore.Mvc;
using Sigma.Core.RemoteHotelEntry;
using static Sigma.Core.Controllers.StoreController;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgreementController : Controller
    {
        public class AgreementsDicrionary : Dictionary<string, AgreementEntity> { };
        public AgreementsDicrionary? _agreements;

        private ILogger<AgreementController> _logger;
        private SOAP1CCleintProviderController _clientProvider;
        private IHttpContextAccessor _httpContextAccessor;

        public AgreementController(ILogger<AgreementController> logger, SOAP1CCleintProviderController clientProvider, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _clientProvider = clientProvider;
            _httpContextAccessor = httpContextAccessor;
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
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
