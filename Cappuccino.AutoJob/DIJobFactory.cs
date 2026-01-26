using System;
using Autofac;
using Quartz;
using Quartz.Spi;

namespace Cappuccino.AutoJob
{
    /// <summary>
    /// 支持依赖注入的Job工厂
    /// </summary>
    public class DIJobFactory : IJobFactory
    {
        private readonly IContainer _container;

        public DIJobFactory(IContainer container)
        {
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _container.Resolve(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            // 释放资源（如需要）
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}