using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Common.Enum
{
    public enum CacheExpirationTypeEnum
    {
        /// <summary>
        /// 绝对过期（到点失效，适合记住密码）
        /// </summary>
        Absolute,
        /// <summary>
        /// 滑动过期（闲置失效，访问续期，适合普通登录）
        /// </summary>
        Sliding
    }
}
