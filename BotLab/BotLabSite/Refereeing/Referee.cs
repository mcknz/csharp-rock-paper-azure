﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compete.Core.Infrastructure;
using Compete.Model.Game;
using Compete.Site.Infrastructure;
using Compete.Site.Models;

using Microsoft.Practices.ServiceLocation;

namespace Compete.Site.Refereeing
{
    public class Referee
    {
        readonly RoundParameters _parameters;

        public Referee(RoundParameters parameters)
        {
            _parameters = parameters;
        }

        public void StartRound()
        {
            using (var staging = new StagingArea(_parameters.AssemblyFiles))
            {
                var round = AppDomainHelper.InSeparateAppDomain<RoundParameters, IEnumerable<MatchResult>>(staging.Root, new RoundParameters(staging.StagedAssemblies, _parameters.TeamNames), RunRound);
                ServiceLocator.Current.GetInstance<IScoreKeeper>().Record(round);
            }
        }

        private static IEnumerable<MatchResult> RunRound(RoundParameters parameters)
        {
            // preload assemblies for DLR support
            //RockPaperAzure.DlrPreload.LoadFauxBots();
            
            var competitionFactory = new CompetitionFactory();
            var competition = competitionFactory.CreateCompetition(parameters.AssemblyFiles);
            return competition.PlayRound(parameters.TeamNames);
        }
    }

    [Serializable]
    public class RoundParameters
    {
        public AssemblyFile[] AssemblyFiles
        {
            get;
            private set;
        }

        public string[] TeamNames
        {
            get;
            private set;
        }

        public RoundParameters(AssemblyFile[] files, string[] teamNames)
        {
            AssemblyFiles = files;
            TeamNames = teamNames;
        }

        public static RoundParameters Merge(params RoundParameters[] parameters)
        {
            List<string> teamNames = new List<string>();
            List<AssemblyFile> assemblyFiles = new List<AssemblyFile>();
            foreach (var p in parameters)
            {
                teamNames.AddRange(p.TeamNames);
                assemblyFiles.AddRange(p.AssemblyFiles);
            }
            return new RoundParameters(assemblyFiles.Distinct().ToArray(), teamNames.Distinct().ToArray());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string name in this.TeamNames)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(name);
            }
            return sb.ToString();
        }
    }
}