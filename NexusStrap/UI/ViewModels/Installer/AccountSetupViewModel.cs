using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Integrations;
using NexusStrap.UI.Elements.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;

namespace NexusStrap.UI.ViewModels.Installer
{
    public partial class AccountSetupViewModel : ObservableObject
    {
        private const string LOG_IDENT = "AccountSetupVM";

        [ObservableProperty] private bool _isLoggedIn;
        [ObservableProperty] private string _displayName = "";
        [ObservableProperty] private string _username = "";
        [ObservableProperty] private string _avatarUrl = "";
        [ObservableProperty] private string _userIdText = "";
        [ObservableProperty] private ObservableCollection<string> _addMethods = new(new[] { "Quick Sign-In", "Browser", "Manual" });
        [ObservableProperty] private string _selectedAddMethod = "Quick Sign-In";
        [ObservableProperty] private bool _isInstallingChromium;
        [ObservableProperty] private bool _isBusy;

        private AccountManager Manager => AccountManager.Shared;

        public AccountSetupViewModel()
        {
            RefreshFromManager();
            Manager.ActiveAccountChanged += OnActiveChanged;
        }

        private void OnActiveChanged(AltAccount? acc)
        {
            App.Current.Dispatcher.Invoke(RefreshFromManager);
        }

        private async void RefreshFromManager()
        {
            var acc = Manager.ActiveAccount;
            if (acc != null)
            {
                IsLoggedIn = true;
                DisplayName = acc.DisplayName;
                Username = $"@{acc.Username}";
                UserIdText = acc.UserId.ToString();
                try
                {
                    var url = $"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={acc.UserId}&size=150x150&format=Png&isCircular=true";
                    var resp = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>(url);
                    AvatarUrl = resp?.Data?.FirstOrDefault()?.ImageUrl ?? "";
                }
                catch { AvatarUrl = ""; }
            }
            else
            {
                IsLoggedIn = false;
                DisplayName = "";
                Username = "";
                UserIdText = "";
                AvatarUrl = "";
            }
        }

        [RelayCommand]
        private async Task Login()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                AltAccount? newAcc = null;
                if (SelectedAddMethod == "Quick Sign-In")
                {
                    newAcc = await Manager.AddAccountByQuickSignInAsync();
                    if (newAcc == null)
                    {
                        Frontend.ShowMessageBox("Quick Sign-In cancelled or failed. Try Browser.", MessageBoxImage.Information);
                        return;
                    }
                }
                else if (SelectedAddMethod == "Browser")
                {
                    IsInstallingChromium = true;
                    newAcc = await Manager.AddAccountByBrowser();
                }
                else // Manual
                {
                    var dlg = new ManualCookieDialog();
                    dlg.Owner = Application.Current.MainWindow;
                    var res = dlg.ShowDialog();
                    if (res == true && dlg.ViewModel.ValidatedAccount != null)
                        newAcc = dlg.ViewModel.ValidatedAccount;
                    else return;
                }

                if (newAcc != null)
                {
                    // ensure stored
                    var existing = Manager.Accounts.FirstOrDefault(a => a.UserId == newAcc.UserId);
                    if (existing == null)
                    {
                        existing = Manager.AddManualAccount(newAcc.SecurityToken, newAcc.UserId, newAcc.Username, newAcc.DisplayName);
                    }
                    if (existing != null)
                    {
                        Manager.SetActiveAccount(existing);
                        NexusFriendRegistry.Register(existing.UserId, existing.Username, existing.DisplayName);
                        RefreshFromManager();
                        Frontend.ShowMessageBox($"Logged in as {existing.DisplayName} (ID {existing.UserId})", MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"{LOG_IDENT}::Login", ex.Message);
                Frontend.ShowMessageBox($"Login failed: {ex.Message}", MessageBoxImage.Error);
            }
            finally
            {
                IsInstallingChromium = false;
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CopyId()
        {
            if (string.IsNullOrWhiteSpace(UserIdText)) return;
            try
            {
                Clipboard.SetText(UserIdText);
                Frontend.ShowMessageBox($"Copied ID {UserIdText}", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"{LOG_IDENT}::CopyId", ex.Message);
            }
        }
    }
}
