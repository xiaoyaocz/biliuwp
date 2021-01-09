using BiliBili.UWP.Api;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules.Live
{
	public class LiveWatchHistoryItemModel
	{
		public string cover { get; set; }

		public bool live_ing
		{
			get
			{
				return live_status == 1;
			}
		}

		public int live_status { get; set; }
		public string name { get; set; }
		public string tag_name { get; set; }
		public string title { get; set; }
		public string uri { get; set; }
		public long view_at { get; set; }
	}

	public class LiveWatchHistoryVM : IModules
	{
		private readonly Api.Live.LiveCenterAPI liveCenterAPI;
		private bool _canLoadMore = true;

		private bool _loading = true;

		public LiveWatchHistoryVM()
		{
			liveCenterAPI = new Api.Live.LiveCenterAPI();
			Historys = new ObservableCollection<LiveWatchHistoryItemModel>();
			RefreshCommand = new RelayCommand(Refresh);
			LoadMoreCommand = new RelayCommand(LoadMore);
		}

		public ObservableCollection<LiveWatchHistoryItemModel> Historys { get; set; }

		public bool Loading
		{
			get { return _loading; }
			set { _loading = value; DoPropertyChanged("Loading"); }
		}

		public ICommand LoadMoreCommand { get; private set; }
		public int Page { get; set; }
		public ICommand RefreshCommand { get; private set; }

		public bool ShowLoadMore
		{
			get { return _canLoadMore; }
			set { _canLoadMore = value; DoPropertyChanged("ShowLoadMore"); }
		}

		public async Task GetHistorys()
		{
			try
			{
				Loading = true;
				ShowLoadMore = false;
				var result = await liveCenterAPI.History(Page).Request();
				if (result.status)
				{
					var data = await result.GetData<ObservableCollection<LiveWatchHistoryItemModel>>();
					if (data.code == 0)
					{
						if (data.data != null && data.data.Count != 0)
						{
							if (Page == 1)
							{
								Historys = data.data;
							}
							else
							{
								foreach (var item in data.data)
								{
									Historys.Add(item);
								}
							}
							Page++;
							ShowLoadMore = true;
						}
					}
					else
					{
						Utils.ShowMessageToast(data.message);
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
			await GetHistorys();
		}

		public async void Refresh()
		{
			if (Loading)
			{
				return;
			}
			Page = 1;
			await GetHistorys();
		}
	}
}