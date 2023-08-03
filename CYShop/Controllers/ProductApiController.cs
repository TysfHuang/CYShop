﻿using CYShop.Data;
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

        public int GetPageSize()
        {
            return PageSize;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var product = await Task.FromResult(_repository.Find(p => true).Select(p => new ProductDTO(p)).ToList());
            if (product == null)
            {
                return NotFound();
            }
            return JsonSerializer.Serialize(product);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(uint id)
        {
            var product = await Task.FromResult(_repository.FindById(id));
            if (product == null)
            {
                return NotFound();
            }
            return JsonSerializer.Serialize(new ProductDTO(product));
        }

        [HttpGet("search/{category}")]
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
            int maxProductNum = products.Count();
            var result = await Task.FromResult(products
                .Skip((currentPage - 1) * GetPageSize())
                .Take(GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToList());

            if (result == null)
            {
                return NotFound();
            }
            var obj = new
            {
                data = result,
                maxPageNum = (int)Math.Ceiling(maxProductNum / (double)GetPageSize())
            };
            return JsonSerializer.Serialize(obj);
        }
    }
}
