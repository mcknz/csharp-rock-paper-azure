using System.Web.Mvc;
using Compete.Site.Filters;
using Compete.Site.Infrastructure;
using Compete.Site.Refereeing;

namespace Compete.Site.Controllers
{

    [RequireAuthenticationFilter]
    public class CompetitionController : Controller
    {
        readonly MatchStarter _matchStarter;
        readonly IRefereeThread _refereeThread;

        public CompetitionController(IRefereeThread refereeThread, MatchStarter matchStarter)
        {
            _refereeThread = refereeThread;
            _matchStarter = matchStarter;
        }

        [OutputCache(Duration = 0, VaryByParam = "none")]
        public ActionResult Index()
        {
            if (!_refereeThread.IsRunning) _matchStarter.QueueForAll();
            return null;
        }
    }
}