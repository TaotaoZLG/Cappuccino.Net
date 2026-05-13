using System;
using System.Collections.Generic;
using System.Linq;
using Cappuccino.Cache;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Enum;
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

        private readonly ISysUserDao _userDao;
        private DataAuthorizeCache _dataAuthorizeCache;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        public SysDataAuthorizeService(ISysUserDao userDao, DataAuthorizeCache dataAuthorizeCache)
        {
            _userDao = userDao;
            _dataAuthorizeCache = dataAuthorizeCache;
        }
        #endregion

        /// <summary>
        /// 获取用户的数据权限 Id 集合。
        /// 如果用户角色为【超级管理员】，直接返回所有部门和用户Id，无需查权限。
        /// </summary>
        public DataAuthorizeInfo GetAuthorizeList(SysUserEntity userEntity = null)
        {
            userEntity = userEntity ?? UserManager.GetCurrentUserInfo();
            if (userEntity == null || userEntity.Id == 0)
            {
                return new DataAuthorizeInfo();
            }
            
            var result = new DataAuthorizeInfo();
            string cacheKey = KeyManager.DataPermission;
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

            // 从缓存中获取数据授权列表，过滤出与用户角色或用户Id相关的授权项
            var dataAuthorizeCacheList = _dataAuthorizeCache.GetList();
            
            var dataAuthorizeList = dataAuthorizeCacheList.Where(x =>
                (x.DataType == (int)AuthorizeTypeEnum.User || x.DataType == (int)AuthorizeTypeEnum.Department) &&
                (
                    (x.AuthorizeType == (int)AuthorizeTypeEnum.Role && roleIds.Contains(x.AuthorizeId.Value)) ||
                    (x.AuthorizeType == (int)AuthorizeTypeEnum.User && x.AuthorizeId == userEntity.Id)
                )
            ).ToHashSet();

            // 提取部门Id和用户Id，合并去重
            result.ChildrenDepartmentIdList = dataAuthorizeList
                    .Where(x => x.DataType == (int)AuthorizeTypeEnum.Department && x.DataId.HasValue)
                    .Select(x => x.DataId.Value)
                    .ToHashSet();

            result.ChildrenUserIdList = dataAuthorizeList
                    .Where(x => x.DataType == (int)AuthorizeTypeEnum.User && x.DataId.HasValue)
                    .Select(x => x.DataId.Value)
                    .ToHashSet();

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