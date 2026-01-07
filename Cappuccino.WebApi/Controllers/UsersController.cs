using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.WebApi.Models;

namespace Cappuccino.WebApi.Controllers
{
    /// <summary>
    /// 用户相关操作的 Web API 控制器。
    /// </summary>
    public class UsersController : BaseApiController
    {
        private readonly ISysUserService _userService;

        public UsersController(ISysUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns>用户列表</returns>
        [HttpGet]
        public IHttpActionResult GetAll()
        {
            var users = _userService.GetList(x => true).ToList();
            return ApiSuccess("成功", users);
        }
    }
}
