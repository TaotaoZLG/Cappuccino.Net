using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Entity.Business
{
    [Table("SysFileRecognition")]
    public class SysFileRecognitionEntity: BaseCreateEntity
    {
        public string BatchId { get; set; } //批次Id
        public string FilePath { get; set; } // 有效文件路径
        public string FileType { get; set; } // 文件类型（身份证正/反、协议等）
        public string AIRecognitionResult { get; set; }
    }
}
