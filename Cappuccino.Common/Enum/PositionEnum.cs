using System.ComponentModel.DataAnnotations;

namespace Cappuccino.Common.Enum
{
    /// <summary>
    /// 显示位置枚举
    /// </summary>
    public enum PositionEnum
    {
        [Display(Name = "表内")]
        FormInside = 0,

        [Display(Name = "表外")]
        FormRightTop = 1,
    }
}
