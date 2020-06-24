using BiliBili.UWP.Models;
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
using Newtonsoft.Json;
using BiliBili.UWP.Pages;
using System.Text.RegularExpressions;
using BiliBili.UWP.Pages.Season;
using BiliBili.UWP.Api.User;
using BiliBili.UWP.Api;
using Newtonsoft.Json.Linq;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BangumiPage : Page
    {
        public BangumiPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
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
            if (e.NavigationMode == NavigationMode.New )
            {
                await Task.Delay(200);
                
                if (ApiHelper.IsLogin())
                {
                    
                    myban.Visibility = Visibility.Visible;
                    LoadMy();
                }
                else
                {
                    myban.Visibility = Visibility.Collapsed;
                    b_btn_Refresh.Visibility = Visibility.Collapsed;
                }
                if (list_ban_jp.ItemsSource == null)
                {
                    LoadHome();
                }
                
            }
            // await Task.Delay(200);
          

        }
        private async void LoadMy()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                var result =await new FollowAPI().MyFollowBangumi().Request();

                if (result.status)
                {
                    var data = await result.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        list_ban_mine.ItemsSource  = (await data.result["follow_list"].ToString().DeserializeJson<List<FollowSeasonModel>>()).Take(9).ToList();
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
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

                    Utils.ShowMessageToast("读取追番失败了", 3000);
                }
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private async void LoadHome()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = string.Format("https://bangumi.bilibili.com/appindex/follow_index_page?appkey={0}&build=5250000&mobi_app=android&platform=wp&ts={1}000",ApiHelper.AndroidKey.Appkey,ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                BangumiHomeModel m = JsonConvert.DeserializeObject<BangumiHomeModel>(results);
                if (m.code==0)
                {
                    this.DataContext = m;
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
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewBox2_num.Width = ActualWidth / 3 - 21;
        }

        private void btn_MyBan_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(FollowSeasonPage), Modules.SeasonType.bangumi);
        }

        private void list_ban_mine_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as FollowSeasonModel).season_id.ToString());
        }

        private void list_ban_jp_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as BangumiHomeModel).season_id.ToString());
        }

        private void list_ban_cn_foot_ItemClick(object sender, ItemClickEventArgs e)
        {
            //妈蛋，B站就一定要返回个链接么,就不能返回个类型加参数吗
            string tag = Regex.Match((e.ClickedItem as BangumiHomeModel).link, @"^http://bangumi.bilibili.com/anime/category/(.*?)$").Groups[1].Value;
            if (tag.Length != 0)
            {
               // MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as BangumiHomeModel).season_id.ToString());
                //NavigatedTo(typeof(BanByTagPage), new string[] { tag, (e.ClickedItem as BanTJModel).title });
                return;
            }
            string ban = Regex.Match((e.ClickedItem as BangumiHomeModel).link, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            if (ban.Length != 0)
            {

                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban);
                return;
            }
            string aid = Regex.Match((e.ClickedItem as BangumiHomeModel).link, @"^http://www.bilibili.com/video/av(.*?)/$").Groups[1].Value;
            if (aid.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage),aid);
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), (e.ClickedItem as BangumiHomeModel).link);
        }

        private void btn_Timeline_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(TimelinePage));
        }

        private void btn_tag_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(SeasonIndexPage),new Modules.Season.SeasonIndexParameter() { 
                type= Modules.Season.IndexSeasonType.Anime
            });
        }

        private void btn_jp_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(JpBangumiPage));
        }

        private void btn_cn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CnBangumiPage));
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                myban.Visibility = Visibility.Visible;
                LoadMy();
            }
            else
            {
                myban.Visibility = Visibility.Collapsed;
            }
            if (list_ban_jp.ItemsSource == null)
            {
                LoadHome();
            }
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            if (ApiHelper.IsLogin())
            {
                myban.Visibility = Visibility.Visible;
                LoadMy();
            }
            else
            {
                myban.Visibility = Visibility.Collapsed;
            }
            if (list_ban_jp.ItemsSource == null)
            {
                LoadHome();
            }
        }
    }
}
