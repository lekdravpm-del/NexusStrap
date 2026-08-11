namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class AntiCheatPage
    {
        public AntiCheatPage()
        {
            DataContext = new ViewModels.Settings.AntiCheatViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Anti-Cheat Awareness");
        }
    }
}
