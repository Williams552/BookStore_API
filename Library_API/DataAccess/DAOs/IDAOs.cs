namespace BookStore_API.DataAccess.DAOs
{
    public interface IDao<T> where T : class
    {
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(object id);
    }
}
