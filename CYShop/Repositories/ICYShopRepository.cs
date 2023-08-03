using CYShop.Models;
using System.Linq.Expressions;

namespace CYShop.Repositories
{
    public interface ICYShopRepository<TEntity, TKey>
        where TEntity : class
    {
        TKey Create(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        TEntity FindById(TKey id);

        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> expression);
    }
}
