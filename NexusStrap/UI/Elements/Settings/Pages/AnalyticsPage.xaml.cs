namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class AnalyticsPage
    {
        public AnalyticsPage()
        {
            DataContext = new ViewModels.Settings.AnalyticsViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Analytics Dashboard");
        }
    }
}
