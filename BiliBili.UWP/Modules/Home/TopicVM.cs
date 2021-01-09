using BiliBili.UWP.Api;
using BiliBili.UWP.Api.Home;
using BiliBili.UWP.Modules.Home.HomeTopicModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules.Home
{
	namespace HomeTopicModels
	{
		public class TabBannerItem
		{
			/// <summary>
			/// http://i0.hdslb.com/bfs/archive/8ce99a09b5639ac5ff74b5736536fabf8cfd5a54.jpg
			/// </summary>
			private string _image;

			public int cm_mark { get; set; }

			public string hash { get; set; }

			/// <summary>
			/// Id
			/// </summary>
			public int id { get; set; }

			public string image
			{
				get { return _image; }
				set { _image = value; }
			}

			public int server_type { get; set; }

			/// <summary>
			/// 骨头骨头
			/// </summary>
			public string title { get; set; }

			public string uri { get; set; }
		}

		public class TabDataModel : IModules
		{
			public List<TabItemModel> item { get; set; }

			//private ObservableCollection<TabItemModel> _item;

			//public ObservableCollection<TabItemModel> item
			//{
			//    get { return _item; }
			//    set { _item = value; DoPropertyChanged("item"); }
			//}
		}

		public class TabItemModel
		{
			/// <summary>
			/// http://i0.hdslb.com/bfs/archive/855bfc0a07e498cb16a520c62f86048a236cbbc8.png
			/// </summary>
			private string _cover;

			/// <summary>
			/// </summary>
			public string @goto { get; set; }

			/// <summary>
			/// 视频征集
			/// </summary>
			public string badge { get; set; }

			public ObservableCollection<TabBannerItem> banner_item { get; set; }

			public string content { get; set; }

			public string cover
			{
				get { return _cover + "@300h.jpg"; }
				set { _cover = value; }
			}

			/// <summary>
			/// 最强烧烤在哪里？
			/// </summary>
			public string desc { get; set; }

			/// <summary>
			/// Hide_badge
			/// </summary>
			public bool hide_badge { get; set; }

			public ObservableCollection<TabVideoItemModel> item { get; set; }

			/// <summary>
			/// 1469
			/// </summary>
			public string param { get; set; }

			/// <summary>
			/// Ratio
			/// </summary>
			public int ratio { get; set; }

			public bool showMore
			{
				get
				{
					return @goto == "tag_rcmd";
				}
			}

			/// <summary>
			/// 独乐串不如众乐串~
			/// </summary>
			public string title { get; set; }

			/// <summary>
			/// https://www.bilibili.com/blackboard/activity-Byw_ZqWbZm.html
			/// </summary>
			public string uri { get; set; }
		}

		public class TabVideoItemModel
		{
			/// <summary>
			/// </summary>
			public string @goto { get; set; }

			/// <summary>
			/// </summary>
			public int cid { get; set; }

			/// <summary>
			/// </summary>
			public int coin { get; set; }

			/// <summary>
			/// </summary>
			public string cover { get; set; }

			/// <summary>
			/// </summary>
			public int ctime { get; set; }

			/// <summary>
			/// </summary>
			public int danmaku { get; set; }

			/// <summary>
			/// 104951弹幕
			/// </summary>
			public string desc { get; set; }

			/// <summary>
			/// </summary>
			public int dislike { get; set; }

			/// <summary>
			/// </summary>
			public int duration { get; set; }

			/// <summary>
			/// </summary>
			public string face { get; set; }

			/// <summary>
			/// </summary>
			public int favorite { get; set; }

			/// <summary>
			/// </summary>
			public int like { get; set; }

			/// <summary>
			/// </summary>
			public int mid { get; set; }

			/// <summary>
			/// bilibili纪录片
			/// </summary>
			public string name { get; set; }

			/// <summary>
			/// </summary>
			public string param { get; set; }

			/// <summary>
			/// </summary>
			public int play { get; set; }

			/// <summary>
			/// </summary>
			public int reply { get; set; }

			public string rightText
			{
				get
				{
					var ts = TimeSpan.FromSeconds(duration);
					return ts.ToString(@"mm\:ss");
				}
			}

			/// <summary>
			/// </summary>
			public int share { get; set; }

			public string text
			{
				get
				{
					return play.ToW() + "观看 " + danmaku.ToW() + "弹幕";
				}
			}

			/// <summary>
			/// </summary>
			public int tid { get; set; }

			/// <summary>
			/// 《人生一串》第六集之《朝圣之地》
			/// </summary>
			public string title { get; set; }

			/// <summary>
			/// 社会·美食·旅行
			/// </summary>
			public string tname { get; set; }

			/// <summary>
			/// </summary>
			public string uri { get; set; }
		}
	}

	public class TopicVM : IModules
	{
		private readonly HomeAPI homeAPI;
		private TabDataModel _detail;
		private bool _loading = false;

		public TopicVM()
		{
			homeAPI = new HomeAPI();
			RefreshCommand = new RelayCommand(Refresh);
		}

		public TabDataModel Detail
		{
			get { return _detail; }
			set { _detail = value; DoPropertyChanged("Detail"); }
		}

		public bool Loading
		{
			get { return _loading; }
			set { _loading = value; DoPropertyChanged("Loading"); }
		}

		public ICommand RefreshCommand { get; private set; }
		public int tab_id { get; set; }

		public async Task GetTabData()
		{
			try
			{
				var result = await homeAPI.TabDetail(tab_id).Request();
				if (result.status)
				{
					var model = await result.GetData<TabDataModel>();
					if (model.code == 0)
					{
						var banner = model.data.item.FirstOrDefault(x => x.@goto == "banner");
						if (banner != null)
						{
							var list = banner.banner_item.Where(x => string.IsNullOrEmpty(x.image)).ToList();
							foreach (var item in list)
							{
								banner.banner_item.Remove(item);
							}
							if (banner.banner_item.Count == 0)
							{
								model.data.item.Remove(banner);
							}
						}
						Detail = model.data;
					}
					else
					{
						Utils.ShowMessageToast(model.message);
					}
				}
				else
				{
					Utils.ShowMessageToast(result.message);
				}
			}
			catch (Exception ex)
			{
				var result = HandelError(ex);
				Utils.ShowMessageToast(ex.Message);
			}
		}

		public async void Refresh()
		{
			if (Loading)
			{
				return;
			}

			await GetTabData();
		}
	}
}