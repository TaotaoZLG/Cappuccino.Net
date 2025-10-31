using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Log;
using Cappuccino.Entity.System;
using Cappuccino.IBLL.System;
using Cappuccino.IDAL;
using Cappuccino.IDAL.System;
using Cappuccino.Web.Core.AutoJob;

namespace Cappuccino.BLL.System
{
    public class SysAutoJobService : BaseService<SysAutoJobEntity>, ISysAutoJobService
    {
        private readonly ISysAutoJobDao _autoJobDao;
        private readonly IJobScheduler _jobScheduler;

        public SysAutoJobService(ISysAutoJobDao autoJobDao, IJobScheduler jobScheduler)
        {
            _autoJobDao = autoJobDao;
            _jobScheduler = jobScheduler;
            base.CurrentDao = autoJobDao;
            this.AddDisposableObject(this.CurrentDao);
        }

        /// <summary>
        /// 启动任务：更新数据库状态 + 调用调度器启动定时任务
        /// </summary>
        public bool StartJob(int id)
        {
            try
            {
                // 1. 查询任务
                var job = _autoJobDao.GetList(x => x.Id == id).FirstOrDefault();
                if (job == null)
                {
                    Log4netHelper.Error($"任务ID:{id}不存在");
                    return false;
                }

                // 3. 调用调度器启动任务
                bool schedulerResult = _jobScheduler.StartJob(job.JobName, job.CronExpression);
                if (!schedulerResult)
                {
                    Log4netHelper.Error($"调度器启动任务ID:{id}失败");
                    return false;
                }

                // 4. 更新数据库状态
                job.LastExecuteTime = DateTime.Now;
                job.UpdateTime = DateTime.Now;
                _autoJobDao.Update(job);

                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"启动任务ID:{id}失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 停止任务：更新数据库状态 + 调用调度器停止任务
        /// </summary>
        public bool StopJob(int id)
        {
            try
            {
                var job = _autoJobDao.GetList(x => x.Id == id).FirstOrDefault();
                if (job == null)
                {
                    Log4netHelper.Error($"任务ID:{id}不存在");
                    return false;
                }

                // 调用调度器停止任务
                bool schedulerResult = _jobScheduler.StopJob(job.JobName);
                if (!schedulerResult)
                {
                    Log4netHelper.Error($"调度器停止任务ID:{id}失败");
                    return false;
                }

                // 更新数据库状态
                job.LastExecuteTime = DateTime.Now;
                job.UpdateTime = DateTime.Now;
                _autoJobDao.Update(job);

                return true;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"停止任务ID:{id}失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 立即执行任务：触发调度器立即执行 + 记录执行时间
        /// </summary>
        public async Task<bool> ExecuteJobImmediately(int id)
        {
            try
            {
                SysAutoJobEntity job = _autoJobDao.GetList(x => x.Id == id).FirstOrDefault();
                if (job == null)
                {
                    Log4netHelper.Error($"任务ID:{id}不存在");
                    return false;
                }

                // 调用调度器立即执行
                bool executeResult = await _jobScheduler.TriggerJobImmediately(job.JobName);
                if (executeResult)
                {
                    // 记录最后执行时间（无论任务是否成功，仅记录触发状态）
                    job.LastExecuteTime = DateTime.Now;
                    _autoJobDao.Update(job);
                }

                return executeResult;
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"立即执行任务ID:{id}失败", ex);
                return false;
            }
        }
    }
}
