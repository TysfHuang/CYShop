using Microsoft.CodeAnalysis;
using NuGet.Packaging;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CYShop.Models
{
    public class ProductOrder
    {
        public uint ID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public string ReceiverName { get; set; }

        [Required]
        public string ReceiverAddress { get; set; }

        [Required]
        public string ReceiverPhone { get; set; }

        [Required]
        public string OrderItems { get; set; }

        [Required]
        public int TotalPrice { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public CYShopUser? User { get; set; }

        private string splitTag = ",.,";
        private string endTag = ";;";
        public void SetOrderItems(List<CartItem> cartItems)
        {
            string result = "";
            foreach (CartItem item in cartItems)
            {
                string temp = item.ProductID.ToString() + splitTag +
                    item.ProductName + splitTag +
                    item.Quantity.ToString() + splitTag +
                    item.Price.ToString() + endTag;
                result += temp;
            }
            OrderItems = result;
        }

        public List<CartItem> GetOrderItems()
        {
            List<CartItem> cartList = new List<CartItem>();
            string[] items = OrderItems.Split(endTag);
            foreach (string item in items)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                string[] metas = item.Split(splitTag);
                if (metas.Length != 4)
                {
                    continue;
                }
                cartList.Add(new CartItem
                {
                    ProductID = uint.Parse(metas[0]),
                    ProductName = metas[1],
                    Quantity = uint.Parse(metas[2]),
                    Price = uint.Parse(metas[3])
                });
            }
            return cartList;
        }
    }

    public class ProductOrderViewModel
    {
        [DisplayName("收件人姓名")]
        public string ReceiverName { get; set; }

        [DisplayName("收件人地址")]
        public string ReceiverAddress { get; set; }

        [DisplayName("收件人電話")]
        public string ReceiverPhone { get; set; }

        [DisplayName("總計")]
        public int TotalPrice { get; set; }

        [DisplayName("下訂日期")]
        public DateTime OrderDate { get; set; }

        public List<CartItem> OrderItems { get; set; }

        public ProductOrderViewModel(ProductOrder order)
        {
            ReceiverName = order.ReceiverName;
            ReceiverAddress = order.ReceiverAddress;
            ReceiverPhone = order.ReceiverPhone;
            OrderItems = order.GetOrderItems();
            TotalPrice = order.TotalPrice;
            OrderDate = order.OrderDate;
        }
    }
}
