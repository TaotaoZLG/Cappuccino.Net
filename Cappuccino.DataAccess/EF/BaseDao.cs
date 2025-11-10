using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cappuccino.IDAL;

namespace Cappuccino.DataAccess
{
    /// <summary>
    /// 实现数据访问基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseDao<T> : IBaseDao<T>, IDisposable where T : class, new()
    {
        private DbContext Db
        {
            get { return DbContextFactory.GetCurrentThreadInstance(); }
        }

        private IDbSet<T> dbSet;

        /// <summary>
        /// Entities
        /// </summary>
        protected virtual IDbSet<T> DbSet
        {
            get
            {
                this.dbSet = this.dbSet ?? Db.Set<T>();
                return this.dbSet;
            }
        }

        /// <summary>
        /// 释放EF上下文
        /// DbContext有默认的垃圾回收机制，但通过BaseRepository实现IDisposable接口，可以在不用EF上下文的时候手动回收，时效性更强。
        /// </summary>
        public void Dispose()
        {
            Db.Dispose();
        }

        public int SaveChanges()
        {
            return Db.SaveChanges();
        }

        #region 查询

        public virtual IQueryable<T> GetList(Expression<Func<T, bool>> whereLambda)
        {
            IQueryable<T> result = DbSet.Where(whereLambda);
            return result;
        }

        public virtual IEnumerable<T> GetList(string sql, params object[] parameters)
        {
            return Db.Database.SqlQuery<T>(sql, parameters);
        }

        public virtual IQueryable<T> GetListByPage<S>(Expression<Func<T, bool>> whereLambada, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount, bool isAsc)
        {
            totalCount = DbSet.Where(whereLambada).Count();
            IQueryable<T> entities = null;
            if (isAsc)
            {
                entities = DbSet.Where(whereLambada)
                    .OrderBy(orderBy)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize);
            }
            else
            {
                entities = DbSet.Where(whereLambada)
                    .OrderByDescending(orderBy)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize);
            }
            return entities;
        }

        public virtual IQueryable<T> GetListByPage(Expression<Func<T, bool>> whereLambada, string sortField, string sortOrder, int pageSize, int pageIndex, out int totalCount)
        {
            // 基础查询与总条数计算
            var query = DbSet.Where(whereLambada);
            totalCount = query.Count();

            // 处理排序方向（默认降序，无效值也按降序处理）
            bool isAsc = !string.IsNullOrEmpty(sortOrder) && sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);

            // 反射获取实体的排序字段属性，不区分大小写查找实体属性（忽略字段名大小写）
            var property = typeof(T).GetProperty(sortField, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            // 构建排序表达式：x => x.排序字段
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var orderByExpr = Expression.Lambda(propertyAccess, parameter);

            // 反射调用 Queryable 的 OrderBy 或 OrderByDescending 静态方法
            var queryableType = typeof(Queryable);
            var methodName = isAsc ? "OrderBy" : "OrderByDescending";

            // 获取泛型方法定义并绑定类型
            var method = queryableType.GetMethods()
                .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                .Single()
                .MakeGenericMethod(typeof(T), property.PropertyType);

            // 执行排序
            var orderedQuery = (IQueryable<T>)method.Invoke(null, new object[] { query, orderByExpr });

            // 分页处理
            return orderedQuery.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

        public virtual IEnumerable<T> GetListByPage(string sql, string sortField, string sortOrder, int pageSize, int pageIndex)
        {
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "asc" : sortOrder;
            sortField = string.IsNullOrEmpty(sortField) ? "Id" : sortField;

            var paginationSql = $@"
                {sql}
                ORDER BY {sortField} {sortOrder}
                OFFSET ({pageIndex} - 1) * {pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY
            ";
            return Db.Database.SqlQuery<T>(paginationSql);
        }

        public virtual int GetRecordCount(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).Count();
        }
        #endregion

        #region 添加
        public virtual T Insert(T entity)
        {
            DbSet.Add(entity);
            return entity;
        }

        public virtual int Insert(params T[] entities)
        {
            int result = 0;
            for (int i = 0; i < entities.Count(); i++)
            {
                if (entities[i] == null) continue;
                DbSet.Add(entities[i]);
                //每累计到10条记录就提交
                if (i != 0 && i % 10 == 0)
                {
                    result += Db.SaveChanges();
                }
            }

            //可能还有不到10条的记录
            if (entities.Count() > 0)
            {
                result += Db.SaveChanges();
            }
            return result;
        }

        /// <summary>
        /// 批量插入数据集
        /// </summary>
        /// <param name="entities">数据集</param>
        public virtual int Insert(IEnumerable<T> entities)
        {
            int count = 0;

            if (entities == null) throw new ArgumentNullException(nameof(entities), "插入的数据集不能为null");

            var entityList = entities.ToList();
            if (!entityList.Any()) return count; // 空集合直接返回

            foreach (var entity in entityList)
            {
                if (entity == null) continue;
                DbSet.Add(entity);
                count++;

                // 每累计10条提交一次
                if (count % 10 == 0)
                {
                    SaveChanges();
                }
            }

            // 提交剩余不足10条的记录
            if (count % 10 != 0)
            {
                SaveChanges();
            }
            return count;
        }

        /// <summary>
        /// 插入单个实体并返回ID
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>插入后的主键ID</returns>
        public virtual object InsertById(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "插入的实体不能为null");

            // 调用Add方法添加实体
            var addedEntity = DbSet.Add(entity);
            // 保存更改（此时数据库会生成主键并回填到实体）
            SaveChanges();

            // 获取主键值（基于BaseField的Id属性）
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException("实体类型未定义Id属性，无法获取主键");

            return idProperty.GetValue(addedEntity);
        }
        #endregion

        #region 删除
        public virtual int Delete(T entity)
        {
            DbSet.Attach(entity);
            Db.Entry(entity).State = EntityState.Deleted;
            return -1;
        }

        public virtual int DeleteBy(Expression<Func<T, bool>> whereLambda)
        {
            var entitiesToDelete = DbSet.Where(whereLambda);
            foreach (var item in entitiesToDelete)
            {
                Db.Entry(item).State = EntityState.Deleted;
            }
            return -1;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entities">删除的数据集</param>
        public virtual void Delete(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities), "删除的数据集不能为null");

            var entityList = entities.ToList();
            if (!entityList.Any())
                return;

            int count = 0;
            foreach (var entity in entityList)
            {
                if (entity == null) continue;
                DbSet.Attach(entity);
                Db.Entry(entity).State = EntityState.Deleted;
                count++;

                // 每10条提交一次
                if (count % 10 == 0)
                {
                    SaveChanges();
                }
            }

            // 提交剩余记录
            if (count % 10 != 0)
            {
                SaveChanges();
            }
        }

        /// <summary>
        /// 通过ID删除实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>是否删除成功</returns>
        public virtual bool DeleteById(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id), "ID不能为null");

            // 获取实体类型的主键属性（默认Id）
            var entity = DbSet.Find(id);
            if (entity == null)
                return false; // 实体不存在

            DbSet.Remove(entity);
            return SaveChanges() > 0;
        }

        /// <summary>
        /// 通过ID（逗号分隔ID）批量删除
        /// </summary>
        /// <param name="ids">逗号分隔的ID字符串</param>
        /// <returns>是否删除成功</returns>
        public virtual bool DeleteByIds(object ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids), "ID字符串不能为null");

            var idsStr = ids.ToString();
            if (string.IsNullOrWhiteSpace(idsStr))
                return false;

            // 分割ID字符串并转换为列表
            var idList = idsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(id => (object)id)
                              .ToList();

            return DeleteByIdList(idList);
        }

        /// <summary>
        /// 通过Id列表批量删除
        /// </summary>
        /// <param name="list">ID列表</param>
        /// <returns>是否删除成功</returns>
        public virtual bool DeleteByIdList(List<object> list)
        {
            if (list == null || !list.Any())
                return false;

            int count = 0;
            foreach (var id in list)
            {
                var entity = DbSet.Find(id);
                if (entity == null) continue;

                DbSet.Remove(entity);
                count++;

                // 每10条提交一次
                if (count % 10 == 0)
                {
                    SaveChanges();
                }
            }

            // 提交剩余记录
            if (count % 10 != 0)
            {
                count = SaveChanges();
            }

            return count > 0;
        }
        #endregion

        #region 修改
        public virtual T Update(T entity)
        {
            if (entity != null)
            {
                DbSet.Attach(entity);
                Db.Entry(entity).State = EntityState.Modified;
            }
            return entity;
        }

        public virtual T Update(T entity, string[] propertys)
        {
            if (entity != null)
            {
                if (propertys.Any() != false)
                {
                    //将model追击到EF容器
                    Db.Entry(entity).State = EntityState.Unchanged;
                    foreach (var item in propertys)
                    {
                        Db.Entry(entity).Property(item).IsModified = true;
                    }

                    //关闭EF对于实体的合法性验证参数
                    Db.Configuration.ValidateOnSaveEnabled = false;
                }
            }

            return entity;
        }

        public virtual int UpdateList(params T[] entities)
        {
            int result = 0;
            for (int i = 0; i < entities.Count(); i++)
            {
                if (entities[i] == null) continue;
                DbSet.Attach(entities[i]);
                Db.Entry(entities[i]).State = EntityState.Modified;
                if (i != 0 && i % 10 == 0)
                {
                    result += Db.SaveChanges();
                }
            }

            //可能还存在不到10条的记录
            if (entities.Count() > 0)
            {
                result += Db.SaveChanges();
            }
            return result;
        }

        public int ExecuteSql(string sql, params object[] parameters)
        {
            return Db.Database.ExecuteSqlCommand(sql, parameters);
        }
        #endregion

        #region 数据源
        public IEnumerable<TElement> ExecuteSqlQuery<TElement>(string sql, params object[] parameters)
        {
            return Db.Database.SqlQuery<TElement>(sql, parameters);
        }

        /// <summary>
        /// 执行SQL查询返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>DataTable结果集</returns>
        public virtual DataTable GetDataTable(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(Db.Database.Connection.ConnectionString))
            {
                var dataTable = new DataTable();
                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        adapter.SelectCommand.Parameters.AddRange(parameters);
                    }
                    adapter.Fill(dataTable);
                }
                return dataTable;
            }
        }

        /// <summary>
        /// 执行SQL查询返回DataSet（多表结果）
        /// </summary>
        /// <param name="sql">SQL语句（可包含多个查询）</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>DataSet结果集</returns>
        public virtual DataSet GetDataSet(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(Db.Database.Connection.ConnectionString))
            {
                var dataSet = new DataSet();
                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        adapter.SelectCommand.Parameters.AddRange(parameters);
                    }
                    adapter.Fill(dataSet);
                }
                return dataSet;
            }
        }
        #endregion

        public async Task<IQueryable<T>> GetListAsync(Expression<Func<T, bool>> whereLambda)
        {
            return await Task.FromResult(DbSet.Where(whereLambda));
        }

        public async Task<(IQueryable<T>, int)> GetListByPageAsync<S>(Expression<Func<T, bool>> whereLambada, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, bool isAsc)
        {
            var query = DbSet.Where(whereLambada);
            var totalCount = await query.CountAsync();

            var queryResult = isAsc
                ? query.OrderBy(orderBy).Skip(pageSize * (pageIndex - 1)).Take(pageSize)
                : query.OrderByDescending(orderBy).Skip(pageSize * (pageIndex - 1)).Take(pageSize);

            return (queryResult, totalCount);
        }

        public async Task<int> GetRecordCountAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.CountAsync(predicate);
        }

        public async Task<int> AddAsync(T entity)
        {
            DbSet.Add(entity);
            return await SaveChangesAsync();
        }

        public async Task<int> AddListAsync(params T[] entities)
        {
            foreach (var entity in entities)
            {
                DbSet.Add(entity);
            }
            return await SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(T entity)
        {
            DbSet.Remove(entity);
            return await SaveChangesAsync();
        }

        public async Task<int> DeleteByAsync(Expression<Func<T, bool>> whereLambda)
        {
            var entities = await DbSet.Where(whereLambda).ToListAsync();
            foreach (var entity in entities)
            {
                DbSet.Remove(entity);
            }
            return await SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            Db.Entry(entity).State = EntityState.Modified;
            return await SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T entity, string[] propertys)
        {
            var entry = Db.Entry(entity);
            entry.State = EntityState.Unchanged;

            foreach (var property in propertys)
            {
                entry.Property(property).IsModified = true;
            }

            return await SaveChangesAsync() > 0;
        }

        public async Task<int> UpdateListAsync(params T[] entities)
        {
            foreach (var entity in entities)
            {
                Db.Entry(entity).State = EntityState.Modified;
            }
            return await SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Db.SaveChangesAsync();
        }

        public async Task<IEnumerable<TElement>> ExecuteSqlQueryAsync<TElement>(string sql, params object[] parameters)
        {
            return await Db.Database.SqlQuery<TElement>(sql, parameters).ToListAsync().ConfigureAwait(false);
        }
    }
}
