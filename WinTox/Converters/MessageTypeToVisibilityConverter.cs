using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using SharpTox.Core;

namespace WinTox.Converters
{
    internal class MessageTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var messageType = (ToxMessageType) value;
            switch (messageType)
            {
                case ToxMessageType.Message:
                    return Visibility.Visible;
                case ToxMessageType.Action:
                    return Visibility.Collapsed;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}