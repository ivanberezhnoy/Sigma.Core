using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class AgreementEntity: Entity
    {
        public AgreementEntity(Agreement agreement): base(agreement.Id, agreement.Name, agreement.IsDeleted)
        {
            Id = agreement.Id;
            Name = agreement.Name;
            Type = agreement.Type;
            ClientID = agreement.ClientId;
        }

        public void FillAgreement(string name, DocumentType type)
        {
            Name = name;
            Type = type;
        }

        public DocumentType Type { get; set; }

        public string ClientID { get; set; }
    }
}
