using System.ComponentModel.DataAnnotations;

namespace Cappuccino.Common.Enum
{
    public enum OperateType
    {
        /// <summary>
        /// 其它
        /// </summary>
        [Display(Name = "其它")]
        Other = 0,

        /// <summary>
        /// 新增
        /// </summary>
        [Display(Name = "新增")]
        Add = 1,

        /// <summary>
        /// 删除
        /// </summary>
        [Display(Name = "删除")]
        Delete = 2,

        /// <summary>
        /// 编辑
        /// </summary>
        [Display(Name = "编辑")]
        Update = 3,

        /// <summary>
        /// 查询
        /// </summary>
        [Display(Name = "查询")]
        Search = 4,

        /// <summary>
        /// 导入
        /// </summary>
        [Display(Name = "导入")]
        Import = 5,

        /// <summary>
        /// 导出
        /// </summary>
        [Display(Name = "导出")]
        Export = 6,

        /// <summary>
        /// 授权
        /// </summary>
        [Display(Name = "授权")]
        Authorize = 7,

        /// <summary>
        /// 登录
        /// </summary>
        [Display(Name = "登录")]
        Login = 8,

        /// <summary>
        /// 退出
        /// </summary>
        [Display(Name = "退出")]
        Exit = 9,

        /// <summary>
        /// 异常
        /// </summary>
        [Display(Name = "异常")]
        Exception = 10
    }
}
