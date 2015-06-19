using System;
using Windows.UI.Xaml.Data;
using WinTox.ViewModel.FileTransfers;

namespace WinTox.Converters
{
    internal class BlockStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FileTransfersViewModel.BlockState) value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}