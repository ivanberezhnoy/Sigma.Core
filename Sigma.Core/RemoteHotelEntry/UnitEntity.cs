using HotelManager;
using System.ComponentModel.DataAnnotations;

namespace Sigma.Core.RemoteHotelEntry
{
    public class UnitEntity: Entity
    {
        public void Reload(Unit unit)
        {
            Id = unit.Id;
            Name = unit.Name;
        }

        public UnitEntity(Unit unit): base(unit.Id, unit.Name, false)
        {
        }
    }
}
