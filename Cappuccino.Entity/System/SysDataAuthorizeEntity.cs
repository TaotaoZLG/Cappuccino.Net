using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Entity
{
    /// <summary>
    /// 数据权限实体类
    /// </summary>
    [Table("SysDataAuthorize")]
    public class SysDataAuthorizeEntity: BaseCreateEntity
    {
        /// <summary>
        /// 数据ID（部门Id或用户Id）
        /// </summary>
        public int? DataId { get; set; }

        /// <summary>
        /// 数据类型（1-机构、2-部门、3-用户）
        /// </summary>
        public int? DataType { get; set; }

        /// <summary>
        /// 授权对象ID（角色Id或者用户Id）
        /// </summary>
        public int? AuthorizeId { get; set; }

        /// <summary>
        /// 授权类型(1角色 2用户)
        /// </summary>
        public int? AuthorizeType { get; set; }
    }
}
