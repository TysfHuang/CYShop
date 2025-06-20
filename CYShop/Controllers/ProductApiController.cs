﻿using CYShop.Data;
using CYShop.Models;
using CYShop.Repositories;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly ICYShopRepository<Product, uint> _repository;
        private const int PageSize = 12;

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
            var query = _repository.Find(p => true);
            string currentSortOrder = sortOrder ?? "default";
            int currentPage = pageNum ?? 1;

            category = HtmlEncoder.Default.Encode(category);
            searchString = searchString != null ? HtmlEncoder.Default.Encode(searchString) : null;

            if (!string.Equals(category, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.ProductCategory.Name == category);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowered = searchString.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowered));
            }

            switch (currentSortOrder)
            {
                case "priceHighToLow":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "priceLowToHigh":
                    query = query.OrderBy(p => p.Price);
                    break;
                default:
                    break;
            }
            int totalNumOfProduct = await query.CountAsync();
            var result = await query
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
