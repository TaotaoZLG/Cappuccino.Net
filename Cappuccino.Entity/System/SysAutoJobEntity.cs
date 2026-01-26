using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    /// <summary>
    /// 任务计划实体
    /// </summary>
    [Table("SysAutoJob")]
    public class SysAutoJobEntity : BaseEntity
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 任务分组
        /// </summary>
        public string JobGroup { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 任务类型（1-程序集 2-HTTP链接）
        /// </summary>
        public int TaskType { get; set; } = 1;

        /// <summary>
        /// HTTP链接（当TaskType=2时使用）
        /// </summary>
        public string HttpUrl { get; set; }

        /// <summary>
        /// HTTP请求方法（GET/POST）
        /// </summary>
        public string HttpMethod { get; set; } = "GET";

        /// <summary>
        /// HTTP请求参数
        /// </summary>
        public string HttpParams { get; set; }

        /// <summary>
        /// 工作类名（包含命名空间）
        /// </summary>
        public string JobClassName { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// 任务状态（0-停止 1-运行）
        /// </summary>
        public int? JobStatus { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? LastExecuteTime { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextExecuteTime { get; set; }
    }
}
