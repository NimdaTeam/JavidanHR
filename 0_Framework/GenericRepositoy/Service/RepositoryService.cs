using System.Linq.Expressions;
using _0_Framework.GenericRepositoy.Interface;
using Microsoft.EntityFrameworkCore;

namespace _0_Framework.GenericRepositoy.Service
{
    public class RepositoryService<TKey, T> : IRepository<TKey, T> where T : class
    {
        private readonly DbContext _context;

        public RepositoryService(DbContext context)
        {
            _context = context;
        }

        public async Task<T?> Get(TKey id)
        {
            return await _context.FindAsync<T>(id);
        }

        public async Task<List<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<List<T>> GetAllByCondition(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().Where(expression).ToListAsync();
        }

        public Task<bool> Create(T entity)
        {
            try
            {
                _context.Add(entity);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(false);
            }
        }

        public Task<bool> Update(T entity)
        {
            try
            {
                _context.Update(entity);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(false);
            }
        }

        public Task<bool> Delete(T entity)
        {
            try
            {
                _context.Remove(entity);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(false);
            }
        }

        public async Task<bool> Delete(TKey id)
        {
            try
            {
                var entity = await Get(id);
                if (entity is not null)
                   await Delete(entity);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> Exists(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().AnyAsync(expression);
        }

        public async Task<bool> SaveChanges()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
