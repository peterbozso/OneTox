using Windows.UI.Xaml.Data;

namespace WinTox.Converters
{
    class FileTransferCountToPlaceholderTextConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            var count = (int) value;
            if (count == 1)
                return "There is 1 ongoing file transfer.";
            return "There are " + count + " ongoing file transfers.";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
