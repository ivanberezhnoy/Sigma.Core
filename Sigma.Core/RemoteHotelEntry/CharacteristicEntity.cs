using HotelManager;
using System.ComponentModel.DataAnnotations;
using Id = System.String;

namespace Sigma.Core.RemoteHotelEntry
{
    public class CharacteristicEntity: Entity
    {
        static Dictionary<String, String> characteristicToEasyMS = new Dictionary<String, String>()
        { };
        public float? Price { get; set; }

        public string? EAN { get; set; }

        public String? easyMSRoomID { get; set; }

        public void Reload(Characteristic characteristic)
        {
            Price = characteristic.Price;
            EAN = characteristic.EAN;
            Name = characteristic.Name;
            easyMSRoomID = characteristic.EasyMSRoomId;
        }

        public CharacteristicEntity(Characteristic characteristic): base(characteristic.Id, characteristic.Name)
        {
            Reload(characteristic);
        }

    }
}
