using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace Cappuccino.Common.Util
{
    /// <summary>
    /// 数据传输对象
    /// </summary>
    public class TData
    {
        /// <summary>
        /// 操作结果，Status为0代表成功，1代表失败，其他的验证返回结果，可根据需要设置
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 提示信息或异常信息
        /// </summary>
        public string Message { get; set; }        
    }

    public class TData<T> : TData
    {
        /// <summary>
        /// 列表的记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }
    }
}
