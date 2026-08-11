using NexusStrap.UI.ViewModels.Settings;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class ThemeStorePage
    {
        public ThemeStorePage()
        {
            DataContext = new ThemeStoreViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("ThemeStore");
        }
    }
}
