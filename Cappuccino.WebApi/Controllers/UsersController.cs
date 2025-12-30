using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Cappuccino.Entity;
using Cappuccino.IBLL;

namespace Cappuccino.WebApi.Controllers
{
    /// <summary>
    /// 用户相关操作的 Web API 控制器。
    /// </summary>
    public class UsersController : ApiController
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
            List<SysUserEntity> users = _userService.GetList(x => true).ToList();
            return Ok(users);
        }
    }
}
