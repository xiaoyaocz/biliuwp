using BiliBili3.Models;
using Newtonsoft.Json;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili3.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MovieCollectPage : Page
    {
        public MovieCollectPage()
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


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;
            }
            if (e.NavigationMode == NavigationMode.New && list.Items.Count == 0)
            {
                await Task.Delay(200);
                _page = 1;
                LoadMy();
            }
        }

        int _page = 1;
        bool _loading = false;
        private async void LoadMy()
        {
            try
            {
                if (_page == 1)
                {
                    list.Items.Clear();

                }
                _loading = true;
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Format("https://bangumi.bilibili.com/follow/api/list/mine?access_key={0}&appkey={1}&build=5250000&mobi_app=android&page={2}&pagesize=20&platform=android&ts={3}",ApiHelper.access_key,ApiHelper._appKey, _page,ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                MyMovieModel m = JsonConvert.DeserializeObject<MyMovieModel>(results);
                if (m.code == 0)
                {
                    if (m.result.Count == 0)
                    {
                        Utils.ShowMessageToast("加载完了...", 3000);
                        return;
                    }
                    m.result.ForEach(x => list.Items.Add(x));
                    // list_ban_mine.ItemsSource = m.result;
                    _page++;
                }
                else
                {
                    Utils.ShowMessageToast(m.message, 3000);
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867 || ex.HResult == -2147012889)
                {
                    Utils.ShowMessageToast("无法连接服务器，请检查你的网络连接", 3000);
                }
                else
                {

                    Utils.ShowMessageToast("读取追番失败了", 3000);
                }
            }
            finally
            {
                _loading = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_loading)
                {

                    LoadMy();
                }
            }
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as MyMovieModel).season_id.ToString());
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            _page = 1;
            LoadMy();
        }
    }


    public class MyMovieModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public string count { get; set; }

        public List<MyMovieModel> result { get; set; }

        public int season_id { get; set; }

        private string _cover;
        public string cover
        {
            get { return _cover + "@300w.jpg"; }
            set { _cover = value; }
        }

        public string new_ep_desc { get; set; }
        public string newest_ep_index { get; set; }
        public string progress { get; set; }
        public string season_type_name { get; set; }
        public string title { get; set; }

    }


}
