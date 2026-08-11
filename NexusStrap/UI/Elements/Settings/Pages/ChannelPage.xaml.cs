using NexusStrap.UI.Elements.ContextMenu;
using NexusStrap.UI.Elements.Dialogs;
using NexusStrap.UI.ViewModels.Settings;
using Microsoft.Win32;
using System.Windows;
using Wpf.Ui.Hardware;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for ChannelPage.xaml
    /// </summary>
    public partial class ChannelPage
    {
        public ChannelPage()
        {
            DataContext = new ChannelViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Settings");
        }

        private void OpenChannelListDialog_Click(object sender, RoutedEventArgs e)
        {
            App.RichPresence?.SetDialog("Channel List");

            var dialog = new ChannelListsDialog();
            dialog.Owner = Window.GetWindow(this);

            dialog.ShowDialog();

            App.RichPresence?.ClearDialog();
        }
    }
}