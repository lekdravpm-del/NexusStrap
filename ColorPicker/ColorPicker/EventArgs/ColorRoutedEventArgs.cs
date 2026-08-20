using System.Windows;
using System.Windows.Media;

namespace ColorPicker.EventArg
{
    public class ColorRoutedEventArgs : RoutedEventArgs
    {
        public ColorRoutedEventArgs(RoutedEvent routedEvent, Color color) : base(routedEvent)
        {
            Color = color;
        }

        public Color Color { get; private set; }
    }
}