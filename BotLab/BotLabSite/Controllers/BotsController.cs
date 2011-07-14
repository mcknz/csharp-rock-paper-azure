using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Compete.Bot.Validation;
using Compete.Model;
using Compete.Model.Repositories;
using Compete.Site.Filters;
using Compete.Site.Models;
using Compete.TeamManagement;

namespace Compete.Site.Controllers
{
    public class BotsController : Controller
    {
        readonly ITeamManagementCommands _teamCommands;
        readonly ITeamRepository _teamRepository;
        readonly IConfigurationRepository _configurationRepository;
        readonly ILeaderboardRepository _leaderboardRepository;

        public BotsController(ITeamManagementCommands teamCommands, ITeamRepository teamRepository, IConfigurationRepository configurationRepository, ILeaderboardRepository leaderboardRepository)
        {
            _teamCommands = teamCommands;
            _teamRepository = teamRepository;
            _configurationRepository = configurationRepository;
            _leaderboardRepository = leaderboardRepository;
        }

        [RequireAuthenticationFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Upload()
        {
            List<BotValidationError> errList = new List<BotValidationError>();
            String teamName = "<Nameless>";
            String dllPath = null; 

            if (Request.Files.Count > 0)
            {
                var hpf = Request.Files[0];
                if (hpf.ContentLength > 0)
                {

                    teamName = Team.ConvertFileToTeamName(hpf.FileName);
                    dllPath = BotValidator.ValidateBot(hpf, out errList);

                } else 
                    errList.Add(new BotValidationError("Bot file contents not received"));
            } else
                errList.Add(new BotValidationError("No bot file received"));

            // if there are no errors
            if (errList.Count == 0)
            {
                // add a team to reflect new file
                _teamCommands.New(teamName);
                new AssemblyFileRepository().Add(dllPath, teamName + ".dll");

            }
            // otherwise, return errors collection to appear in home page
            else
                TempData["BotErrors"] = errList;
            
            return Redirect("~/Home");
        }

        [RequireAuthenticationFilter]
        [AcceptVerbs(HttpVerbs.Get)]
        [OutputCache(Duration = 0, VaryByParam = "none")]
        public ActionResult Remove(String id)
        {
            Team t = _teamRepository.FindByTeamName(id);
            if (t != null)
            {
                _teamRepository.Remove(t);
                new AssemblyFileRepository().Remove(id + ".dll");

                var config = _configurationRepository.GetConfiguration();
                if (Team.ConvertFileToTeamName(config.SelectedBotPath) == id)
                    config.SelectedBotPath = "";
                _configurationRepository.SetConfiguration(config);

                _leaderboardRepository.SetLeaderboard(null);
            }
            return Redirect("~/Home");
        }

        [RequireAuthenticationFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public void Choose(String id)
        {
            AssemblyFileRepository _assemblyRepository = new AssemblyFileRepository();
            AssemblyFile current = _assemblyRepository.FindAllPlayers().
                Where(p => Team.ConvertFileToTeamName(p.Path).Equals(id, StringComparison.OrdinalIgnoreCase)).
                FirstOrDefault();
            if (current != default(AssemblyFile))
            {
                var config = _configurationRepository.GetConfiguration();
                config.SelectedBotPath = current.Path;
                _configurationRepository.SetConfiguration(config);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Transfer()
        {
            String domain = Request.Form["domain"];
            String signature = Request.Form["signature"];

            if ((domain == null) || (signature == null))
            {
                Response.StatusCode = 400;
                return null;
            }

            var config = _configurationRepository.GetConfiguration();
            if (config == null)
            {
                Response.StatusCode = 403;
                return null;
            }

            if (!BotValidator.ValidateSignature(domain + config.AdminPassword, signature))
            {
                Response.StatusCode = 403;
                return null;
            }

            if (System.IO.File.Exists(config.SelectedBotPath))
                return File(config.SelectedBotPath, "application/octet-stream");
            else
            {
                Response.StatusCode = 404;
                return null;
            }
        }
    }
}