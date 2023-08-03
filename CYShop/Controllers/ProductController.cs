using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CYShop.Data;
using CYShop.Models;
using Microsoft.AspNetCore.Authorization;
using System.Xml.Linq;

namespace CYShop.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly CYShopContext _context;
        private readonly IConfiguration _configuration;

        public ProductController(CYShopContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Product
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["BrandSortParm"] = sortOrder == "brand" ? "brand_desc" : "brand";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var cyShopContext = _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductCategory)
                .Select(p => p);

            if (!string.IsNullOrEmpty(searchString))
            {
                cyShopContext = cyShopContext.Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    cyShopContext = cyShopContext.OrderByDescending(c => c.Name);
                    break;
                case "price":
                    cyShopContext = cyShopContext.OrderBy(c => c.Price);
                    break;
                case "price_desc":
                    cyShopContext = cyShopContext.OrderByDescending(c => c.Price);
                    break;
                case "brand":
                    cyShopContext = cyShopContext.OrderBy(c => c.ProductBrand.Name);
                    break;
                case "brand_desc":
                    cyShopContext = cyShopContext.OrderByDescending(c => c.ProductBrand.Name);
                    break;
                default:
                    cyShopContext = cyShopContext.OrderBy(c => c.Name);
                    break;
            }

            int pageSize = _configuration.GetValue("PageSize", 6);
            return View(PaginatedList<Product>.CreateAsync(
                cyShopContext.AsNoTracking(),
                pageNumber ?? 1,
                pageSize));
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(uint? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewData["ProductBrandID"] = new SelectList(_context.Set<ProductBrand>(), "ID", "Name");
            ViewData["ProductCategoryID"] = new SelectList(_context.Set<ProductCategory>(), "ID", "Name");
            return View();
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewProduct(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newProduct = new Product();
                    
                    if (await TryUpdateModelAsync<Product>(
                        newProduct,
                        "",
                        p => p.Name, p => p.Description, p => p.CoverImagePath, p => p.Price,
                        p => p.ProductCategoryID, p => p.ProductBrandID))
                    {
                        _context.Add(newProduct);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "無法創建商品");
            }

            ViewData["ProductBrandID"] = new SelectList(_context.Set<ProductBrand>(), "ID", "Name", product.ProductBrandID);
            ViewData["ProductCategoryID"] = new SelectList(_context.Set<ProductCategory>(), "ID", "Name", product.ProductBrandID);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(uint? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductBrandID"] = new SelectList(_context.Set<ProductBrand>(), "ID", "Name", product.ProductBrandID);
            ViewData["ProductCategoryID"] = new SelectList(_context.Set<ProductCategory>(), "ID", "Name", product.ProductCategoryID);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(uint id, Product product)
        {
            if (id != product.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newProduct = new Product();

                if (await TryUpdateModelAsync<Product>(
                    newProduct,
                    "",
                    p => p.ID, p => p.Name, p => p.Description, p => p.CoverImagePath, p => p.Price,
                    p => p.ProductCategoryID, p => p.ProductBrandID))
                {
                    try
                    {
                        _context.Update(newProduct);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProductExists(product.ID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["ProductBrandID"] = new SelectList(_context.Set<ProductBrand>(), "ID", "Name", product.ProductBrandID);
            ViewData["ProductCategoryID"] = new SelectList(_context.Set<ProductCategory>(), "ID", "Name", product.ProductCategoryID);
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(uint? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(uint id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'CYShopContext.Product'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(uint id)
        {
            return _context.Products.Any(e => e.ID == id);
        }
    }
}
