using NexusStrap.UI.ViewModels.Settings;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class ModsPresetsPage
    {
        private ModsPresetsViewModel _viewModel = null!;

        public ModsPresetsPage()
        {
            _viewModel = new ModsPresetsViewModel();
            DataContext = _viewModel;

            InitializeComponent();
            App.RichPresence?.SetPage("Preset Mods");
        }
    }
}