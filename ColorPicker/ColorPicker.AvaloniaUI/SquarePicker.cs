using Avalonia;
using ColorPicker.Models;

namespace ColorPicker;

public class SquarePicker : PickerControlBase
{
    public static readonly StyledProperty<double> SmallChangeProperty = AvaloniaProperty.Register<SquarePicker, double>(
        nameof(SmallChange), 1.0);

    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }
}