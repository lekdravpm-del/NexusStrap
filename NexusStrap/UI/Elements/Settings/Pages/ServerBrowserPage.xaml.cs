namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class ServerBrowserPage
    {
        public ServerBrowserPage()
        {
            DataContext = new ViewModels.Settings.ServerBrowserViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Server Browser");
        }
    }
}