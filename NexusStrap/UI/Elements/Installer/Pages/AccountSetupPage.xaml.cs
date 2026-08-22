using System.Windows;
using NexusStrap.UI.ViewModels.Installer;

namespace NexusStrap.UI.Elements.Installer.Pages
{
    public partial class AccountSetupPage
    {
        private readonly AccountSetupViewModel _vm = new();

        public AccountSetupPage()
        {
            DataContext = _vm;
            InitializeComponent();
        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow w)
                w.SetNextButtonText("Next");
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow w)
                w.NextPage();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow w)
                w.NextPage();
        }
    }
}
