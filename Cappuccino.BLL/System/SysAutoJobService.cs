using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.AutoJob;
using Cappuccino.Common.Log;
using Cappuccino.Entity.System;
using Cappuccino.IBLL.System;
using Cappuccino.IDAL;
using Cappuccino.IDAL.System;

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
        public async Task<bool> StartJob(int id)
        {
            try
            {
                // 1. 查询任务
                var jobEntity = await Task.Run(() => _autoJobDao.GetList(x => x.Id == id).FirstOrDefault());
                if (jobEntity == null)
                {
                    Log4netHelper.Error($"任务ID:{id}不存在");
                    return false;
                }

                // 3. 调用调度器启动任务
                bool result = await _jobScheduler.AddScheduleJob(jobEntity);
                if (!result)
                {
                    Log4netHelper.Error($"调度器启动任务ID:{id}失败");
                    return false;
                }

                // 4. 更新数据库状态
                jobEntity.JobStatus = 1; // 标记为运行中
                jobEntity.LastExecuteTime = DateTime.Now;
                jobEntity.UpdateTime = DateTime.Now;
                _autoJobDao.Update(jobEntity);

                return result;
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
        public async Task<bool> StopJob(int id)
        {
            try
            {
                SysAutoJobEntity jobEntity = _autoJobDao.GetList(x => x.Id == id).FirstOrDefault();
                if (jobEntity == null)
                {
                    Log4netHelper.Error($"任务ID:{id}不存在");
                    return false;
                }

                // 调用调度器停止任务
                bool schedulerResult = await _jobScheduler.PauseJob(jobEntity.JobName, jobEntity.JobGroup);
                if (!schedulerResult)
                {
                    Log4netHelper.Error($"调度器停止任务ID:{id}失败");
                    return false;
                }

                // 更新数据库状态
                jobEntity.JobStatus = 0; // 标记为停止状态
                jobEntity.LastExecuteTime = DateTime.Now;
                jobEntity.UpdateTime = DateTime.Now;
                _autoJobDao.Update(jobEntity);

                return schedulerResult;
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
        public async Task<bool> ExecuteJob(int id)
        {
            try
            {
                SysAutoJobEntity jobEntity = _autoJobDao.GetList(x => x.Id == id).FirstOrDefault();
                if (jobEntity == null)
                {
                    Log4netHelper.Error($"任务ID:{id}不存在");
                    return false;
                }

                // 调用调度器立即执行
                bool executeResult = await _jobScheduler.TriggerJob(jobEntity.JobName, jobEntity.JobGroup);
                if (executeResult)
                {
                    // 记录最后执行时间（无论任务是否成功，仅记录触发状态）
                    jobEntity.LastExecuteTime = DateTime.Now;
                    _autoJobDao.Update(jobEntity);
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
