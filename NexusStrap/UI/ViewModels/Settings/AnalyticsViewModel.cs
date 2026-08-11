using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class AnalyticsViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand RefreshCommand { get; }

        public ObservableCollection<GamePlayStats> GameStats { get; set; } = new();
        public ObservableCollection<DailyPlayStats> DailyStats { get; set; } = new();

        private double _totalHours;
        public double TotalHours { get => _totalHours; set { _totalHours = value; OnPropertyChanged(nameof(TotalHours)); } }

        private int _totalSessions;
        public int TotalSessions { get => _totalSessions; set { _totalSessions = value; OnPropertyChanged(nameof(TotalSessions)); } }

        private string _mostPlayedGame = "N/A";
        public string MostPlayedGame { get => _mostPlayedGame; set { _mostPlayedGame = value; OnPropertyChanged(nameof(MostPlayedGame)); } }

        private int _uniqueGamesPlayed;
        public int UniqueGamesPlayed { get => _uniqueGamesPlayed; set { _uniqueGamesPlayed = value; OnPropertyChanged(nameof(UniqueGamesPlayed)); } }

        public AnalyticsViewModel()
        {
            RefreshCommand = new RelayCommand(LoadAnalytics);
            LoadAnalytics();
        }

        private void LoadAnalytics()
        {
            GameStats.Clear();
            DailyStats.Clear();

            string historyPath = System.IO.Path.Combine(Paths.Cache, "GameHistory.json");
            if (!System.IO.File.Exists(historyPath)) return;

            try
            {
                var entries = System.Text.Json.JsonSerializer.Deserialize<List<Models.GameHistoryData>>(File.ReadAllText(historyPath));
                if (entries == null) return;

                var sessions = entries
                    .Where(e => e.TimeLeft.HasValue)
                    .Select(e => new Models.PlaySessionData
                    {
                        GameName = e.GameName ?? "Unknown",
                        UniverseId = e.UniverseId,
                        PlaceId = e.PlaceId,
                        TimeJoined = e.TimeJoined,
                        TimeLeft = e.TimeLeft
                    }).ToList();

                TotalHours = Math.Round(sessions.Sum(s => s.MinutesPlayed) / 60.0, 1);
                TotalSessions = sessions.Count;

                var byGame = sessions.GroupBy(s => s.UniverseId).ToList();
                UniqueGamesPlayed = byGame.Count;

                foreach (var group in byGame.OrderByDescending(g => g.Sum(s => s.MinutesPlayed)))
                {
                    GameStats.Add(new GamePlayStats
                    {
                        GameName = group.First().GameName,
                        UniverseId = group.Key,
                        TotalMinutes = Math.Round(group.Sum(s => s.MinutesPlayed), 1),
                        SessionCount = group.Count(),
                        LastPlayed = group.Max(s => s.TimeJoined)
                    });
                }

                MostPlayedGame = GameStats.FirstOrDefault()?.GameName ?? "N/A";

                var byDate = sessions.GroupBy(s => s.TimeJoined.Date).OrderByDescending(g => g.Key).Take(14);
                foreach (var day in byDate)
                {
                    DailyStats.Add(new DailyPlayStats
                    {
                        Date = day.Key,
                        TotalMinutes = Math.Round(day.Sum(s => s.MinutesPlayed), 1),
                        SessionsCount = day.Count(),
                        MostPlayedGame = day.GroupBy(s => s.UniverseId).OrderByDescending(g => g.Sum(s => s.MinutesPlayed)).First().First().GameName
                    });
                }
            }
            catch { }
        }
    }
}
