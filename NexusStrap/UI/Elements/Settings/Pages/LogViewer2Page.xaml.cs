using Wpf.Ui.Controls;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class LogViewer2Page : UiPage
    {
        public LogViewer2Page()
        {
            InitializeComponent();
            DataContext = new ViewModels.Settings.LogViewer2ViewModel();
            App.RichPresence?.SetPage("Log Viewer 2");
        }
    }
}
