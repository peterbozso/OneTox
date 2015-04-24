using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinTox.Converters
{
    public class IsConnectedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var IsConnected = (bool) value;
            if (IsConnected)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}