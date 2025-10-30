using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Web.Core.AutoJob
{
    /// <summary>
    /// 任务执行接口
    /// </summary>
    public interface IJobTask
    {
        Task<string> Execute();
    }
}
