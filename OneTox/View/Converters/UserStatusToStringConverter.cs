using System;
using Windows.UI.Xaml.Data;
using SharpTox.Core;

namespace OneTox.View.Converters
{
    internal class UserStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var userStatus = (ToxUserStatus) value;
            if (userStatus == ToxUserStatus.None)
                return "Available";
            return userStatus;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}