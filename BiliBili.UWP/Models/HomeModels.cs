using System;
using System.Collections.Generic;

namespace BiliBili.UWP.Models
{
	//Banner
	public class BannerModel
	{
		public List<topModel> top { get; set; }
	}

	public class bodyModel
	{
		private string _cover;
		public string _goto { get; set; }
		public string area { get; set; }
		public int area_id { get; set; }

		public string cover
		{
			get
			{
				return _cover + "@300w.jpg"; ;
			}
			set { _cover = value; }
		}

		public string danmaku { get; set; }

		//BANGUMI
		public string desc1 { get; set; }

		public string height { get; set; }
		public string index { get; set; }
		public string mtime { get; set; }
		public string name { get; set; }
		public long online { get; set; }
		public string param { get; set; }
		public string play { get; set; }
		public int status { get; set; }
		public string style { get; set; }

		public string TimeView
		{
			get
			{
				DateTime dt = Convert.ToDateTime(mtime);
				switch (Convert.ToInt32((DateTime.Now - dt).TotalDays))
				{
					case 2:
						return string.Format("前天{0}:{1}", dt.Hour.ToString("00"), dt.Minute.ToString("00"));

					case 1:
						return string.Format("昨天{0}:{1}", dt.Hour.ToString("00"), dt.Minute.ToString("00"));

					case 0:
						return string.Format("今天{0}:{1}", dt.Hour.ToString("00"), dt.Minute.ToString("00"));

					default:
						return string.Format("未知时间");
				}
			}
		}

		public string title { get; set; }

		//LIVE
		public string up_face { get; set; }

		public string uri { get; set; }
		public string width { get; set; }
	}

	public class extModel
	{
		public int live_count { get; set; }
	}

	public class HomeModel
	{
		public BannerModel banner { get; set; }
		public List<bodyModel> body { get; set; }
		public int code { get; set; }
		public List<HomeModel> data { get; set; }
		public string message { get; set; }
		public string param { get; set; }
		public string style { get; set; }
		public string title { get; set; }
		public string type { get; set; }
	}

	public class HomeRefreshModel
	{
		public int code { get; set; }
		public List<bodyModel> data { get; set; }
		public string message { get; set; }
	}

	public class topModel
	{
		public string hash { get; set; }
		public int id { get; set; }
		public string image { get; set; }
		public bool is_ad { get; set; }
		public string title { get; set; }
		public string uri { get; set; }
	}
}