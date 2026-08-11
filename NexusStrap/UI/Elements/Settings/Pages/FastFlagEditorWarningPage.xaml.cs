using NexusStrap.UI.ViewModels.Settings;
using System.Windows;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class FastFlagEditorWarningPage
    {
        private readonly FastFlagEditorWarningViewModel _viewModel;

        public FastFlagEditorWarningPage()
        {
            _viewModel = new FastFlagEditorWarningViewModel(this);
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StartCountdown();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopCountdown();
        }
    }
}
