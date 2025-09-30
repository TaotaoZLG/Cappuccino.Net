﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    /// <summary>
    /// 字典详情
    /// </summary>
    public class SysDictDetailEntity : BaseEntity
    {
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 样式
        /// </summary>
        public string ListClass { get; set; }

        /// <summary>
        /// 分类主键
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        public virtual SysDictEntity SysDict { get; set; }
    }
}
