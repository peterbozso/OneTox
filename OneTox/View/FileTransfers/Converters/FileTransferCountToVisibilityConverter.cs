using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace OneTox.View.FileTransfers.Converters
{
    public class FileTransferCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var count = (int) value;

            if (count > 0)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}