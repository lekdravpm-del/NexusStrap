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
            catch { }
        }

        private void Rejoin()
        {
            if (SelectedEntry == null || string.IsNullOrEmpty(SelectedEntry.JobId)) return;
            var deeplink = $"roblox://experiences/start?placeId={SelectedEntry.PlaceId}&gameInstanceId={SelectedEntry.JobId}";
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(deeplink) { UseShellExecute = true }); }
            catch { }
        }

        private void ClearHistory()
        {
            string historyPath = System.IO.Path.Combine(Paths.Cache, "GameHistory.json");
            try
            {
                if (System.IO.File.Exists(historyPath)) System.IO.File.Delete(historyPath);
                HistoryEntries.Clear();
            }
            catch { }
        }
    }
}
