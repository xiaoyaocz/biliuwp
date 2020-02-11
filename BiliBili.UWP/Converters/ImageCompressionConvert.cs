using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BiliBili.UWP.Converters
{
    public class ImageCompressionConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "ms-appx:///Assets/Logo/PI160_100.png";
            }

            if (value.ToString().Contains("@"))
            {
                return value;
            }
            return value.ToString() + "@" + parameter.ToString() + ".jpg";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
