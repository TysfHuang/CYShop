using CYShop.Data;
using CYShop.Helper;
using CYShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CYShop.Controllers
{
    public class CartController : Controller
    {
        private readonly CYShopContext _context;

        public CartController(CYShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<CartItem> cart = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            int totalPrice = 0;
            if(cart != null)
            {
                foreach (CartItem item in cart)
                {
                    totalPrice += (int)(item.Quantity * item.Price);
                }
            }
            
            return View(cart);
        }

        public async Task<IActionResult> Add(uint id)
        {
            if (_context.Products == null)
            {
                return NoContent();
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NoContent();
            }

            CartItem item = new CartItem
            {
                ProductID = product.ID,
                ProductName = product.Name,
                Quantity = 1,
                Price = product.Price
            };

            List<CartItem> cart = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            if (cart == null)
            {
                cart = new List<CartItem>();
                cart.Add(item);
                SessionHelper.Set<List<CartItem>>(HttpContext.Session, "cart", cart);
            }
            else
            {
                int index = cart.FindIndex(c => c.ProductID.Equals(id));
                if (index != -1)
                {
                    cart[index].Quantity += item.Quantity;
                    cart[index].Price += item.Price;
                }
                else
                {
                    cart.Add(item);
                }
                SessionHelper.Set<List<CartItem>>(HttpContext.Session, "cart", cart);
            }

            return NoContent();
        }

        public async Task<IActionResult> Delete(uint id)
        {
            List<CartItem> cart = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            if (cart == null)
            {
                return NoContent();
            }

            int index = cart.FindIndex(c => c.ProductID.Equals(id));
            if (index != -1)
            {
                cart.RemoveAt(index);
                SessionHelper.Set<List<CartItem>>(HttpContext.Session, "cart", cart);
            }
            return RedirectToAction("Index");
        }
    }
}
