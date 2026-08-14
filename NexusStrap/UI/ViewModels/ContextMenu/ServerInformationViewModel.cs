using NexusStrap.Integrations;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.ContextMenu
{
    internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
    {
        private readonly ActivityWatcher _activityWatcher;

        public string InstanceId => _activityWatcher.Data.JobId;

        public string ServerType => _activityWatcher.Data.ServerType.ToTranslatedString();

        public string ServerLocation { get; private set; } = Strings.Common_Loading;

        public string ServerUptime { get; private set; } = Strings.Common_Loading;

        public Visibility ServerLocationVisibility => App.Settings.Prop.ShowServerDetails ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ServerUptimeVisibility => App.Settings.Prop.ShowServerUptime ? Visibility.Visible : Visibility.Collapsed;

        public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);

        public ServerInformationViewModel(Watcher watcher)
        {
            _activityWatcher = watcher.ActivityWatcher!;

            if (ServerLocationVisibility == Visibility.Visible)
                QueryServerLocation();

            if (ServerUptimeVisibility == Visibility.Visible)
                QueryServerUptime();
        }

        public async void QueryServerLocation()
        {
            string? location = await _activityWatcher.Data.QueryServerLocation();

            if (String.IsNullOrEmpty(location))
                ServerLocation = Strings.Common_NotAvailable;
            else
                ServerLocation = location;

            OnPropertyChanged(nameof(ServerLocation));
        }

        public async void QueryServerUptime()
        {
            DateTime? serverTime = await _activityWatcher.Data.QueryServerTime();
            string? serverUptime = Strings.ContextMenu_ServerInformation_Notification_ServerNotTracked;
            if (serverTime is DateTime time)
            {
                TimeSpan serverUptimeSpan = DateTime.UtcNow - time;
                if (serverUptimeSpan.TotalSeconds > 60)
                    serverUptime = Time.FormatTimeSpan(serverUptimeSpan);
            }

            ServerUptime = serverUptime;

            OnPropertyChanged(nameof(ServerUptime));
        }

        private void CopyInstanceId() => Clipboard.SetDataObject(InstanceId);
    }
}
