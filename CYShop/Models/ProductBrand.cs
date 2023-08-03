using System.ComponentModel;

namespace CYShop.Models
{
    public class ProductBrand
    {
        public uint ID { get; set; }

        [DisplayName("產品品牌")]
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
