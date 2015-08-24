using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace OneTox.View.Converters
{
    internal class IsMutedToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isMuted = (bool)value;

            if (isMuted)
                return Application.Current.Resources["StatusRed"] as SolidColorBrush;

            return Application.Current.Resources["StatusGreen"] as SolidColorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
