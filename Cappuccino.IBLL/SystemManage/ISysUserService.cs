using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysUserService : IBaseService<SysUserEntity>
    {
        bool CheckLogin(string loginName, string loginPassword);

        bool ModifyUserPwd(long userId, ChangePasswordModel viewModel);
    }
}
