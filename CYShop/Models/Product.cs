using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CYShop.Models
{
    public class Product
    {
        public uint ID { get; set; }
        public uint ProductBrandID { get; set; }
        public uint ProductCategoryID { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("產品名稱")]
        public string Name { get; set; }

        [Required]
        [DisplayName("產品簡介")]
        public string Description { get; set; }

        [Required]
        [DisplayName("產品封面位址")]
        public string CoverImagePath { get; set; }

        [Required]
        [DisplayName("產品價格")]
        public uint Price { get; set; }
        
        public ProductBrand? ProductBrand { get; set; }
        public ProductCategory? ProductCategory { get; set; }
    }
}
