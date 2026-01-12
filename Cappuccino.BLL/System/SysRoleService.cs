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
        private readonly ISysRoleDao _roleDao;
        private readonly ISysActionDao _actionDao;
        private readonly ISysDataAuthorizeDao _dataAuthorizeDao;

        public SysRoleService(ISysRoleDao roleDao, ISysActionDao actionDao, ISysDataAuthorizeDao dataAuthorizeDao)
        {
            _roleDao = roleDao;
            _actionDao = actionDao;
            _dataAuthorizeDao = dataAuthorizeDao;
            base.CurrentDao = roleDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 保存角色菜单权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="menuPermissions"></param>
        public void SaveMenuPermissions(SysRoleEntity roleEntity, List<DtreeResponse> menuPermissions)
        {
            roleEntity.SysActions.Clear();

            if (menuPermissions == null || !menuPermissions.Any())
            {
                _roleDao.SaveChanges();
                return;
            }

            var actionIds = menuPermissions.Select(p => Convert.ToInt32(p.NodeId)).Distinct().ToList();
            var actionList = _actionDao.GetList(x => actionIds.Contains(x.Id)).ToList();
            actionList.ForEach(action => roleEntity.SysActions.Add(action));
            //foreach (var item in actionList)
            //{
            //    roleEntity.SysActions.Add(item);
            //}
            _roleDao.SaveChanges();
        }

        /// <summary>
        /// 保存角色数据权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="dataPermissions"></param>
        public void SaveDataPermissions(int roleId, List<DtreeResponse> dataPermissions)
        {
            // 删除旧数据权限
            _dataAuthorizeDao.DeleteByRoleId(roleId);

            // 处理数据权限
            if (dataPermissions != null && dataPermissions.Any())
            {
                List<SysDataAuthorizeEntity> authorizeEntityList = new List<SysDataAuthorizeEntity>();
                int currentUserId = UserManager.GetCurrentUserInfo().Id;

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

        /// <summary>
        /// 保存角色的菜单权限和数据权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="menuPermissions"></param>
        /// <param name="dataPermissions"></param>
        public void SavePermissions(int roleId, List<DtreeResponse> menuPermissions, List<DtreeResponse> dataPermissions)
        {
            // 处理菜单权限
            var role = _roleDao.GetList(x => x.Id == roleId).FirstOrDefault();
            var actionIds = menuPermissions.Select(p => Convert.ToInt32(p.NodeId)).ToList();
            var actions = _actionDao.GetList(x => actionIds.Contains(x.Id)).ToList();

            //清空权限关系
            role.SysActions.Clear();
            actions.ForEach(action => role.SysActions.Add(action));
            _roleDao.Update(role);
            _roleDao.SaveChanges();

            // 删除旧数据权限
            _dataAuthorizeDao.DeleteByRoleId(roleId);
            // 处理数据权限
            if (dataPermissions != null && dataPermissions.Any())
            {
                List<SysDataAuthorizeEntity> authorizeEntityList = new List<SysDataAuthorizeEntity>();
                int currentUserId = UserManager.GetCurrentUserInfo().Id;

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
