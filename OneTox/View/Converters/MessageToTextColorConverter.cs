using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using OneTox.ViewModel.Messaging;
using SharpTox.Core;

namespace OneTox.View.Converters
{
    public class MessageToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var message = (ToxMessageViewModelBase) value;
            switch (message.MessageType)
            {
                case ToxMessageType.Message:
                    if (message is ReceivedMessageViewModel)
                        return Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush;
                    if (message is SentMessageViewModel)
                        return Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
                    break;
                case ToxMessageType.Action:
                    if (message is ReceivedMessageViewModel)
                        return Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
                    if (message is SentMessageViewModel)
                        return Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
                    break;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}