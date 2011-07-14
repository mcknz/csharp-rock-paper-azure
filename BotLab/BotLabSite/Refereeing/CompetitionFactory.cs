using System.Collections.Generic;
using System.IO;
using Compete.Bot;
using Compete.Model.Game;
using Compete.Site.Infrastructure;
using Compete.Site.Models;
using Compete.Model;

namespace Compete.Site.Refereeing
{
    public class CompetitionFactory
    {
        public Competition CreateCompetition(AssemblyFile[] files)
        {
            DynamicAssemblyTypeFinder dynamicAssemblyTypeFinder = new DynamicAssemblyTypeFinder();
            dynamicAssemblyTypeFinder.AddAll(files);

            IGame game = new RockPaperScissorsPro.CompeteGameWrapper();

            Competition competition = new Competition(game);
            IEnumerable<IBotFactory> botFactoryList = dynamicAssemblyTypeFinder.Create<IBotFactory>();

            foreach (IBotFactory botFactory in botFactoryList)
            {
                //if unable to create botFactory, skip adding it to the list
                if (botFactory != null)
                {
                    string teamName = Team.ConvertFileToTeamName(botFactory.GetType().Assembly.Location);
                    competition.AddPlayer(new BotPlayer(teamName, null, botFactory));
                }
            }

            // look for any players that were downloaded but not in competition.  make sure they
            // get a bot that forfeits
            foreach (AssemblyFile f in files)
            {
                string teamName = Team.ConvertFileToTeamName(f.Path);
                if (!competition.ContainsPlayer(teamName))
                {
                    competition.AddPlayer(new BotPlayer(teamName, null, null));
                }
            }

            return competition;
        }
    }
}