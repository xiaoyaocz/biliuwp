using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class MusicAllSongPage : Page
    {
        public MusicAllSongPage()
        {
            this.InitializeComponent();
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();

        }
        MusicHomeSongTypeModel _data;
        subcateModel _selectCategroies;
        int _sort = 0;
        int _page = 1;
        bool loading = false;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New)
            {
                _data = (e.Parameter as object[])[0] as MusicHomeSongTypeModel;
                _selectCategroies = _data.categories.subcate[0];
                gv.ItemsSource = _data.categories.subcate;
                txt_Header.Text = _data.categories.cateTitle + " " + _selectCategroies.cateTitle;
                _sort = 0;
                _page = 1;
                LoadSongs();
            }
        }

        private async void LoadSongs()
        {
            try
            {
                loading = true;
                pr_load.Visibility = Visibility.Visible;
                btn_LoadMore.Visibility = Visibility.Collapsed;
                int cate1id = _selectCategroies.parentId;
                int cate2id = _selectCategroies.cateId;
               
                if (_page == 1)
                {
                    list_songs.ItemsSource = null;
                }
                string url =string.Format( "https://api.bilibili.com/audio/music-service-c/songs/getcatesongslist?access_key={0}&appkey={1}&build=5250000&cate1id={2}&cate2id={3}&mid={4}&mobi_app=android&pageIndex={5}&pageSize=20&platform=android&sortBy={6}&ts={7}",
                    ApiHelper.access_key,ApiHelper.AndroidKey.Appkey, cate1id, cate2id,ApiHelper.GetUserId(),_page,_sort,ApiHelper.GetTimeSpan);
                
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                MusicCateSongsModel obj = JsonConvert.DeserializeObject<MusicCateSongsModel>(results);

                if (obj.code == 0)
                {
                    if (obj.data.list != null && obj.data.list.Count != 0)
                    {
                        if (_page == 1)
                        {
                            list_songs.ItemsSource = obj.data.list;
                        }
                        else
                        {
                            var ls = list_songs.ItemsSource as ObservableCollection<MusicHomeSongModel>;
                            foreach (var item in obj.data.list)
                            {
                                ls.Add(item);
                            }
                        }
                        btn_LoadMore.Visibility = Visibility.Visible;
                        _page++;
                    }
                    else
                    {
                        Utils.ShowMessageToast("加载完了");
                    }
                }
                else
                {
                    Utils.ShowMessageToast(obj.msg);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载歌曲失败");
            }
            finally
            {
                loading = false;
                pr_load.Visibility = Visibility.Collapsed;
            }
        }


        private void btn_SetSort_Click(object sender, RoutedEventArgs e)
        {
            _page = 1;
            _sort = (sender as MenuFlyoutItem).Tag.ToInt32();
            LoadSongs();
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!loading)
            {
                LoadSongs();
            }
        }

        private void list_songs_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage), (e.ClickedItem as MusicHomeSongModel).id);
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!loading)
                {
                    LoadSongs();
                }
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            _page = 1;
            _selectCategroies = e.ClickedItem as subcateModel;
            txt_Header.Text = _data.categories.cateTitle+" "+ _selectCategroies.cateTitle;
            LoadSongs();
        }
    }
    public class MusicCateSongsModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public MusicCateSongsModel data { get; set; }

        public int pages { get; set; }
        public ObservableCollection<MusicHomeSongModel> list { get; set; }
    }
}
