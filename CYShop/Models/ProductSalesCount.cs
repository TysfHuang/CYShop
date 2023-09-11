using System.ComponentModel.DataAnnotations;

namespace CYShop.Models
{
    public class ProductSalesCount
    {
        public uint Id { get; set; }
        public uint Count { get; set; }
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        public uint ProductID { get; set; }
        public Product? Product { get; set; }
    }
}
