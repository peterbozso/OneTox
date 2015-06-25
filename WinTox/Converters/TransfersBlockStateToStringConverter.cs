using System;
using Windows.UI.Xaml.Data;
using WinTox.ViewModel.FileTransfers;

namespace WinTox.Converters
{
    internal class TransfersBlockStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FileTransfersViewModel.FileTransfersVisualStates.TransfersBlockState) value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}