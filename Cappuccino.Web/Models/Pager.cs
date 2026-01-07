using System.Collections.Generic;

namespace Cappuccino.Web.Models
{
    public class Pager
    {
        public static dynamic Paging(IEnumerable<dynamic> list, long total, string message = "查询成功")
        {
            return new { code = 0, msg = message, count = total, data = list };
        }
    }
}