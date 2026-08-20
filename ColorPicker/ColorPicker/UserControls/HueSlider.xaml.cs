using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ColorPicker.Models;

namespace ColorPicker.UserControls
{
    internal partial class HueSlider : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(HueSlider),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(HueSlider),
                new PropertyMetadata(1.0));

        public HueSlider()
        {
            InitializeComponent();
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
        {
            Value = MathHelper.Mod(Value + SmallChange * args.Delta / 120, 360);
            args.Handled = true;
        }
    }
}