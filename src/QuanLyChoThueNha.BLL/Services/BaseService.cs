using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.DAL.Repositories;

namespace QuanLyChoThueNha.BLL.Services
{
    /// <summary>
    /// Service gốc — CRUD chạy được ngay cho mọi Entity.
    /// Mỗi Service cụ thể kế thừa class này và bổ sung nghiệp vụ riêng.
    /// </summary>
    public abstract class BaseService<T> where T : class
    {
        protected readonly IUnitOfWork _uow;

        protected BaseService()
        {
            _uow = new UnitOfWork();
        }

        protected abstract IRepository<T> Repo { get; }

        public virtual T LayTheoMa(object ma) => Repo.GetById(ma);

        public virtual IEnumerable<T> LayTatCa() => Repo.GetAll();

        public virtual IEnumerable<T> Tim(Expression<Func<T, bool>> dkLoc) => Repo.Find(dkLoc);

        public virtual T TimMotBan(Expression<Func<T, bool>> dkLoc) => Repo.FirstOrDefault(dkLoc);

        public virtual bool TonTai(Expression<Func<T, bool>> dkLoc) => Repo.Any(dkLoc);

        public virtual void Them(T entity)
        {
            Repo.Add(entity);
            _uow.Complete();
        }

        public virtual void Sua(T entity)
        {
            Repo.Update(entity);
            _uow.Complete();
        }

        public virtual void Xoa(T entity)
        {
            Repo.Remove(entity);
            _uow.Complete();
        }

        public virtual int DemTatCa() => Repo.Count();
    }
}
