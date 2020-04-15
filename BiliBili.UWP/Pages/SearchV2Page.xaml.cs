using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.SearchModels;
using BiliBili.UWP.Pages.FindMore;
using BiliBili.UWP.Pages.Live;
using BiliBili.UWP.Pages.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages
{
    public class SearchParameter
    {
        public string keyword { get; set; }
        public SearchType searchType { get; set; } = SearchType.Video;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchV2Page : Page
    {
        SearchVM searchVM;
     
        public SearchV2Page()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            searchVM = new SearchVM();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New)
            {
                SearchParameter par = new SearchParameter();
                if (e.Parameter is SearchParameter)
                {
                    par = e.Parameter as SearchParameter;
                }
                else if (e.Parameter is object[])
                {
                    par.keyword = (e.Parameter as object[])[0].ToString();
                }
                else
                {
                    par.keyword = e.Parameter?.ToString() ?? "";
                }
                txtKeyword.Text = par.keyword;
                foreach (var item in searchVM.SearchItems)
                {
                    item.Keyword = par.keyword;
                }
                pivot.SelectedIndex = (int)par.searchType;
            }
           

        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private async void txtKeyword_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(txtKeyword.Text))
            {
                Utils.ShowMessageToast("关键字不能为空啊，喂(#`O′)");
                return;
            }
            if (await MessageCenter.HandelUrl(txtKeyword.Text))
            {
                return;
            }
            foreach (var item in searchVM.SearchItems)
            {
                item.Keyword = txtKeyword.Text;
                item.Page = 1;
                item.HasData = false;
            }
            searchVM.SelectItem.Refresh();
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem != null)
            {
                var item = pivot.SelectedItem as ISearchVM;
                if (!item.HasData && !item.Loading&& !string.IsNullOrEmpty(item.Keyword))
                {
                    await item.LoadData();
                }
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = (sender as ComboBox).DataContext as ISearchVM;
            if (data.HasData && !data.Loading)
            {
                data.Refresh();
            }
        }
        private void Search_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (e.ClickedItem is SearchVideoItem)
            {
                var data = e.ClickedItem as SearchVideoItem;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), data.aid);
                return;
            }
            if (e.ClickedItem is SearchAnimeItem)
            {
                var data = e.ClickedItem as SearchAnimeItem;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), data.season_id);
                return;
            }
            if (e.ClickedItem is SearchUserItem)
            {
                var data = e.ClickedItem as SearchUserItem;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), data.mid);
                return;
            }
            if (e.ClickedItem is SearchLiveRoomItem)
            {
                var data = e.ClickedItem as SearchLiveRoomItem;
                MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPC), data.roomid);
                return;
            }
            if (e.ClickedItem is SearchArticleItem)
            {
                var data = e.ClickedItem as SearchArticleItem;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), data.id);
                return;
            }
            if (e.ClickedItem is SearchTopicItem)
            {
                var data = e.ClickedItem as SearchTopicItem;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.arcurl);
                return;
            }
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
    public class SearchDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate AnimeTemplate { get; set; }
        public DataTemplate TestTemplate { get; set; }
        public DataTemplate LiveRoomTemplate { get; set; }
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate ArticTemplate { get; set; }
        public DataTemplate TopicTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var data = item as ISearchVM;
            switch (data.SearchType)
            {
                case SearchType.Video:
                    return VideoTemplate;
                case SearchType.Anime:
                case SearchType.Movie:
                    return AnimeTemplate;
                case SearchType.User:
                    return UserTemplate;
                case SearchType.Live:
                    return LiveRoomTemplate;
                case SearchType.Article:
                    return ArticTemplate;
                case SearchType.Topic:
                    return TopicTemplate;
                case SearchType.Anchor:
                    return TestTemplate;
                default:
                    return TestTemplate;
            }


        }
    }
}
