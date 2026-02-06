using System.Web.Mvc;

namespace Cappuccino.Web.Areas.Demo
{
    public class BusinessAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Business";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Business_default",
                "Business/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}