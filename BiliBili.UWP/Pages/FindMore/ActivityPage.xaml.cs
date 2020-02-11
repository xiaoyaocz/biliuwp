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
    public sealed partial class ActivityPage : Page
    {
        public ActivityPage()
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
                GetTopic();
            }
        }

        bool IsLoading = true;
        int page = 1;
        private async void GetTopic()
        {
            try
            {
                IsLoading = true;
                btn_More_Video.Visibility = Visibility.Collapsed;
                pr_Load.Visibility = Visibility.Visible;
                string url = string.Format("http://api.bilibili.com/event/getlist?appkey={0}&build=422000&mobi_app=android&page={1}&pagesize=20&platform=android&ts={2}", ApiHelper.AndroidKey.Appkey, page, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                ActivityModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<ActivityModel>(results);

                m.list.ForEach(x =>
                {
                    if (x.link.Length != 0)
                    {
                        grid_View.Items.Add(x);
                    }
                }
                );
                page++;
                //grid_View.ItemsSource = m.list;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取失败了", 2000);
                //throw;
            }
            finally
            {
                IsLoading = false;
                btn_More_Video.Visibility = Visibility.Visible;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_More_Video_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoading)
            {
                GetTopic();
            }
        }

        private void list_Topic_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Regex.IsMatch(((ActivityModel)e.ClickedItem).link, "/video/av(.*)?[/|+](.*)?"))
            {
                string a = Regex.Match(((ActivityModel)e.ClickedItem).link, "/video/av(.*)?[/|+](.*)?").Groups[1].Value;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), a);
            }
            else
            {
                if (Regex.IsMatch(((ActivityModel)e.ClickedItem).link, @"live.bilibili.com/(.*?)"))
                {
                    string a = Regex.Match(((ActivityModel)e.ClickedItem).link + "a", "live.bilibili.com/(.*?)a").Groups[1].Value;
                    // livePlayVideo(a);
                }
                else
                {
                    this.Frame.Navigate(typeof(WebPage), new object[] { ((ActivityModel)e.ClickedItem).link });
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int i = Convert.ToInt32(this.ActualWidth / 400);
            bor_Width.Width = this.ActualWidth / i - 12;
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!IsLoading)
                {
                    GetTopic();
                }
            }
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            page = 1;
            GetTopic();
        }
    }
    public class ActivityModel
    {

        public int code { get; set; }
        public List<ActivityModel> list { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public string link { get; set; }
        public int state { get; set; }
        public Visibility state_0
        {
            get
            {
                if (state == 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public Visibility state_1
        {
            get
            {
                if (state == 1)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
    }
}
