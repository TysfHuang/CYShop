using CYShop.Models;
using CYShop.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace CYShop.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductSalesCountApiController : ControllerBase
    {
        private readonly ICYShopRepository<ProductSalesCount, uint> _repository;
        private const int ListSize = 5;

        public ProductSalesCountApiController(ICYShopRepository<ProductSalesCount, uint> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Get()
        {
            return await Get(7);
        }

        [HttpGet("{days}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Get(int? days = 7)
        {
            if (days <= 0 || days > 30)
            {
                return NotFound();
            }
            DateTime specDate = (DateTime.Now - TimeSpan.FromDays(Convert.ToDouble(days))).Date;
            Expression<Func<ProductSalesCount, bool>> query = p => p.OrderDate >= specDate;
            var salesList = await _repository
                .Find(query)
                .Include(s => s.Product)
                .AsNoTracking()
                .ToListAsync();
            var productIdList = salesList.Select(s => s.ProductID).Distinct();
            List<ProductSalesCountDTO> totalSalesCountsList = new List<ProductSalesCountDTO>();
            foreach (var productId in productIdList)    //計算每個產品的總銷售數
            {
                uint totalCount = Convert.ToUInt32(salesList.Where(s => s.ProductID == productId).Sum(s => s.Count));
                Product product = salesList.Where(s => s.ProductID == productId).First().Product;
                totalSalesCountsList.Add(new ProductSalesCountDTO
                {
                    Name = product.Name,
                    CoverImagePath = product.CoverImagePath,
                    Price = product.Price,
                    SalesCount = totalCount
                });
            }
            totalSalesCountsList.Sort((x, y) => x.SalesCount.CompareTo(y.SalesCount) * -1);
            return Ok(totalSalesCountsList.Take(ListSize));
        }
    }
}
