using BiliBili.UWP.Helper;
using BiliBili.UWP.Pages.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicianPage : Page
    {
        public MusicianPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        string _mid = "";

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New)
            {
                _mid = (e.Parameter as object[])[0].ToString();
               // Utils.ShowMessageToast((e.Parameter as object[])[0].ToString());
           
                ls_songs.ItemsSource = null;
                LoadUpInfo();
            }
           
        }
        private async void LoadUpInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = "https://api.bilibili.com/audio/music-service-c/users/upinfo?access_key={0}&appkey={1}&build=5250000&mid={2}&mobi_app=android&platform=android&ts={3}&upmid={4}";
                url = string.Format(url, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey,ApiHelper.GetUserId(),ApiHelper.GetTimeSpan,_mid);
                url += "&sign=" + ApiHelper.GetSign(url);

                var results=await WebClientClass.GetResults(new Uri(url));

                MusicianInfoModel m =JsonConvert.DeserializeObject<MusicianInfoModel>(results);
                if (m.code==0)
                {
                    this.DataContext = m.data;
                    if (m.data.followed)
                    {
                        btn_FollowUser.Visibility = Visibility.Collapsed;
                        btn_CancelFollowUser.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btn_FollowUser.Visibility = Visibility.Visible;
                        btn_CancelFollowUser.Visibility = Visibility.Collapsed;
                    }
                    ls_songs.ItemsSource =await GetSongs();

                }
                else
                {
                    Utils.ShowMessageToast(m.msg);
                }
            }
            catch (Exception ex)
            {

                Utils.ShowMessageToast("加载信息失败"+ex.HResult);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<List<MusicHomeSongModel>> GetSongs()
        {
            try
            {
                string url = "https://api.bilibili.com/audio/music-service-c/songs/getupsongslist?appkey={0}&build=5250000&isAll=true&mid={1}&mobi_app=android&pageIndex=1&pageSize=20&platform=android&sortBy=0&sortType=0&ts={2}";
                url = string.Format(url, ApiHelper.AndroidKey.Appkey, _mid,ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));
                JObject obj = JObject.Parse(results);
                if (obj["code"].ToInt32()==0)
                {
                    List<MusicHomeSongModel> m = JsonConvert.DeserializeObject<List<MusicHomeSongModel>>(obj["data"]["list"].ToString());

                    return m;

                }
                else
                {
                    Utils.ShowMessageToast("无法读取歌曲列表"+ obj["msg"].ToString());
                    return new List<MusicHomeSongModel>();
                }


            }
            catch (Exception)
            {

                Utils.ShowMessageToast("无法读取歌曲列表");
                return new List<MusicHomeSongModel>();
            }
        }



        private void ls_songs_ItemClick(object sender, ItemClickEventArgs e)
        {
           var item=  e.ClickedItem as MusicHomeSongModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage),item.id);
        }


        private async void btn_FollowUser_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/relation/modify");

                    string content = string.Format(
                        "access_key={0}&act=1&appkey={1}&build=45000&fid={2}&mobi_app=android&platform=android&re_src=90&ts={3}",
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _mid, ApiHelper.GetTimeSpan_2
                        );
                    content += "&sign=" + ApiHelper.GetSign(content);
                    string result = await WebClientClass.PostResults(ReUri,
                        content
                     );
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        Utils.ShowMessageToast("关注成功", 3000);
                        btn_FollowUser.Visibility = Visibility.Collapsed;
                        btn_CancelFollowUser.Visibility = Visibility.Visible;

                        MessageCenter.SendLogined();
                    }
                    else
                    {
                        Utils.ShowMessageToast("关注失败\r\n" + json["message"].ToString(), 3000);

                    }

                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("关注时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);
            }
        }

        private async void btn_CancelFollowUser_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/relation/modify");

                    string content = string.Format(
                        "access_key={0}&act=2&appkey={1}&build=45000&fid={2}&mobi_app=android&platform=android&re_src=90&ts={3}",
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _mid, ApiHelper.GetTimeSpan_2
                        );
                    content += "&sign=" + ApiHelper.GetSign(content);
                    string result = await WebClientClass.PostResults(ReUri,
                        content
                     );
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        Utils.ShowMessageToast("已取消关注", 3000);
                        btn_FollowUser.Visibility = Visibility.Visible;
                        btn_CancelFollowUser.Visibility = Visibility.Collapsed;


                        MessageCenter.SendLogined();
                    }
                    else
                    {
                        Utils.ShowMessageToast("取消关注失败\r\n" + json["message"].ToString(), 3000);

                    }

                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("关注时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);
            }
        }

        private void btn_OpenUser_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), _mid);
        }

        private async void btn_PlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (ls_songs.ItemsSource == null || ls_songs.Items.Count == 0)
            {
                return;
            }
            Utils.ShowMessageToast("开始读取播放地址");
            foreach (MusicHomeSongModel item in ls_songs.Items)
            {

                if (MusicHelper.playList.Find(x => x.songid == item.id.ToString()) == null)
                {
                    var m = await MusicHelper.GetMusicUri(item.id.ToString());
                    if (m != null)
                    {
                        MusicHelper.AddToPlay(new MusicPlayModel()
                        {
                            url = m,
                            artist = item.author,
                            pic = item.cover_url,
                            songid = item.id.ToString(),
                            title = item.title
                        });
                    }
                    else
                    {
                        await new MessageDialog("无法读取歌曲:" + item.title + " 的播放地址").ShowAsync();
                    }
                }
            }

            Utils.ShowMessageToast("已添加到播放列表");

        }
    }

    public class MusicianInfoModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public MusicianInfoModel data { get; set; }
        public int mid { get; set; }
        public string uName { get; set; }
        public string avater { get; set; }
        public int playingCounts { get; set; }
        public int followings { get; set; }
        public string Fans
        {
            get
            {
                if (followings >= 10000)
                {
                    return ((double)followings / 10000).ToString("0.0") + "万";
                }
                return followings.ToString();
            }
        }
        public string Plays
        {
            get
            {
                if (playingCounts >= 10000)
                {
                    return ((double)playingCounts / 10000).ToString("0.0") + "万";
                }
                return playingCounts.ToString();
            }
        }
        public int totalSongs { get; set; }



        public string face { get; set; }
        public bool followed { get; set; }



    }


}
