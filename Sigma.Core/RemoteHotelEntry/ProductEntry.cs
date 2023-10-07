using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using Id = System.String;
using HotelManager;
using Sigma.Core.Controllers;
using System.Reflection.PortableExecutable;

namespace Sigma.Core.RemoteHotelEntry
{
    public class CharacteristicDictionary : Dictionary<string, CharacteristicEntry>
    { }

    public class UntitsDictionary : Dictionary<string, UnitEntity>
    { }


    public class ProductEntry
    {
        [Key]
        public Id Id { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Sku { get; set; }

        public string? EAN { get; set; }

        public float? Price { get; set; }

        public UntitsDictionary Units { get; set; }

        public UnitEntity? DefaultUnit { get; set; }

        public CharacteristicDictionary Characteristics { get; set; }

        public void Reload(Product product)
        {
            Name = product.Name;
            Sku = product.Sku;
            IsDeleted = product.IsDeleted;
            EAN = product.EAN;
            Price = product.Price;

            CharacteristicDictionary newCharacteristics = new CharacteristicDictionary();

            if (product.Characteristics != null)
            {
                foreach(var characteristic in product.Characteristics)
                {
                    CharacteristicEntry? newCharacteristic = null;
                    if (!Characteristics.TryGetValue(characteristic.Id, out newCharacteristic))
                    {
                        newCharacteristic = new CharacteristicEntry(characteristic);
                    }

                    newCharacteristic.Reload(characteristic);

                    newCharacteristics[newCharacteristic.Id] = newCharacteristic;
                }
            }

            Characteristics = newCharacteristics;

            UntitsDictionary newUnits = new UntitsDictionary();
            bool isFirstUnit = true;

            if (product.Units != null)
            {
                foreach(Unit unit in product.Units)
                {
                    UnitEntity? newUnit = null;

                    if (!Units.TryGetValue(unit.Id, out newUnit))
                    {
                        newUnit = new UnitEntity(unit);
                    }

                    newUnit.Reload(unit);

                    newUnits[newUnit.Id] = newUnit;

                    if (isFirstUnit)
                    {
                        DefaultUnit = newUnit;
                        isFirstUnit = false;
                    }
                }
            }

            Units = newUnits;
        }

        public ProductEntry(Product product)
        {
            Id = product.Id;
            Name = product.Name;

            Units = new UntitsDictionary();

            Characteristics = new CharacteristicDictionary();

            Reload(product);
        }

    }
}
