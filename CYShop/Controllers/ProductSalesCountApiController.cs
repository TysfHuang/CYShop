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
        private readonly ICYShopRepository<Product, uint> _repository;
        private readonly ICYShopRepository<ProductHotSalesList, uint> _repository_list;
        private const int ListSize = 5;

        public ProductSalesCountApiController(ICYShopRepository<Product, uint> repository,
            ICYShopRepository<ProductHotSalesList, uint> repository_list)
        {
            _repository = repository;
            _repository_list = repository_list;
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
            if (days != 7)
            {
                return NotFound();
            }
            ProductHotSalesListPeriodType period = ProductHotSalesListPeriodType.Week;
            Expression<Func<ProductHotSalesList, bool>> query = p => p.Period == period;
            List<ProductHotSalesList> hotSalesList = await _repository_list
                .Find(query)
                .OrderByDescending(p => p.RecordDate)
                .AsNoTracking()
                .Take(1)
                .ToListAsync();
            if (hotSalesList == null)
            {
                return NotFound();
            }
            List<ProductSalesCountDTO> totalSalesCountsList = new List<ProductSalesCountDTO>();
            string[] data_arr = hotSalesList[0].Itemslist.Split('.');
            for(int i=0;i< data_arr.Length-1 && i < ListSize; i++)
            {
                string[] productIdAndCount = data_arr[i].Split(',');
                Product product = await _repository.FindByIdAsync(Convert.ToUInt32(productIdAndCount[0]));
                totalSalesCountsList.Add(new ProductSalesCountDTO
                {
                    Name = product.Name,
                    CoverImagePath = product.CoverImagePath,
                    Price = product.Price,
                    SalesCount = Convert.ToUInt32(productIdAndCount[1])
                });
            }
            return Ok(totalSalesCountsList);
        }
    }
}
