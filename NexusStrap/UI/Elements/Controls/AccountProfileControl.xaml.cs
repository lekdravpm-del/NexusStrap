using NexusStrap.Integrations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NexusStrap.UI.Elements.Controls
{
    public partial class AccountProfileControl : INotifyPropertyChanged
    {
        private string _displayName = "";
        private string _avatarUrl = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            set
            {
                _avatarUrl = value;
                OnPropertyChanged();
            }
        }

        public AccountProfileControl()
        {
            DataContext = this;

            InitializeComponent();

            AvatarBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/NX.png"));

            AccountManager.Shared.ActiveAccountChanged += OnActiveAccountChanged;

            Loaded += (_, _) => RefreshFromActiveAccount();
        }

        private void OnActiveAccountChanged(AltAccount? account)
        {
            if (account == null)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            Visibility = Visibility.Visible;
            DisplayName = string.IsNullOrEmpty(account.DisplayName) ? account.Username : account.DisplayName;
            _ = LoadAvatarAsync(account.UserId);
        }

        private void RefreshFromActiveAccount()
        {
            if (!App.Settings.Prop.EnableProfileDisplay)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            var account = AccountManager.Shared.ActiveAccount;
            OnActiveAccountChanged(account);
        }

        private static readonly Dictionary<long, BitmapSource> _avatarCache = new();

        private async Task LoadAvatarAsync(long userId)
        {
            try
            {
                if (_avatarCache.TryGetValue(userId, out var cached))
                {
                    AvatarBrush.ImageSource = cached;
                    return;
                }

                var response = await App.HttpClient.GetAsync($"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={userId}&size=48x48&format=Png&isCircular=true");

                if (!response.IsSuccessStatusCode)
                    return;

                string json = await response.Content.ReadAsStringAsync();

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var imageUrl = doc.RootElement.GetProperty("data")[0].GetProperty("imageUrl").GetString();

                if (string.IsNullOrEmpty(imageUrl))
                    return;

                byte[] imageBytes = await App.HttpClient.GetByteArrayAsync(imageUrl);

                using var stream = new MemoryStream(imageBytes);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                _avatarCache[userId] = bitmap;

                AvatarBrush.ImageSource = bitmap;
            }
            catch
            {
                // avatar loading is cosmetic
            }
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var window = new NexusStrap.UI.Elements.AccountManagers.MainWindow();
            window.ShowDialog();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}