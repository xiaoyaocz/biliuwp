using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class FollowSeasonPage : Page
    {
        public FollowSeasonPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        List<SeasonTabItem> tabs;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (e.NavigationMode == NavigationMode.New || tabs == null)
            {
                if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
                var type = (SeasonType)(e.Parameter as object[])[0];
                if (type == SeasonType.cinema)
                {
                    txt_Header.Text = "我的追剧";
                }
                tabs = new List<SeasonTabItem>() {
                    new SeasonTabItem(){
                        id=2,
                        name="在看",
                        seasonFollow=new SeasonFollow(type, 2)
                    },
                    new SeasonTabItem(){
                        id=1,
                        name="想看",
                        seasonFollow=new SeasonFollow(type, 1)
                    },
                    new SeasonTabItem(){
                        id=3,
                        name="看过",
                        seasonFollow=new SeasonFollow(type, 3)
                    },
                };
                pivot.ItemsSource = tabs;
                Utils.ShowMessageToast("右键或长按可以进行更多操作");
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatedFrom(e);
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            if (sv.VerticalOffset >= sv.ScrollableHeight * 0.8)
            {
                (sv.DataContext as SeasonFollow).LoadData();
            }
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as FollowSeasonInfo).season_id.ToString());
        }

        private void BtnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as HyperlinkButton;
            (btn.DataContext as SeasonFollow).LoadData();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == null)
            {
                return;
            }
            var item = (pivot.SelectedItem as SeasonTabItem).seasonFollow;
            if (item.FollowList.Count == 0)
            {
                item.LoadData();
            }
        }

        private async void MenuCancel_Click(object sender, RoutedEventArgs e)
        {
            var info = (sender as MenuFlyoutItem).DataContext as FollowSeasonInfo;

            var tab= tabs.FirstOrDefault(x => x.id == info._status);
            var result=await tab.seasonFollow.CancelFollow(info.season_id,info.season_type);
            if (result.success)
            {
                tab.seasonFollow.FollowList.Remove(info);
            }
            else
            {
                Utils.ShowMessageToast(result.message);
            }

        }

        private void Item_Holding(object sender, HoldingRoutedEventArgs e)
        {
            ShowMenu(sender as Grid);
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowMenu(sender as Grid);
        }
        private void ShowMenu(FrameworkElement sender)
        {
            menu1.Visibility = Visibility.Visible;
            menu2.Visibility = Visibility.Visible;
            menu3.Visibility = Visibility.Visible;
            switch ((sender.DataContext as FollowSeasonInfo)._status)
            {
                case 1:
                    menu1.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    menu2.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    menu3.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
            menu.ShowAt(sender);
        }

        private async void MenuMove_Click(object sender, RoutedEventArgs e)
        {
            var status = (sender as MenuFlyoutItem).Tag.ToInt32();
            var info = (sender as MenuFlyoutItem).DataContext as FollowSeasonInfo;

            var tab = tabs.FirstOrDefault(x => x.id == info._status);
            var tab_move = tabs.FirstOrDefault(x => x.id == status);
            var result = await tab.seasonFollow.MoveStatus(info.season_id, status);
            if (result.success)
            {
                tab_move.seasonFollow.FollowList.Add(info);
                tab.seasonFollow.FollowList.Remove(info);
            }
            else
            {
                Utils.ShowMessageToast(result.message);
            }
        }
    }

    public class SeasonTabItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public SeasonFollow seasonFollow { get; set; }
    }

}
