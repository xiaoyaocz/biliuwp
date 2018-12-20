using BiliBili3.Models;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili3.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyFollowsBangumiPage : Page
    {
        public MyFollowsBangumiPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
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
            if (e.NavigationMode== NavigationMode.New&&list.Items.Count==0)
            {
                await Task.Delay(200);
                _page = 1;
                LoadMy();
            }
        }

        int _page = 1;
        bool _loading = false;
        private async void LoadMy()
        {
            try
            {
                if (_page==1)
                {
                    list.Items.Clear();

                }
                _loading = true;
                pr_Load.Visibility = Visibility.Visible;
               


                string url = string.Format("http://bangumi.bilibili.com/api/get_concerned_season?build=520001&platform=android&appkey={0}&access_key={1}&page={2}&pagesize=30&ts={3}",ApiHelper._appKey,ApiHelper.access_key, _page,ApiHelper.GetTimeSpan_2);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                MyBangumiModel m = JsonConvert.DeserializeObject<MyBangumiModel>(results);
                if (m.code == 0)
                {
                    if (m.result.Count==0)
                    {
                        Utils.ShowMessageToast("加载完了...", 3000);
                        return;
                    }
                    m.result.ForEach(x=>list.Items.Add(x));
                    // list_ban_mine.ItemsSource = m.result;
                    _page++;
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

                    Utils.ShowMessageToast("读取追番失败了", 3000);
                }
            }
            finally
            {
                _loading = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_loading)
                {
                 
                    LoadMy();
                }
            }
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage),( e.ClickedItem as MyBangumiModel).season_id.ToString());
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            _page = 1;
            LoadMy();
        }
    }
}
