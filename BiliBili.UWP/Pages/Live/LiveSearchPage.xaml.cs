using BiliBili.UWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    public sealed partial class LiveSearchPage : Page
    {
        public LiveSearchPage()
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
            txt_hea_0.Text = "正在直播";
            txt_hea_1.Text = "直播";
            _keyword = "";
            _page_room = 1;
            _page_user = 1;
            search.Visibility = Visibility.Visible;
            list_Feed.Items.Clear();
            gv_Room.Items.Clear();
        }
        int _page_room = 1;
        bool _loadRoom = false;
        bool _loadUser = false;
        int _page_user = 1;
        string _keyword = "";
        private async void Search()
        {
            try
            {
                search.Visibility = Visibility.Collapsed;
                _loadRoom = true;
                _loadUser = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("https://api.bilibili.com/x/web-interface/search/type?search_type=live&highlight=0&keyword={0}&page=1",
                    Uri.EscapeDataString(_keyword));
                // url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                JObject jObject = JObject.Parse(results);
                LiveSearchModel m = JsonConvert.DeserializeObject<LiveSearchModel>(results);
                if (m.code == 0)
                {
                    if (Convert.ToInt32(jObject["data"]["pageinfo"]["live_user"]["total"].ToString()) != 0)
                    {
                        txt_hea_1.Text = "主播(" + jObject["data"]["pageinfo"]["live_user"]["total"].ToString() + ")";
                        JsonConvert.DeserializeObject<List<LiveSearchModel>>(jObject["data"]["result"]["live_user"].ToString()).ForEach(x => list_Feed.Items.Add(x));
                        //_page_user++;
                    }
                    if (Convert.ToInt32(jObject["data"]["pageinfo"]["live_room"]["total"].ToString()) != 0)
                    {
                        txt_hea_0.Text = "正在直播(" + jObject["data"]["pageinfo"]["live_room"]["total"].ToString() + ")";
                        JsonConvert.DeserializeObject<List<LiveSearchModel>>(jObject["data"]["result"]["live_room"].ToString()).ForEach(x => gv_Room.Items.Add(x));
                        _page_room++;
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
                _loadRoom = false;
                _loadUser = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
        private async void AddUser()
        {
            try
            {
                _loadUser = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("https://api.bilibili.com/x/web-interface/search/type?search_type=live_user&highlight=0&keyword={0}&page={1}&order=online&coverType=user_cover", Uri.EscapeDataString(_keyword), _page_user);
                //url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                LiveSearchModel m = JsonConvert.DeserializeObject<LiveSearchModel>(results);
                JObject jObject = JObject.Parse(results);
                if (m.code == 0)
                {
                    List<LiveSearchModel> ls = JsonConvert.DeserializeObject<List<LiveSearchModel>>(jObject["data"]["result"].ToString());


                    if (ls.Count != 0)
                    {
                        ls.ForEach(x => list_Feed.Items.Add(x));
                        _page_user++;
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
                _loadUser = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }

        private async void AddRoom()
        {
            try
            {
                _loadRoom = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("https://api.bilibili.com/x/web-interface/search/type?search_type=live&highlight=0&keyword={0}&page={1}", Uri.EscapeDataString(_keyword), _page_room);
                //url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                LiveSearchModel m = JsonConvert.DeserializeObject<LiveSearchModel>(results);
                JObject jObject = JObject.Parse(results);
                if (m.code == 0)
                {
                    List<LiveSearchModel> ls = JsonConvert.DeserializeObject<List<LiveSearchModel>>(jObject["data"]["result"]["live_room"].ToString());


                    if (ls.Count != 0)
                    {
                        ls.ForEach(x => gv_Room.Items.Add(x));
                        _page_room++;
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
                _loadRoom = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_loadUser)
                {
                    AddUser();
                }
            }
        }

        private void list_Feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as LiveSearchModel).roomid);
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (list_Feed.Items.Count == 0)
            {

                return;
            }
            if (!_loadUser)
            {
                AddUser();
            }
        }

        private void autoSug_Box_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (autoSug_Box.Text.Length < 2)
            {
                Utils.ShowMessageToast("关键字至少需要2个", 3000);
                return;
            }
            txt_hea_0.Text = "正在直播";
            txt_hea_1.Text = "直播";
            _keyword = autoSug_Box.Text;
            _page_room = 1;
            _page_user = 1;
            list_Feed.Items.Clear();
            gv_Room.Items.Clear();
            Search(); 
            search.Visibility = Visibility.Collapsed;
            //AddRoom();
            //AddUser();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                bor_Width2.Width = ActualWidth / 2 - 20;
            }
            else
            {
                int i = Convert.ToInt32(ActualWidth / 200);
                bor_Width2.Width = ActualWidth / i - 15;
            }

            int d = Convert.ToInt32(this.ActualWidth / 400);
            if (d > 3)
            {
                d = 3;
            }
            bor_Width.Width = this.ActualWidth / d - 22;
        }

        private void sv_room_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_room.VerticalOffset == sv_room.ScrollableHeight)
            {
                if (!_loadRoom)
                {
                    AddRoom();
                }
            }
        }

        private void btn_LoadMore_Room_Click(object sender, RoutedEventArgs e)
        {
            if (gv_Room.Items.Count == 0)
            {

                return;
            }

            if (!_loadRoom)
            {
                AddRoom();
            }
        }

        private void gv_Room_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as LiveSearchModel).roomid);
        }
    }
}
