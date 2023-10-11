using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class AgreementEntity
    {
        public AgreementEntity(Agreement agreement)
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

        public string Id { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }

        public string ClientID { get; set; }
    }
}
