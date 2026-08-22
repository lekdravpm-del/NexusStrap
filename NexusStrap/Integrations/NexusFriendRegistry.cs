using Newtonsoft.Json;

namespace NexusStrap.Integrations
{
    public record FriendRegistryEntry(long UserId, string Username, string DisplayName, DateTime LastSeen);

    public static class NexusFriendRegistry
    {
        private static string FilePath => Path.Combine(Paths.Cache, "NexusFriendRegistry.json");

        public static void Register(long userId, string username, string displayName)
        {
            try
            {
                var dict = Load();
                dict[userId] = new FriendRegistryEntry(userId, username, displayName, DateTime.UtcNow);
                Save(dict);
                App.Logger.WriteLine("NexusFriendRegistry", $"Registered {username} ({userId})");
            }
            catch (Exception ex) { App.Logger.WriteException("NexusFriendRegistry::Register", ex); }
        }

        public static bool IsRegistered(long userId)
        {
            var dict = Load();
            return dict.ContainsKey(userId);
        }

        public static FriendRegistryEntry? Get(long userId)
        {
            var dict = Load();
            dict.TryGetValue(userId, out var e);
            return e;
        }

        public static Dictionary<long, FriendRegistryEntry> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new();
                var json = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(json)) return new();
                return JsonConvert.DeserializeObject<Dictionary<long, FriendRegistryEntry>>(json) ?? new();
            }
            catch { return new(); }
        }

        private static void Save(Dictionary<long, FriendRegistryEntry> dict)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(dict, Formatting.Indented));
            }
            catch { }
        }
    }
}
