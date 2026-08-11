using NexusStrap.Models;
using NexusStrap.Models.APIs.Config;
using NexusStrap.UI.ViewModels.Settings;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class RobloxSettingsPage : UiPage
    {
        private RobloxSettingsViewModel? _viewModel;

        public RobloxSettingsPage()
        {
            InitializeComponent();
            Loaded += RobloxSettingsPage_Loaded;
        }

        private async void RobloxSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.RichPresence?.SetPage("Roblox Settings");

            if (_viewModel != null) 
                return;

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow?.ShowLoading("Loading Roblox Settings...");

            try
            {
                await App.RemoteData.WaitUntilDataFetched();

                _viewModel = new RobloxSettingsViewModel(App.RemoteData);
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("RobloxSettingsPage::Loaded", $"Error: {ex}");
                Frontend.ShowMessageBox($"Failed to load Roblox settings:\n\n{ex.Message}", MessageBoxImage.Error);
            }
            finally
            {
                mainWindow?.HideLoading();
            }
        }

        private void ValidateUInt32(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                e.Handled = !uint.TryParse(newText, out _);
            }
        }

        private void ValidateFloat(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                e.Handled = !Regex.IsMatch(newText, @"^-?\d*\.?\d*$");
            }
        }
    }
}
