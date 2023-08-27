using CYShop.Data;
using CYShop.Models;
using CYShop.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Core.Types;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CYShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly ICYShopRepository<Product, uint> _repository;
        private const int PageSize = 10;

        public ProductApiController(ICYShopRepository<Product, uint> repository)
        {
            _repository = repository;
        }

        public static int GetPageSize()
        {
            return PageSize;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Get()
        {
            return await Get("ALL", null, null, 1);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Get(uint id)
        {
            var product = await _repository.FindByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(new ProductDTO(product));
        }

        [HttpGet("search/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Get(
            string category,
            string? searchString,
            string? sortOrder,
            int? pageNum)
        {
            Expression<Func<Product, bool>> categoryQuery = p => true;
            Expression<Func<Product, bool>> nameQuery = p => true;
            string currentSortOrder = sortOrder ?? "default";
            int currentPage = pageNum ?? 1;
            category = HtmlEncoder.Default.Encode(category);
            searchString = searchString != null ? HtmlEncoder.Default.Encode(searchString) : null;

            if (category != "ALL")
            {
                categoryQuery = p => p.ProductCategory.Name == category;
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                nameQuery = p => p.Name.ToLower().Contains(searchString.ToLower());
            }
            var param = Expression.Parameter(typeof(Product), "p");
            var body = Expression.AndAlso(
                Expression.Invoke(categoryQuery, param),
                Expression.Invoke(nameQuery, param)
            );
            var finalQuery = Expression.Lambda<Func<Product, bool>>(body, param);
            var products = _repository.Find(finalQuery);
            switch (currentSortOrder)
            {
                case "priceHighToLow":
                    products = products.OrderByDescending(p => p.Price); break;
                case "priceLowToHigh":
                    products = products.OrderBy(p => p.Price); break;
            }
            int totalNumOfProduct = products.Count();
            var result = await products
                .Skip((currentPage - 1) * GetPageSize())
                .Take(GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToListAsync();
            
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            var obj = ConvertToResponseFormat(result, totalNumOfProduct);
            return Ok(obj);
        }

        private static object ConvertToResponseFormat(List<ProductDTO> productList, int totalNumOfProduct)
        {
            return new
            {
                data = productList,
                maxPageNum = (int)Math.Ceiling(totalNumOfProduct / (double)GetPageSize())
            };
        }
    }
}
