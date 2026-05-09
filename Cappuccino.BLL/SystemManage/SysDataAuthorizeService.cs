using System;
using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
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
        /// 获取用户对指定 DataType 的生效数据 Id 集合（返回 HashSet）
        /// 若 userId 为 null，则使用当前登录用户 Id（UserManager.GetCurrentUserInfo().Id）
        /// 缓存 key 使用 KeyManager.UserDataPermission 前缀统一管理
        /// </summary>
        public HashSet<long> GetEffectiveDataIdsForUser(int dataType, long? userId = null)
        {
            long effectiveUserId = userId ?? UserManager.GetCurrentUserInfo()?.Id ?? 0;
            if (effectiveUserId == 0)
            {
                // 无登录用户也可能是后台任务调用，按业务选择返回空集合
                return new HashSet<long>();
            }

            string cacheKey = KeyManager.UserDataPermission;
            var cached = CacheManager.Get<HashSet<long>>(cacheKey);
            if (cached != null) return cached;

            // 获取当前用户所属角色
            var roleIds = _roleDao.GetList(r => r.SysUsers.Any(u => u.Id == effectiveUserId)).Select(r => r.Id).ToList();

            // 查询角色级和用户级的数据授权记录
            var baseAuths = _dataAuthorizeDao.GetList(x => x.DataType == dataType);
            var authIds = baseAuths
                .Where(x => (x.AuthorizeType == 1 && x.AuthorizeId.HasValue && roleIds.Contains(x.AuthorizeId.Value))
                         || (x.AuthorizeType == 2 && x.AuthorizeId == effectiveUserId))
                .Select(x => x.DataId)
                .ToList();

            var result = new HashSet<long>(authIds.Where(x => x.HasValue).Select(x => x.Value));

            // 针对部门（DataType==2）展开子部门
            if (dataType == 2 && result.Any() && _departmentDao != null)
            {
                var all = _departmentDao.GetList(d => true).ToList();
                var expanded = new HashSet<long>(result);
                foreach (var id in result) ExpandDepartment(id, all, expanded);
                result = expanded;
            }

            // 若无任何授权，回退到用户所属部门/自身（避免默认全量）
            if (result.Count == 0)
            {
                var user = _userDao.GetList(u => u.Id == effectiveUserId).FirstOrDefault();
                if (user != null)
                {
                    if (dataType == 2 && user.DepartmentId.HasValue) result.Add(user.DepartmentId.Value);
                    if (dataType == 3) result.Add(effectiveUserId);
                }
            }

            CacheManager.Set(cacheKey, result, CacheTtl);
            return result;
        }

        private void ExpandDepartment(long id, List<SysDepartmentEntity> all, HashSet<long> set)
        {
            var children = all.Where(d => d.ParentId == id).Select(d => d.Id).ToList();
            foreach (var c in children)
            {
                if (set.Add(c)) ExpandDepartment(c, all, set);
            }
        }

        /// <summary>
        /// 清除缓存，userId 可为 null（表示当前登录用户）
        /// </summary>
        public void ClearCache(long? userId = null, int? dataType = null)
        {
            long effectiveUserId = userId ?? UserManager.GetCurrentUserInfo()?.Id ?? 0;
            if (effectiveUserId == 0) return;

            CacheManager.Remove(KeyManager.UserDataPermission);
        }
    }
}