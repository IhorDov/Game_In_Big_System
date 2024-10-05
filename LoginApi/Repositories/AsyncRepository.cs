using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LoginApi.Repositories
{
    public class AsyncRepository<TContext> : IAsyncRepository<TContext> where TContext : DbContext
    {
        protected readonly TContext _context;

        public AsyncRepository(TContext context)
        {
            _context = context;
        }

        public async Task AddItem<TEntity>(TEntity entity) where TEntity : class
        {
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddItems<TEntity>(List<TEntity> entities) where TEntity : class
        {
            await _context.Set<TEntity>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItem<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }


        public async Task RemoveItem<TEntity>(Expression<Func<TEntity, bool>> searchExpression) where TEntity : class
        {
            var entity = await _context.Set<TEntity>().FirstOrDefaultAsync(searchExpression);
            if (entity != null)
            {
                _context.Set<TEntity>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TEntity> GetItem<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryOperation) where TEntity : class
        {
            return await queryOperation(_context.Set<TEntity>()).FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetAllItems<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryOperation = null) where TEntity : class
        {
            return queryOperation == null
                ? await _context.Set<TEntity>().ToListAsync()
                : await queryOperation(_context.Set<TEntity>()).ToListAsync();
        }

        public async Task UpdateItem<TEntity>(TEntity item) where TEntity : class
        {
            _context.Set<TEntity>().Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItems<TEntity>(List<TEntity> items) where TEntity : class
        {
            _context.Set<TEntity>().UpdateRange(items);
            await _context.SaveChangesAsync();
        }

        public Task RemoveItems<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryOperation) where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}
