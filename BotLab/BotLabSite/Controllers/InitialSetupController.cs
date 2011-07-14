using System.IO;
using System.Linq;
using System.Web.Mvc;
using Compete.Model;
using Compete.Model.Repositories;
using Compete.Site.Filters;
using Compete.Site.Infrastructure;
using Compete.TeamManagement;

namespace Compete.Site.Controllers
{
    [RequireOneTimeSetupFilter]
    public class InitialSetupController : Controller
    {
        readonly ISignin _signin;
        readonly IConfigurationRepository _configurationRepository;
        readonly ITeamManagementCommands _teamCommands;

        public InitialSetupController(IConfigurationRepository configurationRepository, ISignin signin, ITeamManagementCommands teamCommands)
        {
            _configurationRepository = configurationRepository;
            _signin = signin;
            _teamCommands = teamCommands;

            ViewData["inCloud"] = RockPaperAzure.AzureHelper.IsRunningInCloud;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Index(string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(password))
                ModelState.AddModelError("password", "Entry required");
            else if (string.IsNullOrEmpty(confirmPassword))
                ModelState.AddModelError("confirmPassword", "Entry required");
            else if (password != confirmPassword)
                ModelState.AddModelError("password", "Entries did not match; try again");
            if (!ModelState.IsValid)
                return View();
            else
            {
                var configuration = new Configuration
                {
                    AdminPassword = password,
                };
                _configurationRepository.SetConfiguration(configuration);

                // spin up initial bots (new team created for each bot in the /install/bots directory)
                foreach (var f in Directory.EnumerateFiles(Path.Combine(RockPaperAzure.AzureHelper.GetLocalFolder(), "bots")))
                {
                    _teamCommands.New(f);
                }

                _signin.Signin(password);
                return Redirect("~/Home");
            }
        }
    }
}
