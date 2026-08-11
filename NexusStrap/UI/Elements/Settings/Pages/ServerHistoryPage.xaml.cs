namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class ServerHistoryPage
    {
        public ServerHistoryPage()
        {
            DataContext = new ViewModels.Settings.ServerHistoryViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Server History");
        }
    }
}
