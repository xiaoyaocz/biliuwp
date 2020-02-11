using BiliBili.UWP.Helper;
using Newtonsoft.Json;
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
    public sealed partial class MusicCollectPage : Page
    {
        public MusicCollectPage()
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
                LoadCollections();
            }

        }

        private async void LoadCollections()
        {
          
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = "https://api.bilibili.com/audio/music-service-c/collections?access_key={0}&appkey={1}&build=5250000&mid={2}&mobi_app=android&page_index=1&page_size=1000&platform=android&sort=1&ts={3}";
                url = string.Format(url, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetUserId(), ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));

                SongCollectionsModel m = JsonConvert.DeserializeObject<SongCollectionsModel>(results);
                if (m.code == 0)
                {
                    cb_favbox.ItemsSource = m.data.list;
                    cb_favbox.SelectedIndex = 0;
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
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadSongs(string collectId)
        {

            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = "https://api.bilibili.com/audio/music-service-c/collections/{0}/songs?access_key={1}&appkey={2}&build=5250000&collection_id={0}&mid={3}&mobi_app=android&page_index=1&page_size=500&platform=android&sort=1&ts={4}";
                url = string.Format(url, collectId, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetUserId(), ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));

                SongInfoModel m = JsonConvert.DeserializeObject<SongInfoModel>(results);
                if (m.code == 0)
                {
                    list_songs.ItemsSource = m.data.list;
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
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void cb_favbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_favbox.SelectedIndex==-1)
            {
                return;
            }
           
            LoadSongs((cb_favbox.SelectedItem as SongCollectionsModel).collection_id.ToString());
        }

        private void list_songs_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage),(e.ClickedItem as MusicHomeSongModel).id);
        }

        private async void btn_PlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (list_songs.ItemsSource == null || list_songs.Items.Count == 0)
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

        private void btn_selete_Checked(object sender, RoutedEventArgs e)
        {
            list_songs.SelectionMode = ListViewSelectionMode.Multiple;
            list_songs.IsItemClickEnabled = false;
        }

        private void btn_selete_Unchecked(object sender, RoutedEventArgs e)
        {
            list_songs.SelectionMode = ListViewSelectionMode.None;
            list_songs.IsItemClickEnabled = true;
        }

        private async void btn_playSelect_Click(object sender, RoutedEventArgs e)
        {
            if (list_songs.ItemsSource == null || list_songs.Items.Count == 0||list_songs.SelectedItems.Count==0)
            {
                Utils.ShowMessageToast("没有选中任何内容");
                return;
            }

            Utils.ShowMessageToast("开始读取播放地址");
            foreach (MusicHomeSongModel item in list_songs.SelectedItems)
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

        private async void btn_DeleteSelect_Click(object sender, RoutedEventArgs e)
        {
            if (list_songs.ItemsSource == null || list_songs.Items.Count == 0 || list_songs.SelectedItems.Count == 0)
            {
                Utils.ShowMessageToast("没有选中任何内容");
                return;
            }
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
            }
            DeleteCollect();
        }

        private async void DeleteCollect()
        {
            try
            {
                var cid = (cb_favbox.SelectedItem as SongCollectionsModel).collection_id.ToString();
                var songs = "";
                foreach (MusicHomeSongModel item in list_songs.SelectedItems)
                {
                    songs += item.id + ",";
                }
                string url = "https://api.bilibili.com/audio/music-service-c/collections/collectionfresh";
                string content = string.Format("access_key={0}&appkey={1}&build=5250000&collectionId={2}&mid={3}&mobi_app=android&platform=android&songIds={4}&ts={5}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, cid, ApiHelper.GetUserId(),Uri.EscapeDataString(songs), ApiHelper.GetTimeSpan);
                content += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    LoadSongs(cid);
                }
                else
                {
                    Utils.ShowMessageToast(obj["msg"].ToString());
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("删除收藏失败");
            }
        }





    }
}
