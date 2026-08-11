using NexusStrap.UI.ViewModels.Settings;

namespace NexusStrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for NexusStrapPage.xaml
    /// </summary>
    public partial class NexusStrapPage
    {
        public NexusStrapPage()
        {
            DataContext = new NexusStrapViewModel();
            InitializeComponent();
        }
    }
}