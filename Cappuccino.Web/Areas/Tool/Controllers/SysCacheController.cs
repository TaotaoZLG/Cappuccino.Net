using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Enum;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;


namespace Cappuccino.Web.Areas.Tool.Controllers
{
    public class SysCacheController : BaseController
    {
        // GET: Tool/SysCache
        [CheckPermission("tool.cache.list")]
        public override ActionResult Index()
        {
            base.Index(); // 加载按钮权限等基础数据
            return View();
        }

        #region 获取数据
        /// <summary>
        /// 获取缓存列表（适配Layui分页）
        /// </summary>
        [HttpGet, CheckPermission("system.cache.list")]
        public ActionResult GetCacheList(PageInfo pageInfo)
        {
            try
            {
                // 获取所有缓存项
                var cacheItems = GetAllCacheItems();
                var totalCount = cacheItems.Count;

                // 分页处理
                var paginatedItems = cacheItems
                    .Skip((pageInfo.Page - 1) * pageInfo.Limit)
                    .Take(pageInfo.Limit)
                    .ToList();

                // 适配Layui表格返回格式
                return Json(Pager.Paging(paginatedItems, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        /// <summary>
        /// 清除指定缓存
        /// </summary>
        [HttpPost, CheckPermission("system.cache.delete")]
        [LogOperate(Title = "清除缓存", BusinessType = (int)OperateType.Delete)]
        public ActionResult RemoveCache(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                return WriteError("缓存Key不能为空");
            }
            CacheManager.Cache.Remove(cacheKey);
            return WriteSuccess("清除成功");
        }
        #endregion

        #region 私有方法：解析所有缓存项
        /// <summary>
        /// 解析HttpRuntime.Cache中的所有缓存项
        /// </summary>
        /// <returns>缓存项列表</returns>
        private List<CacheItemDto> GetAllCacheItems()
        {
            var cacheItems = new List<CacheItemDto>();
            var cache = HttpRuntime.Cache;

            // 遍历缓存枚举器
            var enumerator = cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                var value = cache.Get(key);
                var cacheEntry = GetCacheEntry(cache, key); // 获取缓存项详细信息

                cacheItems.Add(new CacheItemDto
                {
                    CacheKey = key,
                    ValueType = GetFriendlyName(value?.GetType()),
                    ValuePreview = GetValuePreview(value),
                    AbsoluteExpiration = cacheEntry?.AbsoluteExpiration ?? DateTime.MaxValue,
                    SlidingExpiration = cacheEntry?.SlidingExpiration ?? TimeSpan.Zero,
                    IsNeverExpire = (cacheEntry?.AbsoluteExpiration == Cache.NoAbsoluteExpiration) && (cacheEntry?.SlidingExpiration == Cache.NoSlidingExpiration),
                    CreateTime = DateTime.Now // HttpRuntime.Cache无原生创建时间，此处用当前时间替代（可扩展自定义缓存记录创建时间）
                });
            }

            return cacheItems;
        }

        /// <summary>
        /// 反射获取CacheEntry（HttpRuntime.Cache的私有字段）
        /// </summary>
        private CacheEntry GetCacheEntry(Cache cache, string key)
        {
            try
            {
                var cacheType = cache.GetType();
                var getEntryMethod = cacheType.GetMethod("GetEntry", BindingFlags.NonPublic | BindingFlags.Instance);
                return getEntryMethod?.Invoke(cache, new object[] { key }) as CacheEntry;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取一个类型的友好名称，特别处理了泛型类型。
        /// </summary>
        /// <param name="type">要格式化的类型。</param>
        /// <returns>友好的类型名称字符串。</returns>
        public static string GetFriendlyName(Type type)
        {
            if (type == null)
            {
                return "NULL";
            }

            // 如果不是泛型类型，直接返回名称
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            // 处理泛型类型
            StringBuilder friendlyName = new StringBuilder();
            // 获取泛型类型的定义名称，例如从 "List`1" 获取 "List"
            string genericTypeName = type.GetGenericTypeDefinition().Name;
            // 移除 `1, `2 这样的后缀
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));

            friendlyName.Append(genericTypeName);
            friendlyName.Append('<');

            // 获取所有泛型参数
            Type[] genericArguments = type.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                // 递归调用，因为泛型参数本身也可能是泛型的 (例如 Dictionary<string, List<int>>)
                string argumentName = GetFriendlyName(genericArguments[i]);
                friendlyName.Append(argumentName);

                if (i < genericArguments.Length - 1)
                {
                    friendlyName.Append(", ");
                }
            }

            friendlyName.Append('>');

            return friendlyName.ToString();
        }

        /// <summary>
        /// 获取对象的友好预览字符串。
        /// </summary>
        /// <param name="value">要预览的对象。</param>
        /// <param name="maxLength">预览字符串的最大长度。</param>
        /// <returns>友好的预览字符串。</returns>
        public static string GetValuePreview(object value, int maxLength = 100)
        {
            if (value == null)
            {
                return "NULL";
            }

            // 如果是字符串，直接处理
            if (value is string str)
            {
                return str.Length > maxLength ? str.Substring(0, maxLength) + "..." : str;
            }

            // 如果是集合或字典，尝试展开显示
            if (value is IEnumerable enumerable && !(value is IDictionary))
            {
                StringBuilder preview = new StringBuilder();
                preview.Append("[");
                int count = 0;
                foreach (var item in enumerable)
                {
                    if (count > 0)
                    {
                        preview.Append(", ");
                    }
                    preview.Append(item?.ToString() ?? "null");
                    count++;
                    // 限制预览的元素数量，防止过长
                    if (count >= 10)
                    {
                        preview.Append(", ...");
                        break;
                    }
                }
                preview.Append("]");
                string result = preview.ToString();
                return result.Length > maxLength ? result.Substring(0, maxLength) + "..." : result;
            }

            // 对于字典，可以做类似处理
            if (value is IDictionary dict)
            {
                StringBuilder preview = new StringBuilder();
                preview.Append("{");
                int count = 0;
                foreach (DictionaryEntry de in dict)
                {
                    if (count > 0)
                    {
                        preview.Append(", ");
                    }
                    preview.Append($"{de.Key}: {de.Value}");
                    count++;
                    if (count >= 10)
                    {
                        preview.Append(", ...");
                        break;
                    }
                }
                preview.Append("}");
                string result = preview.ToString();
                return result.Length > maxLength ? result.Substring(0, maxLength) + "..." : result;
            }

            // 其他所有类型，调用其 ToString() 方法
            string valueStr = value.ToString();
            return valueStr.Length > maxLength ? valueStr.Substring(0, maxLength) + "..." : valueStr;
        }

        /// <summary>
        /// 缓存项DTO（适配视图展示）
        /// </summary>
        public class CacheItemDto
        {
            /// <summary>缓存Key</summary>
            public string CacheKey { get; set; }
            /// <summary>值类型</summary>
            public string ValueType { get; set; }
            /// <summary>值预览（截取100字符）</summary>
            public string ValuePreview { get; set; }
            /// <summary>绝对过期时间</summary>
            public DateTime AbsoluteExpiration { get; set; }
            /// <summary>滑动过期时间</summary>
            public TimeSpan SlidingExpiration { get; set; }
            /// <summary>是否永不过期</summary>
            public bool IsNeverExpire { get; set; }
            /// <summary>创建时间（模拟）</summary>
            public DateTime CreateTime { get; set; }
        }

        /// <summary>
        /// 适配反射的CacheEntry类（仅保留需要的字段）
        /// </summary>
        private class CacheEntry
        {
            public DateTime AbsoluteExpiration { get; set; }
            public TimeSpan SlidingExpiration { get; set; }
        }
        #endregion
    }
}