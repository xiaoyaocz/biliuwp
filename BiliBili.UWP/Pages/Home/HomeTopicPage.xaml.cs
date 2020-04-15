using BiliBili.UWP.Modules.Home;
using BiliBili.UWP.Modules.Home.HomeTopicModels;
using BiliBili.UWP.Pages.FindMore;
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

namespace BiliBili.UWP.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomeTopicPage : Page
    {
        readonly TopicVM topicVM;
        public int TabID { get; set; }
        public HomeTopicPage()
        {
            this.InitializeComponent();
            this.Loaded += HomeTopicPage_Loaded;
            topicVM = new TopicVM();
        }

        private async void HomeTopicPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!topicVM.Loading && topicVM.Detail == null)
            {
                topicVM.tab_id = TabID;
                await topicVM.GetTabData();
            }
        }
        private async void btn_special_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as TabItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private async void btn_topic_banner_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as TabBannerItem;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private void ls_rcmd_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as TabVideoItemModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), data.param);
        }

        private void btn_rcmdMore_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as TabItemModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicTopicPage), new object[] { data.title, data.param });
        }

        private async void ls_players_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as TabVideoItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
            //MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), data.param);
        }

        private async void ls_entrance_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as TabVideoItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as TabItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }
    }

    public class HomeTopicTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Nothing { get; set; }
        public DataTemplate Player { get; set; }
        public DataTemplate Special { get; set; }
        public DataTemplate Banner { get; set; }
        public DataTemplate Rcmd { get; set; }
        public DataTemplate News { get; set; }
        public DataTemplate Entrance { get; set; }
        public DataTemplate Converge { get; set; }
        public DataTemplate Unknown { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch ((item as TabItemModel).@goto)
            {
                case "":
                case null:
                    return Nothing;
                case "player":
                    return Player;
                case "special":
                    return Special;
                case "banner":
                    return Banner;
                case "content_rcmd":
                case "tag_rcmd":
                    return Rcmd;
                case "news":
                    return News;
                case "entrance":
                    return Entrance;
                case "converge":
                    return Converge;
                default:
                    return Unknown;
            }

        }
    }

}
