using System;
using System.Globalization;
using System.Windows.Data;

namespace RocketPlus.ValueConverters
{
    public class BoolToConnectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                if (boolean == true)
                {
                    return "Disconnect";
                }
                return "Connect";
            }
            return "Connect";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == "Disconnect")
            {
                return true;
            }
            return false;
        }
    }
}
