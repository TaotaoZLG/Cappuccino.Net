using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Cappuccino.DataAccess;
using Cappuccino.Entity;

namespace Cappuccino.DataAccess.Repository
{
    /// <summary>
    /// 通用仓储实现（EF6）
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class Repository<T> : IRepository<T> where T : BaseEntity, new()
    {
        /// <summary>
        /// EF上下文（线程唯一）
        /// </summary>
        protected readonly DbContext _dbContext;

        /// <summary>
        /// EF DbSet
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        public Repository(EfDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        #region 基础查询
        public IQueryable<T> GetList(Expression<Func<T, bool>> whereLambda)
        {
            return _dbSet.Where(whereLambda);
        }

        public IQueryable<T> GetListByPage<S>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, S>> orderBy,
            int pageSize, int pageIndex, out int totalCount, bool isAsc = true)
        {
            var query = _dbSet.Where(whereLambda);
            totalCount = query.Count();

            // 排序 + 分页
            query = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<T> GetListByPage(Expression<Func<T, bool>> whereLambda, string sortField, string sortOrder,
            int pageSize, int pageIndex, out int totalCount, params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.Where(whereLambda);

            // 关联查询 Include
            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            totalCount = query.Count();

            // 动态排序（基于字段名）
            var parameter = Expression.Parameter(typeof(T), "t");
            var property = Expression.Property(parameter, sortField);
            var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(property, typeof(object)), parameter);
            query = sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(lambda)
                : query.OrderByDescending(lambda);

            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IEnumerable<T> GetListByPage(string sql, string sortField, string sortOrder, int pageSize, int pageIndex)
        {
            // 原生SQL分页（适配SQL Server）
            var pagedSql = $@"
                SELECT * FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY {sortField} {sortOrder}) AS RowNum, * 
                    FROM ({sql}) AS Temp
                ) AS Paged 
                WHERE RowNum BETWEEN {(pageIndex - 1) * pageSize + 1} AND {pageIndex * pageSize}";
            return _dbContext.Database.SqlQuery<T>(pagedSql);
        }

        public int GetRecordCount(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Count(predicate);
        }
        #endregion

        #region 增删改
        public int Insert(T entity)
        {
            _dbSet.Add(entity);
            return _dbContext.SaveChanges();
        }

        public int Insert(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
            return _dbContext.SaveChanges();
        }

        public int Delete(T entity)
        {
            _dbSet.Attach(entity);
            _dbSet.Remove(entity);
            return _dbContext.SaveChanges();
        }

        public int DeleteBy(Expression<Func<T, bool>> whereLambda)
        {
            var list = _dbSet.Where(whereLambda).ToList();
            _dbSet.RemoveRange(list);
            return _dbContext.SaveChanges();
        }

        public bool Update(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        public bool Update(T entity, string[] propertys)
        {
            _dbSet.Attach(entity);
            var entry = _dbContext.Entry(entity);
            entry.State = EntityState.Unchanged;

            // 只更新指定字段
            foreach (var prop in propertys)
            {
                entry.Property(prop).IsModified = true;
            }
            return _dbContext.SaveChanges() > 0;
        }

        public int UpdateList(params T[] entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            return _dbContext.SaveChanges();
        }
        #endregion

        #region 异步方法
        public async Task<IQueryable<T>> GetListAsync(Expression<Func<T, bool>> whereLambda)
        {
            // 异步查询仅封装Task，IQueryable本身延迟执行
            return await Task.FromResult(_dbSet.Where(whereLambda));
        }

        public async Task<int> InsertAsync(T entity)
        {
            _dbSet.Add(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> InsertAsync(params T[] entities)
        {
            _dbSet.AddRange(entities);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(T entity)
        {
            _dbSet.Attach(entity);
            _dbSet.Remove(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteByAsync(Expression<Func<T, bool>> whereLambda)
        {
            var list = await _dbSet.Where(whereLambda).ToListAsync();
            _dbSet.RemoveRange(list);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T entity, string[] propertys)
        {
            _dbSet.Attach(entity);
            var entry = _dbContext.Entry(entity);
            entry.State = EntityState.Unchanged;

            foreach (var prop in propertys)
            {
                entry.Property(prop).IsModified = true;
            }
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<int> UpdateListAsync(params T[] entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            return await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}