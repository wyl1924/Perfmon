using PlatformSolution.Perfmon.Api;
using Synjones.Dreams.AdminModule.Model;
using Synjones.Dreams.GroupModule.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using web.BLL.AdminBusiness;
using web.BLL.SchoolBusiness;
using web.CommonSrv.Attributes.Permission;
using web.Controllers;


namespace PlatformSolution.Perfmon.ctl
{
    [NeedAdminSignIn]
    [AuthorityAttribute]
    public class PerfmonManageController : Controller
    {
        public ActionResult Index()
        {
            var Perfmons = new PerfmonManage().GetPerfmons();
            if (Perfmons == null)
            {
                SetMaxTimes("支付平台", "toPay", 15000);
                SetMaxTimes("开票平台", "openTicket", 15000);
                SetMaxTimes("缴费平台", "esbFee", 15000);
                Perfmons = new PerfmonManage().GetPerfmons();
            }
            return View(Perfmons);
        }
        public JsonResult SetMaxTimes(string name, string code, int maxTimes)
        {
            IPerfmon perf = new PerfmonManage(name, code);
            bool falg = perf.SetMaxTimes(maxTimes);
            return Json(falg);
        }
    }
}
