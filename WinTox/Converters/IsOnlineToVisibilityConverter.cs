using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinTox.Converters
{
    public class IsOnlineToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isOnline = (bool) value;
            if (isOnline)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}