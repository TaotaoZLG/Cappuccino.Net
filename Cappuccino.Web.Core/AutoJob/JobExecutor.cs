using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Log;
using Cappuccino.Entity.System;
using Cappuccino.IBLL.System;
using Cappuccino.Web.Core.AutoJob;
using Quartz;

namespace Cappuccino.Common.AutoJob
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
            SysAutoJobEntity job = _jobService.GetList(x => x.Id == jobId).FirstOrDefault();

            if (job == null) return;

            // 初始化日志信息
            SysAutoJobLogEntity jobLogEntity = new SysAutoJobLogEntity();
            jobLogEntity.JobId = jobId;
            jobLogEntity.JobName = job.JobName;
            jobLogEntity.JobGroup = job.JobGroup;
            jobLogEntity.StartTime = DateTime.Now;

            try
            {
                // 反射执行任务类
                var assembly = Assembly.GetExecutingAssembly();
                var jobType = assembly.GetType(job.JobType);
                if (jobType == null)
                {
                    throw new Exception($"未找到任务类型：{job.JobType}");
                }

                // 执行任务（假设任务类实现了IJobTask接口）
                var task = (IJobScheduler)Activator.CreateInstance(jobType);
                var result = await task.Execute();

                // 更新日志成功信息
                jobLogEntity.ExecuteStatus = 1;
                jobLogEntity.ExecuteResult = result;
                jobLogEntity.Exception = null;
                jobLogEntity.ExecuteDuration = (int)(DateTime.Now - jobLogEntity.StartTime).TotalMilliseconds;

                // 更新任务最后执行时间
                job.LastExecuteTime = DateTime.Now;
                job.NextExecuteTime = context.NextFireTimeUtc?.LocalDateTime;
                await _jobService.UpdateAsync(job);
            }
            catch (Exception ex)
            {
                // 记录异常信息
                jobLogEntity.ExecuteStatus = 0;
                jobLogEntity.ExecuteResult = "执行失败";
                jobLogEntity.Exception = ex.ToString();
                Log4netHelper.Error($"任务执行异常：{ex.Message}", ex);
            }
            finally
            {
                jobLogEntity.EndTime = DateTime.Now;
                await _jobLogService.WriteJobLogAsync(jobLogEntity);
            }
        }
    }
}
