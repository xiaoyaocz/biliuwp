using BiliBili.UWP.Models;
using BiliBili.UWP.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LivePage : Page
    {
        public LivePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void btn_Banner_Click(object sender, RoutedEventArgs e)
        {
            string ban = Regex.Match(((sender as HyperlinkButton).DataContext as HomeLiveModel).link, @"^bilibili://live/(.*?)").Groups[1].Value;
            if (ban.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), ban);

                return;
            }
            //http://live.bilibili.com/AppBanner/index?id=460
            //string ban2 = Regex.Match(((sender as HyperlinkButton).DataContext as HomeLiveModel).link+"/", @"id=(.*?)/").Groups[1].Value;
            //if (ban2.Length != 0)
            //{
            //    MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), ban2.Replace("/",""));

            //    return;
            //}
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), ((sender as HyperlinkButton).DataContext as HomeLiveModel).link);
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;

            }
            if (e.NavigationMode == NavigationMode.New && home_flipView.ItemsSource == null)
            {
                await Task.Delay(200);

                GetLiveInfo();
            }
        }
        public bool isLoaded = false;
        public async void GetLiveInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;

                //gridview_SJ.ItemsSource=null;
                //gridview_DJ.Items.Clear();
                //gridview_FY.Items.Clear();
                //gridview_HH.Items.Clear();
                //gridview_JJ.Items.Clear();

                ////gridview_SH.Items.Clear();
                //gridview_WL.Items.Clear();
                //gridview_YZ.Items.Clear();
                //gridview_CW.Items.Clear();
                string url = string.Format("http://live.bilibili.com/AppNewIndex/common?_device=android&platform=android&scale=xxhdpi");
                string results = await WebClientClass.GetResults_Live(new Uri(url));
                HomeLiveModel model = JsonConvert.DeserializeObject<HomeLiveModel>(results);
                if (model.code == 0)
                {

                    home_flipView.ItemsSource = model.data.banner;

                    model.data.partitions = model.data.partitions.OrderBy(x => x.partition.id).ToList();

                    this.DataContext = model.data;

                    //foreach (HomeLiveModel item in partModel)
                    //{
                    //    HomeLiveModel partitionModel = JsonConvert.DeserializeObject<HomeLiveModel>(item.partition.ToString());
                    //    List<HomeLiveModel> livesModel = JsonConvert.DeserializeObject<List<HomeLiveModel>>(item.lives.ToString());
                    //}
                    isLoaded = true;
                }
                else
                {
                    Utils.ShowMessageToast("读取直播失败" + model.message, 3000);
                    isLoaded = false;
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867 || ex.HResult == -2147012889)
                {
                    Utils.ShowMessageToast("无法连接服务器，请检查你的网络连接", 3000);
                }
                else
                {

                    Utils.ShowMessageToast("读取直播失败" + ex.Message, 3000);
                }

                //ErrorEvent("读取直播失败" + ex.Message);
                isLoaded = false;
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void gridview_Hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as HomeLiveModel).room_id);
            //PlayEvent((e.ClickedItem as HomeLiveModel).room_id);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

            var info = (sender as HyperlinkButton).DataContext as HomeLiveModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LivePartInfoPage), info.partition.id);
            //OpenEvent(7);
        }



        private void hot_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            // OpenEvent(0);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewBox2_num.Width = ActualWidth / 2 - 20;
            //double d = ((ViewBox2_num.Width + 12) / 1.15) * 2;
            //gridview_Hot.Height = d;
            //gridview_DJ.Height = d;
            //gridview_FY.Height = d;
            //gridview_HH.Height = d;
            //gridview_JJ.Height = d;
            //gridview_MZ.Height = d;
            ////gridview_SH.Height = d;
            //gridview_WL.Height = d;
            //gridview_YZ.Height = d;
            //gridview_SJ.Height = d;
            //gridview_CW.Height = d;
        }

        private void btn_LivePart_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LivePartPage));
        }

        private async void btn_myfeed_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                if (!await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
            }

            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveFeedPage));

        }

        private async void btn_liveCenter_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                if (!await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveCenterPage));

        }

        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveSearchPage));
        }

        private void btn_miniVideo_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveVideoPage));
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetLiveInfo();
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            GetLiveInfo();
        }
    }

}
