using OneTox.Model.FileTransfers;
using System;
using Windows.UI.Xaml.Data;

namespace OneTox.View.Converters
{
    internal class TransferStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FileTransferState)value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
