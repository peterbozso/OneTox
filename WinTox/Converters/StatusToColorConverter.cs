using SharpTox.Core;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace WinTox.Converters
{
    internal class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (ToxUserStatus)value;
            switch (status)
            {
                case ToxUserStatus.None:
                    return new SolidColorBrush(Colors.LawnGreen);

                case ToxUserStatus.Busy:
                    return new SolidColorBrush(Colors.Red);

                case ToxUserStatus.Away:
                    return new SolidColorBrush(Colors.Yellow);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
