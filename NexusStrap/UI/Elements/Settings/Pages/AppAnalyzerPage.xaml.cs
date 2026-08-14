namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class AppAnalyzerPage
    {
        public AppAnalyzerPage()
        {
            DataContext = new ViewModels.Settings.AppAnalyzerViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("App Analyzer");
        }
    }
}