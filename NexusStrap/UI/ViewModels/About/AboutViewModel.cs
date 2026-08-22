using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace NexusStrap.UI.ViewModels.About
{
    public partial class AboutViewModel : ObservableObject
    {
        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;
            
        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"https://github.com/{App.ProjectRepository}/commit/{BuildMetadata.CommitHash}";

        public Visibility BuildInformationVisibility => App.IsProductionBuild ? Visibility.Collapsed : Visibility.Visible;
        public Visibility BuildCommitVisibility => App.IsActionBuild ? Visibility.Visible : Visibility.Collapsed;

        [ObservableProperty] private int _activePlayersOnline = 1;
        [ObservableProperty] private int _totalMembers = 1;
        [ObservableProperty] private int _inactivePlayers = 0;
        [ObservableProperty] private bool _isLoadingActive = false;
        [ObservableProperty] private string _activeStatus = "Live — you are online";

        private System.Windows.Threading.DispatcherTimer? _activeTimer;
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };
        private static string _deviceId = "";

        public AboutViewModel()
        {
            EnsureDeviceId();
            _activeTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _activeTimer.Tick += async (s, e) => await FetchActivePlayersAsync();
            _activeTimer.Start();
            _ = FetchActivePlayersAsync();
            _ = HeartbeatAsync();
        }

        private static void EnsureDeviceId()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\NexusStrap");
                var existing = key.GetValue("DeviceId") as string;
                if (!string.IsNullOrWhiteSpace(existing))
                {
                    _deviceId = existing;
                }
                else
                {
                    _deviceId = Guid.NewGuid().ToString("N");
                    key.SetValue("DeviceId", _deviceId);
                }
            }
            catch { _deviceId = Guid.NewGuid().ToString("N"); }
        }

        private async Task HeartbeatAsync()
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

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public async Task FetchActivePlayersAsync()
        {
            try
            {
                IsLoadingActive = true;
                ActiveStatus = "Syncing...";
                await HeartbeatAsync();
                try
                {
                    var resp = await _http.GetAsync("https://nexusstrap.pages.dev/api/presence");
                    if (resp.IsSuccessStatusCode)
                    {
                        var body = await resp.Content.ReadAsStringAsync();
                        var jo = JObject.Parse(body);
                        ActivePlayersOnline = jo["online"]?.Value<int>() ?? 1;
                        TotalMembers = jo["total"]?.Value<int>() ?? 1;
                        if (ActivePlayersOnline < 1) ActivePlayersOnline = 1;
                        if (TotalMembers < ActivePlayersOnline) TotalMembers = ActivePlayersOnline;
                        InactivePlayers = Math.Max(0, TotalMembers - ActivePlayersOnline);
                        ActiveStatus = "Live — strap heartbeat";
                        return;
                    }
                }
                catch { }
                // fallback: single user (you) as requested
                ActivePlayersOnline = 1;
                TotalMembers = 1;
                InactivePlayers = 0;
                ActiveStatus = "Live — you are online (single user)";
            }
            catch
            {
                ActivePlayersOnline = 1;
                TotalMembers = 1;
                InactivePlayers = 0;
                ActiveStatus = "Live — you are online";
            }
            finally
            {
                IsLoadingActive = false;
            }
        }
    }
}
