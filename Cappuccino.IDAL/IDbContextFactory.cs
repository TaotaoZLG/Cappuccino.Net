using System.Data.Entity;

namespace Cappuccino.IDAL
{
    /// <summary>
    /// EF上下文的抽象工厂
    /// </summary>
    public interface IDbContextFactory
    {
        //获取当前上下文的唯一实例
        DbContext GetCurrentThreadInstance();
    }
}
