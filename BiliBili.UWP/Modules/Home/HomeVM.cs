using BiliBili.UWP.Api;
using BiliBili.UWP.Api.Home;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BiliBili.UWP.Modules.Home
{
	public class HomeTabItem
	{
		public int id { get; set; }
		public IModules item { get; set; }
		public string name { get; set; }
		public string tab_id { get; set; }
	}

	public class HomeVM : IModules
	{
		private readonly HomeAPI homeAPI;
		private RecommendVM recommendVM;

		public HomeVM()
		{
			homeAPI = new HomeAPI();
			recommendVM = new RecommendVM();
			Tabs = new ObservableCollection<HomeTabItem>() {
				new HomeTabItem()
				{
					name="推荐",
					item=new RecommendVM()
				},
				new HomeTabItem()
				{
					name="热门",
					item=new HotVM()
				}
			};
		}

		public ObservableCollection<HomeTabItem> Tabs { get; set; }

		/// <summary>
		/// 读取首页TAB信息
		/// </summary>
		/// <returns></returns>
		public async Task GetTab()
		{
			try
			{
				var result = await homeAPI.Tab().Request();
				var model = await result.GetData<JObject>();
				if (model.code == 0)
				{
					var tabs = JsonConvert.DeserializeObject<ObservableCollection<HomeTabItem>>(model.data["tab"].ToString());
					foreach (var item in tabs.Where(x => x.tab_id.ToInt32() != 0))
					{
						item.item = new TopicVM();
						Tabs.Add(item);
					}
				}
				else
				{
					Utils.ShowMessageToast("读取主页TAB失败:" + model.message);
				}
			}
			catch (Exception ex)
			{
				var result = HandelError(ex);
				Utils.ShowMessageToast(result.message);
			}
		}
	}
}