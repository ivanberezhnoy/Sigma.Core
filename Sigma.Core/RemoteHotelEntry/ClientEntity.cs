using HotelManager;
namespace Sigma.Core.RemoteHotelEntry
{
    public class Agreements : Dictionary<string, AgreementEntity> { }
    public class ClientEntity: Entity
    {
        public void FillClient(Client client, Agreements agreements, AgreementEntity? mainAgreement)
        {
            Name = client.Name;

            Agreements = agreements;
            MainAgreement = mainAgreement;
            IsDeleted = client.IsDeleted;
        }
        public ClientEntity(Client client, Agreements agreements, AgreementEntity? mainAgreement): base(client.Id, client.Name, client.IsDeleted)
        {
            Agreements = agreements;
            MainAgreement = mainAgreement;
        }

        public void RemoveAgreement(AgreementEntity agreement)
        {
            Agreements.Remove(agreement.Id);
        }
        public AgreementEntity? MainAgreement { get; set; }
        public Agreements Agreements { get; set; }
    }
}
