using System;
using System.Globalization;
using System.Windows.Data;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Converts from the custom CustomBrush type to a Brush object for WPF databinding
    /// </summary>
    public class CustomBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a CustomBrush object into a Brush object
        /// </summary>
        /// <param name="value">The CustomBrush object to convert</param>
        /// <param name="targetType">Not implemented</param>
        /// <param name="parameter">Not implemented</param>
        /// <param name="culture">Not implemented</param>
        /// <returns>The Brush property from the CustomBrush object</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((CustomBrush)value).Brush;
        }

        /// <summary>
        /// This method is not implemented
        /// </summary>
        /// <param name="value">Not implemented</param>
        /// <param name="targetType">Not implemented</param>
        /// <param name="parameter">Not implemented</param>
        /// <param name="culture">Not implemented</param>
        /// <returns>Null</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
