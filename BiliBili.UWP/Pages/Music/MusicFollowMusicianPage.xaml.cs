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

namespace BiliBili.UWP.Pages.Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicFollowMusicianPage : Page
    {
        public MusicFollowMusicianPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New)
            {
                LoadMyFollow();
            }
        }

        private async void LoadMyFollow()
        {
            try
            {
                string url = "https://api.bilibili.com/audio/music-service-c/users/upmembers?access_key={0}&appkey={1}&build=5250000&mid={2}&mobi_app=android&page_index=1&page_size=1000&platform=android&ts={3}";
                url = string.Format(url,ApiHelper.access_key,ApiHelper.AndroidKey.Appkey,ApiHelper.GetUserId(),ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.GetResults(new Uri(url));
                FollowMusicianModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<FollowMusicianModel>(re);
                if (m.code==0)
                {
                    list_up.ItemsSource = m.data.list;
                }
                else
                {
                    Utils.ShowMessageToast(m.msg);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("读取失败");
            }
        }


        private void list_up_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item= e.ClickedItem as FollowMusicianModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicianPage),item.fid);
        }
    }

    public class FollowMusicianModel
    {
        public int code { get; set; }
        public string msg { get; set; }

        public FollowMusicianModel data { get; set; }

        public List<FollowMusicianModel> list { get; set; }

        public string uname { get; set; }
        public string face { get; set; }
        public string fid { get; set; }
    }

}
