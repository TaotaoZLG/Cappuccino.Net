using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Cappuccino.DataAccess
{
    /// <summary>
    /// Dapper通用操作类（与EF6共用连接字符串）
    /// </summary>
    public class DapperRepository
    {
        /// <summary>
        /// 获取数据库连接（复用EF6的连接字符串）
        /// </summary>
        private static IDbConnection GetConnection()
        {
            // 读取EF6的连接字符串（名称与Web.config中一致）
            string connStr = ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString;
            var conn = new SqlConnection(connStr);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            return conn;
        }

        /// <summary>
        /// 执行查询，返回列表（支持任意类型，仅映射存在的字段）
        /// </summary>
        public static List<T> Query<T>(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.Query<T>(sql, param).AsList();
            }
        }

        /// <summary>
        /// 执行分页查询（适配SQL Server）
        /// </summary>
        /// <typeparam name="T">返回类型（实体类/DTO/匿名类型）</typeparam>
        /// <param name="sql">核心查询SQL（无分页、无排序）</param>
        /// <param name="sortField">排序字段</param>
        /// <param name="sortOrder">排序方向（ASC/DESC）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="param">参数（防SQL注入）</param>
        /// <param name="totalCount">总条数</param>
        /// <returns>分页数据</returns>
        public static List<T> QueryPage<T>(string sql, string sortField, string sortOrder, int pageSize, int pageIndex, object param, out int totalCount)
        {
            // 1. 查询总条数
            string countSql = $"SELECT COUNT(1) FROM ({sql}) AS TempCount";
            using (var conn = GetConnection())
            {
                totalCount = conn.ExecuteScalar<int>(countSql, param);
            }

            // 2. 拼接分页SQL（SQL Server 2012+支持OFFSET/FETCH）
            string orderBy = string.IsNullOrEmpty(sortField) ? "ORDER BY CreateTime DESC" : $"ORDER BY {sortField} {sortOrder}";
            string pageSql = $@"
                {sql}
                {orderBy}
                OFFSET {(pageIndex - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY
            ";

            // 3. 执行分页查询（Dapper自动映射存在的字段，缺失字段赋值为null）
            using (var conn = GetConnection())
            {
                return conn.Query<T>(pageSql, param).AsList();
            }
        }

        /// <summary>
        /// 执行增删改操作
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