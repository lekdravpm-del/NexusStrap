using NexusStrap.UI.ViewModels.Dialogs;

namespace NexusStrap.UI.Elements.Dialogs
{
    public partial class CommunityModInfoDialog
    {
        public CommunityModInfoViewModel ViewModel { get; }

        public CommunityModInfoDialog(CommunityMod mod)
        {
            InitializeComponent();
            ViewModel = new CommunityModInfoViewModel(mod, this);
            DataContext = ViewModel;
        }
    }
}