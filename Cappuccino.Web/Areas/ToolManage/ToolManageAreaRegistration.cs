using System.Web.Mvc;

namespace Cappuccino.Web.Areas.ToolManage
{
    public class ToolManageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ToolManage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ToolManage_default",
                "ToolManage/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}