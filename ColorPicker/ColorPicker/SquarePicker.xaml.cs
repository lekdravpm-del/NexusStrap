using System.Windows;
using ColorPicker.Models;

namespace ColorPicker
{
    public partial class SquarePicker : PickerControlBase
    {
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(SquarePicker),
                new PropertyMetadata(1.0));

        public SquarePicker()
        {
            InitializeComponent();
        }
        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }
    }
}