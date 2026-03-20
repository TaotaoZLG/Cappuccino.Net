using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Cappuccino.Common;
using Cappuccino.Common.Helper;
using Cappuccino.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cappuccino.Entity
{
    public class BaseField
    {
        /// <summary>
        /// 所有表的主键
        /// long返回到前端js的时候，会丢失精度，所以转成字符串
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [JsonConverter(typeof(LongToStringConverter))]
        public virtual long Id { get; set; }

        /// <summary>
        /// 公共雪花ID赋值逻辑
        /// </summary>
        public virtual void CreateId()
        {
            this.Id = IdGeneratorHelper.Instance.NextId();
        }
    }

    /// <summary>
    /// 实体基类
    /// </summary>
    public class BaseEntity : BaseField
    {
        /// <summary>
        /// 创建用户主键
        /// </summary>
        public virtual long? CreateUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 修改用户主键
        /// </summary>
        public virtual long? UpdateUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime? UpdateTime { get; set; }

        public void Create()
        {
            base.CreateId();

            if (this.CreateTime == null)
            {
                this.CreateTime = DateTime.Now;
            }
            if (this.UpdateTime == null)
            {
                this.UpdateTime = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// 创建专用实体基类
    /// </summary>
    public class BaseCreateEntity : BaseField
    {
        /// <summary>
        /// 创建用户主键
        /// </summary>
        public virtual long? CreateUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 生成创建信息
        /// </summary>
        public void Create()
        {
            base.CreateId();

            if (this.CreateTime == null)
            {
                this.CreateTime = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// 修改专用实体基类
    /// </summary>
    public class BaseModifyEntity : BaseField
    {
        /// <summary>
        /// 修改用户主键
        /// </summary>
        public virtual long? UpdateUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 生成修改信息
        /// </summary>
        public void Modify()
        {
            if (this.UpdateTime == null)
            {
                this.UpdateTime = DateTime.Now;
            }
        }
    }
}