﻿using System;
using Windows.UI.Xaml.Data;

namespace BiliBili.UWP.Converters
{
	public class DatetimeConvert : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
			{
				return "";
			}
			var ts = value.ToString();
			if (ts.Length == 10)
			{
				ts += "0000000";
			}
			DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0);
			long lTime = long.Parse(ts);
			TimeSpan toNow = new TimeSpan(lTime);
			DateTime dt = dtStart.Add(toNow).ToLocalTime();
			var parse = "yyyy-MM-dd HH:mm:ss";
			if (parameter != null)
			{
				if (parameter.ToString() == "ts")
				{
					return GetTime(dt);
				}
				parse = parameter.ToString();
			}

			return dt.ToString(parse);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value;
		}

		private string GetTime(DateTime dt)
		{
			TimeSpan span = DateTime.Now - dt;
			if (span.TotalDays > 7)
			{
				return dt.ToString("yyyy-MM-dd");
			}
			else
			if (span.TotalDays > 1)
			{
				return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
			}
			else
			if (span.TotalHours > 1)
			{
				return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
			}
			else
			if (span.TotalMinutes > 1)
			{
				return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
			}
			else
			if (span.TotalSeconds >= 1)
			{
				return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
			}
			else
			{
				return "1秒前";
			}
		}
	}
}