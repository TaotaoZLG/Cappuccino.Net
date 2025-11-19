using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Common.Util
{
    public class TData
    {
        /// <summary>
        /// 操作结果，Status为1代表成功，0代表失败，其他的验证返回结果，可根据需要设置
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 提示信息或异常信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 扩展Message
        /// </summary>
        public string Description { get; set; }
    }
}
