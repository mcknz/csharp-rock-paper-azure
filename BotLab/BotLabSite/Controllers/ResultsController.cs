using System.Linq;
using System.Web.Mvc;
using Compete.TeamManagement;
using Compete.Site.Filters;

namespace Compete.Site.Controllers
{
    [RequireAuthenticationFilter]
    public class ResultsController : Controller
    {
        readonly ITeamManagementQueries _teamManagementQueries;

        public ResultsController(ITeamManagementQueries teamManagementQueries)
        {
            _teamManagementQueries = teamManagementQueries;
        }

        [OutputCache(Duration = 0, VaryByParam = "none")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
             if (Request.AcceptTypes.Any(t => t.Equals("application/json")))
            {
                var standings = _teamManagementQueries.GetTeamStandings().Where(s => s.Rank > 0).OrderBy(s => s.Rank);
                return Json(standings, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View();
            }
        }

        [OutputCache(Duration = 0, VaryByParam = "none")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult TeamList()
        {
            return (Json(_teamManagementQueries.GetAllTeamNames().OrderBy(n => n), JsonRequestBehavior.AllowGet));
        }
    }
}
