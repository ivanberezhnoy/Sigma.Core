using HotelManager;
using System.ComponentModel.DataAnnotations;
using Id = System.String;

namespace Sigma.Core.RemoteHotelEntry
{
    public class CharacteristicEntity
    {
        [Key]
        public Id Id { get; set; }

        public string Name { get; set; }

        public float? Price { get; set; }

        public string? EAN { get; set; }

        public void Reload(Characteristic characteristic)
        {
            Price = characteristic.Price;
            EAN = characteristic.EAN;
            Name = characteristic.Name;
        }

        public CharacteristicEntity(Characteristic characteristic)
        {
            Id = characteristic.Id;
            Name = characteristic.Name;

            Reload(characteristic);
        }

    }
}
