/* Team.cs */
using System.Text.Json.Serialization;



namespace OlympicBasketballTournament
{
    public class Team
    {
        [JsonPropertyName("Team")]
        public string Name { get; set; }

        public string ISOCode { get; set; }



        public int Wins { get; set; }
        
        public int Losses { get; set; }
        
        public int Points { get; set; }
        
        public int PointsScored { get; set; }
        
        public int PointsConceded { get; set; }
        
        public int Ranking { get; set; } // FIBA ranking
        
        public float Form { get; set; }   // New: team form
        
        public int PointDifference => PointsScored - PointsConceded; // Calculate point difference

        public List<MatchResult> MatchHistory { get; set; } = new List<MatchResult>(); // For mutual meeting tiebreaker

        public Team(string name, int ranking)
        {
            Name = name;
            Ranking = ranking;
            Form = 0.5f; // Default form before being calculated from exhibitions
        }
    }

}
