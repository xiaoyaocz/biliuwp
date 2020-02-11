using BiliBili.UWP.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class MusicMenuPage : Page
    {
        public MusicMenuPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
       }
        string _menuId = "";
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            _menuId = (e.Parameter as object[])[0].ToString();
            comment.InitializeComment(new Controls.LoadCommentInfo()
            {
                commentMode = Controls.CommentMode.MusicMenu,
                conmmentSortMode = Controls.ConmmentSortMode.Hot,
                oid = _menuId
            });
            GetMenuInfo();
            
            comment.LoadComment();
        }

        private async void GetMenuInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url =string.Format( "https://api.bilibili.com/audio/music-service-c/menus/{3}?appkey={0}&build=5250000&mid={1}&mobi_app=android&platform=android&ts={2}",ApiHelper.AndroidKey.Appkey,ApiHelper.GetUserId(),ApiHelper.GetTimeSpan, _menuId);
                url += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.GetResults(new Uri(url));
                MusicMenuModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<MusicMenuModel>(re);
                if (m.code==0)
                {
                    if (m.data.menusRespones.collected==0)
                    {
                        btn_Collect.Visibility = Visibility.Visible;
                        btn_CancelCollect.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btn_Collect.Visibility = Visibility.Collapsed;
                        btn_CancelCollect.Visibility = Visibility.Visible;
                    }
                    this.DataContext = m.data.menusRespones;
                    list_songs.ItemsSource = m.data.songsList;
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("无法读取此歌单信息");
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
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

        private void list_songs_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicHomeSongModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), item.id);
        }

        private void btn_Collect_Click(object sender, RoutedEventArgs e)
        {
            doCollect("add");
        }

        private void btn_CancelCollect_Click(object sender, RoutedEventArgs e)
        {
            doCollect("del");
        }

        private async void doCollect(string mode)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
            }
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = string.Format("https://api.bilibili.com/audio/music-service-c/menucollect/{0}?access_key={1}&appkey={2}&build=5250000&menuId={3}&mid={4}&platform=android&ts={5}",
                    mode,ApiHelper.access_key,ApiHelper.AndroidKey.Appkey, _menuId, ApiHelper.GetUserId(), ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.GetResults(new Uri(url));
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    if (mode=="add")
                    {
                        btn_CancelCollect.Visibility = Visibility.Visible;
                        btn_Collect.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btn_CancelCollect.Visibility = Visibility.Collapsed;
                        btn_Collect.Visibility = Visibility.Visible;
                    }
                    Utils.ShowMessageToast("操作成功");
                }
                else
                {
                    Utils.ShowMessageToast("操作失败");
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("操作失败");
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }

        }

        private void btn_Download_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("暂不支持");
        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard("https://m.bilibili.com/audio/am" + _menuId);
            Utils.ShowMessageToast("已复制到剪切板");
        }

        private void btn_ShowComment_Click(object sender, RoutedEventArgs e)
        {
            comment.ShowCommentBox();
        }

        private async void btn_PlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (list_songs.ItemsSource==null|| list_songs.Items.Count==0)
            {
                return;
            }
            Utils.ShowMessageToast("开始读取播放地址");
            foreach (MusicHomeSongModel item in list_songs.Items)
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


    public class MusicMenuModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public MusicMenuModel data { get; set; }


        public MusicHomeMenuModel menusRespones { get; set; }

        public List<MusicHomeSongModel> songsList { get; set; }
    }

}
