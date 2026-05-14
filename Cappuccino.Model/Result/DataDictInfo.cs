using System.Collections.Generic;

namespace Cappuccino.Model
{
    public class DataDictInfo
    {
        /// <summary>
        /// 字典编码
        /// </summary>
        public string DictCode { get; set; }

        /// <summary>
        /// 字典名称
        /// </summary>
        public string DictName { get; set; }

        /// <summary>
        /// 字典详情列表
        /// </summary>
        public List<DataDictDetailInfo> DictInfo { get; set; }
    }

    public class DataDictDetailInfo
    {
        /// <summary>
        /// 字典项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 字典项值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 显示样式
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
