using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using NexusStrap.Integrations;

namespace NexusStrap.UI.Elements.Dialogs
{
    public partial class AddFriendDialog
    {
        private long _foundId;
        private string _foundUsername = "";
        private string _foundDisplayName = "";
        private string _foundAvatar = "";

        public AddFriendDialog()
        {
            InitializeComponent();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var raw = IdBox.Text?.Trim();
            if (!long.TryParse(raw, out long id) || id <= 0)
            {
                StatusText.Text = "Enter a valid numeric ID.";
                StatusText.Visibility = Visibility.Visible;
                ResultBorder.Visibility = Visibility.Collapsed;
                SendBtn.IsEnabled = false;
                return;
            }

            StatusText.Visibility = Visibility.Collapsed;
            ResultBorder.Visibility = Visibility.Collapsed;
            SendBtn.IsEnabled = false;
            LoadingRing.Visibility = Visibility.Visible;

            try
            {
                // 1. Check NexusStrap registry first - must be logged in via NexusStrap
                if (!NexusFriendRegistry.IsRegistered(id))
                {
                    StatusText.Text = "No user was found — make sure they logged in via NexusStrap at least once on this device. (Nexus-verified only)";
                    StatusText.Visibility = Visibility.Visible;
                    return;
                }

                // 2. Verify via Roblox API that ID exists and fetch info
                var info = await FetchRobloxUserAsync(id);
                if (info == null)
                {
                    StatusText.Text = "No user was found for this ID.";
                    StatusText.Visibility = Visibility.Visible;
                    return;
                }

                // Also get avatar
                var avatar = await FetchAvatarAsync(id);

                _foundId = id;
                _foundUsername = info.Value.username;
                _foundDisplayName = info.Value.displayName;

                NameText.Text = _foundDisplayName;
                UserText.Text = $"@{_foundUsername}";
                IdText.Text = $"ID {id}";
                if (!string.IsNullOrEmpty(avatar))
                {
                    try { AvatarBrush.ImageSource = new BitmapImage(new Uri(avatar)); _foundAvatar = avatar; } catch { }
                }

                ResultBorder.Visibility = Visibility.Visible;
                StatusText.Text = "User found — Nexus-verified ✓";
                StatusText.Visibility = Visibility.Visible;
                StatusText.Foreground = (System.Windows.Media.Brush)FindResource("SystemAccentBrush");
                SendBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Search failed: {ex.Message}";
                StatusText.Visibility = Visibility.Visible;
            }
            finally
            {
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<(string username, string displayName)?> FetchRobloxUserAsync(long id)
        {
            try
            {
                using var c = new HttpClient();
                var resp = await c.GetAsync($"https://users.roblox.com/v1/users/{id}");
                if (!resp.IsSuccessStatusCode) return null;
                var body = await resp.Content.ReadAsStringAsync();
                var jo = JObject.Parse(body);
                var name = jo["name"]?.ToString();
                var display = jo["displayName"]?.ToString();
                if (string.IsNullOrEmpty(name)) return null;
                return (name, display ?? name);
            }
            catch { return null; }
        }

        private async Task<string> FetchAvatarAsync(long id)
        {
            try
            {
                using var c = new HttpClient();
                var resp = await c.GetAsync($"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={id}&size=150x150&format=Png&isCircular=true");
                if (!resp.IsSuccessStatusCode) return "";
                var body = await resp.Content.ReadAsStringAsync();
                var jo = JObject.Parse(body);
                return jo["data"]?[0]?["imageUrl"]?.ToString() ?? "";
            }
            catch { return ""; }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_foundId == 0) return;
            // For now, store as pending friend - we keep a local list of added friends
            try
            {
                var path = Path.Combine(Paths.Cache, "NexusFriends.json");
                var list = new List<long>();
                if (File.Exists(path))
                {
                    try { list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(File.ReadAllText(path)) ?? new(); } catch { }
                }
                if (!list.Contains(_foundId)) list.Add(_foundId);
                File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));
            } catch { }

            Frontend.ShowMessageBox($"Friend request sent to {_foundDisplayName} (@{_foundUsername})", System.Windows.MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
