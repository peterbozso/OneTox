using OneTox.ViewModel;
using OneTox.ViewModel.Friends;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace OneTox.View.Converters
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
