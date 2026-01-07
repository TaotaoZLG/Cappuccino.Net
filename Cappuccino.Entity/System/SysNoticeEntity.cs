using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Entity
{
    [Table("SysNotice")]
    public class SysNoticeEntity : BaseEntity
    {
        public string NoticeTitle { get; set; }

        public string NoticeContents { get; set; }

        public int NoticeType { get; set; }

        public int NoticeSender { get; set; }

        public int NoticeAccept { get; set; }

        public int SortCode { get; set; }

        public string Remark { get; set; }
    }
}
