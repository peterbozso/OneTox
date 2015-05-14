using System;
using Windows.UI.Xaml.Data;

namespace WinTox.Converters
{
    internal class FriendNameToTypingStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as string) + " is typing...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}