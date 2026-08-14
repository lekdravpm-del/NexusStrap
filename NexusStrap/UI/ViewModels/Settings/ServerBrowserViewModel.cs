using CommunityToolkit.Mvvm.Input;
using NexusStrap.Enums;
using NexusStrap.Integrations;
using NexusStrap.Models.APIs.Roblox;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class ServerBrowserViewModel : NotifyPropertyChangedViewModel
    {
        public class GameBrowserItem
        {
            public long PlaceId { get; set; }
            public string Name { get; set; } = "Unknown Game";
        }

        public class FilterOption
        {
            public string Key { get; set; } = "";
            public string Display { get; set; } = "";
        }

        private readonly RobloxServerFetcher _fetcher = new();

        public ObservableCollection<GameBrowserItem> Games { get; } = new();
        public ObservableCollection<FilterOption> PlayerCountFilters { get; } = new();
        public ObservableCollection<FilterOption> RegionFilters { get; } = new();
        public ObservableCollection<FilterOption> UptimeFilters { get; } = new();
        public ObservableCollection<ServerInstance> Servers { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand JoinCommand { get; }

        private GameBrowserItem? _selectedGame;
        public GameBrowserItem? SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                OnPropertyChanged(nameof(SelectedGame));

                if (value != null)
                    App.Settings.Prop.SelectedServerFilter = ServerFilter.GameType;
            }
        }

        private string _customPlaceId = "";
        public string CustomPlaceId
        {
            get => _customPlaceId;
            set
            {
                _customPlaceId = value;
                OnPropertyChanged(nameof(CustomPlaceId));
            }
        }

        private string _selectedPlayerCountFilter = "any";
        public string SelectedPlayerCountFilter
        {
            get => _selectedPlayerCountFilter;
            set
            {
                _selectedPlayerCountFilter = value;
                OnPropertyChanged(nameof(SelectedPlayerCountFilter));
                App.Settings.Prop.ServerPlayerCountFilter = value;
                App.Settings.Prop.SelectedServerFilter = ServerFilter.PlayerCount;
            }
        }

        private string _selectedRegionFilter = "any";
        public string SelectedRegionFilter
        {
            get => _selectedRegionFilter;
            set
            {
                _selectedRegionFilter = value;
                OnPropertyChanged(nameof(SelectedRegionFilter));
                App.Settings.Prop.ServerRegionFilter = value;
                App.Settings.Prop.SelectedServerFilter = ServerFilter.Region;
            }
        }

        private string _selectedUptimeFilter = "any";
        public string SelectedUptimeFilter
        {
            get => _selectedUptimeFilter;
            set
            {
                _selectedUptimeFilter = value;
                OnPropertyChanged(nameof(SelectedUptimeFilter));
                App.Settings.Prop.ServerUptimeFilter = value;
                App.Settings.Prop.SelectedServerFilter = ServerFilter.Uptime;
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                ((RelayCommand)RefreshCommand).NotifyCanExecuteChanged();
                ((RelayCommand)LoadMoreCommand).NotifyCanExecuteChanged();
            }
        }

        private bool _hasMore;
        public bool HasMore
        {
            get => _hasMore;
            set
            {
                _hasMore = value;
                OnPropertyChanged(nameof(HasMore));
                ((RelayCommand)LoadMoreCommand).NotifyCanExecuteChanged();
            }
        }

        private bool _regionDetectionEnabled;
        public bool RegionDetectionEnabled
        {
            get => _regionDetectionEnabled;
            set
            {
                _regionDetectionEnabled = value;
                OnPropertyChanged(nameof(RegionDetectionEnabled));
            }
        }

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private string _nextCursor = "";

        public ServerBrowserViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsBusy);
            LoadMoreCommand = new RelayCommand(async () => await LoadMoreAsync(), () => !IsBusy && HasMore);
            JoinCommand = new RelayCommand<ServerInstance>(JoinServer);

            PlayerCountFilters.Add(new FilterOption { Key = "any", Display = Resources.Strings.ServerBrowser_FilterAny });
            PlayerCountFilters.Add(new FilterOption { Key = "empty", Display = Resources.Strings.ServerBrowser_FilterEmpty });
            PlayerCountFilters.Add(new FilterOption { Key = "quarter", Display = Resources.Strings.ServerBrowser_FilterQuarter });
            PlayerCountFilters.Add(new FilterOption { Key = "half", Display = Resources.Strings.ServerBrowser_FilterHalf });
            PlayerCountFilters.Add(new FilterOption { Key = "threequarter", Display = Resources.Strings.ServerBrowser_FilterThreeQuarter });
            PlayerCountFilters.Add(new FilterOption { Key = "full", Display = Resources.Strings.ServerBrowser_FilterFull });

            RegionFilters.Add(new FilterOption { Key = "any", Display = Resources.Strings.ServerBrowser_FilterAny });
            foreach (string region in RobloxServerFetcher.GetKnownRegions())
                RegionFilters.Add(new FilterOption { Key = region, Display = region });

            UptimeFilters.Add(new FilterOption { Key = "any", Display = Resources.Strings.ServerBrowser_FilterAny });
            UptimeFilters.Add(new FilterOption { Key = "under1", Display = Resources.Strings.ServerBrowser_FilterUptimeUnder1 });
            UptimeFilters.Add(new FilterOption { Key = "1to3", Display = Resources.Strings.ServerBrowser_FilterUptime1To3 });
            UptimeFilters.Add(new FilterOption { Key = "3to6", Display = Resources.Strings.ServerBrowser_FilterUptime3To6 });
            UptimeFilters.Add(new FilterOption { Key = "over6", Display = Resources.Strings.ServerBrowser_FilterUptimeOver6 });

            RestoreSavedFilters();
            LoadGames();
        }

        private void RestoreSavedFilters()
        {
            if (SelectOption(PlayerCountFilters, App.Settings.Prop.ServerPlayerCountFilter))
                _selectedPlayerCountFilter = App.Settings.Prop.ServerPlayerCountFilter;

            if (SelectOption(RegionFilters, App.Settings.Prop.ServerRegionFilter))
                _selectedRegionFilter = App.Settings.Prop.ServerRegionFilter;

            if (SelectOption(UptimeFilters, App.Settings.Prop.ServerUptimeFilter))
                _selectedUptimeFilter = App.Settings.Prop.ServerUptimeFilter;
        }

        private static bool SelectOption(ObservableCollection<FilterOption> options, string key)
        {
            return !string.IsNullOrWhiteSpace(key) && options.Any(o => o.Key == key);
        }

        private void LoadGames()
        {
            string historyPath = System.IO.Path.Combine(Paths.Cache, "GameHistory.json");
            if (!System.IO.File.Exists(historyPath)) return;

            try
            {
                string json = System.IO.File.ReadAllText(historyPath);
                var entries = System.Text.Json.JsonSerializer.Deserialize<List<Models.GameHistoryData>>(json);
                if (entries == null) return;

                foreach (var group in entries.Where(e => e.PlaceId > 0)
                                             .GroupBy(e => e.PlaceId)
                                             .OrderByDescending(g => g.Max(e => e.TimeJoined)))
                {
                    var latest = group.OrderByDescending(e => e.TimeJoined).First();
                    Games.Add(new GameBrowserItem
                    {
                        PlaceId = group.Key,
                        Name = !string.IsNullOrEmpty(latest.GameName) ? latest.GameName : "Unknown Game"
                    });
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ServerBrowserViewModel::LoadGames", ex);
            }
        }

        private long ResolvePlaceId()
        {
            if (!string.IsNullOrWhiteSpace(CustomPlaceId) && long.TryParse(CustomPlaceId.Trim(), out long customId) && customId > 0)
                return customId;

            if (SelectedGame != null)
                return SelectedGame.PlaceId;

            return Games.FirstOrDefault()?.PlaceId ?? 0;
        }

        private async Task RefreshAsync()
        {
            if (IsBusy) return;

            long placeId = ResolvePlaceId();
            if (placeId <= 0)
            {
                StatusText = Resources.Strings.ServerBrowser_NoGame;
                return;
            }

            IsBusy = true;
            try
            {
                Servers.Clear();
                _nextCursor = "";
                await FetchPageAsync(placeId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMoreAsync()
        {
            if (IsBusy || string.IsNullOrEmpty(_nextCursor)) return;

            long placeId = ResolvePlaceId();
            if (placeId <= 0) return;

            IsBusy = true;
            try
            {
                await FetchPageAsync(placeId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task FetchPageAsync(long placeId)
        {
            string cookie = "";

            var account = AccountManager.Shared.ActiveAccount;
            if (account != null)
                cookie = AccountManager.Shared.GetRoblosecurityForUser(account.UserId) ?? "";

            RegionDetectionEnabled = !string.IsNullOrWhiteSpace(cookie);

            try
            {
                var result = await _fetcher.FetchServerInstancesAsync(placeId, cookie, _nextCursor);

                if (result == null || !result.Servers.Any())
                {
                    StatusText = Resources.Strings.ServerBrowser_NoServers;
                    HasMore = false;
                    return;
                }

                _nextCursor = result.NextCursor;
                HasMore = !string.IsNullOrEmpty(_nextCursor);

                foreach (var server in result.Servers.Where(MatchesFilters))
                    Servers.Add(server);

                StatusText = string.Format(Resources.Strings.ServerBrowser_Status, Servers.Count);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ServerBrowser", ex);
                StatusText = Resources.Strings.ServerBrowser_FetchFailed;
                HasMore = false;
            }
        }

        private bool MatchesFilters(ServerInstance server)
        {
            switch (SelectedPlayerCountFilter)
            {
                case "empty":
                    if (server.Playing > 0) return false;
                    break;
                case "quarter":
                    if (server.Capacity >= 0.25) return false;
                    break;
                case "half":
                    if (server.Capacity >= 0.50) return false;
                    break;
                case "threequarter":
                    if (server.Capacity >= 0.75) return false;
                    break;
                case "full":
                    if (server.Capacity < 0.75) return false;
                    break;
            }

            if (SelectedRegionFilter != "any" && !string.Equals(server.Region, SelectedRegionFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (SelectedUptimeFilter != "any")
            {
                if (server.FirstSeen == null)
                    return false;

                var age = DateTime.UtcNow - server.FirstSeen.Value;

                switch (SelectedUptimeFilter)
                {
                    case "under1":
                        if (age >= TimeSpan.FromHours(1)) return false;
                        break;
                    case "1to3":
                        if (age < TimeSpan.FromHours(1) || age >= TimeSpan.FromHours(3)) return false;
                        break;
                    case "3to6":
                        if (age < TimeSpan.FromHours(3) || age >= TimeSpan.FromHours(6)) return false;
                        break;
                    case "over6":
                        if (age < TimeSpan.FromHours(6)) return false;
                        break;
                }
            }

            return true;
        }

        private void JoinServer(ServerInstance? server)
        {
            if (server == null || string.IsNullOrEmpty(server.Id)) return;

            long placeId = ResolvePlaceId();
            if (placeId <= 0) return;

            var deeplink = $"roblox://experiences/start?placeId={placeId}&gameInstanceId={server.Id}";
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(deeplink) { UseShellExecute = true }); }
            catch (Exception ex) { App.Logger.WriteException("ServerBrowserViewModel::JoinServer", ex); }
        }
    }
}