using System;
using System.Collections.Generic;

namespace OlympicBasketballTournament
{
    class Program
    {
        static void Main(string[] args)
        {
            // Paths to JSON files
            string groupsFilePath = "./resources/groups.json";
            string exhibitionsFilePath = "./resources/exibitions.json";

            // Load group data
            Dictionary<string, List<Team>> groups = DataLoader.LoadGroups(groupsFilePath);

            // Load exhibition data
            Dictionary<string, List<Exhibition>> exhibitions = DataLoader.LoadExhibitions(exhibitionsFilePath);

            // Flatten the 'groups' dictionary into a single dictionary of ISOCode -> Team
            Dictionary<string, Team> teamsByISOCode = new Dictionary<string, Team>();
            foreach (var group in groups.Values)
            {
                foreach (var team in group)
                {
                    if (!teamsByISOCode.ContainsKey(team.ISOCode))
                    {
                        teamsByISOCode[team.ISOCode] = team;
                    }
                }
            }

            // Initialize team form based on exhibition matches
            TournamentSimulator.InitializeTeamForm(teamsByISOCode, exhibitions);

            // Simulate the group stage matches
            TournamentSimulator.SimulateGroupStage(groups);

            // Display final rankings after the group stage
            TournamentSimulator.DisplayGroupRankings(groups);

            // Rank the teams based on the group stage results
            List<Team> rankedTeams = TournamentSimulator.RankTeams(groups);

            // Draw elimination matches
            List<(Team, Team)> quarterFinals = TournamentSimulator.DrawEliminationMatches(rankedTeams);

            // Simulate the elimination stage
            TournamentSimulator.SimulateEliminationStage(quarterFinals);
        }
    }
}
