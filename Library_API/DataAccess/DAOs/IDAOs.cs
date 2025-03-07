namespace Library_API.DataAccess.DAOs
{
    public interface IDao<T> where T : class
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        IEnumerable<T> GetAll();
        T GetById(object id);
    }
}
