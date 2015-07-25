using System;
using Windows.UI.Xaml.Data;
using WinTox.ViewModel;

namespace WinTox.View.Converters
{
    internal class CallStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (CallViewModel.CallState) value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}