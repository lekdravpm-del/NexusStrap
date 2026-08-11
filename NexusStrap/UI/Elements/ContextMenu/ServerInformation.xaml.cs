using NexusStrap.UI.ViewModels.ContextMenu;

namespace NexusStrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for ServerInformation.xaml
    /// </summary>
    public partial class ServerInformation
    {
        public ServerInformation(Watcher watcher)
        {
            DataContext = new ServerInformationViewModel(watcher);
            InitializeComponent();
        }
    }
}
