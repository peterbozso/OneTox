using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WinTox.ViewModel;

namespace WinTox.Converters
{
    public class SenderTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FriendViewModel)
                return Application.Current.Resources["TextColor"] as SolidColorBrush;

            if (value is UserViewModel)
                return Application.Current.Resources["ChatBackgroundColor"] as SolidColorBrush;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}