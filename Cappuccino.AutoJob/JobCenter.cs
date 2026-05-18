using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cappuccino.Common.Log;
using Cappuccino.Entity;
using Cappuccino.IBLL;

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
            try
            {
                List<SysAutoJobEntity> jobList = _sysAutoJobService.GetList(x => x.JobStatus == 1).ToList();
                if (jobList.Any())
                {
                    // 同步添加任务
                    foreach (var obj in jobList)
                    {
                        _jobScheduler.AddScheduleJob(obj).Wait(); // 同步等待
                    }

                    // 启动调度器（只启动一次）
                    _jobScheduler.Start();

                    Log4netHelper.Info($"共加载了 {jobList.Count} 个定时任务");
                }
                else
                {
                    Log4netHelper.Info("没有启用的定时任务");
                }
            }
            catch (Exception ex)
            {
                Log4netHelper.Error("启动定时任务失败", ex);
            }
        }

        public async Task StartAsync()
        {
            List<SysAutoJobEntity> jobList = _sysAutoJobService.GetList(x => x.JobStatus == 1).ToList();
            if (jobList.Any())
            {
                foreach (var obj in jobList)
                {
                    await _jobScheduler.AddScheduleJob(obj);
                }

                // 启动调度器
                _jobScheduler.Start(); // 或者实现 StartAsync

                Log4netHelper.Info($"共加载了 {jobList.Count} 个定时任务");
            }
            else
            {
                Log4netHelper.Info("没有启用的定时任务");
            }
        }
    }
}