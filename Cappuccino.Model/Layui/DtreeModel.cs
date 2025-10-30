using System.Collections.Generic;

namespace Cappuccino.Model
{
    public class DtreeModel
    {
        public DtreeStatus Status { get; set; }
        public List<DtreeData> Data { get; set; }
    }

    public class DtreeData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string ParentId { get; set; }
        public List<DtreeData> Children { get; set; } = new List<DtreeData>();
        public string CheckArr = "0";
    }

    public class DtreeStatus
    {
        public int Code { get; set; } = 200;
        public string Message { get; set; } = "操作成功";
    }

    public class DtreeResponse
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public string NodeId { get; set; }
        /// <summary>
        /// 父节点ID
        /// </summary>
        public string ParntId { get; set; }
        /// <summary>
        /// 节点内容
        /// </summary>
        public string Context { get; set; }
        /// <summary>
        /// 是否叶子节点
        /// </summary>
        public bool Leaf { get; set; }
        /// <summary>
        /// 层级
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// 节点展开状态
        /// </summary>
        public string Spread { get; set; }
        /// <summary>
        /// 节点标记
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 节点复选框选中状态
        /// </summary>
        public string Checked { get; set; }
        /// <summary>
        /// 节点复选框初始状态
        /// </summary>
        public string Initcheked { get; set; }
    }
}
