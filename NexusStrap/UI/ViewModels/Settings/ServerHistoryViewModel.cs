using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class ServerHistoryViewModel : NotifyPropertyChangedViewModel
    {
        public ObservableCollection<Models.ServerHistoryEntry> HistoryEntries { get; set; } = new();
        public ICommand RejoinCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        private Models.ServerHistoryEntry? _selectedEntry;
        public Models.ServerHistoryEntry? SelectedEntry
        {
            get => _selectedEntry;
            set { _selectedEntry = value; OnPropertyChanged(nameof(SelectedEntry)); OnPropertyChanged(nameof(CanRejoin)); }
        }

        public bool CanRejoin => SelectedEntry != null && !string.IsNullOrEmpty(SelectedEntry.JobId);

        public ServerHistoryViewModel()
        {
            RejoinCommand = new RelayCommand(Rejoin);
            RefreshCommand = new RelayCommand(LoadHistory);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            LoadHistory();
        }

        private async void Rejoin()
        {
            if (SelectedEntry == null || string.IsNullOrEmpty(SelectedEntry.JobId))
                return;

            try
            {
                var mgr = AccountManager.Shared;
                if (mgr?.ActiveAccount == null)
                {
                    Frontend.ShowMessageBox("Please select an account first.", System.Windows.MessageBoxImage.Warning);
                    return;
                }

                mgr.SetCurrentPlaceId(SelectedEntry.PlaceId.ToString());
                mgr.SetCurrentServerInstanceId(SelectedEntry.JobId);

                await mgr.LaunchAccountAsync(mgr.ActiveAccount, SelectedEntry.PlaceId, SelectedEntry.JobId);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ServerHistoryViewModel::Rejoin", ex);
                Frontend.ShowMessageBox($"Failed to rejoin server: {ex.Message}", System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadHistory()
        {
            HistoryEntries.Clear();
            string historyPath = System.IO.Path.Combine(Paths.Cache, "GameHistory.json");
            if (!System.IO.File.Exists(historyPath)) return;

            try
            {
                string json = System.IO.File.ReadAllText(historyPath);
                var entries = System.Text.Json.JsonSerializer.Deserialize<List<Models.GameHistoryData>>(json);
                if (entries == null) return;

                foreach (var e in entries.OrderByDescending(x => x.TimeJoined))
                {
                    HistoryEntries.Add(new Models.ServerHistoryEntry
                    {
                        GameName = e.GameName ?? "Unknown Game",
                        PlaceId = e.PlaceId,
                        JobId = e.JobId,
                        ServerType = e.ServerType,
                        TimeJoined = e.TimeJoined,
                        TimeLeft = e.TimeLeft,
                        UniverseId = e.UniverseId
                    });
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ServerHistoryViewModel::LoadHistoryFailed", ex);
            }
        }

        private void ClearHistory()
        {
            string historyPath = System.IO.Path.Combine(Paths.Cache, "GameHistory.json");
            try
            {
                if (System.IO.File.Exists(historyPath)) System.IO.File.Delete(historyPath);
                HistoryEntries.Clear();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ServerHistoryViewModel::ClearHistoryFailed", ex);
            }
        }
    }
}
