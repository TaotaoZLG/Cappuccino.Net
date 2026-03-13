using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Util;
using Newtonsoft.Json;

namespace Cappuccino.Common.Helper
{
    /// <summary>
    /// AI识别工具类（对接千问AI OCR接口）
    /// </summary>
    public static class AIRecognitionHelper
    {
        #region 修复：静态HttpClient单例，避免Socket耗尽
        private static readonly HttpClient _httpClient;
        static AIRecognitionHelper()
        {
            string timeoutStr = ConfigUtils.AppSetting.GetValue("AITimeout");
            int timeout = timeoutStr.ParseToInt() > 0 ? timeoutStr.ParseToInt() : 30000;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(timeout)
            };
        }
        #endregion

        /// <summary>
        /// 身份证识别结果实体
        /// </summary>
        public class IdCardInfo
        {
            public string Name { get; set; }
            public string IdNumber { get; set; }
            public string Nation { get; set; }
            public string Address { get; set; }
            public string BirthDate { get; set; }
            public string Gender { get; set; }
        }

        /// <summary>
        /// 图片识别结果实体
        /// </summary>
        public class AIRecognitionResult
        {
            public string ImagePath { get; set; }
            public string ImageType { get; set; }
            public IdCardInfo IdCardInfo { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// 图片OCR文本识别（对接千问AI）
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>识别结果</returns>
        public static async Task<string> ImageOcrRecognizeAsync(string imagePath, string batchId, Action<ProcessProgress> progressAction)
        {
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

            progressAction.Invoke(new ProcessProgress
            {
                BatchId = batchId,
                Progress = 0,
                Type = "Recognize",
                Message = $"开始识别图片：{Path.GetFileName(imagePath)}"
            });

            try
            {
                // 读取图片文件为Base64
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string imageBase64 = Convert.ToBase64String(imageBytes);

                // 构建请求参数
                var requestModel = new
                {
                    image = imageBase64,
                    type = "ocr"
                };
                string jsonContent = JsonConvert.SerializeObject(requestModel);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 调用千问AI API（使用静态HttpClient）
                string endpoint = ConfigUtils.AppSetting.GetValue("AIService");
                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseContent);

                // 解析识别结果
                string ocrText = result.data?.text ?? "";
                if (string.IsNullOrEmpty(ocrText))
                {
                    ocrText = "未识别到文本";
                }

                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 100,
                    Type = "Recognize",
                    Message = $"图片识别完成：{Path.GetFileName(imagePath)}"
                });

                return ocrText;
            }
            catch (Exception ex)
            {
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
        /// 结构化图片识别（身份证等）
        /// </summary>
        public static async Task<AIRecognitionResult> RecognizeImageAsync(string imagePath, string batchId, IProgress<ProcessProgress> progress)
        {
            if (!File.Exists(imagePath))
            {
                return new AIRecognitionResult { Success = false, Message = "图片文件不存在", ImagePath = imagePath };
            }

            try
            {
                var endpoint = ConfigUtils.AppSetting.GetValue("AIService");
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Recognize",
                    Progress = 0,
                    Message = $"开始识别图片：{imagePath}"
                });

                // 构造请求
                var requestModel = new { ImagePath = imagePath };
                var json = JsonConvert.SerializeObject(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 调用接口
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseStr = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AIRecognitionResult>(responseStr);

                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Recognize",
                    Progress = 100,
                    Message = $"识别完成：{imagePath}，类型：{result.ImageType}"
                });

                return result;
            }
            catch (Exception ex)
            {
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Recognize",
                    Progress = 0,
                    Message = $"识别失败：{imagePath}，原因：{ex.Message}"
                });
                return new AIRecognitionResult { Success = false, Message = ex.Message, ImagePath = imagePath };
            }
        }
    }
}