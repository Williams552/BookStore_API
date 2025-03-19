using BookStore_API.DataAccess.DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BookStore_API.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IDao<T> _dao;

        public Repository(IDao<T> dao)
        {
            _dao = dao;
        }

        public async Task Add(T entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(T entity)
        {
            await _dao.Update(entity);
        }

        public async Task Delete(T entity)
        {
            await _dao.Delete(entity);
        }

        public async Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes)
        {
            return await _dao.GetAll(includes);
        }

        public async Task<T> GetById(object id, params Expression<Func<T, object>>[] includes)
        {
            // Chuyển đổi id thành kiểu đúng và truyền vào phương thức GetById của dao
            return await _dao.GetById(id, includes);
        }
    }
}
