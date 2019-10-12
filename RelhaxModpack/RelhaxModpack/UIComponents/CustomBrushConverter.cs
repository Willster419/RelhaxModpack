using System;
using System.Globalization;
using System.Windows.Data;

namespace RelhaxModpack.UIComponents
{
    public class CustomBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((CustomBrush)value).Brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
