using System.Linq.Expressions;

namespace Users_API.DataAccess.DAOs
{
    public interface IDao<T> where T : class
    {
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);
        Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes);
        Task<T> GetById(object id, params Expression<Func<T, object>>[] includes);
        Task<T> GetFirstByCondition(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetByCondition(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
    }
}
