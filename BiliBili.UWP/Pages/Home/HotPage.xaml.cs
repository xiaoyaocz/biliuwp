using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.Home;
using BiliBili.UWP.Modules.Home.HotModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.Home
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class HotPage : Page
	{
		private readonly HotVM hotVM;
		private readonly ToView toView;

		public HotPage()
		{
			this.InitializeComponent();
			this.Loaded += HotPage_Loaded;
			hotVM = new HotVM();
			toView = new ToView();
		}

		private async void HotPage_Loaded(object sender, RoutedEventArgs e)
		{
			if (!hotVM.Loading && hotVM.Items == null)
			{
				await hotVM.GetPopular();
			}
		}

		private void List_hot_ItemClick(object sender, ItemClickEventArgs e)
		{
			var data = e.ClickedItem as HotDataItemModel;
			if (data.card_goto == "av")
			{
				MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), data.param);
				return;
			}
			else
			{
				Utils.ShowMessageToast("不支持跳转的类型");
			}
		}

		private void ls_Part_ItemClick(object sender, ItemClickEventArgs e)
		{
			var data = e.ClickedItem as HotTopItemModel;
			if (data.module_id == "rank")
			{
				MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(RankPage));
				return;
			}
			if (data.uri.Contains("https://") || data.uri.Contains("http://"))
			{
				MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
				return;
			}
			else
			{
				Utils.ShowMessageToast("不支持跳转的类型");
			}
		}
	}
}