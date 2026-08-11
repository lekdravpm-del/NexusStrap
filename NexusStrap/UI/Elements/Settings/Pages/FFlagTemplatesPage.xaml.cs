using NexusStrap.UI.ViewModels.Settings;
using System.Windows;
using System.Windows.Controls;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    public partial class FFlagTemplatesPage
    {
        private FFlagTemplatesViewModel _viewModel = null!;

        public FFlagTemplatesPage()
        {
            SetupViewModel();
            InitializeComponent();
            App.RichPresence?.SetPage("FFlag Templates");
        }

        private void SetupViewModel()
        {
            _viewModel = new FFlagTemplatesViewModel();
            DataContext = _viewModel;
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string category)
            {
                _viewModel.SelectedCategory = category;
            }
        }
    }
}
