using CYShop.Data;
using CYShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CYShop.Repositories
{
    public class ProductSalesCountRepository : ICYShopRepository<ProductSalesCount, uint>
    {
        private readonly CYShopContext _context;

        public ProductSalesCountRepository(CYShopContext context)
        {
            _context = context;
        }

        public async Task<uint> CreateAsync(ProductSalesCount entity)
        {
            _context.ProductSalesCounts.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAsync(ProductSalesCount entity)
        {
            _context.ProductSalesCounts.Remove(_context.ProductSalesCounts.Single(p => p.Id == entity.Id));
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductSalesCount entity)
        {
            var oriProduct = await _context.ProductSalesCounts.SingleAsync(p => p.Id == entity.Id);
            _context.Entry(oriProduct).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<ProductSalesCount> Find(Expression<Func<ProductSalesCount, bool>> expression)
        {
            return _context.ProductSalesCounts.Where(expression);
        }

        public async Task<ProductSalesCount> FindByIdAsync(uint id)
        {
            return await _context.ProductSalesCounts.SingleOrDefaultAsync(p => p.Id == id);
        }
    }
}
