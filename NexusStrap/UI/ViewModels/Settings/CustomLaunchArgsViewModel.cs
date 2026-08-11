using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class CustomLaunchArgsViewModel : NotifyPropertyChangedViewModel
    {
        public ObservableCollection<Models.CustomLaunchArg> LaunchArgs { get; set; } = new();
        public ICommand AddArgCommand { get; }
        public ICommand RemoveArgCommand { get; }
        public ICommand ToggleArgCommand { get; }

        private Models.CustomLaunchArg? _selectedArg;
        public Models.CustomLaunchArg? SelectedArg
        {
            get => _selectedArg;
            set { _selectedArg = value; OnPropertyChanged(nameof(SelectedArg)); OnPropertyChanged(nameof(CanRemove)); }
        }

        public bool CanRemove => SelectedArg != null;

        private string _newArgName = "";
        public string NewArgName { get => _newArgName; set { _newArgName = value; OnPropertyChanged(nameof(NewArgName)); } }

        private string _newArgValue = "";
        public string NewArgValue { get => _newArgValue; set { _newArgValue = value; OnPropertyChanged(nameof(NewArgValue)); } }

        private string _newArgDescription = "";
        public string NewArgDescription { get => _newArgDescription; set { _newArgDescription = value; OnPropertyChanged(nameof(NewArgDescription)); } }

        public CustomLaunchArgsViewModel()
        {
            AddArgCommand = new RelayCommand(AddArg);
            RemoveArgCommand = new RelayCommand(RemoveArg);
            ToggleArgCommand = new RelayCommand<Models.CustomLaunchArg>(ToggleArg);
            LoadArgs();
        }

        private void LoadArgs()
        {
            LaunchArgs.Clear();
            foreach (var arg in App.Settings.Prop.CustomLaunchArgs)
                LaunchArgs.Add(arg);
        }

        private void AddArg()
        {
            if (string.IsNullOrWhiteSpace(NewArgName) || string.IsNullOrWhiteSpace(NewArgValue)) return;
            var arg = new Models.CustomLaunchArg { Name = NewArgName, Argument = NewArgValue, Description = NewArgDescription, Enabled = true };
            LaunchArgs.Add(arg);
            App.Settings.Prop.CustomLaunchArgs.Add(arg);
            NewArgName = ""; NewArgValue = ""; NewArgDescription = "";
        }

        private void RemoveArg()
        {
            if (SelectedArg == null) return;
            App.Settings.Prop.CustomLaunchArgs.Remove(SelectedArg);
            LaunchArgs.Remove(SelectedArg);
        }

        private void ToggleArg(Models.CustomLaunchArg? arg)
        {
            if (arg == null) return;
            arg.Enabled = !arg.Enabled;
            OnPropertyChanged(nameof(LaunchArgs));
        }
    }
}
