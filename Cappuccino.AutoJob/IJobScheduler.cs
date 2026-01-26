using System.Threading.Tasks;
using Cappuccino.Entity;

namespace Cappuccino.AutoJob
{
    /// <summary>
    /// 调度器接口
    /// </summary>
    public interface IJobScheduler
    {
        /// <summary>
        /// 同步方式启动调度器
        /// </summary>
        void Start();

        /// <summary>
        /// 异步方式启动调度器
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 关闭调度器
        /// </summary>
        /// <param name="waitForCompletion">是否等待任务完成</param>
        Task ShutdownAsync(bool waitForCompletion = false);

        /// <summary>
        /// 添加一个定时任务
        /// </summary>
        /// <typeparam name="T">任务类型（需实现 IJob 接口）</typeparam>
        /// <param name="jobName">任务名称</param>
        /// <param name="groupName">任务组名</param>
        /// <param name="cronExpression">Cron 表达式</param>
        /// <param name="jobData">任务参数</param>
        Task<bool> AddScheduleJob(SysAutoJobEntity jobEntity);

        /// <summary>
        /// 暂停指定任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="groupName">任务组名</param>
        Task<bool> PauseJob(string jobName, string groupName);

        /// <summary>
        /// 恢复指定任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="groupName">任务组名</param>
        Task<bool> ResumeJob(string jobName, string groupName);

        /// <summary>
        /// 删除指定任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="groupName">任务组名</param>
        Task<bool> DeleteJob(string jobName, string groupName);

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="groupName">任务组名</param>
        Task<bool> TriggerJob(string jobName, string groupName);
    }
}
