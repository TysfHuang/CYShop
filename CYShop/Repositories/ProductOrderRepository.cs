using CYShop.Data;
using CYShop.Models;
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

        public uint Create(ProductOrder entity)
        {
            _context.ProductOrders.Add(entity);
            _context.SaveChanges();
            return entity.ID;
        }

        public void Delete(ProductOrder entity)
        {
            _context.ProductOrders.Remove(_context.ProductOrders.Single(p => p.ID == entity.ID));
            _context.SaveChanges();
        }

        public void Update(ProductOrder entity)
        {
            var oriProductOrder = _context.ProductOrders.Single(p => p.ID == entity.ID);
            _context.Entry(oriProductOrder).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }

        public IQueryable<ProductOrder> Find(Expression<Func<ProductOrder, bool>> expression)
        {
            return _context.ProductOrders.Where(expression);
        }

        public ProductOrder FindById(uint id)
        {
            return _context.ProductOrders.SingleOrDefault(p => p.ID == id);
        }
    }
}
