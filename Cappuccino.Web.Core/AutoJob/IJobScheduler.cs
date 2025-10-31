﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace Cappuccino.Web.Core.AutoJob
{
    /// <summary>
    /// 调度器接口
    /// </summary>
    public interface IJobScheduler
    {
        /// <summary>
        /// 启动调度器
        /// </summary>
        Task Start();

        /// <summary>
        /// 关闭调度器
        /// </summary>
        void Shutdown(bool waitForJobsToComplete = false);

        /// <summary>
        /// 添加一个定时任务
        /// </summary>
        /// <typeparam name="T">任务类型（需实现 IJob 接口）</typeparam>
        /// <param name="jobName">任务名称</param>
        /// <param name="groupName">任务组名</param>
        /// <param name="cronExpression">Cron 表达式</param>
        /// <param name="jobData">任务参数</param>
        Task<bool> ScheduleJob(string jobName, string groupName, string cronExpression);

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

        Task<bool> TriggerJob(string jobName, string groupName);
    }
}
