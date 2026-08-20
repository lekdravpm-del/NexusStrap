using System;
using System.Globalization;
using System.Windows.Data;

namespace ColorPicker.Converters
{
    internal class ProportialConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 3 && values[0] is double size && values[1] is double pos && values[2] is double range)
            {
                if (range == 0) return 0.0;

                double result = size * (pos / range);

                if (values.Length > 3 && values[3] != null && values[3].ToString() == "True")
                {
                    return size - result;
                }

                return result;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}