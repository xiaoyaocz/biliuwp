using BiliBili.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveAllPage : Page
    {
        public LiveAllPage()
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
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.Frame.Name == "bg_Frame")
            {
                g.Background = null;
            }
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
                _TJPage = 1;
                _NewPage = 1;
                _TJLoading = false;
                _NewLoading = false;
                pivot.SelectedIndex = 0;
                await GetTJ();
            }
        }
        int _TJPage = 1;
        int _NewPage = 1;
        bool _TJLoading = false;
        bool _NewLoading = false;
        private async Task GetTJ()
        {
            try
            {
                _TJLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url =ApiHelper.GetSignWithUrl($"https://api.live.bilibili.com/room/v3/Area/getRoomList?access_key={ ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&area_id=0&build={ApiHelper.build}&cate_id=0&mobi_app=android&page={_TJPage}&page_size=30&parent_area_id=0&platform=android&qn=0&sort_type=online&tag_version=1&ts={ApiHelper.GetTimeSpan}",ApiHelper.AndroidKey);

                string results = await WebClientClass.GetResults(new Uri(url));
                var m = results.ToDynamicJObject();

                if (m.code == 0)
                {

                    var data = JsonConvert.DeserializeObject<RoomListModel>(m.json["data"].ToString());
                    if (data.list.Count != 0)
                    {
                        if (_TJPage == 1)
                        {
                            gv_TJ.ItemsSource = data.list;
                        }
                        else
                        {
                            foreach (var item in data.list)
                            {
                                (gv_TJ.ItemsSource as ObservableCollection<RoomListItem>).Add(item);
                            }
                        }
                        _TJPage++;
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
                _TJLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
  

        private async void GetNew()
        {
            try
            {
                _NewLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = ApiHelper.GetSignWithUrl($"https://api.live.bilibili.com/room/v3/Area/getRoomList?access_key={ ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&area_id=0&build={ApiHelper.build}&cate_id=0&mobi_app=android&page={_NewPage}&page_size=30&parent_area_id=0&platform=android&qn=0&sort_type=live_time&tag_version=1&ts={ApiHelper.GetTimeSpan}", ApiHelper.AndroidKey);

                string results = await WebClientClass.GetResults(new Uri(url));
                var m = results.ToDynamicJObject();

                if (m.code == 0)
                {

                    var data = JsonConvert.DeserializeObject<RoomListModel>(m.json["data"].ToString());
                    if (data.list.Count != 0)
                    {
                        if (_NewPage == 1)
                        {
                            gv_New.ItemsSource = data.list;
                        }
                        else
                        {
                            foreach (var item in data.list)
                            {
                                (gv_New.ItemsSource as ObservableCollection<RoomListItem>).Add(item);
                            }
                        }
                        _NewPage++;
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
                _NewLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
   
        private async void sv_TJ_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_TJ.VerticalOffset == sv_TJ.ScrollableHeight)
            {
                if (!_TJLoading)
                {
                    await GetTJ();
                }
            }
        }

        private void gv_TJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as RoomListItem).roomid);
        }

        private void btn_LoadMore_TJ_Click(object sender, RoutedEventArgs e)
        {
            if (!_TJLoading)
            {
                GetTJ();
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex==1)
            {
                if (gv_New.Items.Count == 0)
                {
                    _NewPage = 1;
                    GetNew();
                }
            }
           
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (this.ActualWidth <= 500)
            //{
            //    bor_Width2.Width = ActualWidth / 2 - 20;
            //}
            //else
            //{
            //    int i = Convert.ToInt32(ActualWidth / 200);
            //    bor_Width2.Width = ActualWidth / i - 15;
            //}
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Width<= 500)
            {
                bor_Width2.Width = availableSize.Width / 2 - 20;
            }
            else
            {
                int i = Convert.ToInt32(availableSize.Width / 200);
                bor_Width2.Width = availableSize.Width / i - 15;
            }
            return base.MeasureOverride(availableSize);
        }



        private void sv_New_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_New.VerticalOffset == sv_New.ScrollableHeight)
            {
                if (!_NewLoading)
                {
                    GetNew();
                }
            }
        }

        private void btn_LoadMore_New_Click(object sender, RoutedEventArgs e)
        {
            if (!_NewLoading)
            {
                GetNew();
            }
        }

    
        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    _TJPage = 1;
                    GetTJ();
                    break;
                case 1:
                    _NewPage = 1;
                    GetNew();
                    break;
                default:
                    break;
            }
        }
    }
}
