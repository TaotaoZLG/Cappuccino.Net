using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    /// <summary>
    /// 操作日志
    /// </summary>
    [Table("SysLogOperate")]
    public class SysLogOperateEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 操作标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 业务类型（如ADD/EDIT/DELETE）
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// 请求方法（GET/POST等）
        /// </summary>
        public string RequestMethod { get; set; }

        /// <summary>
        /// 调用的方法名
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 操作URL
        /// </summary>
        public string OperateUrl { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string RequestParam { get; set; }

        /// <summary>
        /// 请求体
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// 响应体
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// IP所在城市
        /// </summary>
        public string IPAddressName { get; set; }

        /// <summary>
        /// 操作人账号
        /// </summary>
        public string OperateName { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        public string SystemOs { get; set; }

        /// <summary>
        /// 浏览器
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}