namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class ModConflictPage
    {
        public ModConflictPage()
        {
            DataContext = new ViewModels.Settings.ModConflictViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("Mod Conflict Detector");
        }
    }
}
