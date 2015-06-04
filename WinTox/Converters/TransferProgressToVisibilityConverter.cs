using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinTox.Converters
{
    internal class TransferProgressToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            var isTransferFinished = ((Double) value).Equals(100.0);
            if (isTransferFinished)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}