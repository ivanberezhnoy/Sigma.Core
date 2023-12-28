using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using Id = System.String;
using HotelManager;
using Sigma.Core.Controllers;
using System.Reflection.PortableExecutable;

namespace Sigma.Core.RemoteHotelEntry
{
    public class CharacteristicDictionary : Dictionary<string, CharacteristicEntity>
    { }

    public class UnitsDictionary : Dictionary<string, UnitEntity>
    { }


    public class ProductEntity: Entity
    {

        public string? Sku { get; set; }

        public string? EAN { get; set; }

        public float? Price { get; set; }

        public UnitsDictionary Units { get; set; }

        public UnitEntity? DefaultUnit { get; set; }

        public CharacteristicDictionary Characteristics { get; set; }

        public void Reload(Product product)
        {
            Name = product.Name;
            Sku = product.Sku;
            IsDeleted = product.IsDeleted;
            EAN = product.EAN;
            Price = product.Price;
            IsDeleted = product.IsDeleted;

            CharacteristicDictionary newCharacteristics = new CharacteristicDictionary();

            if (product.Characteristics != null)
            {
                foreach(var characteristic in product.Characteristics)
                {
                    CharacteristicEntity? newCharacteristic = null;
                    if (!Characteristics.TryGetValue(characteristic.Id, out newCharacteristic))
                    {
                        newCharacteristic = new CharacteristicEntity(characteristic);
                    }

                    newCharacteristic.Reload(characteristic);

                    newCharacteristics[newCharacteristic.Id] = newCharacteristic;
                }
            }

            Characteristics = newCharacteristics;

            UnitsDictionary newUnits = new UnitsDictionary();
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

        public UnitEntity? GetUnitWithID(string unitID)
        {
            UnitEntity? result = null;

            if (!Units.TryGetValue(unitID, out result))
            {
            }

            return result;
        }

        public CharacteristicEntity? GetCharacteristicWithID(string characteristicID)
        {
            CharacteristicEntity? result = null;

            if (!Characteristics.TryGetValue(characteristicID, out result))
            {
            }

            return result;
        }

        public ProductEntity(Product product): base(product.Id, product.Name, product.IsDeleted)
        {
            Units = new UnitsDictionary();

            Characteristics = new CharacteristicDictionary();

            Reload(product);
        }

    }
}
