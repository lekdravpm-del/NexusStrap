using NexusStrap.UI.ViewModels.AccountManagers;
using NexusStrap.UI.Elements.Dialogs;
using System.Windows;

namespace NexusStrap.UI.Elements.AccountManagers.Pages
{
    public partial class AccountsPage
    {
        private AccountsViewModel? _viewModel;

        public AccountsPage()
        {
            DataContext = new AccountsViewModel();
            InitializeComponent();
            _viewModel = DataContext as AccountsViewModel;
        }

        private void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddFriendDialog();
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
        }
    }
}