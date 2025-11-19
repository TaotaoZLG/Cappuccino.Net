using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Log;
using Cappuccino.Entity.System;
using Quartz;
using Quartz.Impl;
using Quartz.Util;

namespace Cappuccino.AutoJob
{
    /// <summary>
    /// 实现调度器接口（基于Quartz.NET）
    /// </summary>
    public class JobScheduler : IJobScheduler
    {
        private static object _lockHelper = new object();

        private readonly IScheduler _scheduler = null;

        /// <summary>
        /// 构造函数，初始化调度器
        /// </summary>
        public JobScheduler()
        {
            lock (_lockHelper)
            {
                // 创建默认的调度器工厂
                ISchedulerFactory factory = new StdSchedulerFactory();
                // 获取调度器实例
                _scheduler = factory.GetScheduler().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 启动调度器
        /// </summary>
        public async Task Start()
        {
            try
            {
                if (!_scheduler.IsStarted)
                {
                    await _scheduler.Start();
                    Log4netHelper.Info("调度器已启动");
                }
            }
            catch (Exception ex)
            {
                Log4netHelper.Info("初始化调度器失败", ex);
            }
        }

        /// <summary>
        /// 关闭调度器
        /// </summary>
        /// <param name="waitForJobsToComplete">是否等待任务完成后再关闭</param>
        public async Task ShutdownAsync(bool waitForCompletion = false)
        {
            if (_scheduler.IsStarted)
            {
                await _scheduler.Shutdown(waitForCompletion);
                Log4netHelper.Info("调度器已关闭");
            }
        }

        /// <summary>
        /// 添加定时任务（基于 Cron 表达式）
        /// </summary>
        public async Task<bool> AddScheduleJob(SysAutoJobEntity jobEntity)
        {
            try
            {
                // Cron表达式校验
                if (!CronExpression.IsValidExpression(jobEntity.CronExpression))
                {
                    Log4netHelper.Error($"Cron表达式无效：{jobEntity.CronExpression}");
                    return false;
                }

                if (jobEntity.StartTime == null)
                {
                    jobEntity.StartTime = DateTime.Now;
                }
                DateTimeOffset starRunTime = DateBuilder.NextGivenSecondDate(jobEntity.StartTime, 1);
                if (jobEntity.EndTime == null)
                {
                    jobEntity.EndTime = DateTime.MaxValue.AddDays(-1);
                }
                DateTimeOffset endRunTime = DateBuilder.NextGivenSecondDate(jobEntity.EndTime, 1);

                var jobKey = new JobKey(jobEntity.JobName, jobEntity.JobGroup);

                // 若任务已存在，先删除再重建
                if (await _scheduler.CheckExists(jobKey))
                {
                    await _scheduler.DeleteJob(jobKey);
                }

                // 构建JobDetail（关联JobExecutor）
                IJobDetail jobDetail = JobBuilder.Create<JobExecutor>()
                    .WithIdentity(jobKey)
                    .UsingJobData("JobId", jobEntity.Id)  // 传递任务ID
                    .Build();

                // 构建Cron触发器
                var trigger = TriggerBuilder.Create()
                    .StartAt(starRunTime)  // 支持开始时间
                    .EndAt(endRunTime)  // 支持结束时间
                    .WithIdentity($"{jobEntity.JobName}_trigger", jobEntity.JobGroup)
                    .WithCronSchedule(jobEntity.CronExpression) // 设置 Cron 表达式
                    .Build();

                // 调度任务
                await _scheduler.ScheduleJob(jobDetail, trigger);  //将创建的任务和触发器条件添加到创建的任务调度器当中
                //await _scheduler.Start();  //启动任务调度器
                Log4netHelper.Info($"任务 [{jobEntity.JobGroup}.{jobEntity.JobName}] 已添加，Cron 表达式：{jobEntity.CronExpression}");
                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"任务[{jobEntity.JobGroup}.{jobEntity.JobName}] 添加失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 暂停指定任务
        /// </summary>
        public async Task<bool> PauseJob(string jobName, string groupName)
        {
            try
            {
                JobKey jobKey = new JobKey(jobName, groupName);

                if (!await _scheduler.CheckExists(jobKey))
                {
                    Log4netHelper.Warn($"任务[{groupName}.{jobName}]不存在，无需停止");
                    return false;
                }

                await _scheduler.PauseJob(jobKey);
                Log4netHelper.Info($"任务[{groupName}.{jobName}]已暂停");
                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"任务[{groupName}.{jobName}]暂停失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 恢复指定任务
        /// </summary>
        public async Task<bool> ResumeJob(string jobName, string groupName)
        {
            try
            {
                JobKey jobKey = new JobKey(jobName, groupName);

                if (!await _scheduler.CheckExists(jobKey))
                {
                    Log4netHelper.Warn($"任务[{groupName}.{jobName}]不存在，无需恢复");
                    return false;
                }

                await _scheduler.ResumeJob(jobKey);
                Log4netHelper.Info($"任务[{groupName}.{jobName}]已恢复");
                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"任务[{groupName}.{jobName}]恢复失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 删除指定任务
        /// </summary>
        public async Task<bool> DeleteJob(string jobName, string groupName)
        {
            try
            {
                JobKey jobKey = new JobKey(jobName, groupName);

                if (!await _scheduler.CheckExists(jobKey))
                {
                    Log4netHelper.Warn($"任务[{groupName}.{jobName}]不存在，无需删除");
                    return false;
                }

                var isDeleted = await _scheduler.DeleteJob(jobKey);
                Log4netHelper.Info($"任务[{groupName}.{jobName}]已删除");
                return isDeleted;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"任务[{groupName}.{jobName}]恢复失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 立即触发指定任务执行
        /// </summary>
        /// <param name="jobKey">任务唯一标识（格式：GroupName.JobName）</param>
        /// <returns>是否触发成功</returns>
        public async Task<bool> TriggerJob(string jobName, string groupName)
        {
            try
            {
                JobKey jobKey = new JobKey(jobName, groupName);
                if (!await _scheduler.CheckExists(jobKey))
                {
                    Log4netHelper.Error($"任务[{jobKey}]不存在，无法立即执行");
                    return false; // 任务不存在
                }

                // 立即触发任务（不影响原有调度）
                await _scheduler.TriggerJob(jobKey);
                Log4netHelper.Info($"任务[{jobKey}]已触发立即执行");
                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Info($"任务[{groupName}.{jobName}]立即执行失败", ex);
                return false;
            }
        }
    }
}