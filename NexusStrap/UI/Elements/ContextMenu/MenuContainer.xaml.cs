using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using NexusStrap.Integrations;
using NexusStrap.Models.Entities;

namespace NexusStrap.UI.Elements.ContextMenu
{
    public partial class MenuContainer
    {
        private readonly Watcher _watcher;
        private ActivityWatcher? _activityWatcher => _watcher.ActivityWatcher;

        private ServerInformation? _serverInformationWindow;
        private GameInformation? _gameInformationWindow;
        private ServerHistory? _gameHistoryWindow;

        private Stopwatch _totalPlaytimeStopwatch = new Stopwatch();
        private TimeSpan _accumulatedTotalPlaytime = TimeSpan.Zero;
        private DispatcherTimer? _playtimeTimer;
        private DateTime? _studioPlaceJoinTime = null;

        private static readonly string BookmarksPath = Path.Combine(Paths.Base, "Bookmarks.json");

        public MenuContainer(Watcher watcher)
        {
            InitializeComponent();
            _watcher = watcher;

            if (_activityWatcher is not null)
            {
                _activityWatcher.OnGameJoin += ActivityWatcher_OnGameJoin;
                _activityWatcher.OnGameLeave += ActivityWatcher_OnGameLeave;
                _activityWatcher.OnStudioPlaceOpened += ActivityWatcher_OnStudioPlaceOpened;
                _activityWatcher.OnStudioPlaceClosed += ActivityWatcher_OnStudioPlaceClosed;

                if (_activityWatcher.InRobloxStudio)
                {
                    InviteDeeplinkMenuItem.Visibility = Visibility.Collapsed;
                    ServerDetailsMenuItem.Visibility = Visibility.Collapsed;
                    GameInformationMenuItem.Visibility = Visibility.Collapsed;
                    GameHistoryMenuItem.Visibility = Visibility.Collapsed;
                    RegionMenuRoot.Visibility = Visibility.Collapsed;

                    if (App.Settings.Prop.PlaytimeCounter)
                    {
                        StartTotalPlaytimeTimer();
                        PlaytimeMenuItem.Visibility = Visibility.Visible;
                        if (_activityWatcher.InStudioPlace) _studioPlaceJoinTime = DateTime.Now;
                    }
                }
                else
                {
                    if (App.Settings.Prop.PlaytimeCounter)
                        StartTotalPlaytimeTimer();


                    UpdateRegionJoinUI();
                    PopulateRegionMenu();

                    if (!App.Settings.Prop.ShowGameHistoryMenu)
                        GameHistoryMenuItem.Visibility = Visibility.Collapsed;
                }
            }

            RichPresenceMenuItem.Visibility = (_watcher.PlayerRichPresence is not null || _watcher.StudioRichPresence is not null) ? Visibility.Visible : Visibility.Collapsed;
            VersionTextBlock.Text = $"{App.ProjectName} v{App.Version}";
        }

        private void StartTotalPlaytimeTimer()
        {
            _totalPlaytimeStopwatch.Start();
            _playtimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _playtimeTimer.Tick += PlaytimeTimer_Tick;
            _playtimeTimer.Start();
        }

        private void PlaytimeTimer_Tick(object? sender, EventArgs e)
        {
            TimeSpan total = _accumulatedTotalPlaytime + _totalPlaytimeStopwatch.Elapsed;
            if (_activityWatcher == null) return;

            if (_activityWatcher.InStudioPlace && _studioPlaceJoinTime.HasValue)
                PlaytimeTextBlock.Text = $"Total: {FormatTimeSpan(total)} | Studio: {FormatTimeSpan(DateTime.Now - _studioPlaceJoinTime.Value)}";
            else if (_activityWatcher.InGame)
                PlaytimeTextBlock.Text = $"Total: {FormatTimeSpan(total)} | Game: {FormatTimeSpan(DateTime.Now - _activityWatcher.Data.TimeJoined)}";
            else
                PlaytimeTextBlock.Text = $"Total: {FormatTimeSpan(total)}";
        }

        private static string FormatTimeSpan(TimeSpan ts) =>
            ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}" : $"{ts.Minutes}:{ts.Seconds:D2}";

        public void ShowServerInformationWindow()
        {
            if (_serverInformationWindow is null)
            {
                _serverInformationWindow = new(_watcher);
                _serverInformationWindow.Closed += (_, _) => _serverInformationWindow = null;
            }

            if (!_serverInformationWindow.IsVisible)
                _serverInformationWindow.ShowDialog();
            else
                _serverInformationWindow.Activate();
        }

        public void ShowGameInformationWindow(long placeId, long universeId)
        {
            if (_gameInformationWindow is null)
            {
                _gameInformationWindow = new GameInformation(placeId, universeId);
                _gameInformationWindow.Closed += (_, _) => _gameInformationWindow = null;
            }

            if (!_gameInformationWindow.IsVisible)
                _gameInformationWindow.ShowDialog();
            else
                _gameInformationWindow.Activate();
        }

        private void ActivityWatcher_OnGameJoin(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_activityWatcher?.Data.ServerType == ServerType.Public)
                    InviteDeeplinkMenuItem.Visibility = Visibility.Visible;

                ServerDetailsMenuItem.Visibility = Visibility.Visible;
                GameInformationMenuItem.Visibility = Visibility.Visible;
                RegionMenuRoot.Visibility = Visibility.Visible;
            });
        }

        private void ActivityWatcher_OnGameLeave(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                InviteDeeplinkMenuItem.Visibility = Visibility.Collapsed;
                ServerDetailsMenuItem.Visibility = Visibility.Collapsed;
                GameInformationMenuItem.Visibility = Visibility.Collapsed;
                RegionMenuRoot.Visibility = Visibility.Collapsed;
                _serverInformationWindow?.Close();
                _gameInformationWindow?.Close();
            });
        }

        private void ActivityWatcher_OnStudioPlaceOpened(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _studioPlaceJoinTime = DateTime.Now;
            });
        }

        private void ActivityWatcher_OnStudioPlaceClosed(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _studioPlaceJoinTime = null;
            });
        }

        private void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            HWND hWnd = (HWND)new WindowInteropHelper(this).Handle;
            int exStyle = PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            exStyle |= 0x00000080; // WS_EX_TOOLWINDOW
            PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);

            PopulateQuickLaunch();
            PopulateBookmarks();
        }

        private void Window_Closed(object sender, EventArgs e) => App.Logger.WriteLine("MenuContainer::Window_Closed", "Context menu container closed");
        private void CloseWatcheMenuItem_Click(object sender, RoutedEventArgs e) => _watcher.Dispose();

        private void ExitNexusStrap_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = Frontend.ShowMessageBox(
                "Are you sure you want to exit NexusStrap?\n\nThis will close the tray icon and all background processes.",
                MessageBoxImage.Question,
                MessageBoxButton.YesNo
            );

            if (result != MessageBoxResult.Yes)
                return;

            App.SoftTerminate();
        }

        private void PopulateQuickLaunch()
        {
            const string LOG_IDENT = "MenuContainer::PopulateQuickLaunch";

            QuickLaunchRoot.Items.Clear();

            try
            {
                string historyPath = Path.Combine(Paths.Cache, "GameHistory.json");

                if (!File.Exists(historyPath))
                {
                    App.Logger.WriteLine(LOG_IDENT, "No game history file found");
                    return;
                }

                string json = File.ReadAllText(historyPath);
                var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
                var gameHistory = JsonSerializer.Deserialize<List<GameHistoryData>>(json, options);

                if (gameHistory == null || !gameHistory.Any())
                {
                    App.Logger.WriteLine(LOG_IDENT, "Game history is empty");
                    return;
                }

                var uniqueGames = gameHistory
                    .Where(h => h.PlaceId != 0 && h.UniverseId != 0)
                    .GroupBy(h => h.UniverseId)
                    .Select(g => g.OrderByDescending(h => h.TimeJoined).First())
                    .OrderByDescending(h => h.TimeJoined)
                    .Take(5)
                    .ToList();

                foreach (var game in uniqueGames)
                {
                    string gameName = game.GameName ?? $"Place {game.PlaceId}";

                    var details = UniverseDetails.LoadFromCache(game.UniverseId);
                    if (details?.Data?.Name != null)
                        gameName = details.Data.Name;

                    var menuItem = new MenuItem { Header = gameName, Tag = game.PlaceId };
                    menuItem.Click += QuickLaunchMenuItem_Click;
                    QuickLaunchRoot.Items.Add(menuItem);
                }

                if (QuickLaunchRoot.Items.Count == 0)
                {
                    QuickLaunchRoot.Items.Add(new MenuItem { Header = "No recent games", IsEnabled = false });
                }

                App.Logger.WriteLine(LOG_IDENT, $"Populated {QuickLaunchRoot.Items.Count} quick launch items");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                QuickLaunchRoot.Items.Add(new MenuItem { Header = "Failed to load history", IsEnabled = false });
            }
        }

        private void QuickLaunchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.Tag is not long placeId)
                return;

            string robloxUri = $"roblox://experiences/start?placeId={placeId}";
            Process.Start(new ProcessStartInfo
            {
                FileName = robloxUri,
                UseShellExecute = true
            });
        }

        private void PopulateBookmarks()
        {
            const string LOG_IDENT = "MenuContainer::PopulateBookmarks";

            BookmarksRoot.Items.Clear();

            try
            {
                if (!File.Exists(BookmarksPath))
                {
                    BookmarksRoot.Items.Add(new MenuItem { Header = "No bookmarks", IsEnabled = false });
                    return;
                }

                string json = File.ReadAllText(BookmarksPath);
                var bookmarks = JsonSerializer.Deserialize<List<BookmarkEntry>>(json);

                if (bookmarks == null || !bookmarks.Any())
                {
                    BookmarksRoot.Items.Add(new MenuItem { Header = "No bookmarks", IsEnabled = false });
                    return;
                }

                foreach (var bookmark in bookmarks.OrderByDescending(b => b.Timestamp))
                {
                    var menuItem = new MenuItem { Header = bookmark.GameName, Tag = bookmark.PlaceId };
                    menuItem.Click += QuickLaunchMenuItem_Click;

                    var removeItem = new MenuItem { Header = "Remove", Tag = bookmark.PlaceId };
                    removeItem.Click += RemoveBookmarkMenuItem_Click;
                    menuItem.Items.Add(removeItem);

                    BookmarksRoot.Items.Add(menuItem);
                }

                App.Logger.WriteLine(LOG_IDENT, $"Populated {BookmarksRoot.Items.Count} bookmark items");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                BookmarksRoot.Items.Add(new MenuItem { Header = "Failed to load bookmarks", IsEnabled = false });
            }
        }

        private void BookmarkCurrentGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "MenuContainer::BookmarkCurrentGameMenuItem_Click";

            if (_activityWatcher?.Data == null || _activityWatcher.Data.PlaceId == 0)
            {
                Frontend.ShowMessageBox("You need to be in a game to bookmark it.", MessageBoxImage.Information);
                return;
            }

            long placeId = _activityWatcher.Data.PlaceId;
            long universeId = _activityWatcher.Data.UniverseId;

            string gameName = $"Place {placeId}";
            var details = UniverseDetails.LoadFromCache(universeId);
            if (details?.Data?.Name != null)
                gameName = details.Data.Name;

            App.Logger.WriteLine(LOG_IDENT, $"Bookmarking game: {gameName} ({placeId})");

            if (SaveBookmark(placeId, gameName))
            {
                Frontend.ShowMessageBox($"Bookmarked: {gameName}", MessageBoxImage.Information);
                PopulateBookmarks();
            }
        }

        private void RemoveBookmarkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.Tag is not long placeId)
                return;

            RemoveBookmark(placeId);
            PopulateBookmarks();
        }

        private bool SaveBookmark(long placeId, string gameName)
        {
            const string LOG_IDENT = "MenuContainer::SaveBookmark";

            try
            {
                List<BookmarkEntry> bookmarks;

                if (File.Exists(BookmarksPath))
                {
                    string json = File.ReadAllText(BookmarksPath);
                    bookmarks = JsonSerializer.Deserialize<List<BookmarkEntry>>(json) ?? new();
                }
                else
                {
                    bookmarks = new();
                }

                if (bookmarks.Any(b => b.PlaceId == placeId))
                {
                    Frontend.ShowMessageBox("This game is already bookmarked.", MessageBoxImage.Information);
                    return false;
                }

                bookmarks.Add(new BookmarkEntry
                {
                    PlaceId = placeId,
                    GameName = gameName,
                    Timestamp = DateTime.Now
                });

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string saveJson = JsonSerializer.Serialize(bookmarks, options);
                File.WriteAllText(BookmarksPath, saveJson);

                App.Logger.WriteLine(LOG_IDENT, $"Saved bookmark for {gameName} ({placeId})");
                return true;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox($"Failed to save bookmark: {ex.Message}", MessageBoxImage.Error);
                return false;
            }
        }

        private void RemoveBookmark(long placeId)
        {
            const string LOG_IDENT = "MenuContainer::RemoveBookmark";

            try
            {
                if (!File.Exists(BookmarksPath))
                    return;

                string json = File.ReadAllText(BookmarksPath);
                var bookmarks = JsonSerializer.Deserialize<List<BookmarkEntry>>(json);

                if (bookmarks == null)
                    return;

                bookmarks.RemoveAll(b => b.PlaceId == placeId);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string updatedJson = JsonSerializer.Serialize(bookmarks, options);
                File.WriteAllText(BookmarksPath, updatedJson);

                App.Logger.WriteLine(LOG_IDENT, $"Removed bookmark for place {placeId}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public class BookmarkEntry
        {
            [JsonPropertyName("placeId")]
            public long PlaceId { get; set; }

            [JsonPropertyName("gameName")]
            public string GameName { get; set; } = string.Empty;

            [JsonPropertyName("timestamp")]
            public DateTime Timestamp { get; set; }
        }

        private void RichPresenceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var isChecked = ((MenuItem)sender).IsChecked;

            _watcher.PlayerRichPresence?.SetVisibility(isChecked);
            _watcher.StudioRichPresence?.SetVisibility(isChecked);
        }

        private void InviteDeeplinkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string deeplink = _activityWatcher?.Data?.GetInviteDeeplink() ?? "No activity data available";
            Clipboard.SetDataObject(deeplink);
        }

        private void ServerDetailsMenuItem_Click(object sender, RoutedEventArgs e) => ShowServerInformationWindow();
        private void GameInformaionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            long placeId = _activityWatcher?.Data?.PlaceId ?? 0;
            long universeId = _activityWatcher?.Data?.UniverseId ?? 0;

            if (placeId == 0)
            {
                Frontend.ShowMessageBox(
                    "Not currently in a game. Please join a game first to view game information.",
                    MessageBoxImage.Error
                );
                return;
            }

            ShowGameInformationWindow(placeId, universeId);
        }

        private void CloseRobloxMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = Frontend.ShowMessageBox(
                Strings.ContextMenu_CloseRobloxMessage,
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo
            );

            if (result != MessageBoxResult.Yes)
                return;

            _watcher.KillRobloxProcess();
        }

        private void JoinLastServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_activityWatcher is null)
                throw new ArgumentNullException(nameof(_activityWatcher));

            if (_gameHistoryWindow is null)
            {
                _gameHistoryWindow = new(_activityWatcher);
                _gameHistoryWindow.Closed += (_, _) => _gameHistoryWindow = null;
            }

            if (!_gameHistoryWindow.IsVisible)
                _gameHistoryWindow.ShowDialog();
            else
                _gameHistoryWindow.Activate();
        }

        private async void PopulateRegionMenu()
        {
            var fetcher = new RobloxServerFetcher();
            var result = await fetcher.GetDatacentersAsync();
            if (result == null || result.Value.regions == null) return;

            RegionSelectionComboBox.Items.Clear();

            foreach (var region in result.Value.regions)
            {
                RegionSelectionComboBox.Items.Add(region);
            }

            RegionSelectionComboBox.SelectedItem = App.Settings.Prop.SelectedRegion;

            RegionSelectionComboBox.SelectionChanged += (s, e) =>
            {
                if (RegionSelectionComboBox.SelectedItem is string selected)
                {
                    App.Settings.Prop.SelectedRegion = selected;
                    UpdateRegionJoinUI();
                }
            };
        }

        private void UpdateRegionJoinUI()
        {
            string region = App.Settings.Prop.SelectedRegion;
            bool hasRegion = !string.IsNullOrEmpty(region);

            RegionJoinTextBlock.Text = hasRegion ? $"Region: {region}" : "Select Region";

            AutoJoinRegionMenuItem.Header = hasRegion ? $"Join {region}" : "Please select a region";
            AutoJoinRegionMenuItem.IsEnabled = hasRegion;
        }

        private async void AutoJoinRegionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_activityWatcher?.InGame != true || _activityWatcher?.Data == null)
            {
                Frontend.ShowMessageBox("You need to be in a game to use this feature.", MessageBoxImage.Warning);
                return;
            }

            string selectedRegion = App.Settings.Prop.SelectedRegion;
            if (string.IsNullOrEmpty(selectedRegion)) return;

            await FindAndJoinServerInRegion(_activityWatcher.Data.PlaceId, selectedRegion);
        }

        private async Task FindAndJoinServerInRegion(long placeId, string selectedRegion)
        {
            var fetcher = new RobloxServerFetcher();
            await App.RemoteData.WaitUntilDataFetched();
            string cookie = App.RemoteData.Prop.Dummy;

            if (!await fetcher.ValidateCookieAsync(cookie))
            {
                Frontend.ShowMessageBox("Authentication failed. Please check your connection.", MessageBoxImage.Error);
                return;
            }

            string? nextCursor = "";
            for (int i = 0; i < 20; i++)
            {
                var result = await fetcher.FetchServerInstancesAsync(placeId, cookie, nextCursor);
                var match = result.Servers.FirstOrDefault(s => s.Region == selectedRegion && s.Playing < s.MaxPlayers);

                if (match != null)
                {
                    MessageBoxResult confirmResult = Frontend.ShowMessageBox(
                            $"Found server in {selectedRegion} with {match.Playing}/{match.MaxPlayers} players.\nDo you want to join?",
                            MessageBoxImage.Question,
                            MessageBoxButton.YesNo
                        );

                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        string robloxUri = $"roblox://experiences/start?placeId={placeId}&gameInstanceId={match.Id}";
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = robloxUri,
                            UseShellExecute = true
                        });

                        _watcher.KillRobloxProcess();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }

                if (string.IsNullOrEmpty(result.NextCursor)) break;
                nextCursor = result.NextCursor;
                await Task.Delay(200);
            }

            Frontend.ShowMessageBox($"No available {selectedRegion} servers found.", MessageBoxImage.Information);
        }
    }
}