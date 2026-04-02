using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cappuccino.Common.Helper
{
    /// <summary>
    /// AI识别工具类（对接千问AI OCR接口）
    /// </summary>
    public static class AIRecognitionHelper
    {
        // 静态HttpClient单例，避免Socket耗尽
        private static readonly HttpClient _httpClient;

        static AIRecognitionHelper()
        {
            string timeoutStr = ConfigUtils.AppSetting.GetValue("AITimeout");
            int timeout = timeoutStr.ParseToInt() > 0 ? timeoutStr.ParseToInt() : 180;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
        }

        /// 图片OCR文本识别（对接本地OCR服务）
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>识别结果（JSON字符串）</returns>
        public static async Task<string> ImageOcrRecognizeAsync(string imagePath, string batchId, Action<ProcessProgress> progressAction)
        {           
            try
            {
                // 校验文件是否存在
                if (!File.Exists(imagePath))
                {
                    string errorMsg = $"图片文件不存在：{imagePath}";
                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 0,
                        Type = "Error",
                        Message = errorMsg
                    });
                    return errorMsg;
                }

                // 2. 触发进度回调：开始识别
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 0,
                    Type = "Recognize",
                    Message = $"开始识别图片：{Path.GetFileName(imagePath)}"
                });

                // 构建multipart/form-data表单请求
                var multipartContent = new MultipartFormDataContent();

                // 添加图片文件（核心字段）
                var fileStream = File.OpenRead(imagePath);
                var fileContent = new StreamContent(fileStream);
                // 设置文件Content-Type（自动识别或固定为image/jpeg，根据实际场景调整）
                string fileContentType = GetContentTypeByExtension(Path.GetExtension(imagePath));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileContentType);
                // 添加file字段（对应curl中的-F 'file=@xxx.JPG;type=image/jpeg'）
                multipartContent.Add(fileContent, "file", Path.GetFileName(imagePath));

                // 添加其他表单字段
                multipartContent.Add(new StringContent("auto"), "language"); // language=auto
                multipartContent.Add(new StringContent("json_kv"), "output_format"); // output_format=json_kv
                multipartContent.Add(new StringContent(""), "extract_fields"); // extract_fields=空值

                // 调用OCR API
                string endpoint = ConfigUtils.AppSetting.GetValue("AIService");
                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, multipartContent);

                // 读取响应结果（接口返回JSON，直接返回或按需解析）
                string responseContent = await response.Content.ReadAsStringAsync();

                // 进度回调：识别完成
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 100,
                    Type = "Recognize",
                    Message = $"图片识别完成：{Path.GetFileName(imagePath)}"
                });

                // 检查响应状态码，返回识别结果
                if (!response.IsSuccessStatusCode)
                {
                    return responseContent;
                }
                else
                {
                    // 返回原始JSON响应（如需解析成特定格式，可在此处处理）
                    var jsonObj = JObject.Parse(responseContent);
                    bool success = jsonObj.Value<bool>("success");
                    if (jsonObj.HasValues && success)
                    {
                        responseContent = jsonObj.Value<string>("data").ToString();
                    }
                }
                return responseContent;
            }
            catch (Exception ex)
            {
                // 异常处理+进度回调
                string errorMsg = $"图片识别失败：{Path.GetFileName(imagePath)}，错误：{ex.Message}";
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 0,
                    Type = "Error",
                    Message = errorMsg
                });
                return errorMsg;
            }
        }

        /// <summary>
        /// 根据文件扩展名获取Content-Type（适配不同图片格式）
        /// </summary>
        /// <param name="extension">文件扩展名（如.jpg/.png）</param>
        /// <returns>Content-Type字符串</returns>
        private static string GetContentTypeByExtension(string extension)
        {
            switch (extension?.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".tif":
                case ".tiff":
                    return "image/tiff";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
