using System;
using Windows.UI.Xaml.Data;
using OneTox.Model.FileTransfers;

namespace OneTox.View.Converters
{
    internal class TransferStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FileTransferState) value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}