using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cappuccino.IDAL
{
    /// <summary>
    /// 数据访问接口基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseDao<T> where T : class, new()
    {
        int SaveChanges();

        /// <summary>
        /// 解除EF实体跟踪
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        void DetachEntity<T>(T entity);

        #region 查询
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        IQueryable<T> GetList(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 执行原生SQL查询
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">SQL参数（避免注入）</param>
        /// <returns>查询结果集</returns>
        IEnumerable<T> GetList(string sql, params object[] parameters);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="whereLambad"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="totalCount"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        IQueryable<T> GetListByPage<S>(Expression<Func<T, bool>> whereLambad, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount, bool isAsc);

        IQueryable<T> GetListByPage(Expression<Func<T, bool>> whereLambad, string sortField, string sortOrder, int pageSize, int pageIndex, out int totalCount);

        IQueryable<T> GetListByPage(Expression<Func<T, bool>> whereLambda, string sortField, string sortOrder, int pageSize, int pageIndex, out int totalCount, params Expression<Func<T, object>>[] includes);

        IEnumerable<T> GetListByPage(string sql, string sortField, string sortOrder, int pageSize, int pageIndex);

        /// <summary>
        /// 查询总数量
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int GetRecordCount(Expression<Func<T, bool>> predicate);
        #endregion

        #region 添加
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        T Insert(T entity);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Insert(params T[] entities);

        /// <summary>
        /// 批量插入数据集
        /// </summary>
        /// <param name="entities">数据集</param>
        int Insert(IEnumerable<T> entities);

        /// <summary>
        /// 插入单个实体并返回ID
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>插入后的主键ID</returns>
        object InsertById(T entity);
        #endregion

        #region 删除
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
        /// 批量删除
        /// </summary>
        /// <param name="entities">删除的数据集</param>
        void Delete(IEnumerable<T> entities);

        /// <summary>
        /// 通过ID删除实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>是否删除成功</returns>
        bool DeleteById(object id);

        /// <summary>
        /// 通过ID（逗号分隔ID）批量删除
        /// </summary>
        /// <param name="ids">逗号分隔的ID字符串</param>
        /// <returns>是否删除成功</returns>
        bool DeleteByIds(object ids);

        /// <summary>
        /// 通过Id列表批量删除
        /// </summary>
        /// <param name="list">ID列表</param>
        /// <returns>是否删除成功</returns>
        bool DeleteByIdList(List<object> list);
        #endregion

        #region 修改
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        T Update(T entity);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertys">要修改的字段</param>
        T Update(T entity, string[] propertys);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int UpdateList(params T[] entities);
        #endregion

        #region 数据源
        /// <summary>
        /// 对数据库执行给定的 DDL/DML 命令。
        /// </summary>
        /// <param name="sql">命令字符串。</param>
        /// <param name="parameters">要应用于命令字符串的参数。</param>
        /// <returns> 执行命令后由数据库返回的结果。</returns>
        int ExecuteSql(string sql, params object[] parameters);

        /// <summary>
        /// 创建一个原始 SQL 查询，该查询将返回给定泛型类型的元素。
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="sql">  SQL 查询字符串。</param>
        /// <param name="parameters"> 要应用于 SQL 查询字符串的参数。</param>
        /// <returns>查询所返回对象的类型</returns>
        IEnumerable<TElement> ExecuteSqlQuery<TElement>(string sql, params object[] parameters);
        #endregion

        /// <summary>
        /// Linq连表查询专用，获取单表所有数据请使用GetList
        /// </summary>
        IQueryable<T> Table { get; }

        Task<IQueryable<T>> GetListAsync(Expression<Func<T, bool>> whereLambda);
        Task<(IQueryable<T>, int)> GetListByPageAsync<S>(Expression<Func<T, bool>> whereLambada, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, bool isAsc);
        Task<int> GetRecordCountAsync(Expression<Func<T, bool>> predicate);
        Task<int> AddAsync(T entity);
        Task<int> AddListAsync(params T[] entities);
        Task<int> DeleteAsync(T entity);
        Task<int> DeleteByAsync(Expression<Func<T, bool>> whereLambda);
        Task<bool> UpdateAsync(T entity);
        Task<bool> UpdateAsync(T entity, string[] propertys);
        Task<int> UpdateListAsync(params T[] entities);
        Task<int> SaveChangesAsync();
        Task<IEnumerable<TElement>> ExecuteSqlQueryAsync<TElement>(string sql, params object[] parameters);
    }
}
