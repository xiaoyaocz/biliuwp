using BiliBili.UWP.Modules;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class LivePartPage : Page
	{
		private LiveArea liveArea;

		public LivePartPage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			liveArea = new LiveArea();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.NavigationMode == NavigationMode.New && pivot.ItemsSource == null)
			{
				await LoadData();
			}
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			if (e.NavigationMode == NavigationMode.Back)
			{
				pivot.ItemsSource = null;
				this.NavigationCacheMode = NavigationCacheMode.Disabled;
			}
			base.OnNavigatingFrom(e);
		}

		private void btn_Back_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.GoBack();
		}

		private void GridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = e.ClickedItem as AreaListItem;
			MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LivePartInfoPage), new object[] {
				   item.parent_id,
				   item.id,
				   item.name
			});
		}

		private async Task LoadData()
		{
			prLoad.Visibility = Visibility.Visible;
			var data = await liveArea.GetAreaList();
			if (data.success)
			{
				pivot.ItemsSource = data.data;
			}
			else
			{
				Utils.ShowMessageToast(data.message);
			}
			prLoad.Visibility = Visibility.Collapsed;
		}
	}
}