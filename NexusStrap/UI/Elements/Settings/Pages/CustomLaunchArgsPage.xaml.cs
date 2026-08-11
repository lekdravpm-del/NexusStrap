namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class CustomLaunchArgsPage
    {
        public CustomLaunchArgsPage()
        {
            DataContext = new ViewModels.Settings.CustomLaunchArgsViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Custom Launch Arguments");
        }
    }
}
