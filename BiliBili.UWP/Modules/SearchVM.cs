using BiliBili.UWP.Api;
using BiliBili.UWP.Modules.SearchModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules
{
	namespace SearchModels
	{
		public class SearchAnimeItem
		{
			private string _pic;
			private string _title;
			public string angle_title { get; set; }
			public string areas { get; set; }

			public string cover
			{
				get { return _pic; }
				set { _pic = "https:" + value; }
			}

			public string cv { get; set; }
			public string desc { get; set; }
			public string media_id { get; set; }
			public long pubtime { get; set; }
			public string season_id { get; set; }
			public string season_type_name { get; set; }

			public bool showBadge
			{
				get
				{
					return !string.IsNullOrEmpty(angle_title);
				}
			}

			public string styles { get; set; }

			public string title
			{
				get { return _title; }
				set
				{
					_title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
				}
			}

			public string type { get; set; }
		}

		public class SearchArticleItem
		{
			private string _title;
			public string category_name { get; set; }

			public string cover
			{
				get
				{
					if (image_urls != null && image_urls.Count != 0)
					{
						return "https:" + image_urls[0];
					}
					return null;
				}
			}

			public string desc { get; set; }
			public string id { get; set; }
			public List<string> image_urls { get; set; }
			public int like { get; set; }
			public string mid { get; set; }
			public int reply { get; set; }

			public string title
			{
				get { return _title; }
				set
				{
					_title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
				}
			}

			public string type { get; set; }
			public int view { get; set; }
		}

		public class SearchFilterItem
		{
			public SearchFilterItem(string name, string value)
			{
				this.name = name;
				this.value = value;
			}

			public string name { get; set; }
			public string value { get; set; }
		}

		public class SearchLiveRoomItem
		{
			private string _cover;
			private string _title;
			private string _uface;
			private string _user_cover;
			public string cate_name { get; set; }

			public string cover
			{
				get { return _cover; }
				set { _cover = "https:" + value; }
			}

			public int online { get; set; }
			public string roomid { get; set; }
			public string tags { get; set; }

			public string title
			{
				get { return _title; }
				set
				{
					_title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
				}
			}

			public string uface
			{
				get { return _uface; }
				set { _uface = "https:" + value; }
			}

			public string uname { get; set; }

			public string user_cover
			{
				get { return _user_cover; }
				set { _user_cover = "https:" + value; }
			}
		}

		public class SearchTopicItem
		{
			private string _description;
			private string _pic;
			private string _title;
			public string arcurl { get; set; }

			public string cover
			{
				get { return _pic; }
				set { _pic = "https:" + value; }
			}

			public string description
			{
				get { return _description; }
				set
				{
					_description = value.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
				}
			}

			public long pubdate { get; set; }

			public string title
			{
				get { return _title; }
				set
				{
					_title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
				}
			}
		}

		public class SearchUserItem
		{
			private string _pic;
			public int fans { get; set; }
			public int is_upuser { get; set; }
			public int level { get; set; }

			public string lv
			{
				get
				{
					return $"ms-appx:///Assets/Icon/lv{level}.png";
				}
			}

			public string mid { get; set; }
			public SearchUserOfficialVerifyItem official_verify { get; set; }

			public string sign
			{
				get
				{
					if (official_verify != null && !string.IsNullOrEmpty(official_verify.desc))
					{
						return official_verify.desc;
					}
					return usign;
				}
			}

			public string uname { get; set; }

			public string upic
			{
				get { return _pic; }
				set { _pic = "https:" + value; }
			}

			public string usign { get; set; }

			public string Verify
			{
				get
				{
					if (official_verify == null)
					{
						return "";
					}
					switch (official_verify.type)
					{
						case 0:
							return "ms-appx:///Assets/Icon/verify0.png";

						case 1:
							return "ms-appx:///Assets/Icon/verify1.png";

						default:
							return "ms-appx:///Assets/MiniIcon/transparent.png";
					}
				}
			}

			public int videos { get; set; }
		}

		public class SearchUserOfficialVerifyItem
		{
			public string desc { get; set; }
			public int type { get; set; }
		}

		public class SearchVideoItem
		{
			private string _pic;
			private string _title;
			public string aid { get; set; }
			public string author { get; set; }
			public string duration { get; set; }
			public int favorites { get; set; }
			public string id { get; set; }

			public string pic
			{
				get { return _pic; }
				set { _pic = "https:" + value; }
			}

			public int play { get; set; }
			public int review { get; set; }
			public string tag { get; set; }

			public string title
			{
				get { return _title; }
				set
				{
					_title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
				}
			}

			public string type { get; set; }
			public string typename { get; set; }
			public int video_review { get; set; }
		}
	}

	public enum SearchType
	{
		/// <summary>
		/// 视频
		/// </summary>
		Video = 0,

		/// <summary>
		/// 番剧
		/// </summary>
		Anime = 1,

		/// <summary>
		/// 直播
		/// </summary>
		Live = 2,

		/// <summary>
		/// 主播
		/// </summary>
		Anchor = 3,

		/// <summary>
		/// 用户
		/// </summary>
		User = 4,

		/// <summary>
		/// 影视
		/// </summary>
		Movie = 5,

		/// <summary>
		/// 专栏
		/// </summary>
		Article = 6,

		/// <summary>
		/// 话题
		/// </summary>
		Topic = 7
	}

	public class ISearchVM : IModules
	{
		public Api.SearchAPI searchAPI;
		private string _keyword;
		private bool _loading = false;
		private bool _Nothing = false;
		private bool _ShowLoadMore = false;

		public ISearchVM()
		{
			searchAPI = new Api.SearchAPI();
			RefreshCommand = new RelayCommand(Refresh);
			LoadMoreCommand = new RelayCommand(LoadMore);
		}

		public bool HasData { get; set; } = false;

		public string Keyword
		{
			get { return _keyword; }
			set { _keyword = value; }
		}

		public bool Loading
		{
			get { return _loading; }
			set { _loading = value; DoPropertyChanged("Loading"); }
		}

		public ICommand LoadMoreCommand { get; private set; }

		public bool Nothing
		{
			get { return _Nothing; }
			set { _Nothing = value; DoPropertyChanged("Nothing"); }
		}

		public int Page { get; set; } = 1;
		public ICommand RefreshCommand { get; private set; }
		public SearchType SearchType { get; set; }

		public bool ShowLoadMore
		{
			get { return _ShowLoadMore; }
			set { _ShowLoadMore = value; DoPropertyChanged("ShowLoadMore"); }
		}

		public string Title { get; set; }

		public async virtual Task LoadData()
		{
		}

		public async virtual void LoadMore()
		{
			await LoadData();
		}

		public async virtual void Refresh()
		{
			HasData = false;
			Page = 1;
			await LoadData();
		}
	}

	public class SearchAnimeVM : ISearchVM
	{
		private ObservableCollection<SearchAnimeItem> _Animes;

		public SearchAnimeVM()
		{
		}

		public ObservableCollection<SearchAnimeItem> Animes
		{
			get { return _Animes; }
			set { _Animes = value; DoPropertyChanged("Animes"); }
		}

		public async override Task LoadData()
		{
			try
			{
				ShowLoadMore = false;
				Loading = true;
				Nothing = false;
				var api = searchAPI.WebSearchAnime(Keyword, Page);
				if (this.SearchType == SearchType.Movie)
				{
					api = searchAPI.WebSearchMovie(Keyword, Page);
				}
				var results = await api.Request();
				if (results.status)
				{
					var data = await results.GetJson<ApiDataModel<JObject>>();
					if (data.success)
					{
						var result = JsonConvert.DeserializeObject<ObservableCollection<SearchAnimeItem>>(data.data["result"]?.ToString() ?? "[]");
						if (Page == 1)
						{
							if (result == null || result.Count == 0)
							{
								Nothing = true;
								Animes?.Clear();
								return;
							}
							Animes = result;
						}
						else
						{
							if (data.data != null)
							{
								foreach (var item in result)
								{
									Animes.Add(item);
								}
							}
						}
						if (Page < data.data["numPages"].ToInt32())
						{
							ShowLoadMore = true;
							Page++;
						}
						HasData = true;
					}
					else
					{
						Utils.ShowMessageToast(data.message);
					}
				}
				else
				{
					Utils.ShowMessageToast(results.message);
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}
	}

	public class SearchArticleVM : ISearchVM
	{
		private ObservableCollection<SearchArticleItem> _Articles;

		private SearchFilterItem _SelectOrder;

		private SearchFilterItem _SelectRegion;

		public SearchArticleVM()
		{
			OrderFilters = new List<SearchFilterItem>() {
				new SearchFilterItem("默认排序","totalrank"),
				new SearchFilterItem("最多阅读","click"),
				new SearchFilterItem("最新发布","pubdate"),
				new SearchFilterItem("最多喜欢","attention"),
				new SearchFilterItem("最多评论","scores")
			};
			SelectOrder = OrderFilters[0];

			RegionFilters = new List<SearchFilterItem>() {
				new SearchFilterItem("全部分区","0"),
				new SearchFilterItem("动画","2"),
				new SearchFilterItem("游戏","1"),
				new SearchFilterItem("影视","28"),
				new SearchFilterItem("生活","3"),
				new SearchFilterItem("兴趣","29"),
				new SearchFilterItem("轻小说","16"),
				new SearchFilterItem("科技","17"),
			};

			SelectRegion = RegionFilters[0];
		}

		public ObservableCollection<SearchArticleItem> Articles
		{
			get { return _Articles; }
			set { _Articles = value; DoPropertyChanged("Articles"); }
		}

		public List<SearchFilterItem> DurationFilters { get; set; }
		public List<SearchFilterItem> OrderFilters { get; set; }
		public List<SearchFilterItem> RegionFilters { get; set; }

		public SearchFilterItem SelectOrder
		{
			get { return _SelectOrder; }
			set { _SelectOrder = value; }
		}

		public SearchFilterItem SelectRegion
		{
			get { return _SelectRegion; }
			set { _SelectRegion = value; }
		}

		public async override Task LoadData()
		{
			try
			{
				ShowLoadMore = false;
				Loading = true;
				Nothing = false;
				var results = await searchAPI.WebSearchArticle(Keyword, Page, SelectOrder.value, SelectRegion.value).Request();
				if (results.status)
				{
					var data = await results.GetJson<ApiDataModel<JObject>>();
					if (data.success)
					{
						var result = JsonConvert.DeserializeObject<ObservableCollection<SearchArticleItem>>(data.data["result"]?.ToString() ?? "[]");
						if (Page == 1)
						{
							if (result == null || result.Count == 0)
							{
								Nothing = true;
								Articles?.Clear();
								return;
							}
							Articles = result;
						}
						else
						{
							if (data.data != null)
							{
								foreach (var item in result)
								{
									Articles.Add(item);
								}
							}
						}
						if (Page < data.data["numPages"].ToInt32())
						{
							ShowLoadMore = true;
							Page++;
						}
						HasData = true;
					}
					else
					{
						Utils.ShowMessageToast(data.message);
					}
				}
				else
				{
					Utils.ShowMessageToast(results.message);
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}
	}

	public class SearchLiveRoomVM : ISearchVM
	{
		private ObservableCollection<SearchLiveRoomItem> _Rooms;

		public SearchLiveRoomVM()
		{
		}

		public ObservableCollection<SearchLiveRoomItem> Rooms
		{
			get { return _Rooms; }
			set { _Rooms = value; DoPropertyChanged("Rooms"); }
		}

		public async override Task LoadData()
		{
			try
			{
				ShowLoadMore = false;
				Loading = true;
				Nothing = false;

				var results = await searchAPI.WebSearchLive(Keyword, Page).Request();
				if (results.status)
				{
					var data = await results.GetJson<ApiDataModel<JObject>>();
					if (data.success)
					{
						var result = JsonConvert.DeserializeObject<ObservableCollection<SearchLiveRoomItem>>(data.data["result"]["live_room"]?.ToString() ?? "[]");
						if (Page == 1)
						{
							if (result == null || result.Count == 0)
							{
								Nothing = true;
								Rooms?.Clear();
								return;
							}
							Rooms = result;
						}
						else
						{
							if (data.data != null)
							{
								foreach (var item in result)
								{
									Rooms.Add(item);
								}
							}
						}
						if (Page < data.data["pageinfo"]["live_room"]["numPages"].ToInt32())
						{
							ShowLoadMore = true;
							Page++;
						}
						HasData = true;
					}
					else
					{
						Utils.ShowMessageToast(data.message);
					}
				}
				else
				{
					Utils.ShowMessageToast(results.message);
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}
	}

	public class SearchTopicVM : ISearchVM
	{
		private ObservableCollection<SearchTopicItem> _Topics;

		public SearchTopicVM()
		{
		}

		public ObservableCollection<SearchTopicItem> Topics
		{
			get { return _Topics; }
			set { _Topics = value; DoPropertyChanged("Topics"); }
		}

		public async override Task LoadData()
		{
			try
			{
				ShowLoadMore = false;
				Loading = true;
				Nothing = false;
				var results = await searchAPI.WebSearchTopic(Keyword, Page).Request();
				if (results.status)
				{
					var data = await results.GetJson<ApiDataModel<JObject>>();
					if (data.success)
					{
						var result = JsonConvert.DeserializeObject<ObservableCollection<SearchTopicItem>>(data.data["result"]?.ToString() ?? "[]");
						if (Page == 1)
						{
							if (result == null || result.Count == 0)
							{
								Nothing = true;
								Topics?.Clear();
								return;
							}
							Topics = result;
						}
						else
						{
							if (data.data != null)
							{
								foreach (var item in result)
								{
									Topics.Add(item);
								}
							}
						}
						if (Page < data.data["numPages"].ToInt32())
						{
							ShowLoadMore = true;
							Page++;
						}
						HasData = true;
					}
					else
					{
						Utils.ShowMessageToast(data.message);
					}
				}
				else
				{
					Utils.ShowMessageToast(results.message);
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}
	}

	public class SearchUserVM : ISearchVM
	{
		private SearchFilterItem _SelectOrder;

		private SearchFilterItem _SelectType;

		private ObservableCollection<SearchUserItem> _users;

		public SearchUserVM()
		{
			OrderFilters = new List<SearchFilterItem>() {
				new SearchFilterItem("默认排序","&order=&order_sort="),
				new SearchFilterItem("粉丝数由高到低","&order=fans&order_sort=0"),
				new SearchFilterItem("粉丝数由低到高","&order=fans&order_sort=1"),
				new SearchFilterItem("LV等级由高到低","&order=level&order_sort=0"),
				new SearchFilterItem("LV等级由低到高","&order=level&order_sort=1"),
			};
			SelectOrder = OrderFilters[0];
			TypeFilters = new List<SearchFilterItem>() {
				new SearchFilterItem("全部用户","&user_type=0"),
				new SearchFilterItem("UP主","&user_type=1"),
				new SearchFilterItem("普通用户","&user_type=2"),
				new SearchFilterItem("认证用户","&user_type=3")
			};
			SelectType = TypeFilters[0];
		}

		public List<SearchFilterItem> OrderFilters { get; set; }

		public SearchFilterItem SelectOrder
		{
			get { return _SelectOrder; }
			set { _SelectOrder = value; }
		}

		public SearchFilterItem SelectType
		{
			get { return _SelectType; }
			set { _SelectType = value; }
		}

		public List<SearchFilterItem> TypeFilters { get; set; }

		public ObservableCollection<SearchUserItem> Users
		{
			get { return _users; }
			set { _users = value; DoPropertyChanged("Users"); }
		}

		public async override Task LoadData()
		{
			try
			{
				ShowLoadMore = false;
				Loading = true;
				Nothing = false;
				var results = await searchAPI.WebSearchUser(Keyword, Page, SelectOrder.value, SelectType.value).Request();
				if (results.status)
				{
					var data = await results.GetJson<ApiDataModel<JObject>>();
					if (data.success)
					{
						var result = JsonConvert.DeserializeObject<ObservableCollection<SearchUserItem>>(data.data["result"]?.ToString() ?? "[]");
						if (Page == 1)
						{
							if (result == null || result.Count == 0)
							{
								Nothing = true;
								Users?.Clear();
								return;
							}
							Users = result;
						}
						else
						{
							if (data.data != null)
							{
								foreach (var item in result)
								{
									Users.Add(item);
								}
							}
						}
						if (Page < data.data["numPages"].ToInt32())
						{
							ShowLoadMore = true;
							Page++;
						}
						HasData = true;
					}
					else
					{
						Utils.ShowMessageToast(data.message);
					}
				}
				else
				{
					Utils.ShowMessageToast(results.message);
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}
	}

	public class SearchVideoVM : ISearchVM
	{
		private SearchFilterItem _SelectDuration;

		private SearchFilterItem _SelectOrder;

		private SearchFilterItem _SelectRegion;

		private ObservableCollection<SearchVideoItem> _videos;

		public SearchVideoVM()
		{
			OrderFilters = new List<SearchFilterItem>() {
				new SearchFilterItem("综合排序",""),
				new SearchFilterItem("最多点击","click"),
				new SearchFilterItem("最新发布","pubdate"),
				new SearchFilterItem("最多弹幕","dm"),
				new SearchFilterItem("最多收藏","stow")
			};
			SelectOrder = OrderFilters[0];
			DurationFilters = new List<SearchFilterItem>() {
				new SearchFilterItem("全部时长",""),
				new SearchFilterItem("10分钟以下","1"),
				new SearchFilterItem("10-30分钟","2"),
				new SearchFilterItem("30-60分钟","3"),
				new SearchFilterItem("60分钟以上","4")
			};
			SelectDuration = DurationFilters[0];
			RegionFilters = new List<SearchFilterItem>() {
				new SearchFilterItem("全部分区","0"),
			};
			foreach (var item in ApiHelper.regions.Where(x => x.children != null && x.children.Count != 0))
			{
				RegionFilters.Add(new SearchFilterItem(item.name, item.tid.ToString()));
			}
			SelectRegion = RegionFilters[0];
		}

		public List<SearchFilterItem> DurationFilters { get; set; }
		public List<SearchFilterItem> OrderFilters { get; set; }
		public List<SearchFilterItem> RegionFilters { get; set; }

		public SearchFilterItem SelectDuration
		{
			get { return _SelectDuration; }
			set { _SelectDuration = value; }
		}

		public SearchFilterItem SelectOrder
		{
			get { return _SelectOrder; }
			set { _SelectOrder = value; }
		}

		public SearchFilterItem SelectRegion
		{
			get { return _SelectRegion; }
			set { _SelectRegion = value; }
		}

		public ObservableCollection<SearchVideoItem> Videos
		{
			get { return _videos; }
			set { _videos = value; DoPropertyChanged("Videos"); }
		}

		public async override Task LoadData()
		{
			try
			{
				ShowLoadMore = false;
				Loading = true;
				Nothing = false;
				var results = await searchAPI.WebSearchVideo(Keyword, Page, SelectOrder.value, SelectDuration.value, SelectRegion.value).Request();
				if (results.status)
				{
					var data = await results.GetData<JObject>();
					if (data.success)
					{
						var result = JsonConvert.DeserializeObject<ObservableCollection<SearchVideoItem>>(data.data["result"]?.ToString() ?? "[]");
						if (Page == 1)
						{
							if (result == null || result.Count == 0)
							{
								Nothing = true;
								Videos?.Clear();
								return;
							}
							Videos = result;
						}
						else
						{
							if (data.data != null)
							{
								foreach (var item in result)
								{
									Videos.Add(item);
								}
							}
						}
						if (Page < data.data["numPages"].ToInt32())
						{
							ShowLoadMore = true;
							Page++;
						}
						HasData = true;
					}
					else
					{
						Utils.ShowMessageToast(data.message);
					}
				}
				else
				{
					Utils.ShowMessageToast(results.message);
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}
	}

	public class SearchVM : IModules
	{
		private ObservableCollection<ISearchVM> _items;

		private ISearchVM _SelectItem;

		public SearchVM()
		{
			SearchItems = new ObservableCollection<ISearchVM>() {
				new SearchVideoVM()
				{
					Title="视频",
					SearchType= SearchType.Video
				},
				new SearchAnimeVM()
				{
					Title="番剧",
					SearchType= SearchType.Anime
				},
				new SearchLiveRoomVM()
				{
					Title="直播",
					SearchType= SearchType.Live
				},
                //new SearchLiveRoomVM()
                //{
                //    Title="主播",
                //    SearchType= SearchType.Anchor
                //},
                new SearchUserVM()
				{
					Title="用户",
					SearchType= SearchType.User
				},
				new SearchAnimeVM()
				{
					Title="影视",
					SearchType= SearchType.Movie
				},
				new SearchArticleVM()
				{
					Title="专栏",
					SearchType= SearchType.Article
				},
				new SearchTopicVM()
				{
					Title="话题",
					SearchType= SearchType.Topic
				}
			};
			SelectItem = SearchItems[0];
		}

		public ObservableCollection<ISearchVM> SearchItems
		{
			get { return _items; }
			set { _items = value; DoPropertyChanged("SearchItems"); }
		}

		public ISearchVM SelectItem
		{
			get { return _SelectItem; }
			set { _SelectItem = value; }
		}
	}
}