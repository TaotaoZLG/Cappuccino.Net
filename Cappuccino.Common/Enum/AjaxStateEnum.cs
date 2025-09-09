using System.ComponentModel;

namespace Cappuccino.Common.Enum
{
    /// <summary>
    /// Ajax请求状态枚举
    /// </summary>
    public enum AjaxStateEnum
    {
        [Description("成功")]
        Sucess = 0,
        [Description("失败")]
        Error = 1,
        [Description("未登录")]
        NoLogin = 2,
        [Description("没有权限")]
        NoPermission = 3
    }
}
