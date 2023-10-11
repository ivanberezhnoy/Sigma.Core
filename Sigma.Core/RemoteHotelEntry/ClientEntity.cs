using HotelManager;
namespace Sigma.Core.RemoteHotelEntry
{
    public class Agreements : Dictionary<string, AgreementEntity> { }
    public class ClientEntity
    {
        public void FillClient(Client client, Agreements agreements, AgreementEntity? mainAgreement)
        {
            Name = client.Name;

            Agreements = agreements;
            MainAgreement = mainAgreement;
        }
        public ClientEntity(Client client, Agreements agreements, AgreementEntity? mainAgreement)
        {
            Id = client.Id;
            Name = client.Name;

            Agreements = agreements;
            MainAgreement = mainAgreement;
        }

        public void RemoveAgreement(AgreementEntity agreement)
        {
            Agreements.Remove(agreement.Id);
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public AgreementEntity? MainAgreement { get; set; }
        public Agreements Agreements { get; set; }
    }
}
