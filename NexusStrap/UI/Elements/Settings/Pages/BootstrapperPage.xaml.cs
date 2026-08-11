using NexusStrap.UI.ViewModels.Settings;
using NexusStrap.UI.Elements.Dialogs;
using Microsoft.Win32;
using System.Windows;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for BehaviourPage.xaml
    /// </summary>
    public partial class BehaviourPage
    {
        public BehaviourPage()
        {
            DataContext = new BehaviourViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Bootstrapper");
        }

        private void OpenMultiblox_Click(object sender, RoutedEventArgs e)
        {
            var window = new MultibloxDialog
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }

        private void BrowseLaunchSound_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Audio Files|*.mp3;*.wav;*.ogg|All Files|*.*",
                Title = "Select Launch Sound"
            };

            if (dialog.ShowDialog() == true)
            {
                var vm = (BehaviourViewModel)DataContext;
                vm.LaunchSoundPath = dialog.FileName;
            }
        }
    }
}
