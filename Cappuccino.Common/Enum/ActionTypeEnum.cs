using System.ComponentModel.DataAnnotations;

namespace Cappuccino.Common.Enum
{
    public enum ActionTypeEnum
    {
        [Display(Name = "菜单")]
        Menu = 0,
        [Display(Name = "按钮")]
        Button = 1,
    }
}
