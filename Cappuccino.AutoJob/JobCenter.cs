using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Log;
using Cappuccino.Entity.System;
using Cappuccino.IBLL.System;
using Quartz;

namespace Cappuccino.AutoJob
{
    public class JobCenter
    {
        private readonly IJobScheduler _jobScheduler;
        private readonly ISysAutoJobService _sysAutoJobService;

        public JobCenter(ISysAutoJobService sysAutoJobService, IJobScheduler jobScheduler)
        {
            _sysAutoJobService = sysAutoJobService;
            _jobScheduler = jobScheduler;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                List<SysAutoJobEntity> jobList = _sysAutoJobService.GetList(x => x.JobStatus == 1).ToList();
                if (jobList.Any())
                {
                    // 并行添加任务以提高效率
                    //var tasks = jobList.Select(job => _jobScheduler.AddScheduleJob(job)).ToList();
                    //await Task.WhenAll(tasks);

                    foreach (var obj in jobList)
                    {
                        await _jobScheduler.AddScheduleJob(obj);
                    }

                    // 启动调度器（只启动一次）
                    //await _jobScheduler.StartAsync();

                    Log4netHelper.Info($"共加载了 {jobList.Count} 个定时任务");
                }
                else
                {
                    Log4netHelper.Info("没有启用的定时任务");
                }
            });
        }

        public async Task StartAsync()
        {
            List<SysAutoJobEntity> jobList = _sysAutoJobService.GetList(x => x.JobStatus == 1).ToList();
            if (jobList.Any())
            {
                // 并行添加任务以提高效率
                //var tasks = jobList.Select(job => _jobScheduler.AddScheduleJob(job)).ToList();
                //await Task.WhenAll(tasks);

                foreach (var obj in jobList)
                {
                    await _jobScheduler.AddScheduleJob(obj);
                }

                // 启动调度器
                //await _jobScheduler.StartAsync();

                Log4netHelper.Info($"共加载了 {jobList.Count} 个定时任务");
            }
            else
            {
                Log4netHelper.Info("没有启用的定时任务");
            }
        }
    }
}