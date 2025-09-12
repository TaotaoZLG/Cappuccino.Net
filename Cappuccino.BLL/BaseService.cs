using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cappuccino.Common;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public abstract class BaseService<T> : IDisposable where T : class, new()
    {

        protected IBaseDao<T> CurrentDao;

        public BaseService()
        {
            this.DisposableObjects = new List<IDisposable>();
        }

        public IQueryable<T> GetList(Expression<Func<T, bool>> whereLambda)
        {
            return this.CurrentDao.GetList(whereLambda);
        }

        public IQueryable<T> GetListByPage<S>(Expression<Func<T, bool>> whereLambada, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount, bool isAsc)
        {
            return this.CurrentDao.GetListByPage<S>(whereLambada, orderBy, pageSize, pageIndex, out totalCount, isAsc);
        }

        public virtual IQueryable<T> GetListByPage(Expression<Func<T, bool>> whereLambada, string sortField, string sortOrder, int pageSize, int pageIndex, out int totalCount)
        {
            return CurrentDao.GetListByPage(whereLambada, sortField, sortOrder, pageSize, pageIndex, out totalCount);
        }

        public int GetRecordCount(Expression<Func<T, bool>> predicate)
        {
            return this.CurrentDao.GetRecordCount(predicate);
        }

        public int Add(T entity)
        {
            this.CurrentDao.Add(entity);
            return CurrentDao.SaveChanges();
        }

        public int AddList(params T[] entities)
        {
            return this.CurrentDao.AddList(entities);
        }

        public int Delete(T entity)
        {
            this.CurrentDao.Delete(entity);
            return CurrentDao.SaveChanges();
        }

        public int DeleteBy(Expression<Func<T, bool>> whereLambda)
        {
            this.CurrentDao.DeleteBy(whereLambda);
            return CurrentDao.SaveChanges();
        }

        public bool Update(T entity)
        {
            this.CurrentDao.Update(entity);
            return this.CurrentDao.SaveChanges() > 0;
        }

        public virtual bool Update(T entity, string[] propertys)
        {
            this.CurrentDao.Update(entity, propertys);
            return this.CurrentDao.SaveChanges() > 0;
        }

        public int UpdateList(params T[] entities)
        {
            return this.CurrentDao.UpdateList(entities);
        }

        // 异步方法实现
        public async Task<IQueryable<T>> GetListAsync(Expression<Func<T, bool>> whereLambda)
        {
            return await CurrentDao.GetListAsync(whereLambda);
        }

        public async Task<(IQueryable<T>, int)> GetListByPageAsync<S>(Expression<Func<T, bool>> whereLambada,Expression<Func<T, S>> orderBy,int pageSize,int pageIndex,bool isAsc)
        {
            return await CurrentDao.GetListByPageAsync(whereLambada, orderBy, pageSize, pageIndex, isAsc);
        }

        public async Task<int> GetRecordCountAsync(Expression<Func<T, bool>> predicate)
        {
            return await CurrentDao.GetRecordCountAsync(predicate);
        }

        public async Task<int> AddAsync(T entity)
        {
            return await CurrentDao.AddAsync(entity);
        }

        public async Task<int> AddListAsync(params T[] entities)
        {
            return await CurrentDao.AddListAsync(entities);
        }

        public async Task<int> DeleteAsync(T entity)
        {
            return await CurrentDao.DeleteAsync(entity);
        }

        public async Task<int> DeleteByAsync(Expression<Func<T, bool>> whereLambda)
        {
            return await CurrentDao.DeleteByAsync(whereLambda);
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            return await CurrentDao.UpdateAsync(entity);
        }

        public async virtual Task<bool> UpdateAsync(T entity, string[] propertys)
        {
            return await CurrentDao.UpdateAsync(entity, propertys);
        }

        public async Task<int> UpdateListAsync(params T[] entities)
        {
            return await CurrentDao.UpdateListAsync(entities);
        }


        public IList<IDisposable> DisposableObjects { get; private set; }

        protected void AddDisposableObject(object obj)
        {
            IDisposable disposable = obj as IDisposable;
            if (disposable != null)
            {
                this.DisposableObjects.Add(disposable);
            }
        }

        public void Dispose()
        {
            foreach (IDisposable obj in this.DisposableObjects)
            {
                if (obj != null)
                {
                    obj.Dispose();
                }
            }
        }
    }
}
