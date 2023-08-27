using CYShop.Data;
using CYShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq.Expressions;

namespace CYShop.Repositories
{
    public class ProductRepository : ICYShopRepository<Product, uint>
    {
        private readonly CYShopContext _context;

        public ProductRepository(CYShopContext context)
        {
            _context = context;
        }

        public async Task<uint> CreateAsync(Product entity)
        {
            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ID;
        }

        public async Task DeleteAsync(Product entity)
        {
            _context.Products.Remove(_context.Products.Single(p => p.ID == entity.ID));
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product entity)
        {
            var oriProduct = await _context.Products.SingleAsync(p => p.ID == entity.ID);
            _context.Entry(oriProduct).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Product> Find(Expression<Func<Product, bool>> expression)
        {
            return _context.Products.Where(expression);
        }

        public async Task<Product> FindByIdAsync(uint id)
        {
            return await _context.Products.SingleOrDefaultAsync(p => p.ID == id);
        }
    }
}
