using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class StoreEntity: Entity
    {
        public StoreEntity(Store store) : base(store.Id, store.Name, store.IsDeleted)
        {
        }
    }
}
