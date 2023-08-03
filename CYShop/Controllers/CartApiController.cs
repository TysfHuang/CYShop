﻿using CYShop.Data;
using CYShop.Helper;
using CYShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CYShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartApiController : ControllerBase
    {
        private readonly CYShopContext _context;

        public CartApiController(CYShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            List<CartItem>? cartList = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            return JsonSerializer.Serialize(cartList);
        }

        [HttpPost]
        public async Task<ActionResult<string>> Post(CartItem cart)
        {
            if (_context.Products == null)
            {
                return NoContent();
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == cart.ProductID);

            if(product == null)
            {
                return NotFound();
            }

            bool isValid =
                product.ID == cart.ProductID &
                product.Name == cart.ProductName &
                product.Price == cart.Price &
                cart.Quantity == 1;

            if (!isValid)
            {
                return BadRequest();
            }

            List<CartItem>? cartList = SessionHelper.Get<List<CartItem>>(HttpContext.Session, "cart");
            if (cartList == null)
            {
                cartList = new List<CartItem>();
                cartList.Add(cart);
                SessionHelper.Set<List<CartItem>>(HttpContext.Session, "cart", cartList);
            }
            else
            {
                int index = cartList.FindIndex(c => c.ProductID.Equals(cart.ProductID));
                if (index != -1)
                {
                    cartList[index].Quantity += cart.Quantity;
                    cartList[index].Price += cart.Price;
                }
                else
                {
                    cartList.Add(cart);
                }
                SessionHelper.Set<List<CartItem>>(HttpContext.Session, "cart", cartList);
            }
            return JsonSerializer.Serialize(cartList);
        }
    }
}
