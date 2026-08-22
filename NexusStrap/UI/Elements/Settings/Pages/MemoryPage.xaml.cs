using System.Windows;
using Wpf.Ui.Controls;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class MemoryPage : UiPage
    {
        public MemoryPage()
        {
            InitializeComponent();
            DataContext = new NexusStrap.UI.ViewModels.Settings.MemoryViewModel();
        }

        private void CleanNow_Click(object sender, RoutedEventArgs e)
        {
            MemoryManager.CleanNow();
            StatusText.Text = "RAM cleaned.";
        }
    }
}
