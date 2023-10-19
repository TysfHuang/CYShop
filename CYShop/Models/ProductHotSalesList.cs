using System.ComponentModel.DataAnnotations;

namespace CYShop.Models
{
    public class ProductHotSalesList
    {
        public uint Id { get; set; }
        public string Itemslist { get; set; }
        [DataType(DataType.Date)]
        public DateTime RecordDate { get; set; }
        public ProductHotSalesListPeriodType Period { get; set; }
    }

    public enum ProductHotSalesListPeriodType
    {
        Daily,
        Week,
        Month
    }
}
