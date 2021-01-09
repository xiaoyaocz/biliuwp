﻿using BiliBili.UWP.Modules.Home;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.Home
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class HomePage : Page
	{
		private readonly HomeVM homeVM;

		public HomePage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
			homeVM = new HomeVM();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.NavigationMode == NavigationMode.New && homeVM.Tabs.Count <= 2)
			{
				await homeVM.GetTab();
			}
		}
	}

	public class HomeTabTemplateSelector : DataTemplateSelector
	{
		public DataTemplate HotTemplate { get; set; }
		public DataTemplate RecommendTemplate { get; set; }
		public DataTemplate TopicTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			var data = item as HomeTabItem;
			if (data.item is RecommendVM)
			{
				return RecommendTemplate;
			}
			if (data.item is HotVM)
			{
				return HotTemplate;
			}
			if (data.item is TopicVM)
			{
				return TopicTemplate;
			}
			return null;
		}
	}
}