using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Converters
{
    internal class SenderTypeToTextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FriendViewModel)
                return TextAlignment.Left;

            if (value is UserViewModel)
                return TextAlignment.Right;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}