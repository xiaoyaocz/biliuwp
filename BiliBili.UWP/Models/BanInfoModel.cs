using BiliBili.UWP.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BiliBili.UWP.Models
{
	public class actorModel
	{
		public string actor { get; set; }
		public string role { get; set; }
	}

	//番剧信息
	public class BangumiInfoModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public List<actorModel> actor { get; set; }
		public string alias { get; set; }

		//别名
		public string area { get; set; }

		public int attention { get; set; }
		public string bangumi_id { get; set; }

		//地区
		public string bangumi_title { get; set; }

		public int code { get; set; }
		public int coins { get; set; }
		public string cover { get; set; }

		public string cv
		{
			get
			{
				string a = "";
				if (actor != null && actor.Count != 0)
				{
					actor.ForEach(x => a += x.role + " : " + x.actor + "\r\n");
				}
				return a;
			}
		}

		public int danmaku_count { get; set; }
		public List<episodesModel> episodes { get; set; }
		public string evaluate { get; set; }

		//介绍
		public int favorites { get; set; }

		public string favoritesCount
		{
			get
			{
				if (favorites > 10000)
				{
					return ((double)favorites / 10000).ToString("0.0" + "万");
				}
				else
				{
					return favorites.ToString();
				}
			}
		}

		public int is_finish { get; set; }

		//是否关注
		public string last_ep_index { get; set; }

		public object list { get; set; }
		public int media_id { get; set; }
		public string message { get; set; }
		public newest_epModel newest_ep { get; set; }

		//是否完结
		public string newest_ep_index { get; set; }

		public int Num { get; set; }

		//订阅
		//硬币
		public int play_count { get; set; }

		public string PlayCount
		{
			get
			{
				if (play_count > 10000)
				{
					return ((double)play_count / 10000).ToString("0.0" + "万");
				}
				else
				{
					return play_count.ToString();
				}
			}
		}

		public DateTime? pub_time
		{
			get;
			set;
		}

		public object rank { get; set; }
		public BangumiInfoModel result { get; set; }

		public string role { get; set; }

		public string season_id { get; set; }

		//番剧名。与season_title不同
		public string season_title { get; set; }//专题名

		public List<BangumiInfoModel> seasons { get; set; }

		//最新话
		public string staff { get; set; }

		public string status
		{
			get
			{
				if (is_finish == 1)
				{
					return string.Format("已完结，共{0}话", total_count);
				}
				else
				{
					return string.Format("连载中，更新至{0}话", newest_ep_index) ?? "";
				}
			}
		}

		public List<tagsModel> tags { get; set; }
		public string title { get; set; }

		//public string title { get; set; }
		public int total_bp_count { get; set; }

		public int total_count { get; set; }
		public int type { get; set; } = 1;

		public string upTime
		{
			get
			{
				if (pub_time.HasValue)
				{
					return pub_time.Value.Date.ToString("d");
				}
				else
				{
					return "";
				}
			}
		}

		public BangumiInfoModel user_season { get; set; }
		public int week_bp_count { get; set; }

		private void RaisePropertyChanged(string Name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(Name));
			}
		}
	}

	public class CBRankModel
	{
		public int code { get; set; }
		public string face { get; set; }
		public string hidden { get; set; }
		public object list { get; set; }
		public string message { get; set; }

		//0 false,1 true
		public string rank { get; set; }

		public object result { get; set; }
		public string uid { get; set; }
		public string uname { get; set; }
	}

	public class episodesModel
	{
		private string _av_id;
		private int _danmaku;

		//public string episode_id { get; set; }
		private string _episode_id;

		public int aid { get; set; }

		public string av_id
		{
			get
			{
				if (aid != 0)
				{
					return aid.ToString();
				}
				else
				{
					return _av_id;
				}
			}
			set
			{
				_av_id = value;
			}
		}

		public string badge { get; set; }
		public int cid { get; set; }

		public int danmaku
		{
			get
			{
				if (cid != 0)
				{
					return cid;
				}
				else
				{
					return _danmaku;
				}
			}
			set
			{
				_danmaku = value;
			}
		}

		public int ep_id { get; set; }

		public string episode_id
		{
			get
			{
				if (ep_id != 0)
				{
					return ep_id.ToString();
				}
				else
				{
					return _episode_id;
				}
			}
			set
			{
				_episode_id = value;
			}
		}

		public int episode_status { get; set; }

		public SolidColorBrush f
		{
			get
			{
				var record = SqlHelper.GetVideoWatchRecord(danmaku.ToString());
				if (record != null)
				{
					return new SolidColorBrush(Colors.Gray);
				}
				else
				{
					return new SolidColorBrush(Colors.White);
				}
			}
		}

		public int id { get; set; }
		public string index { get; set; }
		public string index_title { get; set; }
		public bool inLocal { get; set; }

		public Visibility IsBadge
		{
			get
			{
				if (badge != null && badge.Length != 0)
				{
					return Visibility.Visible;
				}
				else
				{
					return Visibility.Collapsed;
				}
			}
		}

		public Visibility IsDowned
		{
			get
			{
				if (DownloadHelper2.downloadeds.ContainsKey(danmaku.ToString()))
				{
					return Visibility.Visible;
				}
				else
				{
					return Visibility.Collapsed;
				}
			}
		}

		public string long_title { get; set; }
		public int orderindex { get; set; }
		public int page { get; set; }
		public int season_type { get; set; } = 1;
		public int section_type { get; set; }
		public string title { get; set; }
	}

	public class newest_epModel
	{
		public string desc { get; set; }
		public int id { get; set; }
		public string index { get; set; }
	}

	public class OrderModel
	{
		public string cashier_url { get; set; }
		public int code { get; set; }
		public string data { get; set; }
		public string order_id { get; set; }
		public string pay_pay_order_no { get; set; }
		public string qrcode { get; set; }
		public object result { get; set; }
	}

	public class tagsModel
	{
		public string index { get; set; }
		public int tag_id { get; set; }
		public string tag_name { get; set; }
	}

	public class TokenModel
	{
		public int code { get; set; }
		public string message { get; set; }
		public object result { get; set; }
		public string token { get; set; }
	}
}