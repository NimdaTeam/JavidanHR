using System.Linq.Expressions;

namespace _0_Framework.GenericRepositoy.Interface
{
    public interface IRepository <TKey , T> where T :class
    {
        Task<T?> Get(TKey id);

        Task<List<T>> GetAll();

       Task<List<T>> GetAllByCondition(Expression<Func<T, bool>> expression);

       Task<bool> Create(T entity);

        Task<bool> Update(T entity);

        Task<bool> Delete(T entity);

        Task<bool> Delete(TKey id);

        Task<bool> Exists(Expression<Func<T, bool>> expression);

        Task<bool> SaveChanges();
    }
}
