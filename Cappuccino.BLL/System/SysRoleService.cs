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
            _roleDao.SaveChanges();

            // 2. 处理数据权限
            _dataAuthorizeDao.DeleteByRoleId(roleId); // 删除旧数据权限
            if (dataPermissions != null && dataPermissions.Any())
            {
                List<SysDataAuthorizeEntity> authorizeEntityList = new List<SysDataAuthorizeEntity>();
                int currentUserId = UserManager.GetCurrentUserInfo().Id;

                // 补充创建人、创建时间等公共字段
                foreach (var dp in dataPermissions)
                {
                    SysDataAuthorizeEntity authorizeEntity = new SysDataAuthorizeEntity
                    {
                        DataId = dp.NodeId.ParseToInt(),
                        AuthorizeId = roleId,
                        AuthorizeType = 1, // 1表示角色
                        DataType = dp.Type == "dept" ? 2 : 3,
                        CreateUserId = currentUserId,
                        CreateTime = DateTime.Now
                    };
                    authorizeEntityList.Add(authorizeEntity);
                }
                _dataAuthorizeDao.Insert(authorizeEntityList);
            }
        }
    }
}
