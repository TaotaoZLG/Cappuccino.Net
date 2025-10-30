using System;
using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common.Extensions;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;
using Cappuccino.Web.Core;

namespace Cappuccino.BLL
{
    public class SysRoleService : BaseService<SysRoleEntity>, ISysRoleService
    {
        #region 依赖注入
        ISysRoleDao _roleDao;
        ISysActionDao _actionDao;
        private readonly ISysDataAuthorizeDao _dataAuthorizeDao;

        public SysRoleService(ISysRoleDao roleDao, ISysActionDao actionDao,ISysDataAuthorizeDao dataAuthorizeDao)
        {
            this._roleDao = roleDao;
            this._actionDao = actionDao;
            _dataAuthorizeDao = dataAuthorizeDao;
            base.CurrentDao = roleDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        public void Add(int id, List<DtreeResponse> dtrees)
        {
            List<int> actionIds = new List<int>();
            foreach (var item in dtrees)
            {
                actionIds.Add(Convert.ToInt32(item.NodeId));
            }
            var role = _roleDao.GetList(x => x.Id == id).FirstOrDefault();
            var actions = _actionDao.GetList(x => actionIds.Contains(x.Id)).ToList();
            //清空权限关系
            role.SysActions.Clear();
            foreach (var item in actions)
            {
                role.SysActions.Add(item);
            }
            _roleDao.Update(role);
            _roleDao.SaveChanges();
        }

        public void SavePermissions(int roleId, List<DtreeResponse> menuPermissions, List<DtreeResponse> dataPermissions)
        {
            // 1. 处理菜单权限（复用原有逻辑）
            var role = _roleDao.GetList(x => x.Id == roleId).FirstOrDefault();
            var actionIds = menuPermissions.Select(p => Convert.ToInt32(p.NodeId)).ToList();
            var actions = _actionDao.GetList(x => actionIds.Contains(x.Id)).ToList();

            role.SysActions.Clear();
            actions.ForEach(action => role.SysActions.Add(action));
            _roleDao.Update(role);

            // 2. 处理数据权限（先删后加，避免冗余）
            _dataAuthorizeDao.DeleteByRoleId(roleId); // 删除旧数据权限
            if (dataPermissions != null && dataPermissions.Any())
            {
                SysDataAuthorizeEntity authorizeEntity = new SysDataAuthorizeEntity();

                // 补充创建人、创建时间等公共字段
                dataPermissions.ForEach(dp =>
                {
                    authorizeEntity.DataId = dp.NodeId.ParseToInt();
                    authorizeEntity.AuthorizeId = roleId;
                    authorizeEntity.AuthorizeType = 1; // 1表示角色
                    authorizeEntity.DataType = dp.ParntId == "0" ? 2 : 3;
                    authorizeEntity.CreateUserId = UserManager.GetCurrentUserInfo().Id; // 实际应从当前登录用户获取
                    authorizeEntity.CreateTime = DateTime.Now;
                });

                _dataAuthorizeDao.Insert(authorizeEntity);
            }

            _roleDao.SaveChanges();
        }
    }
}
