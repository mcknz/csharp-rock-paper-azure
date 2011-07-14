using System.Web.Mvc;
using Compete.Model.Repositories;
using Compete.Site.Filters;

namespace Compete.Site.Controllers
{
    [RequireAuthenticationFilter]
    public class GameLogController : Controller
    {
        readonly ILeaderboardRepository _leaderboardRepository;

        public GameLogController(ILeaderboardRepository leaderboardRepository)
        {
            _leaderboardRepository = leaderboardRepository;
        }

        [OutputCache(Duration = 0, VaryByParam = "none")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index(string teamName, string otherTeamName)
        {
            var leaderboard = _leaderboardRepository.GetLeaderboard();
            var result = leaderboard.GetMatchResultsForMatchBetween(teamName, otherTeamName);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
