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
using Newtonsoft.Json.Linq;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using System.Collections.ObjectModel;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LivePartInfoPage : Page
    {
        LiveArea liveArea;
        public LivePartInfoPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            liveArea = new LiveArea();
        }
        int parent_area_id, area_id;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New && gv.ItemsSource == null)
            {
                parent_area_id = (e.Parameter as object[])[0].ToInt32();
                area_id = (e.Parameter as object[])[1].ToInt32();
                top_txt_Header.Text = (e.Parameter as object[])[2].ToString();
                _page = 1;
                await GetData();
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.Back)
            {
                gv.ItemsSource = null;
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);

        }
        int _page = 1;
        bool _Loading = true;
        private async Task GetData()
        {
            _Loading = true;
            pr_Load.Visibility = Visibility.Visible;
            var sort = "online";
            if (grid_tag.SelectedItem != null)
            {
                sort = (grid_tag.SelectedItem as new_tags).sort_type;
            }
            var data = await liveArea.GetRoomList(area_id, parent_area_id, _page, sort);
            if (data.success)
            {
                if (_page==1)
                {
                    gv.ItemsSource = data.data.list;
                    if (grid_tag.ItemsSource==null)
                    {
                        grid_tag.ItemsSource = data.data.new_tags;
                        grid_tag.SelectedIndex = 0;
                    }
                }
                else
                {
                    var list = gv.ItemsSource as ObservableCollection<RoomListItem>;
                    foreach (var item in data.data.list)
                    {
                        list.Add(item);
                    }
                }
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            _Loading = false;
            pr_Load.Visibility = Visibility.Collapsed;
        }
        private async void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset >= sv.ScrollableHeight - 200)
            {
                if (!_Loading)
                {
                    _page++;
                    await GetData();
                }
            }
        }

        private async void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!_Loading)
            {
                _page++;
                await GetData();
            }
        }

        private void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as RoomListItem).roomid);
        }

        private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (!_Loading)
            {
                _page = 1;
                await GetData();
            }
        }

        private void btn_Type_Checked(object sender, RoutedEventArgs e)
        {
            grid_tag.Visibility = Visibility.Visible;
        }

        private void btn_Type_Unchecked(object sender, RoutedEventArgs e)
        {
            grid_tag.Visibility = Visibility.Collapsed;
        }

        private async void grid_tag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Loading || grid_tag.ItemsSource == null)
            {
                return;
            }
            _page = 1;
            await GetData();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                //ViewBox_num.Width = ActualWidth / 2 - 18;
                //ViewBox2_num.Width = ActualWidth / 2 - 18;
                bor_Width2.Width = ActualWidth / 2 - 20;
                Grid.SetRow(com_bar, 2);

                com_bar.HorizontalAlignment = HorizontalAlignment.Stretch;
                com_bar.VerticalAlignment = VerticalAlignment.Bottom;

            }
            else
            {
                //int i = Convert.ToInt32(ActualWidth / 200);
                //ViewBox_num.Width = ActualWidth / i - 13;
                //ViewBox2_num.Width = ActualWidth / i - 13;
                int i = Convert.ToInt32(ActualWidth / 200);
                bor_Width2.Width = ActualWidth / i - 15;
                Grid.SetRow(com_bar, 0);
                Grid.SetRowSpan(com_bar, 2);
                com_bar.HorizontalAlignment = HorizontalAlignment.Right;
                com_bar.VerticalAlignment = VerticalAlignment.Top;
            }



        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }

    public class LivePartTagModel
    {
        public string tag_name { get; set; }
        public string tag_value { get; set; }
    }

}
