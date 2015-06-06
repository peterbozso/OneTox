using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using SharpTox.Core;

namespace WinTox.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (ToxUserStatus) value;
            switch (status)
            {
                case ToxUserStatus.None:
                    return Application.Current.Resources["StatusGreen"];

                case ToxUserStatus.Busy:
                    return Application.Current.Resources["StatusRed"];

                case ToxUserStatus.Away:
                    return Application.Current.Resources["StatusYellow"];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}