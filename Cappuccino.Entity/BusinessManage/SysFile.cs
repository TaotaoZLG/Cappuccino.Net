using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Entity.BusinessManage
{
    [Table("SysFile")]
    public class SysFile : BaseEntity
    {
        /// <summary>
        /// 文件关联Id
        /// </summary>
        private int ObjectId { get; set; }
        /// <summary>
        /// 原始文件名
        /// </summary>
        private string FileName { get; set; }
        /// <summary>
        /// 文件扩展名
        /// </summary>
        private string FileExtension { get; set; }
        /// <summary>
        /// 文件大小 (字节)
        /// </summary>
        private int FileSizeBytes { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        private string FileType { get; set; }
        /// <summary>
        /// 文件存储路径
        /// </summary>
        private string FilePath { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        private string Remark { get; set; }
    }
}
