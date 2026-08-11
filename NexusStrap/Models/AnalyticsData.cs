namespace NexusStrap.Models
{
    public class PlaySessionData
    {
        [JsonPropertyName("GameName")]
        public string GameName { get; set; } = string.Empty;

        [JsonPropertyName("UniverseId")]
        public long UniverseId { get; set; }

        [JsonPropertyName("PlaceId")]
        public long PlaceId { get; set; }

        [JsonPropertyName("TimeJoined")]
        public DateTime TimeJoined { get; set; }

        [JsonPropertyName("TimeLeft")]
        public DateTime? TimeLeft { get; set; }

        public double MinutesPlayed => TimeLeft.HasValue ? (TimeLeft.Value - TimeJoined).TotalMinutes : 0;
    }

    public class DailyPlayStats
    {
        public DateTime Date { get; set; }
        public double TotalMinutes { get; set; }
        public int SessionsCount { get; set; }
        public string MostPlayedGame { get; set; } = string.Empty;
    }

    public class GamePlayStats
    {
        public string GameName { get; set; } = string.Empty;
        public long UniverseId { get; set; }
        public double TotalMinutes { get; set; }
        public int SessionCount { get; set; }
        public DateTime LastPlayed { get; set; }
    }
}
