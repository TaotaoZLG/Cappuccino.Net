using System.ComponentModel;

namespace Cappuccino.Common.Enum
{
    /// <summary>
    ///  启用标志枚举
    /// </summary>
    public enum EnabledMarkEnum
    {
        [Description("有效")]
        Valid = 1,
        [Description("无效")]
        Invalid = 0,
    }
}
