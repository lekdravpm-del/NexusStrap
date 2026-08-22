using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class LogViewer2ViewModel : NotifyPropertyChangedViewModel
    {
        // Log Viewer properties
        public ObservableCollection<string> LogFiles { get; set; } = new();

        private string? _selectedLogFile;
        public string? SelectedLogFile
        {
            get => _selectedLogFile;
            set
            {
                if (_selectedLogFile != value)
                {
                    _selectedLogFile = value;
                    OnPropertyChanged(nameof(SelectedLogFile));
                    LoadLogFile();
                }
            }
        }

        private string _logContent = "";
        public string LogContent
        {
            get => _logContent;
            set
            {
                if (_logContent != value)
                {
                    _logContent = value;
                    OnPropertyChanged(nameof(LogContent));
                }
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilterLogs();
                }
            }
        }

        private string[]? _allLines;

        // Launch Arguments properties
        public ObservableCollection<CustomLaunchArg> LaunchArgs { get; set; } = new();

        private string _newArgName = "";
        public string NewArgName
        {
            get => _newArgName;
            set { if (_newArgName != value) { _newArgName = value; OnPropertyChanged(nameof(NewArgName)); } }
        }

        private string _newArgValue = "";
        public string NewArgValue
        {
            get => _newArgValue;
            set { if (_newArgValue != value) { _newArgValue = value; OnPropertyChanged(nameof(NewArgValue)); } }
        }

        private string _newArgDescription = "";
        public string NewArgDescription
        {
            get => _newArgDescription;
            set { if (_newArgDescription != value) { _newArgDescription = value; OnPropertyChanged(nameof(NewArgDescription)); } }
        }

        private CustomLaunchArg? _selectedArg;
        public CustomLaunchArg? SelectedArg
        {
            get => _selectedArg;
            set { if (_selectedArg != value) { _selectedArg = value; OnPropertyChanged(nameof(SelectedArg)); } }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddArgCommand { get; }
        public RelayCommand RemoveArgCommand { get; }

        public LogViewer2ViewModel()
        {
            RefreshCommand = new RelayCommand(() => LoadLogFiles());
            AddArgCommand = new RelayCommand(() => AddArg());
            RemoveArgCommand = new RelayCommand(() => RemoveArg(), () => SelectedArg != null);
            LoadLogFiles();
            LoadArgs();
        }

        private void AddArg()
        {
            if (string.IsNullOrWhiteSpace(NewArgName) || string.IsNullOrWhiteSpace(NewArgValue))
                return;

            var arg = new CustomLaunchArg
            {
                Name = NewArgName,
                Argument = NewArgValue,
                Description = NewArgDescription,
                Enabled = true
            };

            LaunchArgs.Add(arg);
            App.Settings.Prop.CustomLaunchArgs.Add(arg);
            App.Settings.Save();

            NewArgName = "";
            NewArgValue = "";
            NewArgDescription = "";
        }

        private void RemoveArg()
        {
            if (SelectedArg != null)
            {
                LaunchArgs.Remove(SelectedArg);
                App.Settings.Prop.CustomLaunchArgs.Remove(SelectedArg);
                App.Settings.Save();
            }
        }

        private void LoadLogFiles()
        {
            LogFiles.Clear();
            if (Directory.Exists(Paths.Logs))
            {
                foreach (var file in Directory.GetFiles(Paths.Logs, "*.log"))
                {
                    LogFiles.Add(file);
                }
            }
        }

        private void LoadLogFile()
        {
            if (string.IsNullOrEmpty(SelectedLogFile) || !File.Exists(SelectedLogFile))
            {
                LogContent = "";
                _allLines = null;
                return;
            }

            try
            {
                _allLines = File.ReadAllLines(SelectedLogFile);
                FilterLogs();
            }
            catch
            {
                LogContent = "Failed to load log file";
                _allLines = null;
            }
        }

        private void FilterLogs()
        {
            if (_allLines == null)
            {
                LogContent = "";
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LogContent = string.Join("\n", _allLines);
            }
            else
            {
                var filtered = _allLines.Where(line => line.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                LogContent = string.Join("\n", filtered);
            }
        }

        private void LoadArgs()
        {
            LaunchArgs.Clear();
            foreach (var arg in App.Settings.Prop.CustomLaunchArgs)
            {
                LaunchArgs.Add(arg);
            }
        }
    }
}
