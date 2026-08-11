using NexusStrap.UI.ViewModels.Settings;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class OptimizationSetupPage
    {
        private readonly OptimizationSetupViewModel _viewModel;

        public OptimizationSetupPage()
        {
            _viewModel = new OptimizationSetupViewModel(this);
            DataContext = _viewModel;

            InitializeComponent();
        }
    }
}