using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BiliBili.UWP.Converters
{
    public class ImageSourceConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/Logo/PI160_100.png"));
            }
            if (value.ToString().Contains("@"))
            {
                return new BitmapImage(new Uri(value.ToString()));
            }
            if (value.ToString().Contains("ms-appx"))
            {
                return new BitmapImage(new Uri(value.ToString()));
            }
            var url = value.ToString() + "@" + parameter.ToString() + ".jpg";
            return new BitmapImage(new Uri(url));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
