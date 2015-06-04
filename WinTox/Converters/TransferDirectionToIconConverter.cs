using Windows.UI.Xaml.Data;
using WinTox.ViewModel.FileTransfer;

namespace WinTox.Converters
{
    class TransferDirectionToIconConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            var direction = (FileTransferDirection) value;
            switch (direction)
            {
                case FileTransferDirection.Down:
                    return "";
                case FileTransferDirection.Up:
                    return "";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
