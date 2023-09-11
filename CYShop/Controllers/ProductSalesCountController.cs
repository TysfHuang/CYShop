using CYShop.Models;
using CYShop.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CYShop.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductSalesCountController : ControllerBase
    {
        private readonly ICYShopRepository<Product, uint> _productRepository;
        private readonly ICYShopRepository<ProductSalesCount, uint> _productSalesCountRepository;
        private const int ListSize = 5;

        public ProductSalesCountController(ICYShopRepository<Product, uint> productRepository,
            ICYShopRepository<ProductSalesCount, uint> productSalesCountRepository)
        {
            _productSalesCountRepository = productSalesCountRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Get(int? days = 7)
        {
            if (days != 1 && days != 7)
            {
                return NotFound();
            }
            DateTime specDate = (DateTime.Now - TimeSpan.FromDays(7)).Date;
            Expression<Func<ProductSalesCount, bool>> query = p => p.OrderDate >= specDate;
            var salesList = await _productSalesCountRepository
                .Find(query)
                .OrderByDescending(s => s.Count)
                .Take(ListSize)
                .ToListAsync();
            List<ProductDTO> list = new List<ProductDTO>();
            foreach(var item in salesList)
            {
                var product = await _productRepository.FindByIdAsync(item.ProductID);
                list.Add(new ProductDTO(product));
            }
            return Ok(list);
        }
    }
}
