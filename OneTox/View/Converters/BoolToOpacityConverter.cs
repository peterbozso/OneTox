using System;
using Windows.UI.Xaml.Data;

namespace OneTox.View.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isTrue = (bool)value;
            if (isTrue)
                return 1;
            return 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
