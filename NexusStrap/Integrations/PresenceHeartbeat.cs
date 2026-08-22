using System.Net.Http;
using System.Windows.Threading;

namespace NexusStrap.Integrations
{
    public static class PresenceHeartbeat
    {
        private static DispatcherTimer? _timer;
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };
        private static string _deviceId = "";

        public static void Start()
        {
            try
            {
                EnsureDeviceId();
                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
                _timer.Tick += async (s, e) => await BeatAsync();
                _timer.Start();
                _ = BeatAsync();
                // also heartbeat when Roblox watcher is active, keep timer running
                App.Logger.WriteLine("PresenceHeartbeat", $"Started for {_deviceId}");
            }
            catch { }
        }

        private static void EnsureDeviceId()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\NexusStrap");
                var existing = key.GetValue("DeviceId") as string;
                if (!string.IsNullOrWhiteSpace(existing)) _deviceId = existing;
                else { _deviceId = Guid.NewGuid().ToString("N"); key.SetValue("DeviceId", _deviceId); }
            }
            catch { _deviceId = Guid.NewGuid().ToString("N"); }
        }

        private static async Task BeatAsync()
        {
            try
            {
                var payload = new { deviceId = _deviceId, version = App.Version };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                await _http.PostAsync("https://nexusstrap.pages.dev/api/presence", content);
            }
            catch { }
        }

        public static void Stop()
        {
            try { _timer?.Stop(); } catch { }
        }
    }
}
