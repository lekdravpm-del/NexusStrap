using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class LogViewerViewModel : NotifyPropertyChangedViewModel
    {
        public ObservableCollection<string> LogFiles { get; set; } = new();
        public ObservableCollection<string> LogLines { get; set; } = new();
        public ICommand RefreshCommand { get; }
        public ICommand ClearLogCommand { get; }

        private string? _selectedLogFile;
        public string? SelectedLogFile
        {
            get => _selectedLogFile;
            set { _selectedLogFile = value; OnPropertyChanged(nameof(SelectedLogFile)); LoadLogFile(); }
        }

        private string _logContent = "";
        public string LogContent { get => _logContent; set { _logContent = value; OnPropertyChanged(nameof(LogContent)); } }

        private string _searchText = "";
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(nameof(SearchText)); FilterLogs(); } }

        private List<string> _allLines = new();

        public LogViewerViewModel()
        {
            RefreshCommand = new RelayCommand(LoadLogFiles);
            ClearLogCommand = new RelayCommand(ClearCurrentLog);
            LoadLogFiles();
        }

        private void LoadLogFiles()
        {
            LogFiles.Clear();
            if (!Directory.Exists(Paths.Logs)) return;

            foreach (var file in new DirectoryInfo(Paths.Logs).GetFiles("*.log").OrderByDescending(f => f.LastWriteTime))
                LogFiles.Add(file.FullName);
        }

        private void LoadLogFile()
        {
            LogContent = "";
            _allLines.Clear();
            if (string.IsNullOrEmpty(SelectedLogFile) || !File.Exists(SelectedLogFile)) return;

            try
            {
                _allLines = File.ReadAllLines(SelectedLogFile).ToList();
                FilterLogs();
            }
            catch { LogContent = "Failed to read log file."; }
        }

        private void FilterLogs()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                LogContent = string.Join("\r\n", _allLines);
            else
                LogContent = string.Join("\r\n", _allLines.Where(l => l.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        private void ClearCurrentLog()
        {
            if (string.IsNullOrEmpty(SelectedLogFile) || !File.Exists(SelectedLogFile)) return;
            try { File.WriteAllText(SelectedLogFile, ""); LoadLogFile(); } catch { }
        }
    }
}
