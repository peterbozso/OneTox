using System;
using Windows.UI.Xaml.Data;
using WinTox.ViewModel.Messaging;

namespace WinTox.View.Converters
{
    internal class DeliveryStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (MessageDeliveryState) value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}