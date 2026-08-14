namespace NexusStrap.Models
{
    public class FFlagTemplate
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public Dictionary<string, string> Flags { get; set; } = new();
        public List<string> FlagsToRemove { get; set; } = new();

        [JsonIgnore]
        public int ChangeCount => Flags.Count + FlagsToRemove.Count;
    }
}
