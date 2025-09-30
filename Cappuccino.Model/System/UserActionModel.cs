namespace Cappuccino.Model.System
{
    public class UserActionModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// 状态 0.默认 1.启用 2.禁用
        /// </summary>
        public int Status { get; set; }

    }

    public class UserActionSaveViewModel : UserActionModel
    {
        public string LAY_TABLE_INDEX { get; set; }
        public string pid { get; set; }
        public bool isParent { get; set; }
    }
}
