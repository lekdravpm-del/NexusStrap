using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class HealthCheckViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand RunCheckCommand { get; }

        private string _checkResults = "";
        public string CheckResults { get => _checkResults; set { _checkResults = value; OnPropertyChanged(nameof(CheckResults)); } }

        private int _issuesFound;
        public int IssuesFound { get => _issuesFound; set { _issuesFound = value; OnPropertyChanged(nameof(IssuesFound)); OnPropertyChanged(nameof(StatusText)); } }

        private int _passedChecks;
        public int PassedChecks { get => _passedChecks; set { _passedChecks = value; OnPropertyChanged(nameof(PassedChecks)); OnPropertyChanged(nameof(StatusText)); } }

        public string StatusText => $"Passed: {PassedChecks} | Issues: {IssuesFound}";

        public HealthCheckViewModel()
        {
            RunCheckCommand = new RelayCommand(RunHealthCheck);
        }

        private void RunHealthCheck()
        {
            var results = new System.Text.StringBuilder();
            IssuesFound = 0;
            PassedChecks = 0;

            CheckRobloxInstallation(results);
            CheckClientSettings(results);
            CheckFlagManager(results);
            CheckNetwork(results);
            CheckPaths(results);
            CheckPermissions(results);

            CheckResults = results.ToString();
        }

        private void CheckRobloxInstallation(System.Text.StringBuilder results)
        {
            string robloxPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
            if (System.IO.Directory.Exists(robloxPath))
            {
                var versions = System.IO.Directory.GetDirectories(System.IO.Path.Combine(robloxPath, "Versions"));
                if (versions.Length > 0)
                {
                    results.AppendLine("✓ Roblox is installed with " + versions.Length + " version(s)");
                    PassedChecks++;
                }
                else
                {
                    results.AppendLine("✗ Roblox Versions directory is empty");
                    IssuesFound++;
                }
            }
            else
            {
                results.AppendLine("✗ Roblox installation not found");
                IssuesFound++;
            }
        }

        private void CheckClientSettings(System.Text.StringBuilder results)
        {
            string clientSettings = System.IO.Path.Combine(Paths.Base, "ClientSettings", "ClientAppSettings.json");
            if (System.IO.File.Exists(clientSettings))
            {
                var info = new System.IO.FileInfo(clientSettings);
                results.AppendLine("✓ FFlag file exists (" + info.Length + " bytes, modified " + info.LastWriteTime.ToString("MMM dd HH:mm") + ")");
                PassedChecks++;
            }
            else
            {
                results.AppendLine("✗ FFlag file (ClientAppSettings.json) not found");
                IssuesFound++;
            }
        }

        private void CheckFlagManager(System.Text.StringBuilder results)
        {
            try
            {
                var flags = App.FastFlags.GetAllFlags();
                results.AppendLine("✓ FastFlagManager loaded with " + flags.Count() + " flags");
                PassedChecks++;
            }
            catch (Exception ex)
            {
                results.AppendLine("✗ FastFlagManager error: " + ex.Message);
                IssuesFound++;
            }
        }

        private void CheckNetwork(System.Text.StringBuilder results)
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = client.GetAsync("https://roblox.com").Result;
                results.AppendLine("✓ Network connectivity OK (status: " + response.StatusCode + ")");
                PassedChecks++;
            }
            catch
            {
                results.AppendLine("✗ Network connectivity issue - cannot reach roblox.com");
                IssuesFound++;
            }
        }

        private void CheckPaths(System.Text.StringBuilder results)
        {
            if (Paths.Initialized)
            {
                results.AppendLine("✓ NexusStrap paths initialized (" + Paths.Base + ")");
                PassedChecks++;
            }
            else
            {
                results.AppendLine("✗ NexusStrap paths not initialized");
                IssuesFound++;
            }
        }

        private void CheckPermissions(System.Text.StringBuilder results)
        {
            try
            {
                string testFile = System.IO.Path.Combine(Paths.Base, "write_test.tmp");
                System.IO.File.WriteAllText(testFile, "test");
                System.IO.File.Delete(testFile);
                results.AppendLine("✓ Write permissions OK");
                PassedChecks++;
            }
            catch
            {
                results.AppendLine("✗ Write permission issue in NexusStrap directory");
                IssuesFound++;
            }
        }
    }
}
