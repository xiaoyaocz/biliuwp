using BiliBili.UWP.Models;
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
using Newtonsoft.Json;
using BiliBili.UWP.Modules;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BiliBili.UWP.Modules.LiveModels;
using BiliBili.UWP.Modules.LiveCenterModels;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveFeedPage : Page
    {
        LiveCenter liveCenter;
        public LiveFeedPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            liveCenter = new LiveCenter();
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        int _page = 1;
        bool _loading = false;
        bool _loadend = false;

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
                LoadData();
            }
        }

        private async void LoadData()
        {
          
            _page = 1;
            _loading = true;
            _loadend = false;
            list_Live.ItemsSource = null;
            list_UnLive.ItemsSource = null;
            await LoadLive();
            LoadUnLive();
        }

        /// <summary>
        /// 加载直播中
        /// </summary>
        private async Task LoadLive()
        {
            pr_Load.Visibility = Visibility.Visible;
            var data = await liveCenter.GetLiveList();
            if (data.success)
            {
                list_Live.ItemsSource = data.data;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            pr_Load.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 加载未在直播
        /// </summary>
        private async void LoadUnLive()
        {
            pr_Load.Visibility = Visibility.Visible;
            _loading = true;
            var data = await liveCenter.GetUnLiveList(_page);
            if (data.success)
            {
                if (data.data == null || data.data.Count == 0)
                {
                    _loadend = true;
                    Utils.ShowMessageToast("加载完了");
                    return;
                }
                if (list_UnLive.ItemsSource == null)
                {
                    list_UnLive.ItemsSource = data.data;
                }
                else
                {
                    var ls = (list_UnLive.ItemsSource as ObservableCollection<NotLivingModel>);
                    foreach (var item in data.data)
                    {
                        ls.Add(item);
                    }
                }

                _page++;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            pr_Load.Visibility = Visibility.Collapsed;
            _loading = false;
           
        }


        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_loading && !_loadend)
                {
                    LoadUnLive();
                }
            }
        }

        private void list_Feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            var m = e.ClickedItem as NotLivingModel;

            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), m.roomid);
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
            if (!_loading && !_loadend)
            {
                LoadUnLive();
            }
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void list_Live_ItemClick(object sender, ItemClickEventArgs e)
        {
            var m= e.ClickedItem as LivingModel;
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), m.roomid);
        }
    }
}
