namespace Cappuccino.Web.Models
{
    public class PageInfo
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数据量
        /// </summary>
        public int Limit { get; set; } = 15;

        /// <summary>
        /// 排序字段
        /// </summary>
        public string Field { get; set; } = "CreateTime";

        /// <summary>
        /// 排序方式
        /// </summary>
        public string Order { get; set; } = "DESC";
    }
}