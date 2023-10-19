using CYShop.Data;
using CYShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CYShop.Repositories
{
    public class ProductHotSalesListRepository : ICYShopRepository<ProductHotSalesList, uint>
    {
        private readonly CYShopContext _context;

        public ProductHotSalesListRepository(CYShopContext context)
        {
            _context = context;
        }

        public async Task<uint> CreateAsync(ProductHotSalesList entity)
        {
            _context.ProductHotSalesLists.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAsync(ProductHotSalesList entity)
        {
            _context.ProductHotSalesLists.Remove(_context.ProductHotSalesLists.Single(p => p.Id == entity.Id));
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductHotSalesList entity)
        {
            var oriProduct = await _context.ProductHotSalesLists.SingleAsync(p => p.Id == entity.Id);
            _context.Entry(oriProduct).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<ProductHotSalesList> Find(Expression<Func<ProductHotSalesList, bool>> expression)
        {
            return _context.ProductHotSalesLists.Where(expression);
        }

        public async Task<ProductHotSalesList> FindByIdAsync(uint id)
        {
            return await _context.ProductHotSalesLists.SingleOrDefaultAsync(p => p.Id == id);
        }
    }
}
