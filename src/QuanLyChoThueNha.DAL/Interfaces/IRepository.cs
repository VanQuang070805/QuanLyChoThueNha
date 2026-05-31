using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QuanLyChoThueNha.DAL.Interfaces
{
    /// <summary>
    /// Generic Repository Interface — CRUD cho mọi Entity.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        T GetById(object id);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        bool Any(Expression<Func<T, bool>> predicate);

        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        int Count(Expression<Func<T, bool>> predicate = null);
    }
}
