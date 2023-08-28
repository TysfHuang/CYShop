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
        private readonly UserManager<CYShopUser> _userManager;

        public OrderController(ICYShopRepository<ProductOrder, uint> repository, UserManager<CYShopUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            List<CartItem>? cart = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            var order = new OrderViewModel()
            {
                UserName = _userManager.GetUserName(User)
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
