using System.Data.Entity;
using System.Reflection;
using Cappuccino.Common.Reflection;

namespace Cappuccino.DataAccess
{
    /// <summary>
    /// EF上下文类
    /// </summary>
    public class EfDbContext : DbContext
    {
        public EfDbContext() : base("sqlconn") 
        {
            // 禁用数据库初始化器
            Database.SetInitializer<EfDbContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 自动注册所有实体类（继承自Entity）
            var entityTypes = EntityScanner.GetAllEntityTypes();
            foreach (var entityType in entityTypes)
            {
                // 直接调用非泛型重载：RegisterEntityType(Type)
                modelBuilder.RegisterEntityType(entityType);
            }

            // 加载程序集中的 Fluent API 配置（确保存在有效配置类）
            modelBuilder.Configurations.AddFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
