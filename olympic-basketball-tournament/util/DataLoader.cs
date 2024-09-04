using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OlympicBasketballTournament
{
    public static class DataLoader
    {
        public static Dictionary<string, List<Exhibition>> LoadExhibitions(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, List<Exhibition>>>(json);
        }

        public static Dictionary<string, List<Team>> LoadGroups(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, List<Team>>>(json);
        }
    }
}
