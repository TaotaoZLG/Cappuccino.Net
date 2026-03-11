using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Model
{
    public class SysTemplateModel : BaseEntity
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// 模板内容
        /// </summary>
        public string TemplateContent { get; set; }

        /// <summary>
        /// 模板类型
        /// </summary>
        public string TemplateType { get; set; }

        /// <summary>
        /// 模板状态：0-禁用，1-启用
        /// </summary>
        public int TemplateStatus { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? SortCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
