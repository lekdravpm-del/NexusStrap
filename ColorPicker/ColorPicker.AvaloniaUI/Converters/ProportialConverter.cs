using System.Globalization;
using Avalonia.Data.Converters;
using System.Linq;
using Avalonia; // Required for AvaloniaProperty.UnsetValue

namespace ColorPicker.Converters;

internal class ProportialConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Any(v => v == AvaloniaProperty.UnsetValue || v == null))
        {
            return 0.0;
        }

        try
        {
            double[] doubleValues = values.Select(System.Convert.ToDouble).ToArray();

            if (doubleValues.Length >= 3 && doubleValues[2] != 0)
            {
                return doubleValues[0] * (doubleValues[1] / doubleValues[2]);
            }
        }
        catch
        {
            return 0.0;
        }

        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}