using BiliBili.UWP.Models;
using Newtonsoft.Json;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveHistoryPage : Page
    {
        public LiveHistoryPage()
        {
            this.InitializeComponent();
        }
        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }


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
            if (e.NavigationMode == NavigationMode.New)
            {
                _page = 1;
                list_Feed.Items.Clear();
                LoadFeed();
            }
        }
        int _page = 1;
        bool _loading = false;
        private async void LoadFeed()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                _loading = true;
                string url = string.Format("http://api.live.bilibili.com/i/api/history?page={0}&pageSize=12", _page);
             
                string results = await WebClientClass.GetResults(new Uri(url));
                FeedModel m = JsonConvert.DeserializeObject<FeedModel>(results);
                if (m.code == 0)
                {
                    if (m.data.list.Count != 0)
                    {
                        m.data.list.ForEach(x => list_Feed.Items.Add(x));
                        _page++;
                    }
                    else
                    {
                        Utils.ShowMessageToast("加载完了...", 3000);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(m.message, 3000);
                }

            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    Utils.ShowMessageToast("检查你的网络连接！", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("发生错误\r\n" + ex.Message, 3000);
                }
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                _loading = false;
            }
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_loading)
                {
                    LoadFeed();
                }
            }
        }

        private void list_Feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as FeedModel).roomid);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int d = Convert.ToInt32(this.ActualWidth / 400);
            if (d > 3)
            {
                d = 3;
            }
            bor_Width.Width = this.ActualWidth / d - 22;
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!_loading)
            {
                LoadFeed();
            }
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            _page = 1;
            list_Feed.Items.Clear();
            LoadFeed();
        }
    }
}
