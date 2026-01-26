using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace Cappuccino.WebApi.Models
{
    /// <summary>
    /// Web API 自定义 JSON 结果（基于 Newtonsoft.Json）
    /// </summary>
    public class JsonNetApiResult : IHttpActionResult
    {
        private readonly object _data;
        private readonly HttpStatusCode _statusCode;
        private readonly JsonSerializerSettings _settings;

        public JsonNetApiResult(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _data = data;
            _statusCode = statusCode;
            // 复用与 MVC 一致的 JSON 配置（循环引用、日期格式、驼峰命名）
            _settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(_data, _settings))
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            return Task.FromResult(response);
        }
    }
}