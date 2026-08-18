using System.Windows;
using System.Windows.Controls;

using NexusStrap.UI.ViewModels.Installer;

namespace NexusStrap.UI.Elements.Installer.Pages
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class InstallPage
    {
        private readonly InstallViewModel _viewModel = new();

        public InstallPage()
        {
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.DoInstall() && Window.GetWindow(this) is MainWindow window)
                window.NextPage();
        }
    }
}
