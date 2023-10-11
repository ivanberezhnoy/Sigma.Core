using HotelManager;

namespace Sigma.Core.RemoteHotelEntry
{
    public class ProductSaleEntity
    {
        public ProductSaleEntity(uint index, ProductEntity product, float quantity, float price, UnitEntity unit, CharacteristicEntity? characteristic)
        {
            Index = index;
            Product = product;
            Quantity = quantity;
            Price = price;
            Unit = unit;
            Characteristic = characteristic;
        }

        public uint Index;
        public ProductEntity Product { get; set; }

        public float Quantity { get; set; }

        public float Price { get; set; }

        public UnitEntity Unit { get; set; }

        public CharacteristicEntity? Characteristic { get; set; }
    }
}
