/*
 *  NexusStrap
 *  Copyright (c) NexusStrap Team
 *
 *  This file is part of NexusStrap and is distributed under the terms of the
 *  GNU Affero General Public License, version 3 or later.
 *
 *  SPDX-License-Identifier: AGPL-3.0-or-later
 *
 *  Description: Nix flake for shipping for Nix-darwin, Nix, NixOS, and modules
 *               of the Nix ecosystem. 
 */

using NexusStrap.Integrations;
using NexusStrap.UI.ViewModels.AccountManagers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace NexusStrap.UI.ViewModels.Settings
{
    public partial class RegionSelectorViewModel : ObservableObject
    {
        private const string LOG_IDENT = "RegionSelectorViewModel";
        private readonly HashSet<string> _displayedServerIds = new();
        private RobloxServerFetcher? _fetcher;
        private CancellationTokenSource? _searchDebounceCts;

        [ObservableProperty] private bool _hasSearched;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SearchCommand))] private string _placeId = "";
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ServerListMessage), nameof(IsServerListEmptyAndNotLoading), nameof(ShowLoadingIndicator))]
        [NotifyCanExecuteChangedFor(nameof(SearchCommand), nameof(LoadMoreCommand), nameof(SearchGamesCommand))] private bool _isLoading;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowLoadingIndicator))]
        [NotifyCanExecuteChangedFor(nameof(SearchGamesCommand))] private bool _isGameSearchLoading;
        [ObservableProperty] private string _loadingMessage = "";
        [ObservableProperty] private string _nextCursor = "";
        [ObservableProperty] private string? _roblosecurity;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ServerListMessage))]
        [NotifyCanExecuteChangedFor(nameof(SearchCommand), nameof(SearchGamesCommand))] private bool _hasValidCookies;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SearchGamesCommand))] private string _searchQuery = "";
        [ObservableProperty] private GameSearchResult? _selectedSearchResult;
        [ObservableProperty] private int _selectedSortOrder = 2;
        [ObservableProperty] private int _lastFetchProcessedCount;
        [ObservableProperty] private bool _isAutoSelecting;
        [ObservableProperty] private string _autoSelectStatus = "";

        public ObservableCollection<string> Regions { get; } = new();
        public ObservableCollection<ServerEntry> Servers { get; } = new();
        public ObservableCollection<GameSearchResult> SearchResults { get; } = new();

        public List<SortOrderComboBoxItem> SortOrderOptions { get; } = new()
        {
            new() { Content = "Large Servers", Tag = 2 },
            new() { Content = "Small Servers", Tag = 1 }
        };

        public bool IsServerListEmpty => Servers.Count == 0;
        public bool IsServerListEmptyAndNotLoading => IsServerListEmpty && !IsLoading;
        public bool ShowLoadingIndicator => IsLoading && !IsGameSearchLoading;

        public string ServerListMessage => !HasValidCookies ? "Dummy not found, Please notify us in our discord server." :
            IsLoading ? "" :
            !HasSearched ? "Enter a Place ID and click Search to view servers." :
            IsServerListEmpty ? (LastFetchProcessedCount == 0 ? "No public servers found." : "No servers found for specified region.") : "";

        public IAsyncRelayCommand SearchCommand { get; }
        public IAsyncRelayCommand LoadMoreCommand { get; }
        public IAsyncRelayCommand SearchGamesCommand { get; }
        public IAsyncRelayCommand AutoSelectRegionCommand { get; }

        public RegionSelectorViewModel()
        {
            Servers.CollectionChanged += (_, _) => {
                OnPropertyChanged(nameof(IsServerListEmpty));
                OnPropertyChanged(nameof(IsServerListEmptyAndNotLoading));
            };

            SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(PlaceId) && HasValidCookies);
            SearchGamesCommand = new AsyncRelayCommand(SearchGamesAsync, () => !IsLoading && !IsGameSearchLoading && !string.IsNullOrWhiteSpace(SearchQuery) && HasValidCookies);
            LoadMoreCommand = new AsyncRelayCommand(LoadMoreServersAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(NextCursor));
            AutoSelectRegionCommand = new AsyncRelayCommand(AutoSelectBestRegionAsync, () => !IsLoading && !IsAutoSelecting && HasValidCookies && Regions.Count > 0);

            _ = InitializeCookiesAsync();
        }

        partial void OnSearchQueryChanged(string value)
        {
            if (long.TryParse(value, out _)) PlaceId = value;

            _searchDebounceCts?.Cancel();
            _searchDebounceCts?.Dispose();
            _searchDebounceCts = new CancellationTokenSource();
            _ = DebouncedSearchTriggerAsync(_searchDebounceCts.Token);
        }

        partial void OnSelectedSearchResultChanged(GameSearchResult? value)
        {
            if (value == null) return;
            PlaceId = value.RootPlaceId.ToString();
            SearchQuery = value.RootPlaceId.ToString();
        }

        public string? SelectedRegion
        {
            get => App.Settings.Prop.SelectedRegion;
            set
            {
                App.Settings.Prop.SelectedRegion = value!;
                OnPropertyChanged();
                SearchCommand.NotifyCanExecuteChanged();
                App.Settings.Save();
            }
        }

        private async Task DebouncedSearchTriggerAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(600, token);
                if (!token.IsCancellationRequested && !IsLoading && !string.IsNullOrWhiteSpace(SearchQuery))
                    await SearchGamesAsync();
            }
            catch (OperationCanceledException) { }
        }

        private async Task InitializeCookiesAsync()
        {
            try
            {
                await App.RemoteData.WaitUntilDataFetched();

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    Roblosecurity = App.RemoteData.Prop.Dummy;

                    if (!string.IsNullOrWhiteSpace(Roblosecurity))
                    {
                        _fetcher = new RobloxServerFetcher();
                        HasValidCookies = await _fetcher.ValidateCookieAsync(Roblosecurity);
                        break;
                    }

                    await Task.Delay(1000);
                }

                if (HasValidCookies) await LoadRegionsAsync();
            }
            catch (Exception ex) { App.Logger.WriteException(LOG_IDENT, ex); }
        }

        private async Task LoadRegionsAsync()
        {
            IsLoading = true;
            LoadingMessage = "Loading datacenters...";

            var result = await _fetcher!.GetDatacentersAsync();

            if (result == null)
            {
                LoadingMessage = "Failed to load datacenters.";
                IsLoading = false;
                return;
            }

            if (result.Value.regions != null)
            {
                Regions.Clear();
                foreach (var r in result.Value.regions) Regions.Add(r);
            }

            SelectedRegion = Regions.FirstOrDefault(r => r.Equals(App.Settings.Prop.SelectedRegion, StringComparison.OrdinalIgnoreCase)) ?? Regions.FirstOrDefault();

            LoadingMessage = $"Loaded {Regions.Count} regions.";
            IsLoading = false;
            await Task.Delay(800);
            LoadingMessage = "";
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedRegion))
            {
                Frontend.ShowMessageBox("Please select a region first.", MessageBoxImage.Warning);
                return;
            }

            HasSearched = true;
            IsLoading = true;
            LoadingMessage = "Searching servers...";
            Servers.Clear();
            _displayedServerIds.Clear();
            NextCursor = "";
            LastFetchProcessedCount = 0;

            int pagesChecked = 0;
            while (pagesChecked < 3)
            {
                await LoadServersAsync(pagesChecked == 0);
                pagesChecked++;
                if (string.IsNullOrWhiteSpace(NextCursor)) break;
            }

            IsLoading = false;
            await Task.Delay(800);
            LoadingMessage = "";
        }

        private async Task LoadServersAsync(bool resetCursor = false)
        {
            if (string.IsNullOrWhiteSpace(PlaceId) || string.IsNullOrWhiteSpace(SelectedRegion) || string.IsNullOrWhiteSpace(Roblosecurity)) return;

            if (resetCursor) NextCursor = "";
            if (!long.TryParse(PlaceId, out var placeIdLong)) return;

            var result = await _fetcher!.FetchServerInstancesAsync(placeIdLong, Roblosecurity, NextCursor, SelectedSortOrder);
            if (result == null) return;

            int number = Servers.Count + 1;
            foreach (var s in result.Servers)
            {
                if (_displayedServerIds.Add(s.Id) && s.Region == SelectedRegion)
                {
                    Servers.Add(new ServerEntry
                    {
                        Number = number++,
                        ServerId = s.Id,
                        Players = $"{s.Playing}/{s.MaxPlayers}",
                        Region = s.Region,
                        Uptime = s.UptimeDisplay,
                        JoinCommand = new RelayCommand(() => JoinServer(s.Id))
                    });
                }
            }

            LastFetchProcessedCount = result.Servers.Count;
            NextCursor = result.NextCursor;
        }

        private void JoinServer(string serverId)
        {
            if (!long.TryParse(PlaceId, out var placeId)) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"roblox://experiences/start?placeId={placeId}&gameInstanceId={serverId}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex) { App.Logger.WriteException(LOG_IDENT, ex); }
        }

        private async Task SearchGamesAsync()
        {
            IsGameSearchLoading = true;
            LoadingMessage = "Searching games...";
            try
            {
                var results = await GameSearching.GetGameSearchResultsAsync(SearchQuery);
                SearchResults.Clear();
                foreach (var r in results) SearchResults.Add(r);
                LoadingMessage = SearchResults.Count == 0 ? "No games found." : "";
            }
            catch { LoadingMessage = "Search failed."; }
            finally { IsGameSearchLoading = false; }
        }

        private async Task LoadMoreServersAsync()
        {
            IsLoading = true;
            int initial = Servers.Count;
            for (int i = 0; i < 5 && !string.IsNullOrWhiteSpace(NextCursor); i++)
                await LoadServersAsync();
            IsLoading = false;
        }

        private async Task AutoSelectBestRegionAsync()
        {
            const string LOG_IDENT = "RegionSelectorViewModel::AutoSelectBestRegion";
            IsAutoSelecting = true;
            AutoSelectStatus = "Pinging regions...";

            try
            {
                string? bestRegion = null;
                long bestPing = long.MaxValue;

                var regionEndpoints = new Dictionary<string, string>
                {
                    { "us", "https://gamejoin.roblox.com/v1/multi-game-place" },
                    { "eu", "https://gamejoin.roblox-eu.roblox.com/v1/multi-game-place" },
                    { "asia", "https://gamejoin.roblox-asia.roblox.com/v1/multi-game-place" },
                    { "au", "https://gamejoin.roblox-au.roblox.com/v1/multi-game-place" },
                    { "sa", "https://gamejoin.roblox-sa.roblox.com/v1/multi-game-place" },
                    { "jp", "https://gamejoin.roblox-jp.roblox.com/v1/multi-game-place" }
                };

                foreach (var region in Regions)
                {
                    var regionLower = region.ToLowerInvariant();
                    string url = null!;

                    foreach (var endpoint in regionEndpoints)
                    {
                        if (regionLower.Contains(endpoint.Key))
                        {
                            url = endpoint.Value;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(url))
                        url = $"https://gamejoin.roblox.com/v1/multi-game-place";

                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        using var request = new HttpRequestMessage(HttpMethod.Head, url);
                        request.Headers.Add("User-Agent", App.HttpClient.DefaultRequestHeaders.UserAgent.ToString());
                        await App.HttpClient.SendAsync(request);
                        sw.Stop();

                        long ping = sw.ElapsedMilliseconds;
                        App.Logger.WriteLine(LOG_IDENT, $"Region '{region}': {ping}ms");

                        if (ping < bestPing)
                        {
                            bestPing = ping;
                            bestRegion = region;
                        }
                    }
                    catch
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Region '{region}': failed to ping");
                    }
                }

                if (!string.IsNullOrEmpty(bestRegion))
                {
                    SelectedRegion = bestRegion;
                    AutoSelectStatus = $"Best region: {bestRegion} ({bestPing}ms)";
                    App.Settings.Save();
                }
                else
                {
                    AutoSelectStatus = "Failed to detect best region.";
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                AutoSelectStatus = "Error during region detection.";
            }
            finally
            {
                IsAutoSelecting = false;
                await Task.Delay(3000);
                AutoSelectStatus = "";
            }
        }
    }
}