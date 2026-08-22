using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class MainWindowViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand OpenAboutCommand => new RelayCommand(OpenAbout);
        public ICommand OpenAccountManagerCommand => new RelayCommand(OpenAccountManager);
        public ICommand SaveSettingsCommand => new RelayCommand(SaveSettings);
        public ICommand SaveAndLaunchPlayerCommand => new RelayCommand(() => SaveAndLaunch("player"));
        public ICommand SaveAndLaunchStudioCommand => new RelayCommand(() => SaveAndLaunch("studio"));
        public ICommand RestartAppCommand => new RelayCommand(RestartApp);
        public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);

        public EventHandler? RequestSaveNoticeEvent;
        public EventHandler? RequestCloseWindowEvent;
        public bool GBSEnabled = App.GlobalSettings.Loaded;

        public bool ServerBrowserEnabled = App.Settings.Prop.EnableServerBrowser;
        public event EventHandler? SettingsSaved;

        public bool TestModeEnabled
        {
            get => App.LaunchSettings.TestModeFlag.Active;
            set
            {
                if (value && !App.State.Prop.TestModeWarningShown)
                {
                    var result = Frontend.ShowMessageBox(Strings.Menu_TestMode_Prompt, MessageBoxImage.Information, MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes)
                        return;

                    App.State.Prop.TestModeWarningShown = true;
                }

                App.LaunchSettings.TestModeFlag.Active = value;
            }
        }

        public bool IsSidebarExpanded
        {
            get => App.Settings.Prop.IsNavigationSidebarExpanded;
            set => App.Settings.Prop.IsNavigationSidebarExpanded = value;
        }

        private void OpenAbout()
        {
            App.RichPresence?.SetDialog("About");

            new Elements.About.MainWindow().ShowDialog();

            App.RichPresence?.ClearDialog();
        }

        private void OpenAccountManager()
        {
            App.RichPresence?.SetDialog("Account Manager");

            new Elements.AccountManagers.MainWindow().ShowDialog();

            App.RichPresence?.ClearDialog();
        }

        private void CloseWindow() => RequestCloseWindowEvent?.Invoke(this, EventArgs.Empty);

        public void SaveSettings()
        {
            const string LOG_IDENT = "MainWindowViewModel::SaveSettings";

            App.Settings.Save();
            App.State.Save();
            App.FastFlags.Save();
            App.GlobalSettings.Save();

            foreach (var pair in App.PendingSettingTasks)
            {
                var task = pair.Value;

                if (task.Changed)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Executing pending task '{task}'");
                    task.Execute();
                }
            }

            App.PendingSettingTasks.Clear();

            RequestSaveNoticeEvent?.Invoke(this, EventArgs.Empty);
        }

        public void SaveAndLaunch(string mode)
        {
            SaveSettings();

            if (!App.LaunchSettings.TestModeFlag.Active)
            {
                if (AccountManager.Shared.ActiveAccount is null)
                {
                    var result = Frontend.ShowMessageBox("Would you like to log in?", MessageBoxImage.Question, MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                        OpenAccountManager();
                }

                Process.Start(Paths.Application, $"-{mode.ToLower()}");
                App.DeferredTerminate();
            }

            CloseWindow();
        }

        private async void RestartApp()
        {
            SaveSettings();

            SettingsSaved?.Invoke(this, EventArgs.Empty);

            await Task.Delay(750);

            var startInfo = new ProcessStartInfo(Environment.ProcessPath!)
            {
                Arguments = "-menu"
            };

            Process.Start(startInfo);

            App.RichPresence?.Dispose();
            CloseWindow();
        }
    }
}