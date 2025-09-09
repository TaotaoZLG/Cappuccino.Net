using System;
using System.Linq;
using System.Reflection;

namespace Cappuccino.Common.Reflection
{
    public class EntityScanner
    {
        /// <summary>
        /// 获取所有继承自Entity的实体类型
        /// </summary>
        /// <returns></returns>
        public static Type[] GetAllEntityTypes()
        {
            // 加载实体所在的程序集
            Assembly entityAssembly = Assembly.Load("Cappuccino.Entity");

            // 筛选：非抽象类、继承自Entity、不是基类本身
            return entityAssembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsClass && t.Name == "SysLogOperateEntity")
                .ToArray();
        }
    }
}