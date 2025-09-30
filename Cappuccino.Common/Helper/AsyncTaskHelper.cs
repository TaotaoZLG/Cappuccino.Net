using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Log;

namespace Cappuccino.Common.Helper
{
    public class AsyncTaskHelper
    {
        /// <summary>
        /// 开始异步任务
        /// </summary>
        /// <param name="action"></param>
        public static void StartTask(Action action)
        {
            try
            {
                Action newAction = () =>
                { };
                newAction += action;
                Task task = new Task(newAction);
                task.Start();
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(ex.Message, ex);
            }
        }
    }
}