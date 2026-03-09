namespace Cappuccino.Model
{
    public class UserActionModel
    {
        public long Id { get; set; }
        public long ParentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// 状态 0.默认 1.启用 2.禁用
        /// </summary>
        public int Status { get; set; }

    }

    public class UserActionSaveModel : UserActionModel
    {
        public string LAY_TABLE_INDEX { get; set; }
        public string pid { get; set; }
        public bool isParent { get; set; }
    }
}
