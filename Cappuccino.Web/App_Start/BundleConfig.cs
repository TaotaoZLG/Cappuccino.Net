using System.Web.Optimization;

namespace Cappuccino.Web
{
    public class BundleConfig
    {
        // 有关捆绑的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // 生产环境可以设置为 true 以启用合并与压缩
            BundleTable.EnableOptimizations = false;

            // 样式捆绑：pear 与 后台 样式
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/component/pear/css/pear.css",
                "~/Content/admin/css/load.css",
                "~/Content/admin/css/admin.css"
            ));

            // jQuery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Content/component/jquery/2.2.4/jquery.min.js"
            ));

            // layui 框架
            bundles.Add(new ScriptBundle("~/bundles/layui").Include(
                "~/Content/component/layui/layui.js"
            ));

            // pear 框架及后台脚本，按需要包含 module 下的脚本
            bundles.Add(new ScriptBundle("~/bundles/pear").Include(
                "~/Content/component/pear/pear.js",
                "~/Content/admin/js/cap-data.js",
                "~/Content/admin/js/cap.js",
                "~/Content/admin/js/cap-init.js"
            ));

            // 包含 pear module 目录下的所有脚本（递归）
            bundles.Add(new ScriptBundle("~/bundles/pear-modules").IncludeDirectory(
                "~/Content/component/pear/module", "*.js", true));
        }
    }
}
