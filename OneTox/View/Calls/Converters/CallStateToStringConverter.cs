using System;
using Windows.UI.Xaml.Data;
using OneTox.ViewModel.Calls;

namespace OneTox.View.Calls.Converters
{
    internal class CallStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (AudioCallViewModel.CallState) value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}