using System.Windows.Controls;

using NexusStrap.UI.ViewModels.Settings;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for IntegrationsPage.xaml
    /// </summary>
    public partial class IntegrationsPage
    {
        public IntegrationsPage()
        {
            DataContext = new IntegrationsViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Integrations");
        }

        public void CustomIntegrationSelection(object sender, SelectionChangedEventArgs e)
        {
            IntegrationsViewModel viewModel = (IntegrationsViewModel)DataContext;
            viewModel.SelectedCustomIntegration = (CustomIntegration)((ListBox)sender).SelectedItem;
            viewModel.OnPropertyChanged(nameof(viewModel.SelectedCustomIntegration));
        }

        private static string StartMenuShortcutPath =>
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "NexusStrap.lnk");

        private void StartMenuTile_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            string args = App.Settings.Prop.StartMenuTileArgs;
            if (string.IsNullOrWhiteSpace(args))
                args = "-player";

            Shortcut.Create(Paths.Process, args, StartMenuShortcutPath);
        }

        private void StartMenuTile_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(StartMenuShortcutPath))
                    System.IO.File.Delete(StartMenuShortcutPath);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("IntegrationsPage::RemoveShortcut", ex);
            }
        }
    }
}
