using CYShop.Models;
using System.Linq.Expressions;

namespace CYShop.Repositories
{
    public interface ICYShopRepository<TEntity, TKey>
        where TEntity : class
    {
        Task<TKey> CreateAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task<TEntity> FindByIdAsync(TKey id);

        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> expression);
    }
}
