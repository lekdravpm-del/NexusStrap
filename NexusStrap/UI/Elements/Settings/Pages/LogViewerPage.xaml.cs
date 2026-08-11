namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class LogViewerPage
    {
        public LogViewerPage()
        {
            DataContext = new ViewModels.Settings.LogViewerViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Log Viewer");
        }
    }
}
