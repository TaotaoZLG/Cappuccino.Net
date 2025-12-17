using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Model
{
    public class SysNoticeModel : BaseEntity
    {
        public string Title { get; set; }

        public string Contents { get; set; }

        public int Type { get; set; }

        public int Sender { get; set; }

        public int Accept { get; set; }

        public int SortCode { get; set; }

        public string Remark { get; set; }
    }
}
