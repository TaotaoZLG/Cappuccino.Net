using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task Start()
        {
            List<SysAutoJobEntity> jobList = _sysAutoJobService.GetList(x => x.JobStatus == 1).ToList();
            if (jobList.Any())
            {
                foreach (var obj in jobList)
                {
                    await _jobScheduler.AddScheduleJob(obj);
                }

                // 启动调度器
                await _jobScheduler.Start();
            }
        }
    }
}