using System.Web.Mvc;

namespace Cappuccino.Web.Areas.Tool
{
    public class ToolAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Tool";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Tool_default",
                "Tool/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}