using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Messaging.Converters
{
    internal class SenderTypeToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FriendViewModel)
                return HorizontalAlignment.Left;

            if (value is UserViewModel)
                return HorizontalAlignment.Right;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}