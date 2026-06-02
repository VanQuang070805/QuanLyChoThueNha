using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using QuanLyChoThueNha.DAL.Interfaces;

namespace QuanLyChoThueNha.DAL.Repositories
{
    /// <summary>
    /// Generic Repository sử dụng EF6 DbContext.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public T GetById(object id) => _dbSet.Find(id);

        public IEnumerable<T> GetAll() => _dbSet.ToList();

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) =>
            _dbSet.Where(predicate).ToList();

        public T FirstOrDefault(Expression<Func<T, bool>> predicate) =>
            _dbSet.Where(predicate).FirstOrDefault();

        public bool Any(Expression<Func<T, bool>> predicate) =>
            _dbSet.Any(predicate);

        public void Add(T entity) => _dbSet.Add(entity);

        public void AddRange(IEnumerable<T> entities) => _dbSet.AddRange(entities);

        public void Update(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(T entity) => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        public int Count(Expression<Func<T, bool>> predicate = null) =>
            predicate == null ? _dbSet.Count() : _dbSet.Count(predicate);
    }
}
