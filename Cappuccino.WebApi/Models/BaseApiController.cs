using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Services.Description;
using Cappuccino.Common.Enum;

namespace Cappuccino.WebApi.Models
{
    /// <summary>
    /// Web API 基础控制器（提供统一响应格式）
    /// </summary>
    public class BaseApiController : ApiController
    {
        protected const string SuccessText = "操作成功！";
        protected const string ErrorText = "操作失败！";
        protected IList<IDisposable> DisposableObjects { get; private set; }

        public BaseApiController()
        {
            DisposableObjects = new List<IDisposable>();
        }

        /// <summary>
        /// 成功响应（无数据）
        /// </summary>
        protected IHttpActionResult ApiSuccess(string message = SuccessText)
        {
            var result = new { Status = (int)AjaxStateEnum.Sucess, Message = message };
            return new JsonNetApiResult(result);
        }

        /// <summary>
        /// 成功响应（带数据）
        /// </summary>
        protected IHttpActionResult ApiSuccess(string message, object data)
        {
            var result = new { Status = (int)AjaxStateEnum.Sucess, Message = message, Data = data };
            return new JsonNetApiResult(result);
        }

        /// <summary>
        /// 错误响应（自定义消息）
        /// </summary>
        protected IHttpActionResult ApiError(string message = ErrorText, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var result = new { Status = (int)AjaxStateEnum.Error, Message = message };
            return new JsonNetApiResult(result, statusCode);
        }

        /// <summary>
        /// 错误响应（异常信息）
        /// </summary>
        protected IHttpActionResult ApiError(Exception ex, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            // 获取最内层异常信息
            var innerEx = ex.InnerException ?? ex;
            while (innerEx.InnerException != null)
            {
                innerEx = innerEx.InnerException;
            }
            var result = new { Status = (int)AjaxStateEnum.Error, Message = innerEx.Message };
            return new JsonNetApiResult(result, statusCode);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var obj in DisposableObjects)
                {
                    obj?.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        protected void AddDisposableObject(IDisposable obj)
        {
            if (obj != null)
            {
                DisposableObjects.Add(obj);
            }
        }
    }
}