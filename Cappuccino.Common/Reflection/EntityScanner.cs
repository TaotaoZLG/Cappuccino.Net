using System;
using System.ComponentModel.DataAnnotations.Schema;
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

            // 筛选：非抽象类、基于命名约定，排除BaseEntity
            return entityAssembly.GetTypes()
                .Where(t => !t.IsAbstract  // 非抽象类
                              && t.IsClass  // 是类类型
                              && t.Name.EndsWith("Entity", StringComparison.Ordinal)  // 类名以"Entity"结尾（符合实体命名约定）
                              && t.Name != "BaseEntity"     //排除基类
                              && t.GetCustomAttribute<TableAttribute>() != null) // 必须包含[Table]特性
                .ToArray();
        }
    }
}