using BiliBili3.Models;
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

namespace BiliBili3.Pages
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
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
                _HotPage = 1;
                _NewPage = 1;
                _LBPage = 1;
                _TJLoading = false;
                _HotLoading = false;
                _NewLoading = false;
                _LBLoading = false;
                pivot.SelectedIndex = 0;
                GetTJ();
            }
        }
        int _TJPage = 1;
        int _HotPage = 1;
        int _NewPage = 1;
        int _LBPage = 1;
        bool _TJLoading = false;
        bool _HotLoading = false;
        bool _NewLoading = false;
        bool _LBLoading = false;
        private async void GetTJ()
        {
            try
            {
                if (_TJPage==1)
                {
                    gv_TJ.Items.Clear();
                }
                _TJLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("http://live.bilibili.com/mobile/rooms?access_key={0}&appkey={1}&area_id=0&build=434000&mobi_app=android&page={2}&platform=android&sort=suggestion", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _TJPage);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                AllLiveModel m = JsonConvert.DeserializeObject<AllLiveModel>(results);
                if (m.code == 0)
                {
                    if (m.data.Count != 0)
                    {
                        m.data.ForEach(x => gv_TJ.Items.Add(x));
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
        private async void GetHot()
        {
            try
            {
                if (_HotPage == 1)
                {
                    gv_Hot.Items.Clear();
                }
                _HotLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("http://live.bilibili.com/mobile/rooms?access_key={0}&appkey={1}&area_id=0&build=434000&mobi_app=android&page={2}&platform=android&sort=hottest", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _HotPage);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                AllLiveModel m = JsonConvert.DeserializeObject<AllLiveModel>(results);
                if (m.code == 0)
                {
                    if (m.data.Count != 0)
                    {
                        m.data.ForEach(x => gv_Hot.Items.Add(x));
                        _HotPage++;
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
                _HotLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }

        private async void GetNew()
        {
            try
            {
                if (_NewPage == 1)
                {
                    gv_New.Items.Clear();
                }
                _NewLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("http://live.bilibili.com/mobile/rooms?access_key={0}&appkey={1}&area_id=0&build=434000&mobi_app=android&page={2}&platform=android&sort=latest", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _NewPage);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                AllLiveModel m = JsonConvert.DeserializeObject<AllLiveModel>(results);
                if (m.code == 0)
                {
                    if (m.data.Count != 0)
                    {
                        m.data.ForEach(x => gv_New.Items.Add(x));
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
        private async void GetLB()
        {
            try
            {
                if (_LBPage == 1)
                {
                    gv_LB.Items.Clear();
                }
                _LBLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("http://live.bilibili.com/mobile/rooms?access_key={0}&appkey={1}&area_id=0&build=434000&mobi_app=android&page={2}&platform=android&sort=roundroom", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _LBPage);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                AllLiveModel m = JsonConvert.DeserializeObject<AllLiveModel>(results);
                if (m.code == 0)
                {
                    if (m.data.Count != 0)
                    {
                        m.data.ForEach(x => gv_LB.Items.Add(x));
                        _LBPage++;
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
                _LBLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
        private void sv_TJ_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_TJ.VerticalOffset == sv_TJ.ScrollableHeight)
            {
                if (!_TJLoading)
                {
                    GetTJ();
                }
            }
        }

        private void gv_TJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as AllLiveModel).room_id);
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
            switch (pivot.SelectedIndex)
            {
                case 1:
                    if (gv_Hot.Items.Count == 0)
                    {
                        _HotPage = 1;
                        GetHot();
                    }
                    break;
                case 2:
                    if (gv_New.Items.Count == 0)
                    {
                        _HotPage = 1;
                        GetNew();
                    }
                    break;
                case 3:
                    if (gv_LB.Items.Count == 0)
                    {
                        _LBPage = 1;
                        GetLB();
                    }
                    break;
                default:
                    break;
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
        private void sv_Hot_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_Hot.VerticalOffset == sv_Hot.ScrollableHeight)
            {
                if (!_HotLoading)
                {
                    GetHot();
                }
            }
        }


        private void btn_LoadMore_Hot_Click(object sender, RoutedEventArgs e)
        {
            if (!_HotLoading)
            {
                GetHot();
            }
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

        private void sv_LB_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_LB.VerticalOffset == sv_LB.ScrollableHeight)
            {
                if (!_LBLoading)
                {
                    GetLB();
                }
            }
        }

        private void btn_LoadMore_LB_Click(object sender, RoutedEventArgs e)
        {
            if (!_LBLoading)
            {
                GetLB();

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
                    _HotPage = 1;
                    GetHot();
                    break;
                case 2:
                    _HotPage = 1;
                    GetNew();
                    break;
                case 3:

                    _LBPage = 1;
                    GetLB();
                    break;
                default:
                    break;
            }
        }
    }
}
