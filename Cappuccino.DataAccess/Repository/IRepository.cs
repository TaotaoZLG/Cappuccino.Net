using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cappuccino.Entity;

namespace Cappuccino.DataAccess.Repository
{
    /// <summary>
    /// 通用仓储接口（封装所有实体的基础数据访问逻辑）
    /// </summary>
    /// <typeparam name="T">实体类型（继承BaseEntity）</typeparam>
    public interface IRepository<T> where T : BaseEntity, new()
    {
        #region 基础查询
        /// <summary>
        /// 条件查询
        /// </summary>
        IQueryable<T> GetList(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 分页查询（指定排序表达式）
        /// </summary>
        IQueryable<T> GetListByPage<S>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, S>> orderBy,
            int pageSize, int pageIndex, out int totalCount, bool isAsc = true);

        /// <summary>
        /// 分页查询（动态排序字段）
        /// </summary>
        IQueryable<T> GetListByPage(Expression<Func<T, bool>> whereLambda, string sortField, string sortOrder,
            int pageSize, int pageIndex, out int totalCount, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// 原生SQL分页
        /// </summary>
        IEnumerable<T> GetListByPage(string sql, string sortField, string sortOrder, int pageSize, int pageIndex);

        /// <summary>
        /// 统计数量
        /// </summary>
        int GetRecordCount(Expression<Func<T, bool>> predicate);
        #endregion

        #region 增删改
        /// <summary>
        /// 新增
        /// </summary>
        int Insert(T entity);

        /// <summary>
        /// 批量新增
        /// </summary>
        int Insert(IEnumerable<T> entities);

        /// <summary>
        /// 删除
        /// </summary>
        int Delete(T entity);

        /// <summary>
        /// 条件批量删除
        /// </summary>
        int DeleteBy(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 更新（全字段）
        /// </summary>
        bool Update(T entity);

        /// <summary>
        /// 更新（指定字段）
        /// </summary>
        bool Update(T entity, string[] propertys);

        /// <summary>
        /// 批量更新
        /// </summary>
        int UpdateList(params T[] entities);
        #endregion

        #region 异步方法
        Task<IQueryable<T>> GetListAsync(Expression<Func<T, bool>> whereLambda);
        Task<int> InsertAsync(T entity);
        Task<int> InsertAsync(params T[] entities);
        Task<int> DeleteAsync(T entity);
        Task<int> DeleteByAsync(Expression<Func<T, bool>> whereLambda);
        Task<bool> UpdateAsync(T entity);
        Task<bool> UpdateAsync(T entity, string[] propertys);
        Task<int> UpdateListAsync(params T[] entities);
        #endregion
    }
}