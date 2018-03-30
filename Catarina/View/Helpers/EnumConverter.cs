using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Catarina.View.Helpers
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            foreach (var one in Enum.GetValues(parameter as Type))
            {
                if (value.Equals(one))
                    return one.GetDescription();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            foreach (var one in Enum.GetValues(parameter as Type))
            {
                if (value.ToString() == one.GetDescription())
                    return one;
            }
            return null;
        }
    }

    public static class EnumerationExtension
    {
        public static string GetDescription(this object value)
        {
            // get attributes  
            var field = value.GetType().GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

            // return description
            return attributes.Any() ? ((System.ComponentModel.DescriptionAttribute)attributes.ElementAt(0)).Description : "Description Not Found";
        }
    }
}
