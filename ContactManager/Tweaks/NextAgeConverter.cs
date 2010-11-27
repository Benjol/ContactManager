using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace ContactManager
{
    public class NextAgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) return "";
            int age = (int)value;
            return age + 1;
        }
        
            //int adjust = 0;
            //if(reference.Month >= target.Month && reference.Day > target.Day) adjust = 1;
            //return target.AddYears((reference.Year - target.Year) + adjust);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
