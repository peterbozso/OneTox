using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using SharpTox.Core;

namespace WinTox.Converters
{
    class MessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var messageType = (ToxMessageType)value;
            switch (messageType)
            {
                case ToxMessageType.Message:
                    return Application.Current.Resources["MessageColor"] as SolidColorBrush;
                case ToxMessageType.Action:
                    return Application.Current.Resources["MainColor"] as SolidColorBrush;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
