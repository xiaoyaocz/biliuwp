using BiliBili.UWP.Api;
using BiliBili.UWP.Api.Season;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules.Season
{
	public enum IndexSeasonType
	{
		Anime = 1,
		Movie = 2,
		Documentary = 3,
		Guochuang = 4,
		TV = 5,
		Variety = 7
	}

	public class SeasonIndexConditionFilterItemModel
	{
		public string id { get; set; }
		public string name { get; set; }
	}

	public class SeasonIndexConditionFilterModel : IModules
	{
		private SeasonIndexConditionFilterItemModel _current;

		public SeasonIndexConditionFilterItemModel current
		{
			get { return _current; }
			set { _current = value; }
		}

		public string id { get; set; }
		public string name { get; set; }
		public List<SeasonIndexConditionFilterItemModel> value { get; set; }
	}

	public class SeasonIndexParameter
	{
		public string area { get; set; } = "-1";
		public string month { get; set; } = "-1";
		public string order { get; set; } = "3";
		public string style { get; set; } = "-1";
		public IndexSeasonType type { get; set; } = IndexSeasonType.Anime;
		public string year { get; set; } = "-1";
	}

	public class SeasonIndexResultItemModel
	{
		public string badge { get; set; }
		public int badge_type { get; set; }
		public string cover { get; set; }
		public string index_show { get; set; }
		public int is_finish { get; set; }
		public string link { get; set; }
		public int media_id { get; set; }
		public SeasonIndexResultItemOrderModel order { get; set; }
		public int season_id { get; set; }

		public bool show_badge
		{
			get
			{
				return !string.IsNullOrEmpty(badge);
			}
		}

		public string title { get; set; }
	}

	public class SeasonIndexResultItemOrderModel
	{
		public string bottom_text
		{
			get
			{
				if (type == "follow")
				{
					return follow;
				}
				else
				{
					return Utils.HandelTimestamp(renewal_time.ToString()) + "更新";
				}
			}
		}

		public string follow { get; set; }
		public string play { get; set; }
		public long pub_date { get; set; }
		public long pub_real_time { get; set; }
		public long renewal_time { get; set; }
		public string score { get; set; }

		public bool show_score
		{
			get
			{
				return type == "score";
			}
		}

		public string type { get; set; }
	}

	public class SeasonIndexVM : IModules
	{
		private readonly SeasonIndexAPI seasonIndexAPI;
		private ObservableCollection<SeasonIndexConditionFilterModel> _Conditions;

		private bool _Conditionsloading = true;

		private bool _loading = true;

		private int _page = 1;

		private ObservableCollection<SeasonIndexResultItemModel> _result;

		public SeasonIndexVM()
		{
			seasonIndexAPI = new SeasonIndexAPI();
			RefreshCommand = new RelayCommand(Refresh);
			LoadMoreCommand = new RelayCommand(LoadMore);
		}

		public ObservableCollection<SeasonIndexConditionFilterModel> Conditions
		{
			get { return _Conditions; }
			set { _Conditions = value; DoPropertyChanged("Conditions"); }
		}

		public bool ConditionsLoading
		{
			get { return _Conditionsloading; }
			set { _Conditionsloading = value; DoPropertyChanged("ConditionsLoading"); }
		}

		public bool Loading
		{
			get { return _loading; }
			set { _loading = value; DoPropertyChanged("Loading"); }
		}

		public ICommand LoadMoreCommand { get; private set; }

		public int Page
		{
			get { return _page; }
			set { _page = value; }
		}

		public SeasonIndexParameter Parameter { get; set; }

		public ICommand RefreshCommand { get; private set; }

		public ObservableCollection<SeasonIndexResultItemModel> Result
		{
			get { return _result; }
			set { _result = value; DoPropertyChanged("Result"); }
		}

		public async Task LoadConditions()
		{
			try
			{
				ConditionsLoading = true;
				var results = await seasonIndexAPI.Condition((int)Parameter.type).Request();
				if (results.status)
				{
					var data = results.GetJObject();
					if (data["code"].ToInt32() == 0)
					{
						var items = JsonConvert.DeserializeObject<ObservableCollection<SeasonIndexConditionFilterModel>>(data["result"]["filter"].ToString());
						foreach (var item in items)
						{
							if (item.id == "style_id")
							{
								item.current = item.value.FirstOrDefault(x => x.id == Parameter.style);
							}
							else if (item.id == "area")
							{
								item.current = item.value.FirstOrDefault(x => x.id == Parameter.area);
							}
							else if (item.id == "pub_date")
							{
								item.current = item.value.FirstOrDefault(x => x.id == Parameter.year);
							}
							else if (item.id == "season_month")
							{
								item.current = item.value.FirstOrDefault(x => x.id == Parameter.month);
							}
							else
							{
								item.current = item.value.FirstOrDefault();
							}
						}
						var orders = new List<SeasonIndexConditionFilterItemModel>()
						{
							new SeasonIndexConditionFilterItemModel()
							{
								id="3",
								name="最多追"+((Parameter.type== IndexSeasonType.Anime)?"番":"剧")
							},
							new SeasonIndexConditionFilterItemModel()
							{
								id="0",
								name="最近更新"
							},
							new SeasonIndexConditionFilterItemModel()
							{
								id="4",
								name="最高评分"
							}
						};
						items.Insert(0, new SeasonIndexConditionFilterModel()
						{
							name = "排序",
							value = orders,
							id = "order",
							current = orders.FirstOrDefault(x => x.id == Parameter.order),
						});
						Conditions = items;
					}
					else
					{
						Utils.ShowMessageToast(data["message"].ToString());
					}
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError<SeasonIndexConditionFilterModel>(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				ConditionsLoading = false;
			}
		}

		public async void LoadMore()
		{
			if (Loading)
			{
				return;
			}
			if (Conditions == null || Conditions.Count == 0 || Result == null || Result.Count == 0)
			{
				return;
			}
			await LoadResult();
		}

		public async Task LoadResult()
		{
			try
			{
				Loading = true;
				if (Page == 1)
				{
					Result = null;
				}
				var con = "";
				foreach (var item in Conditions)
				{
					con += $"&{item.id}={Uri.EscapeDataString(item.current.id)}";
				}
				con += $"&sort=0";
				var results = await seasonIndexAPI.Result(Page, (int)Parameter.type, con).Request();
				if (results.status)
				{
					var data = results.GetJObject();
					if (data["code"].ToInt32() == 0)
					{
						var items = JsonConvert.DeserializeObject<ObservableCollection<SeasonIndexResultItemModel>>(data["result"]["data"].ToString());
						if (Page == 1)
						{
							Result = items;
						}
						else
						{
							foreach (var item in items)
							{
								Result.Add(item);
							}
						}
						Page++;
					}
					else
					{
						Utils.ShowMessageToast(data["message"].ToString());
					}
				}
			}
			catch (Exception ex)
			{
				var handel = HandelError<SeasonIndexConditionFilterModel>(ex);
				Utils.ShowMessageToast(handel.message);
			}
			finally
			{
				Loading = false;
			}
		}

		public async void Refresh()
		{
			if (Loading)
			{
				return;
			}
			Page = 1;
			if (Conditions == null)
			{
				await LoadConditions();
			}
			if (Conditions != null)
			{
				await LoadResult();
			}
		}
	}
}