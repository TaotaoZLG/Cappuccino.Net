using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Enum;
using Cappuccino.IBLL;

namespace Cappuccino.Web.Core
{
    public class BaseController : Controller
    {
        protected IList<IDisposable> DisposableObjects { get; private set; }
        protected const string SuccessText = "操作成功！";
        protected const string ErrorText = "操作失败！";
        public BaseController()
        {
            this.DisposableObjects = new List<IDisposable>();
        }

        public virtual ActionResult Index()
        {
            string url = Request.Url.AbsolutePath.ToString();
            ISysActionButtonService sysActionButtonService = GlobalContext.Container.Resolve<ISysActionButtonService>();
            ViewData["RightButtonList"] = sysActionButtonService.GetButtonListByUserIdAndMenuId(UserManager.GetCurrentUserInfo().Id, url, PositionEnum.FormInside);
            ViewData["TopButtonList"] = sysActionButtonService.GetButtonListByUserIdAndMenuId(UserManager.GetCurrentUserInfo().Id, url, PositionEnum.FormRightTop);
            return View();
        }

        protected void AddDisposableObject(object obj)
        {
            IDisposable disposable = obj as IDisposable;
            if (disposable != null)
            {
                this.DisposableObjects.Add(disposable);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IDisposable obj in this.DisposableObjects)
                {
                    if (null != obj)
                    {
                        obj.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }

        #region 封装ajax请求的返回方法
        /// <summary>
        /// 返回原始JSON对象（主要用于兼容旧代码或特殊结构）
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        /// <returns>JsonResult</returns>
        protected ActionResult WriteJson(object obj)
        {
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 返回包含数据的成功响应（结构需由调用者自行定义）
        /// </summary>
        /// <param name="obj">包含状态、消息等的数据对象</param>
        /// <returns>JsonResult</returns>
        protected ActionResult WriteSuccess(object obj)
        {
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 返回默认格式的成功响应（仅包含状态和消息）
        /// </summary>
        /// <param name="message">成功提示信息，默认为"操作成功"</param>
        /// <returns>JsonResult</returns>
        protected ActionResult WriteSuccess(string message = SuccessText)
        {
            return Json(new { Status = (int)AjaxStateEnum.Sucess, Message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 返回包含数据和消息的成功响应（标准格式）
        /// </summary>
        /// <param name="message">成功提示信息</param>
        /// <param name="obj">需要返回给前端的具体业务数据</param>
        /// <returns>JsonResult</returns>
        protected ActionResult WriteSuccess(string message, object obj)
        {
            return Json(new { Status = (int)AjaxStateEnum.Sucess, Message = message, Data = obj }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 返回原始JSON对象作为错误响应（主要用于兼容旧代码）
        /// </summary>
        /// <param name="obj">包含错误信息的对象</param>
        /// <returns>JsonResult</returns>
        protected ActionResult WriteError(object obj)
        {
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 返回默认格式的错误响应（仅包含状态和消息）
        /// </summary>
        /// <param name="message">错误提示信息，默认为"操作失败"</param>
        /// <returns>JsonResult</returns>
        protected ActionResult WriteError(string message = ErrorText)
        {
            return Json(new { Status = (int)AjaxStateEnum.Error, Message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 捕获系统异常并返回错误信息
        /// 逻辑：递归获取最深层的内部异常（InnerException），防止返回笼统的"错误"
        /// </summary>
        /// <param name="ex">捕获到的异常对象</param>
        /// <returns>JsonResult（包含最具体的错误描述）</returns>
        protected ActionResult WriteError(Exception ex)
        {
            // 获取ex的第一级内部异常，如果无内部异常则指向自身
            Exception innerEx = ex.InnerException == null ? ex : ex.InnerException;

            // 循环获取内部异常，直到找到最底层的异常信息为止
            while (innerEx.InnerException != null)
            {
                innerEx = innerEx.InnerException;
            }

            return Json(new { Status = (int)AjaxStateEnum.Error, Message = innerEx.Message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
