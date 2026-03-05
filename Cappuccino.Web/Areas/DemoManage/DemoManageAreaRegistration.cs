using System.Web.Mvc;

namespace Cappuccino.Web.Areas.DemoManage
{
    public class DemoManageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DemoManage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DemoManage_default",
                "DemoManage/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}