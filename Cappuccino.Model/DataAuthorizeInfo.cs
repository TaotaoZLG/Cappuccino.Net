using System.Collections.Generic;

namespace Cappuccino.Model
{
    public class DataAuthorizeInfo
    {
        public long DepartmentId { get; set; }

        public string UserIds { get; set; }

        /// <summary>
        /// 部门Id权限集合
        /// </summary>
        public HashSet<long> ChildrenDepartmentIdList { get; set; } = new HashSet<long>();

        /// <summary>
        /// 用户Id权限集合
        /// </summary>
        public HashSet<long> ChildrenUserIdList { get; set; } = new HashSet<long>();
    }
}
