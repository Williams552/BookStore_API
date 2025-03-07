using Microsoft.EntityFrameworkCore;

namespace Library_API.DataAccess.DAOs
{
    public class Dao<T> : IDao<T> where T : class
    {
        private readonly BookstoreContext _context;
        private DbSet<T> _dbSet;

        public Dao(BookstoreContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T GetById(object id)
        {
            return _dbSet.Find(id);
        }
    }
}
