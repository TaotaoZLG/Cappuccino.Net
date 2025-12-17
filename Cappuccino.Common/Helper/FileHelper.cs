using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Log;
using Cappuccino.Common.Net;
using Cappuccino.Common.Util;

namespace Cappuccino.Common.Helper
{
    public class FileHelper
    {
        public static void CreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void DeleteDirectory(string filePath)
        {
            try
            {
                if (Directory.Exists(filePath)) //如果存在这个文件夹删除之 
                {
                    foreach (string d in Directory.GetFileSystemEntries(filePath))
                    {
                        if (File.Exists(d))
                            File.Delete(d); //直接删除其中的文件                        
                        else
                            DeleteDirectory(d); //递归删除子文件夹 
                    }
                    Directory.Delete(filePath, true); //删除已空文件夹                 
                }
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(ex.Message, ex);
            }
        }

        public static string ConvertDirectoryToHttp(string directory)
        {
            directory = directory.ParseToString();
            directory = directory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return directory;
        }

        public static string ConvertHttpToDirectory(string http)
        {
            http = http.ParseToString();
            http = http.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return http;
        }

        public static TData CheckFileExtension(string fileExtension, string allowExtension)
        {
            TData obj = new TData();
            string[] allowArr = TextHelper.SplitToArray<string>(allowExtension.ToLower(), '|');
            if (allowArr.Where(p => p.Trim() == fileExtension.ParseToString().ToLower()).Any())
            {
                obj.Status = 1;
            }
            else
            {
                obj.Message = "只有文件扩展名是 " + allowExtension + " 的文件才能上传";
            }
            return obj;
        }

        public static string FilterFilePath(string filePath)
        {
            filePath = filePath.Replace("../", string.Empty);
            filePath = filePath.Replace("..", string.Empty);
            filePath = filePath.TrimStart('/');
            return filePath;
        }
    }
}
