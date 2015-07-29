using System;
using Windows.UI.Xaml.Data;
using OneTox.ViewModel;

namespace OneTox.View.Converters
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