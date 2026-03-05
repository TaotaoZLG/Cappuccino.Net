using System;
using System.Linq;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysUserService : BaseService<SysUserEntity>, ISysUserService
    {
        #region 依赖注入
        ISysUserDao dao;
        public SysUserService(ISysUserDao dao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        public bool CheckLogin(string loginName, string loginPassword)
        {
            var user = dao.GetList(x => x.UserName == loginName && x.UserStatus == (int)EnabledMarkEnum.Valid).SingleOrDefault(x => x.UserName == loginName);
            if (user == null)
            {
                return false;
            }
            else
            {
                string dbPwdHash = user.PasswordHash;
                string salt = user.PasswordSalt;
                string userPwdHash = Md5Utils.EncryptTo32(salt + loginPassword);
                return dbPwdHash == userPwdHash;
            }
        }

        public bool ModifyUserPwd(int userId, ChangePasswordModel viewModel)
        {
            string salt = VerifyCodeUtils.CreateVerifyCode(5);
            string passwordHash = Md5Utils.EncryptTo32(salt + viewModel.Password);
            var user = dao.GetList(x => x.Id == userId).FirstOrDefault();
            user.PasswordSalt = salt;
            user.PasswordHash = passwordHash;
            user.UpdateUserId = userId;
            user.UpdateTime = DateTime.Now;
            return dao.SaveChanges() >= 1;
        }
    }
}
