using System.Web.Mvc;
using Compete.Site.Refereeing;
using Compete.Site.Filters;

namespace Compete.Site.Controllers
{
    [RequireAuthenticationFilter]
    public class MatchController : Controller
    {
        readonly IRefereeThread _refereeThread;

        public MatchController(IRefereeThread refereeThread)
        {
            _refereeThread = refereeThread;
        }

        [OutputCache(Duration = 0, VaryByParam = "none")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Status()
        {
            return Json(_refereeThread.IsRunning, JsonRequestBehavior.AllowGet);
        }
    }
}
