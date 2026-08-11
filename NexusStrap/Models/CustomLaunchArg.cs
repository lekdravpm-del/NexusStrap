namespace NexusStrap.Models
{
    public class CustomLaunchArg
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Argument")]
        public string Argument { get; set; } = string.Empty;

        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;
    }
}
