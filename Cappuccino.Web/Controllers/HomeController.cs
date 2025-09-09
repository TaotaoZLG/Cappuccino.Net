using System;
using System.IO;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ISysActionMenuService _sysActionMenuService;
        private readonly ISysActionButtonService _sysActionButtonService;

        public HomeController(ISysActionMenuService sysActionMenuService, ISysActionButtonService sysActionButtonService)
        {
            _sysActionMenuService = sysActionMenuService;
            _sysActionButtonService = sysActionButtonService;
            this.AddDisposableObject(SysActionMenuService);
        }

        public override ActionResult Index()
        {
            return View();
        }

        public ActionResult Console()
        {
            return View();
        }

        public ActionResult Menu()
        {
            var menu = SysActionMenuService.GetMenu(UserManager.GetCurrentUserInfo().Id);
            return WriteSuccess(SuccessText, menu);
        }

        public ActionResult ExportFile()
        {
            UploadFile uploadFile = new UploadFile();
            try
            {
                var file = Request.Files[0];    //获取选中文件
                var filecombin = file.FileName.Split('.');
                if (file == null || string.IsNullOrEmpty(file.FileName) || file.ContentLength == 0 || filecombin.Length < 2)
                {
                    uploadFile.Code = -1;
                    uploadFile.Src = "";
                    uploadFile.Msg = "上传出错!请检查文件名或文件内容";
                    return Json(uploadFile, JsonRequestBehavior.AllowGet);
                }
                //定义本地路径位置
                string localPath = Server.MapPath("~/Upload");
                string filePathName = string.Empty; //最终文件名
                filePathName = DateTimeExtensions.CreateNo() + "." + filecombin[1];
                //Upload不存在则创建文件夹
                if (!System.IO.Directory.Exists(localPath))
                {
                    System.IO.Directory.CreateDirectory(localPath);
                }
                file.SaveAs(Path.Combine(localPath, filePathName));  //保存图片
                uploadFile.Code = 0;
                uploadFile.Src = Path.Combine("/Upload/", filePathName);
                uploadFile.Msg = "上传成功";
                return Json(uploadFile, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                uploadFile.Code = -1;
                uploadFile.Src = "";
                uploadFile.Msg = "上传出错!程序异常";
                return Json(uploadFile, JsonRequestBehavior.AllowGet);
            }
        }
    }
}