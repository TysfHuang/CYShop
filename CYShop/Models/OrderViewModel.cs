using Microsoft.Build.Framework;
using System.ComponentModel;

namespace CYShop.Models
{
    public class OrderViewModel
    {
        [DisplayName("付款者姓名")]
        public string UserName { get; set; }

        [Required]
        [DisplayName("收件人姓名")]
        public string ReceiverName { get; set; }

        [Required]
        [DisplayName("收件人地址")]
        public string ReceiverAddress { get; set; }

        [Required]
        [DisplayName("收件人電話")]
        public string ReceiverPhone { get; set; }
    }
}
