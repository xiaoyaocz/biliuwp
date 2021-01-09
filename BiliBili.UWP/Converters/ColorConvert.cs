﻿using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace BiliBili.UWP.Converters
{
	public class ColorConvert : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
			{
				return new SolidColorBrush(Colors.Transparent);
			}
			Color color = new Color();
			try
			{
				var obj = value.ToString().Replace("#", "");

				if (obj.Length == 4)
				{
					obj = "00" + obj;
				}
				if (obj.Length == 6)
				{
					color.R = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					color.G = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
					color.B = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
					color.A = 255;
				}
				if (obj.Length == 8)
				{
					color.R = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
					color.G = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
					color.B = byte.Parse(obj.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
					color.A = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				}
			}
			catch (Exception)
			{
				color = Colors.Transparent;
			}

			return new SolidColorBrush(color);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value;
		}
	}
}