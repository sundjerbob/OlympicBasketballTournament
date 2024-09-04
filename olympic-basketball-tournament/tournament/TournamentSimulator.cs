using System;
using System.Collections.Generic;
using System.Linq;

namespace OlympicBasketballTournament
{
    public static class TournamentSimulator
    {
        public static void InitializeTeamForm(Dictionary<string, Team> teams, Dictionary<string, List<Exhibition>> exhibitions)
        {
            foreach (var team in teams.Values)
            {
                if (exhibitions.ContainsKey(team.ISOCode))
                {
                    var teamExhibitions = exhibitions[team.ISOCode];
                    int gamesWon = 0, gamesPlayed = teamExhibitions.Count;
                    int teamPoints = 0, totalPoints = 0;

                    foreach (var exhibition in teamExhibitions)
                    {
                        var (teamScore, opponentScore) = exhibition.GetScores();

                        if (teamScore > opponentScore)
                            gamesWon++;

                        teamPoints += teamScore;
                        totalPoints += teamScore + opponentScore;
                    }

                    // Calculate win and point frequencies
                    float winFrequency = gamesWon / (float)gamesPlayed;
                    float pointFrequency = teamPoints / (float)totalPoints;

                    // Set team form based on win and point frequency
                    team.Form = winFrequency * pointFrequency;
                }
            }
        }

        

        public static MatchResult SimulateMatch(Team team1, Team team2)
        {
           float oddsTeam1 = team1.Form;
           float oddsTeam2 = team2.Form;

           // Adjust odds based on relative form difference
           float formDifference = (team1.Form - team2.Form) * 0.1f;
           oddsTeam1 += formDifference;
           oddsTeam2 -= formDifference;

           Random random = new Random();

           // Calculate forfeit probabilities based on form
           float forfeitProbability1 = (1 - oddsTeam1) * 0.005f;
           float forfeitProbability2 = (1 - oddsTeam2) * 0.005f;

           // Check if team1 forfeits
           if (random.NextDouble() < forfeitProbability1)
           {
               return new MatchResult
               {
                   Team1 = team1,
                   Team2 = team2,
                   Team1Score = -1, // Indicate team1 forfeits
                   Team2Score = 0 // Team2 wins by forfeit
               };
           }

           // Check if team2 forfeits
           if (random.NextDouble() < forfeitProbability2)
           {
               return new MatchResult
               {
                   Team1 = team1,
                   Team2 = team2,
                   Team1Score = 0, // Team1 wins by forfeit
                   Team2Score = -1 // Indicate team2 forfeits
               };
           }

           // Simulate regular match if no one forfeits
           int score1 = Math.Clamp((int)(random.Next(70, 100) * (1 + oddsTeam1)), 70, 120);
           int score2 = Math.Clamp((int)(random.Next(70, 100) * (1 + oddsTeam2)), 70, 120);

           // Ensure no draw using odds
           if (score1 == score2)
           {
               // Favor the team with higher odds
               float totalOdds = oddsTeam1 + oddsTeam2;
               float oddsThreshold = oddsTeam1 / totalOdds; // Ratio of oddsTeam1 to total odds

               if (random.NextDouble() < oddsThreshold)
               {
                   score1 += 1; // Team1 wins
               }
               else
               {
                   score2 += 1; // Team2 wins
               }
           }

           return new MatchResult
           {
               Team1 = team1,
               Team2 = team2,
               Team1Score = score1,
               Team2Score = score2
           };
       }




        public static void DisplayMatchResult(MatchResult result)
        {
            if (result.ForfeitedTeam != null)
            {
                Console.WriteLine($"{result.Team1.Name} vs {result.Team2.Name}: {result.ForfeitedTeam.Name} forfeited");
            }
            else
            {
                Console.WriteLine($"{result.Team1.Name} - {result.Team2.Name} ({result.Team1Score}:{result.Team2Score})");
            }
        }


        public static List<Team> RankTeams(Dictionary<string, List<Team>> groups)
        {
            var rankedTeams = new List<Team>();

            foreach (var group in groups.Values)
            {
                var rankedGroup = group.OrderByDescending(t => t.Points)
                                       .ThenByDescending(t => t.PointsScored - t.PointsConceded)
                                       .ThenByDescending(t => t.PointsScored)
                                       .ToList();

                rankedTeams.AddRange(rankedGroup);
            }

            return rankedTeams;
        }


        private static void UpdateTeamStats(MatchResult result)
        {
            if (result.IsForfeit())
            {
                // If the match was forfeited, assign 0 points to the forfeiting team and 2 points to the winning team
                if (result.Team1Score == -1)
                {
                    result.Team1.Points += 0;  // Forfeiting team
                    result.Team2.Points += 2;  // Winning team
                }
                else
                {
                    result.Team2.Points += 0;  // Forfeiting team
                    result.Team1.Points += 2;  // Winning team
                }
            }
            else
            {
                // Regular match logic
                if (result.Team1Score > result.Team2Score)
                {
                    result.Team1.Wins++;
                    result.Team2.Losses++;
                    result.Team1.Points += 2;  // 2 points to win
                    result.Team2.Points += 1;  // 1 point for a loss
                }
                else
                {
                    result.Team2.Wins++;
                    result.Team1.Losses++;
                    result.Team2.Points += 2;
                    result.Team1.Points += 1;
                }
            }

            // Update points scored/conceded for both teams
            if (result.Team1Score != -1 && result.Team2Score != -1)
            {
                result.Team1.PointsScored += result.Team1Score;
                result.Team1.PointsConceded += result.Team2Score;

                result.Team2.PointsScored += result.Team2Score;
                result.Team2.PointsConceded += result.Team1Score;
            }
        }


        public static void SimulateGroupStage(Dictionary<string, List<Team>> groups)
        {
            foreach (var group in groups)
            {
                Console.WriteLine($"\nGroup {group.Key}:");
                for (int i = 0; i < group.Value.Count; i++)
                {
                    for (int j = i + 1; j < group.Value.Count; j++)
                    {
                        Team team1 = group.Value[i];
                        Team team2 = group.Value[j];

                        Console.WriteLine($"Simulating match between {team1.Name} and {team2.Name}");

                        MatchResult result = SimulateMatch(team1, team2);
                        DisplayMatchResult(result);
                        UpdateTeamStats(result);
                    }
                }
            }
        }

        public static void DisplayFinalRanking(Dictionary<string, List<Team>> groups)
        {
            foreach (var group in groups)
            {
                Console.WriteLine($"\nFinal ranking in Group {group.Key}:");

                var rankedTeams = group.Value.OrderByDescending(t => t.Points)
                                             .ThenByDescending(t => t.PointsScored - t.PointsConceded)
                                             .ThenByDescending(t => t.PointsScored)
                                             .ToList();

                for (int i = 0; i < rankedTeams.Count; i++)
                {
                    var team = rankedTeams[i];
                    Console.WriteLine($"{i + 1}. {team.Name} {team.Wins} / {team.Losses} / {team.Points} / {team.PointsScored} / {team.PointsConceded} / {team.PointDifference}");
                }
            }
        }

        public static List<(Team, Team)> DrawEliminationMatches(List<Team> rankedTeams)
        {
            var hatD = rankedTeams.Take(2).ToList(); // Teams ranked 1 and 2
            var hatE = rankedTeams.Skip(2).Take(2).ToList(); // Teams ranked 3 and 4
            var hatF = rankedTeams.Skip(4).Take(2).ToList(); // Teams ranked 5 and 6
            var hatG = rankedTeams.Skip(6).Take(2).ToList(); // Teams ranked 7 and 8

            var quarterFinals = new List<(Team, Team)>();

            var random = new Random();

            quarterFinals.Add((hatD[0], hatG[random.Next(hatG.Count)]));
            hatG.Remove(quarterFinals.Last().Item2);

            quarterFinals.Add((hatD[1], hatG[random.Next(hatG.Count)]));
            hatG.Remove(quarterFinals.Last().Item2);

            quarterFinals.Add((hatE[0], hatF[random.Next(hatF.Count)]));
            hatF.Remove(quarterFinals.Last().Item2);

            quarterFinals.Add((hatE[1], hatF[random.Next(hatF.Count)]));

            return quarterFinals;
        }



        public static void SimulateEliminationStage(List<(Team, Team)> quarterFinals)
        {
            if (quarterFinals == null || quarterFinals.Count != 4)
            {
                Console.WriteLine("Error: The quarterfinals list is invalid.");
                return;
            }

            var semiFinals = new List<(Team, Team)>();
            Team winner1 = null, winner2 = null, winner3 = null, winner4 = null;

            Console.WriteLine("Quarterfinals:");
            foreach (var match in quarterFinals)
            {
                if (match.Item1 == null || match.Item2 == null)
                {
                    Console.WriteLine("Error: One of the quarterfinal teams is null.");
                    return;
                }

                var result = SimulateMatch(match.Item1, match.Item2);
                DisplayMatchResult(result);

                var winner = result.Team1Score > result.Team2Score ? result.Team1 : result.Team2;
                if (winner == null)
                {
                    Console.WriteLine("Error: The winner of the match is null.");
                    return;
                }

                // Assign winners to appropriate semifinal spots
                if (winner1 == null) winner1 = winner;
                else if (winner2 == null) winner2 = winner;
                else if (winner3 == null) winner3 = winner;
                else if (winner4 == null) winner4 = winner;
            }

            // Ensure that all semifinalists are valid
            if (winner1 == null || winner2 == null || winner3 == null || winner4 == null)
            {
                Console.WriteLine("Error: Not all semifinal spots are filled.");
                return;
            }

            Console.WriteLine("\nSemifinals setup:");
            semiFinals.Add((winner1, winner2)); // First semifinal pair
            semiFinals.Add((winner3, winner4)); // Second semifinal pair

            foreach (var match in semiFinals)
            {
                Console.WriteLine($"Semi-final pair: {match.Item1?.Name} vs {match.Item2?.Name}");
                if (match.Item1 == null || match.Item2 == null)
                {
                    Console.WriteLine("Error detected: A team in the semifinals is null before match execution.");
                    return;
                }
            }

            var semiFinalResults = new List<MatchResult>();
            foreach (var match in semiFinals)
            {
                var result = SimulateMatch(match.Item1, match.Item2);
                DisplayMatchResult(result);

                if (result.Team1 == null || result.Team2 == null)
                {
                    Console.WriteLine("Error: One of the semi-final results has a null team.");
                    return;
                }

                semiFinalResults.Add(result);
            }

            if (semiFinalResults.Count == 2)
            {
                var final = new List<(Team, Team)>();
                var bronzeMatch = new List<(Team, Team)>();

                var finalTeam1 = semiFinalResults[0].Team1Score > semiFinalResults[0].Team2Score ? semiFinalResults[0].Team1 : semiFinalResults[0].Team2;
                var finalTeam2 = semiFinalResults[1].Team1Score > semiFinalResults[1].Team2Score ? semiFinalResults[1].Team1 : semiFinalResults[1].Team2;

                var bronzeTeam1 = semiFinalResults[0].Team1Score < semiFinalResults[0].Team2Score ? semiFinalResults[0].Team1 : semiFinalResults[0].Team2;
                var bronzeTeam2 = semiFinalResults[1].Team1Score < semiFinalResults[1].Team2Score ? semiFinalResults[1].Team1 : semiFinalResults[1].Team2;

                // Ensure no duplicate medals are awarded
                var alreadyAwardedTeams = new HashSet<Team>();

                // Gold and Silver medal match
                Console.WriteLine("\nFinals:");
                var finalResult = SimulateMatch(finalTeam1, finalTeam2);
                DisplayMatchResult(finalResult);

                var goldTeam = finalResult.Team1Score > finalResult.Team2Score ? finalResult.Team1 : finalResult.Team2;
                var silverTeam = finalResult.Team1Score < finalResult.Team2Score ? finalResult.Team1 : finalResult.Team2;

                if (!alreadyAwardedTeams.Contains(goldTeam))
                {
                    Console.WriteLine($"Gold: {goldTeam.Name}");
                    alreadyAwardedTeams.Add(goldTeam);
                }

                if (!alreadyAwardedTeams.Contains(silverTeam))
                {
                    Console.WriteLine($"Silver: {silverTeam.Name}");
                    alreadyAwardedTeams.Add(silverTeam);
                }

                // Bronze medal match
                Console.WriteLine("\nThird place match:");
                var bronzeResult = SimulateMatch(bronzeTeam1, bronzeTeam2);
                DisplayMatchResult(bronzeResult);

                var bronzeTeam = bronzeResult.Team1Score > bronzeResult.Team2Score ? bronzeResult.Team1 : bronzeResult.Team2;

                if (!alreadyAwardedTeams.Contains(bronzeTeam))
                {
                    Console.WriteLine($"Bronze: {bronzeTeam.Name}");
                    alreadyAwardedTeams.Add(bronzeTeam);
                }
                else
                {
                    Console.WriteLine("Error: A team cannot win two medals.");
                }
            }
            else
            {
                Console.WriteLine("Error: Unexpected number of semi-final results.");
                return;
            }
        }




        public static void DisplayGroupRankings(Dictionary<string, List<Team>> groups)
        {
            Console.WriteLine("\nFinal ranking in groups:");

            foreach (var group in groups)
            {
                Console.WriteLine($"Group {group.Key} (Name - wins/losses/group-points/points scored/points conceded/point difference):");

                var rankedTeams = group.Value.OrderByDescending(t => t.Points)
                                            .ThenByDescending(t => t.PointDifference)
                                            .ThenByDescending(t => t.PointsScored)
                                            .ToList();

                for (int i = 0; i < rankedTeams.Count; i++)
                {
                    var team = rankedTeams[i];
                    Console.WriteLine($"{i + 1}. {team.Name} {team.Wins} / {team.Losses} / {team.Points} / {team.PointsScored} / {team.PointsConceded} / {team.PointDifference}");
                }

                Console.WriteLine(); // Print an empty line after each group
            }
        }


        public static List<MatchResult> SimulateFinals(List<(Team, Team)> semiFinals)
        {
            var finalResults = new List<MatchResult>();

            Console.WriteLine("\nSemifinals:");
            foreach (var match in semiFinals)
            {
                var result = SimulateMatch(match.Item1, match.Item2);
                DisplayMatchResult(result);
                finalResults.Add(result);
            }

            return finalResults;
        }

        public static void DisplayFinalResults(List<MatchResult> semiFinalResults)
        {
            var final = new List<(Team, Team)>();
            var bronzeMatch = new List<(Team, Team)>();

            var finalTeam1 = semiFinalResults[0].Team1Score > semiFinalResults[0].Team2Score ? semiFinalResults[0].Team1 : semiFinalResults[0].Team2;
            var finalTeam2 = semiFinalResults[1].Team1Score > semiFinalResults[1].Team2Score ? semiFinalResults[1].Team1 : semiFinalResults[1].Team2;

            var bronzeTeam1 = semiFinalResults[0].Team1Score < semiFinalResults[0].Team2Score ? semiFinalResults[0].Team1 : semiFinalResults[0].Team2;
            var bronzeTeam2 = semiFinalResults[1].Team1Score < semiFinalResults[1].Team2Score ? semiFinalResults[1].Team1 : semiFinalResults[1].Team2;

            final.Add((finalTeam1, finalTeam2));
            bronzeMatch.Add((bronzeTeam1, bronzeTeam2));

            Console.WriteLine("\nThird place match:");
            var bronzeResult = SimulateMatch(bronzeMatch[0].Item1, bronzeMatch[0].Item2);
            DisplayMatchResult(bronzeResult);

            Console.WriteLine("\nFinals:");
            var finalResult = SimulateMatch(final[0].Item1, final[0].Item2);
            DisplayMatchResult(finalResult);

            Console.WriteLine("\nMedals:");
            Console.WriteLine($"Gold: {finalResult.Team1.Name}");
            Console.WriteLine($"Silver: {finalResult.Team2.Name}");
            Console.WriteLine($"Bronze: {(bronzeResult.Team1Score > bronzeResult.Team2Score ? bronzeResult.Team1.Name : bronzeResult.Team2.Name)}");
        }
    }
}
