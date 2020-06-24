using BiliBili.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using BiliBili.UWP.Pages;
using Windows.UI;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using BiliBili.UWP.Pages.Music;
using BiliBili.UWP.Pages.FindMore;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.ChannelModels;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ChannelPage : Page
    {
        public ChannelPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            channel = new Channel();
        }
        Channel channel;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;

            }
            if (e.NavigationMode == NavigationMode.New)
            {

                SetRegion();
                LoadFindMore();


            }

        }
        protected override Size MeasureOverride(Size availableSize)
        {
            bor_width.Width = (availableSize.Width - 16 - 24 - 12) / 3;
            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// 设置分区
        /// </summary>
        private async void SetRegion()
        {
            if (ApiHelper.regions == null)
            {
                await ApiHelper.SetRegions();
            }

            ls_Part.ItemsSource = ApiHelper.regions;



        }
        /// <summary>
        /// 加载频道
        /// </summary>
        private async void LoadFindMore()
        {
            if (!ApiHelper.IsLogin())
            {
                grid_Login.Visibility = Visibility.Visible;

            }
            else
            {
                grid_Login.Visibility = Visibility.Collapsed;
            }
            pr_Load.Visibility = Visibility.Visible;
            var data = await channel.GetChannel();
            if (data.success)
            {
                this.DataContext = data.data;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            pr_Load.Visibility = Visibility.Collapsed;
        }
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {


        }



        private void ls_Part_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as RegionModel;
            if (item.name == "放映厅")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(Pages.Season.SeasonIndexPage),new Modules.Season.SeasonIndexParameter() { 
                    type= Modules.Season.IndexSeasonType.Movie
                });
                return;
            }
            if (item.name == "相簿")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(AlbumPage));
                return;
            }
            if (item.name == "音频")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Home, typeof(MusicHomePage));
                return;
            }
            if (item.name == "小视频")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveVideoPage));
                return;
            }
            if (item.name == "专栏")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Home, typeof(ArticlePage));
                return;
            }
            if (item.name == "直播")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Home, typeof(LiveV2Page));
                return;
            }
            if (item.name.Contains( "排行榜"))
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(RankPage));
                return;
            }
            if (item.name== "话题中心")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(TopicPage));
                return;
            }
            if (item.name == "活动中心")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ActivityPage));
                return;
            }
            //if (item.name == "漫画")
            //{
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "https://manga.bilibili.com");
            //    return;
            //}
            if (item.uri!=null&&item.uri.Contains("https://"))
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), item.uri);
            }
            else
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), item);
            }
            
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadFindMore();
        }

        private async void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            if (await Utils.ShowLoginDialog())
            {
                LoadFindMore();
            }

        }

        private void btn_All_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MyTagPage));
        }

        private void grid_find_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item= e.ClickedItem as Rec_channel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicTopicPage),new object[] {
                 item.name,
                 item.id
            });
        }

        private void grid_myatton_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Atten_channel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicTopicPage), new object[] {
                 item.name,
                 item.id
            });
        }
        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin()&& !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            var m= (sender as HyperlinkButton).DataContext as Modules.ChannelModels.Rec_channel;
            var data=await channel.FollowChannel(m.id);
            if (data.success)
            {
                Utils.ShowMessageToast("订阅成功");
                LoadFindMore();
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            var m = (sender as HyperlinkButton).DataContext as Modules.ChannelModels.Rec_channel;
            var data = await channel.CancelFollowChannel(m.id);
            if (data.success)
            {
                Utils.ShowMessageToast("已经取消订阅");
                LoadFindMore();
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }
    }

}
