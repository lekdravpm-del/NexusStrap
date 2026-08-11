using NexusStrap.UI.ViewModels.AccountManagers;

namespace NexusStrap.UI.Elements.AccountManagers.Pages
{
    /// <summary>
    /// Interaction logic for FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage
    {
        public FriendsPage()
        {
            DataContext = new FriendsViewModel();
            InitializeComponent();
        }
    }
}
