using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    /// <summary>
    /// 任务执行日志实体
    /// </summary>
    [Table("SysAutoJobLog")]
    public class SysAutoJobLogEntity : BaseCreateEntity
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 任务分组
        /// </summary>
        public string JobGroup { get; set; }

        /// <summary>
        /// 执行状态（0-失败 1-成功）
        /// </summary>
        public int ExecuteStatus { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public string ExecuteResult { get; set; }

        /// <summary>
        /// 执行时长（毫秒）
        /// </summary>
        public int ExecuteDuration { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Exception { get; set; }
    }
}
