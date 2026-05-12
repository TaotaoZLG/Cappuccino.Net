using System;
using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Extensions;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;
using Cappuccino.Web.Core;

namespace Cappuccino.BLL
{
    public class SysDataAuthorizeService : BaseService<SysDataAuthorizeEntity>, ISysDataAuthorizeService
    {
        #region 依赖注入

        private readonly ISysDataAuthorizeDao _dataAuthorizeDao;
        private readonly ISysRoleDao _roleDao;
        private readonly ISysUserDao _userDao;
        private readonly ISysDepartmentDao _departmentDao;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        public SysDataAuthorizeService(ISysDataAuthorizeDao dataAuthorizeDao, ISysRoleDao roleDao, ISysUserDao userDao, ISysDepartmentDao departmentDao)
        {
            _dataAuthorizeDao = dataAuthorizeDao;
            _roleDao = roleDao;
            _userDao = userDao;
            _departmentDao = departmentDao;
            base.CurrentDao = _dataAuthorizeDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 获取用户对指定 DataType 的生效数据 Id 集合。
        /// - 只查找【部门】和【用户】数据权限（DataType=2,3），并集返回。
        /// - 如果用户角色为【超级管理员】，直接返回所有部门和用户Id，无需查权限。
        /// </summary>
        public DataAuthorizeInfo GetEffectiveDataIdsForUser(SysUserEntity userEntity = null)
        {
            userEntity = userEntity ?? UserManager.GetCurrentUserInfo();
            if (userEntity == null || userEntity.Id == 0)
            {
                return new DataAuthorizeInfo();
            }
            var result = new DataAuthorizeInfo();
            string cacheKey = KeyManager.UserDataPermission;
            var cached = CacheManager.Get<DataAuthorizeInfo>(cacheKey);
            if (cached != null) return cached;

            // 查询用户角色
            bool isSuperAdmin = userEntity.SysRoles.Any(r => r.Code == "Administrator");
            if (isSuperAdmin)
            {
                // 超级管理员，跳过权限检查
                return new DataAuthorizeInfo();
            }

            // 先取出角色Id集合，避免EF无法解析导航属性
            var roleIds = userEntity.SysRoles.Select(r => r.Id).ToList();

            // 普通用户，查找【部门】和【用户】数据权限
            var dataAuthorizeList = _dataAuthorizeDao.GetList(x =>
                (x.DataType == (int)AuthorizeTypeEnum.User || x.DataType == (int)AuthorizeTypeEnum.Department) &&
                (
                    (x.AuthorizeType == (int)AuthorizeTypeEnum.Role && roleIds.Contains(x.AuthorizeId.Value)) ||
                    (x.AuthorizeType == (int)AuthorizeTypeEnum.User && x.AuthorizeId == userEntity.Id)
                )
            ).ToHashSet();

            // 提取部门Id和用户Id，合并去重
            result.ChildrenDepartmentIdList = new HashSet<long>(
                dataAuthorizeList
                    .Where(x => x.DataType == (int)AuthorizeTypeEnum.Department && x.DataId.HasValue)
                    .Select(x => x.DataId.Value)
            );

            result.ChildrenUserIdList = new HashSet<long>(
                dataAuthorizeList
                    .Where(x => x.DataType == (int)AuthorizeTypeEnum.User && x.DataId.HasValue)
                    .Select(x => x.DataId.Value)
            );

            // 若无授权，默认至少包含用户所在部门和用户自己
            if (result.ChildrenDepartmentIdList.Count == 0 && result.ChildrenUserIdList.Count == 0)
            {
                var user = _userDao.GetList(u => u.Id == userEntity.Id).FirstOrDefault();
                if (user != null)
                {
                    if (user.DepartmentId.HasValue) result.ChildrenDepartmentIdList.Add(user.DepartmentId.Value);
                    result.ChildrenUserIdList.Add(user.Id);
                }
            }

            CacheManager.Set(cacheKey, result, CacheTtl);
            return result;
        }

        #region 私有方法
        private void ExpandDepartment(long id, List<SysDepartmentEntity> all, HashSet<long> set)
        {
            var children = all.Where(d => d.ParentId == id).Select(d => d.Id).ToList();
            foreach (var c in children)
            {
                if (set.Add(c)) ExpandDepartment(c, all, set);
            }
        }
        #endregion
    }
}