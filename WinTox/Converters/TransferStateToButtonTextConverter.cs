using System;
using Windows.UI.Xaml.Data;
using WinTox.ViewModel.FileTransfer;

namespace WinTox.Converters
{
    internal class TransferStateToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FileTransferState) value;
            if (state == FileTransferState.Downloading || state == FileTransferState.Uploading)
                return "Pause";
            if (state == FileTransferState.Paused)
                return "Resume";
            return null; // The button should be hidden in the other two states.
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}