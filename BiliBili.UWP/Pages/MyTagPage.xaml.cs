using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.ChannelModels;
using BiliBili.UWP.Pages.FindMore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
	public class MyTagModel
	{
		public int archive_count { get; set; }
		public int count { get; set; }
		public string cover { get; set; }
		public MyTagModel data { get; set; }
		public string message { get; set; }
		public string name { get; set; }
		public int notify { get; set; }
		public bool status { get; set; }
		public int tag_id { get; set; }
		public List<MyTagModel> tags { get; set; }
		public string updated_ts { get; set; }
	}

	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class MyTagPage : Page
	{
		private Channel channel;

		private Atten_channel selectItem;

		public MyTagPage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
			channel = new Channel();
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			bor_width.Width = (availableSize.Width - 16 - 24 - 12) / 3;
			return base.MeasureOverride(availableSize);
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.NavigationMode == NavigationMode.New)
			{
				await Task.Delay(200);
				LoadList();
			}
		}

		private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
		{
			LoadList();
		}

		private void Border_Holding(object sender, HoldingRoutedEventArgs e)
		{
			selectItem = (sender as Border).DataContext as Atten_channel;
			menu.ShowAt(sender as Border);
		}

		private void Border_RightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			selectItem = (sender as Border).DataContext as Atten_channel;
			menu.ShowAt(sender as Border);
		}

		private void btn_Back_Click(object sender, RoutedEventArgs e)
		{
			if (this.Frame.CanGoBack)
			{
				this.Frame.GoBack();
			}
		}

		private async void btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			var data = await channel.CancelFollowChannel(selectItem.id);
			if (data.success)
			{
				(grid_myatton.ItemsSource as ObservableCollection<Atten_channel>).Remove(selectItem);
			}
			else
			{
				Utils.ShowMessageToast(data.message);
			}
		}

		private void grid_myatton_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = e.ClickedItem as Atten_channel;
			MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicTopicPage), new object[] {
				 item.name,
				 item.id
			});
		}

		private async void LoadList()
		{
			pr_Load.Visibility = Visibility.Visible;
			var data = await channel.GetFollowChannel();
			if (data.success)
			{
				grid_myatton.ItemsSource = data.data;
			}
			else
			{
				Utils.ShowMessageToast(data.message);
			}
			pr_Load.Visibility = Visibility.Collapsed;
		}
	}

	public class TagVideoModel
	{
		public string aid { get; set; }
		public int code { get; set; }
		public string message { get; set; }
		public int page { get; set; }
		public int pagesize { get; set; }
		public string pic { get; set; }
		public string play { get; set; }
		public List<TagVideoModel> result { get; set; }
		public string title { get; set; }
		public int total { get; set; }
		public string video_review { get; set; }
	}
}