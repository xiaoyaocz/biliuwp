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
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media;
using BiliBili.UWP.Helper;
using Windows.UI.Popups;
using BiliBili.UWP.Pages.Music;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicInfoPage : Page
    {
        public MusicInfoPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        string _songId;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _songId = (e.Parameter as object[])[0].ToString();
            comment.InitializeComment(new Controls.LoadCommentInfo() {
                 commentMode= Controls.CommentMode.MusicSong,
                 conmmentSortMode= Controls.ConmmentSortMode.Hot,
                 oid= _songId
            });
            LoadMusic(_songId);
            comment.LoadComment();
        }

        private async void LoadMusic(string id)
        {
            try
            {
                string url = string.Format("https://api.bilibili.com/audio/music-service-c/songs/playing?access_key={0}&appkey={1}&build=5250000&mid={2}&mobi_app=android&platform=android&song_id={3}&ts={4}",
               ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetUserId(), id, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                string re = await WebClientClass.GetResults(new Uri(url));

                SongInfoModel obj = JsonConvert.DeserializeObject<SongInfoModel>(re);
                if (obj.code == 0)
                {
                    if (obj.data.mid == 0)
                    {
                        user.Visibility = Visibility.Collapsed;
                        intro.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        user.Visibility = Visibility.Visible;
                        intro.Visibility = Visibility.Visible;
                    }
                    if (obj.data.pgc_info!=null)
                    {
                        btn_OpenPcMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btn_OpenPcMenu.Visibility = Visibility.Collapsed;
                    }
                    this.DataContext = obj.data;
                   
                    //if (obj.data.is_collect==0)
                    //{
                    //    btn_Collect.Visibility = Visibility.Visible;
                    //    btn_CancelCollect.Visibility = Visibility.Collapsed;
                    //}
                    //else
                    //{
                    //    btn_Collect.Visibility = Visibility.Collapsed;
                    //    btn_CancelCollect.Visibility = Visibility.Visible;
                    //}

                    if (obj.data.up_is_follow==0)
                    {
                        btn_FollowUser.Visibility = Visibility.Visible;
                        btn_CancelFollowUser.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btn_FollowUser.Visibility = Visibility.Collapsed;
                        btn_CancelFollowUser.Visibility = Visibility.Visible;
                    }


                    if (MusicHelper.playList.Find(x=>x.songid== _songId)==null)
                    {
                        var m = await MusicHelper.GetMusicUri(_songId);
                        if (m != null)
                        {

                            MusicHelper.AddToPlay(new MusicPlayModel()
                            {
                                url = m,
                                artist = obj.data.author,
                                pic = obj.data.cover_url,
                                songid = id,
                                title = obj.data.title
                            });
                        }
                        else
                        {
                            await new MessageDialog("无法读取歌曲播放地址").ShowAsync();
                        }
                        Utils.ShowMessageToast("已经添加到播放列表");
                    }
                }
                else
                {
                    Utils.ShowMessageToast(obj.msg);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取歌曲信息错误");
            }





        }

       
        private async void LoadCollections()
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                btn_Collect.Flyout.Hide();
                return;
            }
            try
            {
                pr_loadCollect.Visibility = Visibility.Visible;
                string url = "https://api.bilibili.com/audio/music-service-c/collections?access_key={0}&appkey={1}&build=5250000&mid={2}&mobi_app=android&page_index=1&page_size=1000&platform=android&sort=1&ts={3}";
                url = string.Format(url,ApiHelper.access_key,ApiHelper.AndroidKey.Appkey,ApiHelper.GetUserId(),ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));

                SongCollectionsModel m = JsonConvert.DeserializeObject<SongCollectionsModel>(results);
                if (m.code==0)
                {
                    ls_collections.ItemsSource = m.data.list;
                }
                else
                {
                    Utils.ShowMessageToast(m.msg);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载收藏夹失败");
            }
            finally
            {
                pr_loadCollect.Visibility = Visibility.Collapsed;
            }
        }



        private void btn_ShowComment_Click(object sender, RoutedEventArgs e)
        {
            comment.ShowCommentBox();
        }

        private async void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext==null)
            {
                return;
            }
           

           
            var m = await MusicHelper.GetMusicUri(_songId);
            if (m != null)
            {
                MusicHelper.AddToPlay(new MusicPlayModel()
                {
                    url = m
                });
            }
            else
            {
                await new MessageDialog("无法读取歌曲播放地址").ShowAsync();
            }
            Utils.ShowMessageToast("已经添加到播放列表");
        }

        private async void btn_Collect_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            LoadCollections();
        }

        private void btn_CancelCollect_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btn_Download_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("暂不支持");
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (comment.HasMore)
                {
                    comment.LoadMore();
                }
            }
        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard("https://m.bilibili.com/audio/au" + _songId);

        }

        private void btn_OpenUser_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicianPage),( this.DataContext as SongInfoModel).up_mid);
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
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, (this.DataContext as SongInfoModel).up_mid, ApiHelper.GetTimeSpan_2
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
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, (this.DataContext as SongInfoModel).up_mid, ApiHelper.GetTimeSpan_2
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

        private async void btn_CreateCollect_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin()&&!await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            await cd_Create.ShowAsync();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (txt_title.Text.Trim().Length==0)
            {
                Utils.ShowMessageToast("检查你的输入");
               
                return;
            }
            CreateCollectFolder();
        }
        private async void CreateCollectFolder()
        {
            try
            {
                var isOpen = 0;
                if (cb_isopen.IsChecked.Value)
                {
                    isOpen = 1;
                }
                string url = "https://api.bilibili.com/audio/music-service-c/collections";
                string content = string.Format("access_key={0}&appkey={1}&build=5250000&is_open={5}&mid={2}&mobi_app=android&platform=android&title={3}&ts={4}",ApiHelper.access_key,ApiHelper.AndroidKey.Appkey,ApiHelper.GetUserId(),txt_title.Text,ApiHelper.GetTimeSpan, isOpen);
                content += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32()==0)
                {
                    cd_Create.Hide();
                    LoadCollections();
                }
                else
                {
                    Utils.ShowMessageToast("创建收藏夹失败");
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("创建失败");
            }
        }

        private async void ls_collections_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SongCollectionsModel;
            try
            {
               
                string url = "https://api.bilibili.com/audio/music-service-c/collections/songs/"+_songId;
                string content = string.Format("access_key={0}&appkey={1}&build=5250000&collection_id_list={2}&mid={3}&mobi_app=android&platform=android&song_id={4}&ts={5}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey,item.collection_id, ApiHelper.GetUserId(), _songId, ApiHelper.GetTimeSpan);
                content += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    btn_Collect.Flyout.Hide();
                    Utils.ShowMessageToast("收藏成功");
                }
                else
                {
                    Utils.ShowMessageToast(obj["msg"].ToString());
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("收藏失败");
            }

        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCollections();


            var ls= ls_collections.ItemsSource as List<SongCollectionsModel>;
            if (ls==null)
            {
                Utils.ShowMessageToast("先读取收藏夹！！");
                return;
            }

            foreach (var item in ls)
            {
                
                try
                {

                    string url = "https://api.bilibili.com/audio/music-service-c/collections/"+item.collection_id+"/del" ;
                    string content = string.Format("access_key={0}&appkey={1}&build=5250000&collection={2}&mid={3}&mobi_app=android&platform=android&ts={4}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, item.collection_id, ApiHelper.GetUserId(), ApiHelper.GetTimeSpan);
                    content += "&sign=" + ApiHelper.GetSign(url);
                    var re = await WebClientClass.PostResults(new Uri(url), content);
                    JObject obj = JObject.Parse(re);
                    if (obj["code"].ToInt32() == 0)
                    {
                        Utils.ShowMessageToast("删除"+item.collection_id);
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj["msg"].ToString());
                    }
                }
                catch (Exception)
                {

                    Utils.ShowMessageToast("删除失败");
                }
            }

        }

        private void btn_OpenPcMenu_Click(object sender, RoutedEventArgs e)
        {
            var data= this.DataContext as SongInfoModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage), data.pgc_info.pgc_menu);
        }
    }

    public class SongInfoModel
    {
        public int code { get; set; }
        public string msg { get; set; }

        public SongInfoModel data { get; set; }

        public List<MusicHomeSongModel> list { get; set; }


        public int id { get; set; }
        public int up_mid { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public int mid { get; set; }
        public string cover_url { get; set; }
        public string intro { get; set; }
        public string  ctime_str { get; set; }
        public string up_name { get; set; }
        public string author { get; set; }

        public string up_img { get; set; }
        public int up_is_follow { get; set; }
        public int is_collect { get; set; }
        public int play_count { get; set; }
        public string palyNum_str
        {
            get
            {
                if (play_count >= 10000)
                {
                    return ((double)play_count / 10000).ToString("0.0") + "万";
                }
                return play_count.ToString();
            }
        }


        public int collect_count { get; set; }
        public string collect_str
        {
            get
            {
                if (collect_count >= 10000)
                {
                    return ((double)collect_count / 10000).ToString("0.0") + "万";
                }
                return collect_count.ToString();
            }
        }

        public int snum { get; set; }
        public string snum_str
        {
            get
            {
                if (snum >= 10000)
                {
                    return ((double)snum / 10000).ToString("0.0") + "万";
                }
                return snum.ToString();
            }
        }

        public Visibility vip
        {
            get
            {
                if (up_name!=null&& up_name== "付费音乐")
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }



        public SongInfoModel pgc_info { get; set; }
        public SongInfoModel pgc_menu { get; set; }
        public int menuId { get; set; }
        public string coverUrl { get; set; }
        public string mbnames { get; set; }
       


    }

    public class SongCollectionsModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public SongCollectionsModel data { get; set; }

        public List<SongCollectionsModel> list { get; set; }


        public long id { get; set; }

        public long mid { get; set; }
        public long collection_id { get; set; }
        public string title { get; set; }
        public string img_url { get; set; }
        public int records_num { get; set; }
        public int is_open { get; set; }

        public string open
        {
            get
            {
                if (is_open==1)
                {
                    return "公开";
                }
                else
                {
                    return "不公开";
                }
            }
        }

    }

}
