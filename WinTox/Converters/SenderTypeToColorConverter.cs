using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WinTox.ViewModel;

namespace WinTox.Converters
{
    internal class SenderTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var senderType = (MessageViewModel.MessageSenderType)value;
            switch (senderType)
            {
                case MessageViewModel.MessageSenderType.Friend:
                    return Application.Current.Resources["TextColor"] as SolidColorBrush;

                case MessageViewModel.MessageSenderType.User:
                    return Application.Current.Resources["ChatBackgroundColor"] as SolidColorBrush;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
