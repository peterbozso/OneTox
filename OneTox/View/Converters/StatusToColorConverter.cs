using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using OneTox.ViewModel;

namespace OneTox.View.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (ExtendedToxUserStatus) value;
            switch (status)
            {
                case ExtendedToxUserStatus.Available:
                    return Application.Current.Resources["StatusGreen"];
                case ExtendedToxUserStatus.Busy:
                    return Application.Current.Resources["StatusRed"];
                case ExtendedToxUserStatus.Away:
                    return Application.Current.Resources["StatusYellow"];
                case ExtendedToxUserStatus.Offline:
                    return Application.Current.Resources["StatusGrey"];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}