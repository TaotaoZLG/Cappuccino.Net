namespace Cappuccino.Model
{
    public class SysConfigModel : BaseEntity
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 参数键名
        /// </summary>
        public string ConfigKeys { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ConfigValue { get; set; }

        /// <summary>
        /// 参数类型（1：系统内置 2：自定义）
        /// </summary>
        public int ConfigType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
