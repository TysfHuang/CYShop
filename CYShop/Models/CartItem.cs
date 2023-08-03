namespace CYShop.Models
{
    public class CartItem
    {
        public uint ProductID { get; set; }
        public string ProductName { get; set; }
        public uint Quantity { get; set; }
        public uint Price { get; set; }
    }
}
