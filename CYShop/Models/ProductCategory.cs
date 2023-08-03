using System.ComponentModel;

namespace CYShop.Models
{
    public class ProductCategory
    {
        public uint ID { get; set; }

        [DisplayName("產品類型")]
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
