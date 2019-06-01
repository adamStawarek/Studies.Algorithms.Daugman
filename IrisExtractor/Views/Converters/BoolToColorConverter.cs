using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ImageEditor.Views.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Color.FromRgb(255, 255, 255));
            return (bool) value ? new SolidColorBrush(Color.FromRgb(0,255,0)) : new SolidColorBrush(Color.FromRgb(255,0,0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}