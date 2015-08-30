using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Converters
{
    internal class SenderTypeToMessageMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FriendViewModel)
                return new Thickness(0, 0, 120, 0);

            if (value is UserViewModel)
                return new Thickness(120, 0, 0, 0);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}