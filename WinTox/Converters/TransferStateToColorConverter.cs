using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinTox.ViewModel.FileTransfer;

namespace WinTox.Converters
{
    internal class TransferStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FileTransferState) value;
            switch (state)
            {
                case FileTransferState.Uploading:
                case FileTransferState.Downloading:
                    return Application.Current.Resources["MessageColor"];
                case FileTransferState.Finished:
                    return Application.Current.Resources["StatusGreen"];
                case FileTransferState.Cancelled:
                    return Application.Current.Resources["StatusRed"];
                case FileTransferState.PausedByUser:
                case FileTransferState.PausedByFriend:
                    return Application.Current.Resources["StatusYellow"];
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