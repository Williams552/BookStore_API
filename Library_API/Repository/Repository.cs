using Library_API.DataAccess.DAOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library_API.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IDao<T> _dao;

        public Repository(IDao<T> dao)
        {
            _dao = dao;
        }

        public void Add(T entity)
        {
            _dao.Add(entity);
        }

        public void Update(T entity)
        {
            _dao.Update(entity);
        }

        public void Delete(T entity)
        {
            _dao.Delete(entity); 
        }

        public IEnumerable<T> GetAll()
        {
            return _dao.GetAll(); 
        }

        public T GetById(object id)
        {
            return _dao.GetById(id);  
        }
    }

}
