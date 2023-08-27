using CYShop.Data;
using CYShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CYShop.Repositories
{
    public class ProductOrderRepository : ICYShopRepository<ProductOrder, uint>
    {
        private readonly CYShopContext _context;

        public ProductOrderRepository(CYShopContext context)
        {
            _context = context;
        }

        public async Task<uint> CreateAsync(ProductOrder entity)
        {
            _context.ProductOrders.Add(entity);
            await _context.SaveChangesAsync();
            return entity.ID;
        }

        public async Task DeleteAsync(ProductOrder entity)
        {
            _context.ProductOrders.Remove(_context.ProductOrders.Single(p => p.ID == entity.ID));
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductOrder entity)
        {
            var oriProductOrder = await _context.ProductOrders.SingleAsync(p => p.ID == entity.ID);
            _context.Entry(oriProductOrder).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<ProductOrder> Find(Expression<Func<ProductOrder, bool>> expression)
        {
            return _context.ProductOrders.Where(expression);
        }

        public async Task<ProductOrder> FindByIdAsync(uint id)
        {
            return await _context.ProductOrders.SingleOrDefaultAsync(p => p.ID == id);
        }
    }
}
