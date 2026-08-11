using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace NexusStrap.UI.ViewModels.Bootstrapper
{
    public class NexusStrapDialogViewModel : BootstrapperDialogViewModel
    {
        public BackgroundType WindowBackdropType { get; set; } = BackgroundType.Mica;

        public SolidColorBrush BackgroundColourBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        public NexusStrapDialogViewModel(IBootstrapperDialog dialog) : base(dialog)
        {
        }
    }
}