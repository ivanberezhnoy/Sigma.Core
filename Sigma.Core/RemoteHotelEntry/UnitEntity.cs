using HotelManager;
using System.ComponentModel.DataAnnotations;

namespace Sigma.Core.RemoteHotelEntry
{
    public class UnitEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public void Reload(Unit unit)
        {
            Id = unit.Id;
            Name = unit.Name;
        }

        public UnitEntity(Unit unit)
        {
            Id = unit.Id;
            Name = unit.Name;
        }
    }
}
