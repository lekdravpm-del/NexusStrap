using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace NexusStrap.UI.ViewModels.Settings
{
    public partial class ThemeStoreViewModel : ObservableObject
    {
        private const string LOG_IDENT = "ThemeStoreViewModel";
        private const string THEMES_REPO_URL = "https://api.github.com/repos/NexusStrap/Themes/contents/themes";

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _statusMessage = "";
        [ObservableProperty] private StoreTheme? _selectedTheme;

        public ObservableCollection<StoreTheme> AvailableThemes { get; } = new();

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand InstallCommand { get; }

        public ThemeStoreViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadThemesAsync);
            InstallCommand = new AsyncRelayCommand(InstallThemeAsync, () => SelectedTheme != null && !IsLoading);
            _ = LoadThemesAsync();
        }

        private async Task LoadThemesAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading themes...";
            AvailableThemes.Clear();

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("NexusStrap", "1.0"));

                var response = await client.GetStringAsync(THEMES_REPO_URL);
                var items = JsonConvert.DeserializeObject<List<GitHubContentItem>>(response);

                if (items != null)
                {
                    foreach (var item in items.Where(i => i.Type == "dir"))
                    {
                        AvailableThemes.Add(new StoreTheme
                        {
                            Name = item.Name,
                            Description = item.Path,
                            DownloadUrl = item.Url,
                            IsInstalled = Directory.Exists(Path.Combine(Paths.CustomThemes, item.Name))
                        });
                    }
                }

                StatusMessage = AvailableThemes.Count == 0 ? "No themes found." : $"Found {AvailableThemes.Count} themes.";
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                StatusMessage = "Failed to load themes. Check your internet connection.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task InstallThemeAsync()
        {
            if (SelectedTheme == null) return;

            IsLoading = true;
            StatusMessage = $"Installing '{SelectedTheme.Name}'...";

            try
            {
                string themeDir = Path.Combine(Paths.CustomThemes, SelectedTheme.Name);
                Directory.CreateDirectory(themeDir);

                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("NexusStrap", "1.0"));

                var response = await client.GetStringAsync(SelectedTheme.DownloadUrl);
                var files = JsonConvert.DeserializeObject<List<GitHubContentItem>>(response);

                if (files != null)
                {
                    foreach (var file in files.Where(f => f.Type == "file"))
                    {
                        var fileResponse = await client.GetByteArrayAsync(file.DownloadUrl);
                        string filePath = Path.Combine(themeDir, file.Name);
                        await File.WriteAllBytesAsync(filePath, fileResponse);
                    }
                }

                SelectedTheme.IsInstalled = true;
                StatusMessage = $"Installed '{SelectedTheme.Name}' successfully!";

                App.Settings.Prop.SelectedCustomTheme = SelectedTheme.Name;
                App.Settings.Prop.BootstrapperStyle = Enums.BootstrapperStyle.CustomDialog;
                App.Settings.Save();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                StatusMessage = $"Failed to install '{SelectedTheme.Name}'.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class StoreTheme
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public bool IsInstalled { get; set; }
    }

    public class GitHubContentItem
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("path")]
        public string Path { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("url")]
        public string Url { get; set; } = "";

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; } = "";
    }
}
