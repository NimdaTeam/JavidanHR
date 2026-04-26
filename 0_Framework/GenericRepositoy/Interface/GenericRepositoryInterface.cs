using System.Linq.Expressions;
using _0_Framework.DTO;
using _0_Framework.EntityBase;

namespace _0_Framework.GenericRepositoy.Interface
{
    public interface IRepository<TKey, T> where T : class
    {
        // Get Methods
        Task<T?> GetAsync(TKey id, CancellationToken cancellationToken = default);
        Task<T?> GetAsNoTrackingAsync(TKey id, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<T>> GetAllWithTrackingAsync(CancellationToken cancellationToken = default);
        Task<T?> SingleOrDefaultByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        Task<T?> SingleOrDefaultWithTrackingAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllByConditionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllByConditionWithTrackingAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        Task<T?> GetWithIncludesAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAllWithIncludesAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);

        Task<PaginatedList<T>> GetAllPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken cancellationToken = default);

        // Create Methods
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Update Methods (EF Core tracks changes, so just mark as modified)
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Delete Methods
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Exists & Count
        Task<bool> ExistsAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default);

        // SaveChanges
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Transaction
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<bool> ExecuteInTransactionAsync(Func<Task<bool>> action, CancellationToken cancellationToken = default);
    }
}