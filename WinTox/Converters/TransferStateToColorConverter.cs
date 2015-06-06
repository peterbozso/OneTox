using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
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