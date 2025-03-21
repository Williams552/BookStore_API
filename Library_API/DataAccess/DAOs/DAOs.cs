using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BookStore_API.DataAccess.DAOs
{
    public class Dao<T> : IDao<T> where T : class
    {
        private readonly BookStoreContext _context;
        private DbSet<T> _dbSet;
        private readonly string _primaryKeyName;
        public Dao(BookStoreContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();

            var entityType = _context.Model.FindEntityType(typeof(T));
            var primaryKey = entityType.FindPrimaryKey();
            _primaryKeyName = primaryKey.Properties[0].Name;
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            // Lấy giá trị primary key của entity
            var keyValue = _context.Entry(entity).Property(_primaryKeyName).CurrentValue;

            // Tìm entity hiện tại trong database hoặc context
            var existingEntity = await _dbSet.FindAsync(keyValue);

            if (existingEntity == null)
            {
                throw new InvalidOperationException("Entity not found.");
            }

            // Copy giá trị từ entity mới sang entity hiện tại
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);

            await _context.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }
        public async Task<T> GetById(object id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Sử dụng primary key đã lấy được
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, _primaryKeyName) == (int)id
            );


        }

        public async Task<T> GetProfileByEmail(string email, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Email") == email);
        }
    }
}