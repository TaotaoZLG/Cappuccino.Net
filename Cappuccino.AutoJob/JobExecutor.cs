using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.AutoJob.Job;
using Cappuccino.Common.Log;
using Cappuccino.Entity.System;
using Cappuccino.IBLL.System;
using Quartz;

namespace Cappuccino.AutoJob
{
    /// <summary>
    /// 任务执行器
    /// </summary>
    public class JobExecutor : IJob
    {
        private readonly ISysAutoJobService _jobService;
        private readonly ISysAutoJobLogService _jobLogService;

        // 依赖注入
        public JobExecutor(ISysAutoJobService jobService, ISysAutoJobLogService jobLogService)
        {
            _jobService = jobService;
            _jobLogService = jobLogService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.JobDetail.JobDataMap.GetInt("JobId");
            // 获取数据库中的任务
            SysAutoJobEntity jobEntity = _jobService.GetList(x => x.Id == jobId).FirstOrDefault();

            if (jobEntity == null) return;

            // 初始化日志信息
            SysAutoJobLogEntity jobLogEntity = new SysAutoJobLogEntity();
            jobLogEntity.JobId = jobId;
            jobLogEntity.JobName = jobEntity.JobName;
            jobLogEntity.JobGroup = jobEntity.JobGroup;
            jobLogEntity.StartTime = DateTime.Now;

            try
            {
                if (jobEntity.JobStatus != 1) // 任务未启用则直接记录日志
                {
                    jobLogEntity.ExecuteStatus = 0;
                    jobLogEntity.ExecuteResult = "任务未启用（状态为停止）";
                    return;
                }

                // 从所有程序集查找任务类型（解决跨程序集问题）
                var jobType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(asm => asm.GetType(jobEntity.JobType))
                    .FirstOrDefault(t => t != null);

                if (jobType == null)
                {
                    var errorMsg = $"未找到任务类型：{jobEntity.JobType}";
                    Log4netHelper.Info(errorMsg);
                    jobLogEntity.ExecuteStatus = 0;
                    jobLogEntity.ExecuteResult = errorMsg;
                    return;
                }

                // 实例化任务并执行（调用IJobTask的Execute方法）
                var jobInstance = (IJob)Activator.CreateInstance(jobType);
                await jobInstance.Execute(context);

                // 更新日志和任务状态
                jobLogEntity.ExecuteStatus = 1;
                jobLogEntity.ExecuteResult = "执行成功";

                jobEntity.LastExecuteTime = DateTime.Now;
                jobEntity.NextExecuteTime = context.NextFireTimeUtc?.LocalDateTime;
                await _jobService.UpdateAsync(jobEntity);
            }
            catch (Exception ex)
            {
                jobLogEntity.ExecuteStatus = 0;
                jobLogEntity.ExecuteResult = "执行失败";
                jobLogEntity.Exception = ex.ToString();
                Log4netHelper.Error($"任务执行异常：{ex.Message}", ex);
            }
            finally
            {
                jobLogEntity.EndTime = DateTime.Now;
                jobLogEntity.ExecuteDuration = (int)(jobLogEntity.EndTime - jobLogEntity.StartTime).TotalMilliseconds;
                await _jobLogService.WriteJobLogAsync(jobLogEntity);
            }
        }
    }
}
