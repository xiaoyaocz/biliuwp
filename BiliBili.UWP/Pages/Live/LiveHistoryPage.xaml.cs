using BiliBili.UWP.Modules.Live;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class LiveHistoryPage : Page
	{
		private readonly LiveWatchHistoryVM watchHistoryVM;

		public LiveHistoryPage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			watchHistoryVM = new LiveWatchHistoryVM();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.NavigationMode == NavigationMode.New)
			{
				await watchHistoryVM.GetHistorys();
			}
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			if (e.NavigationMode == NavigationMode.Back)
			{
				this.NavigationCacheMode = NavigationCacheMode.Disabled;
			}
			base.OnNavigatingFrom(e);
		}

		private async void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			await MessageCenter.HandelUrl((e.ClickedItem as LiveWatchHistoryItemModel).uri);
		}

		private void btn_Back_Click(object sender, RoutedEventArgs e)
		{
			if (this.Frame.CanGoBack)
			{
				this.Frame.GoBack();
			}
		}
	}
}