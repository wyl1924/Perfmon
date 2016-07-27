using System.Web.Mvc;

namespace PlatformSolution.Perfmon.ctl
{
    public class PerfmonAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Perfmon";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Perfmon_default",
                "Perfmon/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
