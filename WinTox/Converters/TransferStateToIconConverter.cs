using System;
using Windows.UI.Xaml.Data;
using WinTox.ViewModel.FileTransfer;

namespace WinTox.Converters
{
    internal class TransferStateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var direction = (FileTransferState) value;
            switch (direction)
            {
                case FileTransferState.Downloading:
                    return "";
                case FileTransferState.Uploading:
                    return "";
                case FileTransferState.Paused:
                    return "";
                case FileTransferState.Finished:
                    return "";
                case FileTransferState.Cancelled:
                    return "";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}