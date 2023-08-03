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

        public uint Create(Product entity)
        {
            _context.Products.Add(entity);
            _context.SaveChanges();
            return entity.ID;
        }

        public void Delete(Product entity)
        {
            _context.Products.Remove(_context.Products.Single(p => p.ID == entity.ID));
            _context.SaveChanges();
        }

        public void Update(Product entity)
        {
            var oriProduct = _context.Products.Single(p => p.ID == entity.ID);
            _context.Entry(oriProduct).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }

        public IQueryable<Product> Find(Expression<Func<Product, bool>> expression)
        {
            return _context.Products.Where(expression);
        }

        public Product FindById(uint id)
        {
            return _context.Products.SingleOrDefault(p => p.ID == id);
        }
    }
}
