using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    [Table("SysActionMenu")]
    public class SysActionMenuEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; set; }

        public SysActionEntity SysAction { get; set; }
    }
}
