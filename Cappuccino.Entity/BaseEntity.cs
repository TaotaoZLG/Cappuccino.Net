using System;

namespace Cappuccino.Entity
{
    public class BaseField
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }
    }

    /// <summary>
    /// 实体基类
    /// </summary>
    public class BaseEntity : BaseField
    {
        /// <summary>
        /// 创建用户主键
        /// </summary>
        public virtual int? CreateUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 修改用户主键
        /// </summary>
        public virtual int? UpdateUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 创建
    /// </summary>
    public class BaseCreateEntity : BaseField
    {
        /// <summary>
        /// 创建用户主键
        /// </summary>
        public virtual int? CreateUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
    }

    /// <summary>
    /// 修改
    /// </summary>
    public class BaseModifyEntity : BaseField
    {
        /// <summary>
        /// 修改用户主键
        /// </summary>
        public virtual int? UpdateUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
