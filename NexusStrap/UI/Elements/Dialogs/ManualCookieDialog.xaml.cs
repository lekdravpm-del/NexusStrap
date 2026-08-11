using NexusStrap.UI.Elements.Base;
using NexusStrap.UI.ViewModels.Dialogs;

namespace NexusStrap.UI.Elements.Dialogs
{
    public partial class ManualCookieDialog : WpfUiWindow
    {
        public ManualCookieDialogViewModel ViewModel { get; }

        public ManualCookieDialog()
        {
            ViewModel = new ManualCookieDialogViewModel(this);
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}