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
    public sealed partial class TopicPage : Page
    {
        public TopicPage()
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
                string url = string.Format("http://api.bilibili.com/topic/getlist?access_key={0}&appkey={1}&build=424000&mobi_app=android&page={2}&pagesize=20&platform=android&ts={3}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, page, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                TopicModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<TopicModel>(results);

                m.list.ForEach(x =>
                {
                    if (x.link.Length != 0)
                    {
                        if (!x.cover.Contains("http:"))
                        {
                            x.cover = "http:" + x.cover;
                        }
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
            if (Regex.IsMatch(((TopicModel)e.ClickedItem).link, "/video/av(.*)?[/|+](.*)?"))
            {
                string a = Regex.Match(((TopicModel)e.ClickedItem).link, "/video/av(.*)?[/|+](.*)?").Groups[1].Value;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), a);
            }
            else
            {
                if (Regex.IsMatch(((TopicModel)e.ClickedItem).link, @"live.bilibili.com/(.*?)"))
                {
                    string a = Regex.Match(((TopicModel)e.ClickedItem).link + "a", "live.bilibili.com/(.*?)a").Groups[1].Value;
                    // livePlayVideo(a);
                }
                else
                {
                    this.Frame.Navigate(typeof(WebPage), new object[] { ((TopicModel)e.ClickedItem).link});
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

    public class TopicModel
    {
        public int code { get; set; }
        public List<TopicModel> list { get; set; }
        public string title { get; set; }
        private string _cover;
        public string cover {
            get {
                if (_cover.Length==0)
                {
                    return "ms-appx:///Assets/Logo/PI900_300.png";
                }
                else
                {
                    return _cover;
                }
            }
            set { _cover = value; } }
        public string link { get; set; }



    }

}
