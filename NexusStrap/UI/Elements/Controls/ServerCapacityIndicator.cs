using System.Windows;
using System.Windows.Media;

namespace NexusStrap.UI.Elements.Controls
{
    public class ServerCapacityIndicator : FrameworkElement
    {
        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register(nameof(Capacity), typeof(double), typeof(ServerCapacityIndicator),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Capacity
        {
            get => (double)GetValue(CapacityProperty);
            set => SetValue(CapacityProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double width = ActualWidth;
            double height = ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            const int stepCount = 5;
            const double gap = 2.0;
            double stepWidth = (width - gap * (stepCount - 1)) / stepCount;
            double capacity = Math.Clamp(Capacity, 0, 1);

            for (int i = 0; i < stepCount; i++)
            {
                double stepHeight = height * (i + 1) / stepCount;
                double level = (i + 1) / (double)stepCount;
                bool lit = capacity >= level - 0.0001;

                var brush = lit
                    ? new SolidColorBrush(Color.FromRgb(0x3D, 0xDC, 0x68))
                    : new SolidColorBrush(Color.FromRgb(0x3A, 0x41, 0x49));
                brush.Freeze();

                dc.DrawRectangle(brush, null, new Rect(i * (stepWidth + gap), height - stepHeight, stepWidth, stepHeight));
            }
        }
    }
}