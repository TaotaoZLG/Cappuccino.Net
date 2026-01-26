using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;

namespace Cappuccino.AutoJob.Job
{
    public class DatabaseBackupTask : IJobTask
    {
        private readonly string _connectionString = ConfigUtils.ConnSetting.GetValue("sqlconn").ConnectionString;

        // 备份文件保存路径
        private readonly string _backupPath = ConfigUtils.AppSetting.GetValue("DbBackupPath") ?? "~/App_Data/DbBackups";

        public async Task<TData> Start()
        {
            TData obj = new TData();
            try
            {
                Log4netHelper.Info($"开始执行数据库备份任务");

                // 确保备份目录存在
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string adjustedPath = _backupPath.Replace("~/", "");
                string physicalPath = Path.Combine(basePath, adjustedPath);
                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                // 生成备份文件名（包含时间戳）
                string fileName = $"Cappuccino_{DateTime.Now:yyyyMMddHHmmss}.bak";
                string filePath = Path.Combine(physicalPath, fileName);

                // 执行备份（SQL Server示例）
                bool backupSuccess = await BackupDatabaseAsync(filePath);
                if (backupSuccess)
                {
                    obj.Status = 1;
                    obj.Message = $"数据库备份成功，文件路径：{filePath}";
                    Log4netHelper.Info($"数据库备份成功，文件路径：{filePath}");

                    // 可选：清理旧备份文件（保留最近30天）
                    CleanupDbOldBackups(physicalPath, 30);
                }
                else
                {
                    obj.Status = 0;
                    obj.Message = $"数据库备份失败";
                    Log4netHelper.Error("数据库备份失败");
                }
            }
            catch (Exception ex)
            {
                Log4netHelper.Error("数据库备份任务执行异常", ex);
                throw;
            }
            return obj;
        }

        /// <summary>
        /// 执行数据库备份
        /// </summary>
        private async Task<bool> BackupDatabaseAsync(string filePath)
        {
            // 解析连接字符串获取数据库名称
            var connStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            string databaseName = connStringBuilder.InitialCatalog;

            // 使用SQL命令进行备份
            string backupCommand = $"BACKUP DATABASE [{databaseName}] TO DISK = '{filePath}' WITH INIT";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(backupCommand, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    return true;
                }
            }
        }


        /// <summary>
        /// 清理旧备份文件
        /// </summary>
        private void CleanupDbOldBackups(string path, int keepDays)
        {
            var cutoffDate = DateTime.Now.AddDays(-keepDays);
            var files = Directory.GetFiles(path, "Cappuccino_*.bak")
                                 .Select(f => new FileInfo(f))
                                 .Where(f => f.CreationTime < cutoffDate);

            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                    Log4netHelper.Info($"已删除旧备份文件：{file.FullName}");
                }
                catch (Exception ex)
                {
                    Log4netHelper.Error($"删除旧备份文件失败：{file.FullName}", ex);
                }
            }
        }
    }
}
