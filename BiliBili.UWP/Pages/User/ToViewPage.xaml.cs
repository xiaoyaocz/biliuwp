using BiliBili.UWP.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static BiliBili.UWP.Modules.ToView;


// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ToViewPage : Page
    {
        public ToViewPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        Modules.ToView toView;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                if (toView == null)
                {
                    toView = new Modules.ToView();
                }
                //延迟200毫秒，直接加载会导致跳转卡顿
                await Task.Delay(200);
                LoadList();
            }
        }


        private async void LoadList()
        {
            pr_Load.Visibility = Visibility.Visible;
            var data = await toView.GetToViewList();
            if (data.success)
            {

                var list = data.data.ToList();

                list_Videos.ItemsSource = list;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            pr_Load.Visibility = Visibility.Collapsed;
        }

        private void list_Videos_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectitem = e.ClickedItem as ToViewsModel;
            var videos = list_Videos.ItemsSource as List<ToViewsModel>;


            List<PlayerModel> ls = new List<PlayerModel>();
            int i = 0;
            foreach (var item in videos)
            {

                if (item.bangumi==null)
                {
                    foreach (var item1 in item.pages)
                    {
                        ls.Add(new PlayerModel()
                        {
                            Aid = item.aid,
                            ImageSrc = item.pic,
                            index = i,
                            Mid = item1.cid.ToString(),
                            Mode = PlayMode.Video,
                            Title = item.title,
                            VideoTitle = $"{item.title}\r\nP{item1.page + "  " + item1.part}"
                        });
                        i++;
                    }
                }
                else
                {
                    //番剧
                    foreach (var item1 in item.pages)
                    {
                        ls.Add(new PlayerModel()
                        {
                            Aid = item.aid,
                            ImageSrc = item.pic,
                            index = i,
                            Mid = item1.cid.ToString(),
                            Mode = PlayMode.Bangumi,
                            Title = item.bangumi.season.title,
                            banId= item.bangumi.season.season_id.ToString(),
                            episode_id=item.bangumi.ep_id.ToString(),
                            playMode= PlayMode.Bangumi,
                            VideoTitle = $"{item.bangumi.season.title}\r\nP{item.bangumi.title + "  " + item.bangumi.long_title}"
                        });
                        i++;
                    }
                }

               
            }
            var index = 0;
            if (selectitem.cid != 0)
            {
                index = ls.IndexOf(ls.Find(x => x.Mid == selectitem.cid.ToString()));
            }
            else
            {
                index = ls.IndexOf(ls.Find(x => x.Mid == selectitem.pages[0].cid.ToString()));
            }
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, index });
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadList();
        }

        private async void btn_ClearPlayed_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog("确定要清除已观看?");
            messageDialog.Commands.Add(new UICommand("确定", async (ee) =>
            {
                var data = await toView.ClearPlayed();
                if (data.success)
                {
                    Utils.ShowMessageToast("已清除已观看视频");
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }
                LoadList();

            }));
            messageDialog.Commands.Add(new UICommand("取消"));
            await messageDialog.ShowAsync();
        }

        private async void btn_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog("确定要清除所有?"); messageDialog.Commands.Add(new UICommand("确定", async (ee) =>
            {
                var data = await toView.ClearToView();
                if (data.success)
                {
                    Utils.ShowMessageToast("已清除所有");
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }
                LoadList();
            }));
            messageDialog.Commands.Add(new UICommand("取消"));
            await messageDialog.ShowAsync();
        }

        private async void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as ToViewsModel;
            var data = await toView.DeleteToView(item.aid);
            if (data.success)
            {
                Utils.ShowMessageToast("已删除");
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            LoadList();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            menu.ShowAt((FrameworkElement)sender);
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            menu.ShowAt((FrameworkElement)sender);
        }

        private void btn_Sort_Click(object sender, RoutedEventArgs e)
        {
            var videos = list_Videos.ItemsSource as List<ToViewsModel>;
            if (sc.ScaleY==-1)
            {
                sc.ScaleY = 1;
                list_Videos.ItemsSource = videos.OrderBy(x=>x.add_at).ToList();
            }
            else
            {
                sc.ScaleY = -1;
                list_Videos.ItemsSource = videos.OrderByDescending(x => x.add_at).ToList();
            }
        }
    }
}
