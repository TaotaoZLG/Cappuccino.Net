using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Common.Enum
{
    public enum UploadFileType
    {
        [Description("头像")]
        Portrait = 1,

        [Description("新闻图片")]
        News = 2,

        [Description("导入的文件")]
        Import = 10
    }
}
