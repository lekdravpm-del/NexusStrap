namespace NexusStrap.Models
{
    public class FlagConflict
    {
        [JsonPropertyName("FlagA")]
        public string FlagA { get; set; } = string.Empty;

        [JsonPropertyName("FlagB")]
        public string FlagB { get; set; } = string.Empty;

        [JsonPropertyName("Reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("Severity")]
        public string Severity { get; set; } = "Warning";
    }
}
