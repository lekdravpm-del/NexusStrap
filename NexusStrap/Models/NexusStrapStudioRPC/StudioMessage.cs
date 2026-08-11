namespace NexusStrap.Models.NexusStrapStudioRPC;

public class StudioMessage
{
    [JsonPropertyName("command")]
    public string StudioCommand { get; set; } = null!;

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}
