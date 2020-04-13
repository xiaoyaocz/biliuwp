using BiliBili.UWP.Controls;
using BiliBili.UWP.Models;
using BiliBili.UWP.Pages.User;
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
    public sealed partial class LiveVideoPage : Page
    {
        public LiveVideoPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
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
                _GZPage = "";
                _AllPage = "";

                _GZLoading = false;
                _AllLoading = false;
                gv_All.Items.Clear();
                gv_GZ.Items.Clear();
                pivot.SelectedIndex = 1;
                GetAll();
            }
        }
        string _GZPage = "";
        string _AllPage = "";
        bool _GZLoading = false;
        bool _AllLoading = false;
        private async void GetGz()
        {
            try
            {
                _GZLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("http://api.vc.bilibili.com/clip/v1/feed/followedVideoList?next_offset={0}&page_size=10",  _GZPage);
                //url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                LiveVideoModel m = JsonConvert.DeserializeObject<LiveVideoModel>(results.Replace("default", "_default"));
                if (m.code == 0)
                {
                    if (m.data.items.Count != 0)
                    {
                        m.data.items.ForEach(x => gv_GZ.Items.Add(x));
                        _GZPage = m.data.next_offset;
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
                _GZLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
        private async void GetAll()
        {
            try
            {
                _AllLoading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("http://api.vc.bilibili.com/clip/v1/video/index?access_key={0}&appkey={1}&build=434000&mobi_app=android&need_playurl=1&next_offset={2}&page_size=10&platform=android&src=master&trace_id=20170203233400032&version=4.34.0.434000", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _AllPage);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                LiveVideoModel m = JsonConvert.DeserializeObject<LiveVideoModel>(results.Replace("default", "_default"));
                if (m.code == 0)
                {
                    if (m.data.items.Count != 0)
                    {
                        m.data.items.ForEach(x => gv_All.Items.Add(x));
                        _AllPage = m.data.next_offset;
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
                _AllLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
        private async void gv_GZ_ItemClick(object sender, ItemClickEventArgs e)
        {
            var info = e.ClickedItem as LiveVideoModel;
            if (info.item.type == 1)
            {
                this.Frame.Navigate(typeof(WebPage), new object[] { info.item.jump_url });
                return;
            }

           // MiniVideoDialog miniVideoDialog = new MiniVideoDialog();

           // miniVideoDialog.ShowAsync(info.);

            //var x = new ContentDialog();
            //StackPanel st = new StackPanel();
            //MediaElement me = new MediaElement() {
            //    AreTransportControlsEnabled = true,
            //    AutoPlay = true,
            //    Source=new Uri(info.item.video_playurl),
            //    TransportControls=new MediaTransportControls() {
            //        IsZoomButtonVisible=false,
            //        IsFullWindowButtonVisible=false
            //    }
            //};
            //st.Children.Add(me);
            //var v = videoinfo;

            //st.Children.Add(v);
            //x.Content = st;

            //x.PrimaryButtonText = "关闭";
            //x.IsPrimaryButtonEnabled = true;
            //await x.ShowAsync();
            cd.DataContext = info;
            await cd.ShowAsync();
        }

        private void sv_GZ_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_GZ.VerticalOffset == sv_GZ.ScrollableHeight)
            {
                if (!_GZLoading)
                {
                    GetGz();
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                ViewBox2_num.Width = ActualWidth - 20;
            }
            else
            {
                int i = Convert.ToInt32(ActualWidth / 300);
                ViewBox2_num.Width = ActualWidth / i - 15;
            }
        }

        private void sv_All_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_All.VerticalOffset == sv_All.ScrollableHeight)
            {
                if (!_AllLoading)
                {
                    GetAll();
                }
            }
        }

        private void btn_LoadMore_GZ_Click(object sender, RoutedEventArgs e)
        {
            if (!_GZLoading)
            {
                GetGz();
            }
        }

        private void btn_LoadMore_All_Click(object sender, RoutedEventArgs e)
        {
            if (!_AllLoading)
            {
                GetAll();
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    if (gv_GZ.Items.Count == 0)
                    {
                        _GZPage = "";
                        GetGz();
                    }
                    break;
                default:
                    break;
            }
        }

        private void cd_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText((sender.DataContext as LiveVideoModel).item.share_url);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            Utils.ShowMessageToast("已将内容复制到剪切板", 3000);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cd.Hide();
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), ((sender as Button).DataContext as LiveVideoModel).user.uid);
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {


                case 0:
                    gv_GZ.Items.Clear();
                    _GZPage = "";
                    GetGz();
                    break;
                case 1:
                    gv_All.Items.Clear();
                    _AllPage = "";
                    GetAll();
                    break;
                default:
                    break;
            }
        }
    }
}
