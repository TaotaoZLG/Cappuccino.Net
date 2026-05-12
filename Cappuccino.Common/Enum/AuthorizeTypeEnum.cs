using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cappuccino.Common.Enum
{
    public enum AuthorizeTypeEnum
    {
        /// <summary>
        /// 角色
        /// </summary>
        [Description("角色")]
        Role = 1,

        /// <summary>
        /// 用户
        /// </summary>
        [Description("用户")]
        User = 2,

        /// <summary>
        /// 部门
        /// </summary>
        [Description("部门")]
        Department = 3,
    }
}
