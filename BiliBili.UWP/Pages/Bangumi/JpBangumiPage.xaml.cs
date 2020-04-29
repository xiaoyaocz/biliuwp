using BiliBili.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class JpBangumiPage : Page
    {
        public JpBangumiPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        DispatcherTimer time;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;
            }
            if (e.NavigationMode== NavigationMode.New&&this.DataContext==null)
            {
                cursor = "-1";
                LoadHome();
            }
            if (time == null)
            {
                time = new DispatcherTimer();
                time.Interval = new TimeSpan(0, 0, 3);
                time.Tick += Time_Tick;
                time.Start();
            }
        }
        private void Time_Tick(object sender, object e)
        {
            if (home_flipView.SelectedIndex == -1)
            {
                return;
            }
            int i = home_flipView.SelectedIndex;
            i++;
            if (i >= home_flipView.Items.Count)
            {
                i = 0;
            }
            home_flipView.SelectedIndex = i;
        }
       
        private async void LoadHome()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = string.Format("https://bangumi.bilibili.com/appindex/followjp_index_page?access_key={0}&appkey={1}&build=5250000&mobi_app=android&platform=wp&ts={2}000",ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                JpHomeModel m = JsonConvert.DeserializeObject<JpHomeModel>(results);
                if (m.code == 0)
                {
                    sp_Home.DataContext = m.result;
                }
                else
                {
                    Utils.ShowMessageToast(m.message, 3000);
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

                    Utils.ShowMessageToast("读取推荐信息", 3000);
                }
            }
            finally
            {
                LoadTj();
                //pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        string cursor = "-1";
        private async void LoadTj()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                _loading = true;
                if (cursor=="-1")
                {
                    list_ban_jp_foot.Items.Clear();
                }
                string url = string.Format("https://bangumi.bilibili.com/api/bangumi_recommend?access_key={0}&appkey={1}&build=5250000&cursor={2}&mobi_app=android&pagesize=10&platform=wp&ts={3}000&type=0", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, cursor,ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                BanTJModel m = JsonConvert.DeserializeObject<BanTJModel>(results);
                if (m.code == 0)
                {
                    if (m.result.Count!=0)
                    {
                        m.result.ForEach(x => list_ban_jp_foot.Items.Add(x));
                        //list_ban_jp_foot.ItemsSource = m.result;
                        cursor = m.result.Last().cursor;
                    }
                    else
                    {
                        Utils.ShowMessageToast("全部加载完...",2000);
                    }
                   
                }
                else
                {
                    Utils.ShowMessageToast(m.message, 3000);
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

                    Utils.ShowMessageToast("读取推荐信息", 3000);
                }
            }
            finally
            {
                _loading = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void list_ban_jp_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as JpHomeModel).season_id.ToString());
        }

        private void list_ban_new_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as JpHomeModel).season_id.ToString());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewBox2_num.Width = ActualWidth / 3 - 21;
        }

        private void btn_Banner_Click(object sender, RoutedEventArgs e)
        {
            string ban = Regex.Match(((sender as HyperlinkButton).DataContext as JpHomeModel).link, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            if (ban.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban);
                return;
            }
            //
            string aid = Regex.Match(((sender as HyperlinkButton).DataContext as JpHomeModel).link, @"^http://www.bilibili.com/video/av(.*?)/$").Groups[1].Value;
            if (aid.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), aid);
                return;
            }

            string aid2 = Regex.Match(((sender as HyperlinkButton).DataContext as JpHomeModel).link, @"^bilibili://video/(.*?)$").Groups[1].Value;
            if (aid2.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), aid2);
                return;
            }

            string game = Regex.Match(((sender as HyperlinkButton).DataContext as JpHomeModel).link, @"^bilibili://game/(.*?)$").Groups[1].Value;
            if (game.Length != 0)
            {

                Utils.ShowMessageToast("不支持游戏链接跳转", 3000);
                return;
            }

            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), ((sender as HyperlinkButton).DataContext as JpHomeModel).link);
        }

        private void btn_NewBan_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(TimelinePage),2);
        }

        private void btn_10Ban_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(Season.SeasonIndexPage), new Modules.Season.SeasonIndexParameter()
            {
                type = Modules.Season.IndexSeasonType.Anime
            });
            //MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanTagPage));
        }

        private void list_ban_jp_foot_ItemClick(object sender, ItemClickEventArgs e)
        {
            string ban = Regex.Match((e.ClickedItem as BanTJModel).link, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            if (ban.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban);
                return;
            }
            //
            string aid = Regex.Match((e.ClickedItem as BanTJModel).link, @"^http://www.bilibili.com/video/av(.*?)/$").Groups[1].Value;
            if (aid.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), aid);
                return;
            }

            string aid2 = Regex.Match((e.ClickedItem as BanTJModel).link, @"^bilibili://video/(.*?)$").Groups[1].Value;
            if (aid2.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), aid2);
                return;
            }

            string game = Regex.Match((e.ClickedItem as BanTJModel).link, @"^bilibili://game/(.*?)$").Groups[1].Value;
            if (game.Length != 0)
            {

                Utils.ShowMessageToast("不支持游戏链接跳转", 3000);
                return;
            }

            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), (e.ClickedItem as BanTJModel).link);
        }
        bool _loading = false;
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_loading)
                {
                    LoadTj();
                }
            }
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            cursor = "-1";
            LoadHome();
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            cursor = "-1";
            LoadHome();
        }
    }
}
