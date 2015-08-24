using OneTox.ViewModel.Calls;
using System;
using Windows.UI.Xaml.Data;

namespace OneTox.View.Converters
{
    internal class CallStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (AudioCallViewModel.CallState)value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
