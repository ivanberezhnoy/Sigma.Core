using HotelManager;
using System.ComponentModel.DataAnnotations;
using Id = System.String;

namespace Sigma.Core.RemoteHotelEntry
{
    public class CharacteristicEntity: Entity
    {

        public float? Price { get; set; }

        public string? EAN { get; set; }

        public void Reload(Characteristic characteristic)
        {
            Price = characteristic.Price;
            EAN = characteristic.EAN;
            Name = characteristic.Name;
        }

        public CharacteristicEntity(Characteristic characteristic): base(characteristic.Id, characteristic.Name)
        {
            Reload(characteristic);
        }

    }
}
