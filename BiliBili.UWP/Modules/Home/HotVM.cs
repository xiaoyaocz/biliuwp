﻿using BiliBili.UWP.Api;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules.Home.HotModels;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules.Home
{
	namespace HotModels
	{
		public class HotDataItemModel
		{
			public string card_goto { get; set; }
			public string card_type { get; set; }
			public string cover { get; set; }
			public string cover_right_text_1 { get; set; }
			public string idx { get; set; }
			public string param { get; set; }
			public RecommendModels.RecommendRcmdReasonStyleModel rcmd_reason_style { get; set; }
			public string right_desc_1 { get; set; }
			public string right_desc_2 { get; set; }
			public string title { get; set; }
			public string uri { get; set; }
		}

		public class HotTopItemModel
		{
			public int entrance_id { get; set; }
			public string icon { get; set; }
			public string module_id { get; set; }
			public string title { get; set; }
			public string uri { get; set; }
		}
	}

	public class HotItemSource : IIncrementalSource<HotDataItemModel>
	{
		private readonly Api.Home.HotAPI hotAPI;
		private List<HotDataItemModel> hot_items;

		private string last_idx = "0";

		private string last_param = "0";

		public HotItemSource(List<HotDataItemModel> items)
		{
			hotAPI = new Api.Home.HotAPI();
			last_idx = items.LastOrDefault().idx ?? "0";
			last_param = items.LastOrDefault()?.param ?? "";
			this.hot_items = items;
		}

		public async Task<List<HotDataItemModel>> GetHot()
		{
			try
			{
				var result = await hotAPI.Popular(last_idx, last_param).Request();
				if (result.status)
				{
					var data = result.GetJObject();
					if (data["code"].ToInt32() == 0)
					{
						var items = JsonConvert.DeserializeObject<List<HotDataItemModel>>(data["data"].ToString());
						for (int i = items.Count - 1; i >= 0; i--)
						{
							if (items[i].card_goto != "av")
								items.Remove(items[i]);
						}
						last_idx = items.LastOrDefault()?.idx ?? "0";
						last_param = items.LastOrDefault()?.param ?? "";
						return items;
					}
					else
					{
						Utils.ShowMessageToast(data["message"].ToString());
						return new List<HotDataItemModel>();
					}
				}
				else
				{
					Utils.ShowMessageToast(result.message);
					return new List<HotDataItemModel>();
				}
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("加载首页推荐信息失败", LogType.ERROR, ex);
				Utils.ShowMessageToast("加载首页推荐信息失败");
				return new List<HotDataItemModel>();
			}
		}

		public async Task<IEnumerable<HotDataItemModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
		{
			if (pageIndex == 0)
			{
				return hot_items;
			}
			return await GetHot();
		}
	}

	public class HotVM : IModules
	{
		private readonly Api.Home.HotAPI hotAPI;
		private IncrementalLoadingCollection<HotItemSource, HotDataItemModel> _items;

		private bool _loading = false;

		private List<HotTopItemModel> _topItems;

		public HotVM()
		{
			hotAPI = new Api.Home.HotAPI();
			RefreshCommand = new RelayCommand(Refresh);
			LoadMoreCommand = new RelayCommand(LoadMore);
		}

		public IncrementalLoadingCollection<HotItemSource, HotDataItemModel> Items
		{
			get { return _items; }
			set { _items = value; DoPropertyChanged("Items"); }
		}

		public bool Loading
		{
			get { return _loading; }
			set { _loading = value; DoPropertyChanged("Loading"); }
		}

		public ICommand LoadMoreCommand { get; private set; }
		public ICommand RefreshCommand { get; private set; }

		public List<HotTopItemModel> TopItems
		{
			get { return _topItems; }
			set { _topItems = value; DoPropertyChanged("TopItems"); }
		}

		public async Task GetPopular()
		{
			try
			{
				Loading = true;
				var result = await hotAPI.Popular("0", "").Request();
				if (result.status)
				{
					var data = result.GetJObject();
					if (data["code"].ToInt32() == 0)
					{
						if (TopItems == null)
						{
							TopItems = JsonConvert.DeserializeObject<List<HotTopItemModel>>(data["config"]["top_items"].ToString());
						}

						var items = JsonConvert.DeserializeObject<List<HotDataItemModel>>(data["data"].ToString());
						for (int i = items.Count - 1; i >= 0; i--)
						{
							if (items[i].card_goto != "av")
								items.Remove(items[i]);
						}
						Items = new IncrementalLoadingCollection<HotItemSource, HotDataItemModel>(new HotItemSource(items));
					}
					else
					{
						Utils.ShowMessageToast(data["message"].ToString());
					}
				}
				else
				{
					Utils.ShowMessageToast(result.message);
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

		public async void LoadMore()
		{
			if (Loading)
			{
				return;
			}
			await Items.LoadMoreItemsAsync(20);
		}

		public async void Refresh()
		{
			if (Loading)
			{
				return;
			}
			TopItems = null;
			await GetPopular();
		}
	}
}