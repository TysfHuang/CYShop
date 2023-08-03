namespace CYShop.Models
{
    public class ProductDTO
    {
        public uint ID { get; set; }
        public string Name { get; set; }
        public string CoverImagePath { get; set; }
        public uint Price { get; set; }

        public ProductDTO(Product product)
        {
            ID = product.ID;
            Name = product.Name;
            CoverImagePath = product.CoverImagePath;
            Price = product.Price;
        }
    }
}
