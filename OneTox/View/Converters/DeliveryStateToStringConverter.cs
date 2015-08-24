using OneTox.ViewModel.Messaging;
using System;
using Windows.UI.Xaml.Data;

namespace OneTox.View.Converters
{
    internal class DeliveryStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (MessageDeliveryState)value;
            return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
