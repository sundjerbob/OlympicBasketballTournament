namespace OlympicBasketballTournament
{
	public class MatchResult
	{
		public Team Team1 { get; set; }
		public Team Team2 { get; set; }
		public int Team1Score { get; set; }  // Team1 score (-1 if they forfeited)
		public int Team2Score { get; set; }  // Team2 score (-1 if they forfeited)

		// Nullable ForfeitedTeam property to handle match forfeits
		public Team ForfeitedTeam { get; set; }

		// Constructor for regular matches
		public MatchResult(Team team1, Team team2, int team1Score, int team2Score)
		{
			Team1 = team1;
			Team2 = team2;
			Team1Score = team1Score;
			Team2Score = team2Score;
			ForfeitedTeam = null;
		}

		public MatchResult()
		{

		}
		// Constructor for forfeited matches
		public MatchResult(Team forfeitedTeam, Team winningTeam)
		{
			ForfeitedTeam = forfeitedTeam;
			Team1 = winningTeam;
			Team1Score = 0;   // Winner score for non-forfeited team
			Team2 = forfeitedTeam;
			Team2Score = -1;  // Loser forfeited, indicated by -1
		}

		// Check if the match was a forfeited match
		public bool IsForfeit()
		{
			return Team1Score == -1 || Team2Score == -1;
		}
	}
}
