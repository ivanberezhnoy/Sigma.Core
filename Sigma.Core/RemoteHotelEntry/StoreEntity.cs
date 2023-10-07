using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class StoreEntity
    {
        public StoreEntity(Store store)
        {
            Id = store.Id;
            Name = store.Name;
        }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
