using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.AutoJob;
using Cappuccino.Common.Log;
using Quartz;
using Quartz.Impl;

namespace Cappuccino.Web.Core.AutoJob
{
    /// <summary>
    /// 实现调度器接口
    /// </summary>
    public class JobScheduler : IJobScheduler
    {
        private static object lockHelper = new object();

        private static IScheduler _scheduler = null;

        public JobScheduler()
        {
            try
            {
                lock (lockHelper)
                {
                    ISchedulerFactory factory = new StdSchedulerFactory();
                    _scheduler = factory.GetScheduler().Result;
                    if (!_scheduler.IsStarted)
                    {
                        _scheduler.Start().Wait(); // 启动调度器
                        Log4netHelper.Info("Quartz调度器已启动");
                    }
                }
            }
            catch (Exception ex)
            {
                Log4netHelper.Error("初始化调度器失败", ex);
                throw;
            }
        }

        public Task<string> Execute()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 启动定时任务（基于Cron表达式）
        /// </summary>
        public bool StartJob(string jobKey, string cronExpression)
        {
            try
            {
                if (string.IsNullOrEmpty(jobKey) || string.IsNullOrEmpty(cronExpression))
                {
                    Log4netHelper.Error("任务标识或Cron表达式不能为空");
                    return false;
                }

                // 解析任务标识（格式：GroupName.JobName）
                var (groupName, jobName) = ParseJobKey(jobKey);
                var key = new JobKey(jobName, groupName);

                // 若任务已存在，先删除再重建
                if (_scheduler.CheckExists(key).Result)
                {
                    _scheduler.DeleteJob(key).Wait();
                }

                // 创建任务实例（实际执行逻辑在JobExecutor中）
                IJobDetail job = JobBuilder.Create<JobExecutor>()
                    .WithIdentity(key)
                    .Build();

                // 创建Cron触发器
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobName}Trigger", groupName)
                    .WithCronSchedule(cronExpression)
                    .Build();

                // 调度任务
                _scheduler.ScheduleJob(job, trigger).Wait();
                Log4netHelper.Info($"任务[{jobKey}]启动成功，Cron表达式：{cronExpression}");
                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"启动任务[{jobKey}]失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public bool StopJob(string jobKey)
        {
            try
            {
                var (groupName, jobName) = ParseJobKey(jobKey);
                var key = new JobKey(jobName, groupName);

                if (!_scheduler.CheckExists(key).Result)
                {
                    Log4netHelper.Warn($"任务[{jobKey}]不存在，无需停止");
                    return true;
                }

                bool result = _scheduler.DeleteJob(key).Result;
                if (result) Log4netHelper.Info($"任务[{jobKey}]已停止");
                return result;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"停止任务[{jobKey}]失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 异步启动定时任务（基于Cron表达式）
        /// </summary>
        public async Task<bool> StartJobAsync(string jobKey, string cronExpression)
        {
            try
            {
                if (string.IsNullOrEmpty(jobKey) || string.IsNullOrEmpty(cronExpression))
                {
                    Log4netHelper.Error("任务标识或Cron表达式不能为空");
                    return false;
                }

                // 解析任务标识（格式：GroupName.JobName）
                var (groupName, jobName) = ParseJobKey(jobKey);
                var key = new JobKey(jobName, groupName);

                // 若任务已存在，先删除再重建（使用await异步操作）
                if (await _scheduler.CheckExists(key))
                {
                    await _scheduler.DeleteJob(key);
                }

                // 创建任务实例（关联执行器）
                IJobDetail job = JobBuilder.Create<JobExecutor>()
                    .WithIdentity(key)
                    .Build();

                // 创建Cron触发器
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobName}Trigger", groupName)
                    .WithCronSchedule(cronExpression)
                    .Build();

                // 异步调度任务
                await _scheduler.ScheduleJob(job, trigger);
                Log4netHelper.Info($"任务[{jobKey}]启动成功，Cron表达式：{cronExpression}");
                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"启动任务[{jobKey}]失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 异步停止任务
        /// </summary>
        public async Task<bool> StopJobAsync(string jobKey)
        {
            try
            {
                var (groupName, jobName) = ParseJobKey(jobKey);
                var key = new JobKey(jobName, groupName);

                if (!await _scheduler.CheckExists(key))
                {
                    Log4netHelper.Warn($"任务[{jobKey}]不存在，无需停止");
                    return true;
                }

                // 异步删除任务
                bool result = await _scheduler.DeleteJob(key);
                if (result) Log4netHelper.Info($"任务[{jobKey}]已停止");
                return result;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"停止任务[{jobKey}]失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 立即触发任务执行（不影响原有定时计划）
        /// </summary>
        public async Task<bool> TriggerJobImmediately(string jobKey)
        {
            try
            {
                var (groupName, jobName) = ParseJobKey(jobKey);
                var key = new JobKey(jobName, groupName);

                if (!await _scheduler.CheckExists(key))
                {
                    Log4netHelper.Error($"任务[{jobKey}]不存在，无法立即执行");
                    return false;
                }

                await _scheduler.TriggerJob(key);
                Log4netHelper.Info($"任务[{jobKey}]已触发立即执行");
                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"立即执行任务[{jobKey}]失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 解析任务标识为组名和任务名
        /// </summary>
        private (string groupName, string jobName) ParseJobKey(string jobKey)
        {
            var parts = jobKey.Split('.');
            return parts.Length == 2 ? (parts[0], parts[1]) : ("DefaultGroup", jobKey);
        }
    }
}
