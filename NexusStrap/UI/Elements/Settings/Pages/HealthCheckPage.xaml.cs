namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class HealthCheckPage
    {
        public HealthCheckPage()
        {
            DataContext = new ViewModels.Settings.HealthCheckViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Health Check");
        }
    }
}
