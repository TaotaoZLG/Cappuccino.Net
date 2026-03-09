using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Entity
{
    [Table("SysFile")]
    public class SysFileEntity : BaseEntity
    {
        /// <summary>
        /// 文件关联Id
        /// </summary>
        public long ObjectId { get; set; }
        /// <summary>
        /// 原始文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// 文件大小 (字节)
        /// </summary>
        public int FileSizeBytes { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 文件存储路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
