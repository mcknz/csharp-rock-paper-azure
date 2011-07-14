using System.IO;
using System.Linq;
using System.Web.Mvc;
using Compete.Model.Repositories;
using Compete.Site.Filters;
using Compete.TeamManagement;
using Compete.Model;

namespace Compete.Site.Controllers
{
    [RequireAuthenticationFilter]
    public class HomeController : Controller
    {
        readonly ITeamManagementQueries _teamManagementQueries;
        readonly IConfigurationRepository _configurationRepository;

        public HomeController(ITeamManagementQueries teamManagementQueries, 
                              IConfigurationRepository configurationRepository)
        {
            _teamManagementQueries = teamManagementQueries;
            _configurationRepository = configurationRepository;
            ViewData["inCloud"] = RockPaperAzure.AzureHelper.IsRunningInCloud;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            ViewData["teamList"] = _teamManagementQueries.GetAllTeamNames().OrderBy(x => x);
            ViewData["currentTeam"] = "";
            var botPath = _configurationRepository.GetConfiguration().SelectedBotPath;
            if (botPath != null)
            {
                if (System.IO.File.Exists(botPath))
                {
                    ViewData["currentTeam"] = Team.ConvertFileToTeamName(botPath);
                }
            }
            return View();
        }
    }
}