using System.Text.Json.Serialization;

namespace OlympicBasketballTournament
{
	public class Exhibition
	{
		[JsonPropertyName("Date")]
		public string Date { get; set; }

		[JsonPropertyName("Opponent")]
		public string Opponent { get; set; }

		[JsonPropertyName("Result")]
		public string Result { get; set; }

		public Exhibition() { }

		public (int teamScore, int opponentScore) GetScores()
		{
			string[] scores = Result.Split("-");
			return (int.Parse(scores[0]), int.Parse(scores[1]));
		}
	}
}
