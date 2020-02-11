using BiliBili.UWP.Modules;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveV2Page : Page
    {
        public LiveV2Page()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            liveHome = new LiveHome();
            this.DataContext = liveHome;
        }
        LiveHome liveHome;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New&& liveHome.RoomList==null)
            {
                await liveHome.LoadHome();
            }
            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;

            }
        }
      

        private void BtnTopbar_MyFeed_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, Type.GetType((sender as Button).Tag.ToString()));
           
        }

      
        private void Gv_areas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)gv_areas.ItemsPanelRoot;
            if (panel != null)
            {
                panel.ItemWidth = e.NewSize.Width / 5;
            }
            
        }


        private void GvRoomItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)(sender as GridView).ItemsPanelRoot;
            if (panel != null)
            {
                panel.ItemWidth = (e.NewSize.Width) / 2;
            }
        }

        private void Gv_room_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (e.ClickedItem as room_list_item);
            liveHome.OpenLiveRoom(item.roomid);
        }
        private void Gv_areas_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (e.ClickedItem as live_area_entrance_v2_item);
            liveHome.HandelLiveUrl(item.link);
        }

        private async void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await liveHome.LoadHome();
        }
    }
}
