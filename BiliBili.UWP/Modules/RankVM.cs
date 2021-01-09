using BiliBili.UWP.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiliBili.UWP.Modules
{
	public class RankFilterItem
	{
		public int id { get; set; }
		public string name { get; set; }
	}

	public class RankItemModel
	{
		public string aid { get; set; }
		public string author { get; set; }
		public int coins { get; set; }
		public string duration { get; set; }
		public string mid { get; set; }
		public string pic { get; set; }
		public int play { get; set; }
		public int pts { get; set; }
		public int rank { get; set; }
		public string title { get; set; }
		public int video_review { get; set; }
	}

	public class RankRegionModel : IModules
	{
		private List<RankItemModel> _Items;

		public List<RankItemModel> Items
		{
			get { return _Items; }
			set { _Items = value; DoPropertyChanged("Items"); }
		}

		public string name { get; set; }
		public int rid { get; set; }
	}

	public class RankVM : IModules
	{
		private readonly Api.RankAPI rankAPI;
		private RankRegionModel _current;

		private bool _loading = true;

		private List<RankRegionModel> _RegionItems;

		private RankFilterItem _SelectDayFilter;

		private RankFilterItem _SelectTypeFilter;

		public RankVM()
		{
			rankAPI = new Api.RankAPI();
			TypeFilter = new List<RankFilterItem>()
			{
				new RankFilterItem()
				{
					id=1,
					name="全站"
				},
				new RankFilterItem()
				{
					id=2,
					name="原创"
				}
			};
			SelectTypeFilter = TypeFilter[0];

			DayFilter = new List<RankFilterItem>() {
				new RankFilterItem()
				{
					id=1,
					name="日排行"
				},
				new RankFilterItem()
				{
					id=3,
					name="3日排行"
				},
				new RankFilterItem()
				{
					id=7,
					name="周排行"
				},
				new RankFilterItem()
				{
					id=30,
					name="月排行"
				}
			};
			SelectDayFilter = DayFilter[0];

			List<RankRegionModel> regions = new List<RankRegionModel>() {
				new RankRegionModel()
				{
					name="全站",
					rid=0,
				},
				new RankRegionModel()
				{
					name="国创相关",
					rid=168,
				}
			};
			foreach (var item in ApiHelper.regions.Where(x => x.children != null && x.children.Count != 0 && x.tid != 13 && x.tid != 167 && x.name != "广告"))
			{
				regions.Add(new RankRegionModel()
				{
					name = item.name,
					rid = item.tid
				});
			}
			RegionItems = regions;
		}

		public RankRegionModel Current
		{
			get { return _current; }
			set { _current = value; DoPropertyChanged("Current"); }
		}

		public List<RankFilterItem> DayFilter { get; set; }

		public bool Loading
		{
			get { return _loading; }
			set { _loading = value; DoPropertyChanged("Loading"); }
		}

		public List<RankRegionModel> RegionItems
		{
			get { return _RegionItems; }
			set { _RegionItems = value; DoPropertyChanged("RegionItems"); }
		}

		public RankFilterItem SelectDayFilter
		{
			get { return _SelectDayFilter; }
			set { _SelectDayFilter = value; }
		}

		public RankFilterItem SelectTypeFilter
		{
			get { return _SelectTypeFilter; }
			set { _SelectTypeFilter = value; }
		}

		public List<RankFilterItem> TypeFilter { get; set; }

		public async Task LoadRankDetail(RankRegionModel region)
		{
			try
			{
				Loading = true;
				var results = await rankAPI.Rank(region.rid, SelectTypeFilter.id, SelectDayFilter.id).Request();
				if (results.status)
				{
					var data = await results.GetJson<ApiDataModel<JObject>>();
					if (data.success)
					{
						var result = JsonConvert.DeserializeObject<List<RankItemModel>>(data.data["list"].ToString());
						int i = 1;
						result = result.Take(36).ToList();
						foreach (var item in result)
						{
							item.rank = i;
							i++;
						}
						region.Items = result;
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
				var handel = HandelError<ApiDataModel<List<RankRegionModel>>>(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}
	}
}