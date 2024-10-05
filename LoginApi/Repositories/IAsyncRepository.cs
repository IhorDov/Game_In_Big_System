using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LoginApi.Repositories
{
    public interface IAsyncRepository<TContext> where TContext : DbContext
    {
        Task AddItem<TEntity>(TEntity entity) where TEntity : class;
        //Task AddItem<T>(T entity) where T : class;

        Task AddItems<TEntity>(List<TEntity> entities) where TEntity : class;

        Task RemoveItem<TEntity>(TEntity entity) where TEntity : class;
        //Task DeleteItem<T>(T entity) where T : class;

        Task RemoveItem<TEntity>(Expression<Func<TEntity, bool>> searchExpression) where TEntity : class;

        Task RemoveItems<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryOperation) where TEntity : class;

        Task<TEntity> GetItem<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryOperation) where TEntity : class;
        //Task<T> GetItem<T>(Func<IQueryable<T>, IQueryable<T>> query) where T : class; // for posgress db

        Task<List<TEntity>> GetAllItems<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryOperation = null) where TEntity : class;
        //Task<List<T>> GetAllItems<T>() where T : class;

        Task UpdateItem<TEntity>(TEntity item) where TEntity : class;
        //Task UpdateItem<T>(T entity) where T : class;

        Task UpdateItems<TEntity>(List<TEntity> items) where TEntity : class;
    }
}
