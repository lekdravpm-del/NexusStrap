namespace NexusStrap.Models
{
    public class ServerHistoryEntry
    {
        [JsonPropertyName("GameName")]
        public string GameName { get; set; } = string.Empty;

        [JsonPropertyName("PlaceId")]
        public long PlaceId { get; set; }

        [JsonPropertyName("JobId")]
        public string JobId { get; set; } = string.Empty;

        [JsonPropertyName("ServerType")]
        public int ServerType { get; set; }

        [JsonPropertyName("TimeJoined")]
        public DateTime TimeJoined { get; set; }

        [JsonPropertyName("TimeLeft")]
        public DateTime? TimeLeft { get; set; }

        [JsonPropertyName("UniverseId")]
        public long UniverseId { get; set; }

        public string TimeJoinedDisplay => TimeJoined.ToLocalTime().ToString("MMM dd, yyyy HH:mm");
        public string Duration => TimeLeft.HasValue ? (TimeLeft.Value - TimeJoined).ToString(@"hh\:mm\:ss") : "In progress";
        public string ServerTypeDisplay => ServerType switch
        {
            0 => "Public",
            1 => "Private",
            2 => "Reserved",
            _ => "Unknown"
        };
    }
}
