using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cappuccino.IBLL
{
    /// <summary>
    /// 业务逻辑层基类接口
    /// </summary>
    public interface IBaseService<T> where T : class, new()
    {
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        IQueryable<T> GetList(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="whereLambada"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="totalCount"></param>
        /// <param name="isAsc">是否升序</param>
        /// <returns></returns>
        IQueryable<T> GetListByPage<S>(Expression<Func<T, bool>> whereLambada, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount, bool isAsc);

        IQueryable<T> GetListByPage(Expression<Func<T, bool>> whereLambad, string sortField, string sortOrder, int pageSize, int pageIndex, out int totalCount);

        /// <summary>
        /// 原生SQL分页查询
        /// </summary>
        /// <typeparam name="TElement">返回结果类型</typeparam>
        /// <param name="sql">基础查询SQL（不含排序和分页）</param>
        /// <param name="parameters">SQL参数（防注入）</param>
        /// <param name="sortField">排序字段（对应SQL查询结果中的列名）</param>
        /// <param name="sortOrder">排序方向（asc/desc）</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="totalCount">输出总条数</param>
        /// <returns>分页数据列表</returns>
        IEnumerable<T> GetListByPage(string sql, string sortField, string sortOrder, int pageSize, int pageIndex);

        /// <summary>
        /// 查询总数量
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int GetRecordCount(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Add(T entity);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int AddList(params T[] entities);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Delete(T entity);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        int DeleteBy(Expression<Func<T, bool>> whereLambda);        

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Update(T entity);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertys">要修改的字段</param>
        bool Update(T entity, string[] propertys);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int UpdateList(params T[] entities);

        Task<IQueryable<T>> GetListAsync(Expression<Func<T, bool>> whereLambda);

        Task<int> AddAsync(T entity);
        Task<int> AddListAsync(params T[] entities);
        Task<int> DeleteAsync(T entity);
        Task<int> DeleteByAsync(Expression<Func<T, bool>> whereLambda);
        Task<bool> UpdateAsync(T entity);
        Task<bool> UpdateAsync(T entity, string[] propertys);
        Task<int> UpdateListAsync(params T[] entities);
    }
}
