using CYShop.Data;
using CYShop.Helper;
using CYShop.Models;
using CYShop.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq.Expressions;

namespace CYShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICYShopRepository<ProductOrder, uint> _repository;
        ICYShopRepository<ProductSalesCount, uint> _repository_sales;
        private readonly UserManager<CYShopUser> _userManager;

        public OrderController(
            ICYShopRepository<ProductOrder, uint> repository,
            ICYShopRepository<ProductSalesCount, uint> repository_sales,
            UserManager<CYShopUser> userManager)
        {
            _repository = repository;
            _repository_sales = repository_sales;
            _userManager = userManager;
        }

        private async Task<string> GetUserPhone()
        {
            var user = await _userManager.GetUserAsync(User);
            return user.PhoneNumber != null ? user.PhoneNumber : "";
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            List<CartItem>? cart = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            
            var order = new OrderViewModel()
            {
                UserName = _userManager.GetUserName(User),
                ReceiverPhone = await GetUserPhone()
            };
            
            ViewData["Cartlist"] = cart;
            ViewData["TotalPrice"] = GetTotalPriceFromCart(cart);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel orderViewModel)
        {
            List<CartItem>? cart = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            try
            {
                if (ModelState.IsValid && cart != null && cart.Count != 0)
                {
                    ProductOrder order = new ProductOrder
                    {
                        UserID = _userManager.GetUserId(User),
                        ReceiverName = orderViewModel.ReceiverName,
                        ReceiverAddress = orderViewModel.ReceiverAddress,
                        ReceiverPhone = orderViewModel.ReceiverPhone,
                        TotalPrice = GetTotalPriceFromCart(cart),
                        OrderDate = DateTime.Now
                    };
                    order.SetOrderItems(cart);
                    uint id = await _repository.CreateAsync(order);
                    await RecordToSalesCount(cart);
                    SessionHelper.Remove(HttpContext.Session, "cart");
                    return RedirectToAction("Result");
                }
                else if (ModelState.IsValid && (cart == null || cart.Count == 0))
                {
                    ModelState.AddModelError("", "購物車內尚無產品");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "無法創建訂單");
            }

            ViewData["Cartlist"] = cart;
            ViewData["TotalPrice"] = GetTotalPriceFromCart(cart);
            return View(orderViewModel);
        }

        private async Task RecordToSalesCount(List<CartItem> cart)
        {
            foreach(var item in cart)
            {
                var oldSales = await _repository_sales.FindByIdAsync(item.ProductID);
                if (oldSales != null)
                {
                    if(oldSales.OrderDate.Date == DateTime.Now.Date)
                    {
                        oldSales.Count += item.Quantity;
                        await _repository_sales.UpdateAsync(oldSales);
                    }
                    else
                    {
                        await _repository_sales.CreateAsync(ConvertToPSC(item));
                    }
                }
                else
                {
                    await _repository_sales.CreateAsync(ConvertToPSC(item));
                }
            }
        }

        private ProductSalesCount ConvertToPSC(CartItem cart)
        {
            return new ProductSalesCount
            {
                ProductID = cart.ProductID,
                Count = cart.Quantity,
                OrderDate = DateTime.Now.Date
            };
        }

        private int GetTotalPriceFromCart(List<CartItem> cart)
        {
            if (cart == null)
            {
                return 0;
            }

            int totalPrice = 0;
            foreach (CartItem cartItem in cart)
            {
                totalPrice += (int)(cartItem.Price);
            }
            return totalPrice;
        }

        public IActionResult Result()
        {

            return View();
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            Expression<Func<ProductOrder, bool>> expression = p => p.UserID == _userManager.GetUserId(User);
            var orderList = _repository.Find(expression)
                .OrderByDescending(p => p.OrderDate)
                .Select(p => new ProductOrderViewModel(p));

            return View(await PaginatedList<ProductOrderViewModel>.CreateAsync(orderList, pageNumber ?? 1, 2));
        }
    }
}
