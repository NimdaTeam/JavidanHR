using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using _0_Framework.DTO;
using _0_Framework.EntityBase;
using _0_Framework.GenericRepositoy.Interface;

namespace _0_Framework.GenericRepositoy.Service
{
    public class RepositoryService<TKey, T> : IRepository<TKey, T>, IDisposable
        where T : class
    {
        protected readonly DbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public RepositoryService(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Get Methods

        public virtual async Task<T?> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _context.FindAsync<T>(new object[] { id! }, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<T?> GetAsNoTrackingAsync(TKey id, CancellationToken cancellationToken = default)
        {
            // رفع مشکل: استفاده از AsNoTracking به جای FindAsync + Detach
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id, typeof(TKey));
            var body = Expression.Equal(property, constant);
            var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);

            return await _context.Set<T>()
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<List<T>> GetAllWithTrackingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<T?> SingleOrDefaultByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .AsNoTracking()
                .SingleOrDefaultAsync(expression, cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<T?> SingleOrDefaultWithTrackingAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .SingleOrDefaultAsync(expression, cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<List<T>> GetAllByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .AsNoTracking()
                .Where(expression)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<List<T>> GetAllByConditionWithTrackingAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .Where(expression)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<T?> GetWithIncludesAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var include in includes)
                query = query.Include(include);

            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id, typeof(TKey));
            var body = Expression.Equal(property, constant);
            var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);

            return await query.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<List<T>> GetAllWithIncludesAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var include in includes)
                query = query.Include(include);

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<PaginatedList<T>> GetAllPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>();

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await PaginatedList<T>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);
        }

        #endregion

        #region Create Methods

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            var entry = await _context.Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Update Methods

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            _context.Set<T>().UpdateRange(entities);
            return Task.CompletedTask;
        }

        #endregion

        #region Delete Methods

        public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (entity == null)
                return false;

            _context.Set<T>().Remove(entity);
            return true;
        }

        public virtual Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Set<T>().Remove(entity);
            return Task.FromResult(true);
        }

        public virtual Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            _context.Set<T>().RemoveRange(entities);
            return Task.FromResult(true);
        }

        #endregion

        #region Exists & Count

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().AnyAsync(expression, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default)
        {
            if (expression == null)
                return await _context.Set<T>().CountAsync(cancellationToken).ConfigureAwait(false);
            return await _context.Set<T>().CountAsync(expression, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region SaveChanges

        public virtual async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return affectedRows > 0;
        }

        #endregion

        #region Transaction Methods

        public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _transaction = null;
            }
        }

        public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _transaction = null;
            }
        }

        public virtual async Task<bool> ExecuteInTransactionAsync(Func<Task<bool>> action, CancellationToken cancellationToken = default)
        {
            try
            {
                await BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                var result = await action().ConfigureAwait(false);

                if (result)
                    await CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
                else
                    await RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);

                return result;
            }
            catch (Exception)
            {
                await RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }
                _context.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}