using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Converters
{
    public class SenderTypeToMessageBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FriendViewModel)
                return Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;

            if (value is UserViewModel)
                return Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"] as SolidColorBrush;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}