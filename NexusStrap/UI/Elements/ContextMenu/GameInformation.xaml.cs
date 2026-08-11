using NexusStrap.UI.ViewModels.ContextMenu;

namespace NexusStrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for GameInformation.xaml
    /// </summary>
    public partial class GameInformation
    {
        public GameInformation(long placeId, long universeId)
        {
            DataContext = new GameInformationViewModel(placeId, universeId);
            InitializeComponent();
        }
    }
}