using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Cappuccino.DataAccess
{
    /// <summary>
    /// Dapper通用操作类（与EF6共用连接字符串）
    /// </summary>
    public class DapperHelper
    {
        private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString;

        /// <summary>
        /// 获取数据库连接（复用EF6的连接字符串）
        /// </summary>
        private static IDbConnection GetConnection()
        {
            var conn = new SqlConnection(_connectionString);
            // 注：无需主动Open，Dapper会自动打开/释放连接（更安全）
            return conn;
        }

        /// <summary>
        /// 执行查询，返回List<T>（已加载到内存，支持所有集合操作）
        /// </summary>
        /// <typeparam name="T">实体类/DTO/动态类型</typeparam>
        /// <param name="sql">原生SQL</param>
        /// <param name="param">参数（匿名对象/SqlParameter，防注入）</param>
        /// <returns>内存集合，无延迟执行风险</returns>
        public static List<T> Query<T>(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.Query<T>(sql, param).ToList();
            }
        }

        /// <summary>
        /// 执行分页查询，返回List<T>（核心优化：返回List）
        /// </summary>
        /// <typeparam name="T">返回类型（实体类/DTO/匿名类型）</typeparam>
        /// <param name="sql">核心查询SQL（无分页、无排序）</param>
        /// <param name="sortField">排序字段（建议传入带表别名的字段，如u.CreateTime）</param>
        /// <param name="sortOrder">排序方向（ASC/DESC）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="totalCount">总条数（输出参数）</param>
        /// <returns>分页数据（List<T>，已加载到内存）</returns>
        public static List<T> QueryPage<T>(string sql, string sortField, string sortOrder, int pageSize, int pageIndex, out int totalCount)
        {
            string countSql = $"SELECT COUNT(1) FROM ({sql}) AS TempCount";
            using (var conn = GetConnection())
            {
                totalCount = conn.ExecuteScalar<int>(countSql);
            }

            var safeSortField = string.IsNullOrEmpty(sortField) ? "CreateTime" : sortField;
            var safeSortOrder = (sortOrder ?? "DESC").ToUpper() == "ASC" ? "ASC" : "DESC";

            string orderBy = $"ORDER BY {safeSortField} {safeSortOrder}";
            string pageSql = $@"
                {sql}
                {orderBy}
                OFFSET {(pageIndex - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY
            ";

            using (var conn = GetConnection())
            {
                return conn.Query<T>(pageSql).ToList();
            }
        }

        /// <summary>
        /// 执行分页查询，返回List<T>（核心优化：返回List）
        /// </summary>
        /// <typeparam name="T">返回类型（实体类/DTO/匿名类型）</typeparam>
        /// <param name="sql">核心查询SQL（无分页、无排序）</param>
        /// <param name="sortField">排序字段（建议传入带表别名的字段，如u.CreateTime）</param>
        /// <param name="sortOrder">排序方向（ASC/DESC）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="param">参数（防SQL注入）</param>
        /// <param name="totalCount">总条数（输出参数）</param>
        /// <returns>分页数据（List<T>，已加载到内存）</returns>
        public static List<T> QueryPage<T>(string sql, string sortField, string sortOrder, int pageSize, int pageIndex, object param, out int totalCount)
        {
            string countSql = $"SELECT COUNT(1) FROM ({sql}) AS TempCount";
            using (var conn = GetConnection())
            {
                totalCount = conn.ExecuteScalar<int>(countSql, param);
            }

            var safeSortField = string.IsNullOrEmpty(sortField) ? "CreateTime" : sortField;
            var safeSortOrder = (sortOrder ?? "DESC").ToUpper() == "ASC" ? "ASC" : "DESC";

            string orderBy = $"ORDER BY {safeSortField} {safeSortOrder}";
            string pageSql = $@"
                {sql}
                {orderBy}
                OFFSET {(pageIndex - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY
            ";

            using (var conn = GetConnection())
            {
                return conn.Query<T>(pageSql, param).ToList();
            }
        }

        /// <summary>
        /// 超大数据集、流式处理（如导出 10 万条数据到 Excel）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryStream<T>(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                // 不调用ToList，直接返回枚举器，流式遍历
                foreach (var item in conn.Query<T>(sql, param))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 执行增删改操作，返回受影响行数
        /// </summary>
        public static int Execute(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.Execute(sql, param);
            }
        }
    }
}