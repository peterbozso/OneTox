using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace OneTox.View.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isTrue = (bool)value;
            if (isTrue)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
