using System.Collections.Generic;

namespace Cappuccino.Web.Models
{
    public class Pager
    {
        public static dynamic Paging(IEnumerable<dynamic> list, long total)
        {
            return new { code = 0, msg = "", count = total, data = list };
        }
    }
}