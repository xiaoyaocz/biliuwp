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
    public sealed partial class MusicSearchPage : Page
    {
        public MusicSearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                _keyword = "";
                autoSug_Box.Text = "";
                _pageSongs = 1;
                _SongLoading = false;
                _pageMenus = 1;
                _MenusLoading = false;
                _pageUsers = 1;
                _UsersLoading = false;
                list_Songs.ItemsSource = null;
                list_Menus.ItemsSource = null;
                list_Users.ItemsSource = null;

            }
        }

        private void autoSug_Box_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (autoSug_Box.Text.Trim().Length == 0)
            {
                Utils.ShowMessageToast("检查你的输入");
                return;
            }

            _keyword = autoSug_Box.Text;
            _pageSongs = 1;
            _SongLoading = false;
            _pageMenus = 1;
            _MenusLoading = false;
            _pageUsers = 1;
            _UsersLoading = false;
            list_Songs.ItemsSource = null;
            list_Menus.ItemsSource = null;
            list_Users.ItemsSource = null;
            pivot.SelectedIndex = 0;
            SearchSongs();
        }
        string _keyword = "";
        int _pageSongs = 1;
        bool _SongLoading = false;
        private async void SearchSongs()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                btn_LoadMoreSongs.Visibility = Visibility.Collapsed;
                _SongLoading = true;

                string url = "https://api.bilibili.com/audio/music-service-c/s?appkey={0}&build=5250000&keyword={1}&mobi_app=android&page={2}&pagesize=20&platform=android&search_type=music&ts={3}&";
                url = string.Format(url, ApiHelper.AndroidKey.Appkey, Uri.EscapeDataString(_keyword), _pageSongs, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));
                var m = JsonConvert.DeserializeObject<MusicSearchSongModel>(results);
                if (m.code == 0)
                {
                    if (m.data.num_pages < _pageSongs)
                    {
                        btn_LoadMoreSongs.Visibility = Visibility.Visible;
                    }
                    if (m.data.result.Count == 0)
                    {
                        btn_LoadMoreSongs.Visibility = Visibility.Collapsed;
                        Utils.ShowMessageToast("加载完了");

                    }
                    else
                    {
                        if (list_Songs.ItemsSource == null)
                        {
                            list_Songs.ItemsSource = m.data.result;
                        }
                        else
                        {
                            var ls = list_Songs.ItemsSource as ObservableCollection<MusicSearchSongModel>;
                            foreach (var item in m.data.result)
                            {
                                ls.Add(item);
                            }
                        }
                        _pageSongs++;
                    }

                }
                else
                {
                    Utils.ShowMessageToast(m.msg);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("搜索单曲失败" + ex.HResult);
            }
            finally
            {
                _SongLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private void btn_LoadMoreSongs_Click(object sender, RoutedEventArgs e)
        {
            if (!_SongLoading)
            {
                SearchSongs();
            }
        }

        private void sv_songs_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_songs.VerticalOffset == sv_songs.ScrollableHeight)
            {
                if (!_SongLoading)
                {
                    SearchSongs();
                }
            }
        }
        private void list_Songs_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicSearchSongModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), item.id);
        }
        int _pageMenus = 1;
        bool _MenusLoading = false;

        private async void SearchMenus()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                btn_LoadMoreMenus.Visibility = Visibility.Collapsed;
                _MenusLoading = true;

                string url = "https://api.bilibili.com/audio/music-service-c/s?appkey={0}&build=5250000&keyword={1}&mobi_app=android&page={2}&pagesize=20&platform=android&search_type=menus&ts={3}&";
                url = string.Format(url, ApiHelper.AndroidKey.Appkey, Uri.EscapeDataString(_keyword), _pageMenus, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));
                var m = JsonConvert.DeserializeObject<MusicSearchMenuModel>(results);
                if (m.code == 0)
                {
                    if (m.data.num_pages < _pageMenus)
                    {
                        btn_LoadMoreMenus.Visibility = Visibility.Visible;
                    }
                    if (m.data.result.Count == 0)
                    {
                        btn_LoadMoreMenus.Visibility = Visibility.Collapsed;
                        Utils.ShowMessageToast("加载完了");

                    }
                    else
                    {
                        if (list_Menus.ItemsSource == null)
                        {
                            list_Menus.ItemsSource = m.data.result;
                        }
                        else
                        {
                            var ls = list_Menus.ItemsSource as ObservableCollection<MusicSearchMenuModel>;
                            foreach (var item in m.data.result)
                            {
                                ls.Add(item);
                            }
                        }
                        _pageMenus++;
                    }


                }
                else
                {
                    Utils.ShowMessageToast(m.msg);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("搜索歌单失败" + ex.HResult);
            }
            finally
            {
                _MenusLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void sv_menu_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_menu.VerticalOffset == sv_menu.ScrollableHeight)
            {
                if (!_MenusLoading)
                {
                    SearchMenus();
                }
            }
        }
        private void list_Menus_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicSearchMenuModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage), item.id);
        }

        private void btn_LoadMoreMenus_Click(object sender, RoutedEventArgs e)
        {
            if (!_MenusLoading)
            {
                SearchMenus();
            }
        }
        int _pageUsers = 1;
        bool _UsersLoading = false;

        private async void SearchUsers()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                btn_LoadMoreUsers.Visibility = Visibility.Collapsed;
                _UsersLoading = true;

                string url = "https://api.bilibili.com/audio/music-service-c/s?appkey={0}&build=5250000&keyword={1}&mobi_app=android&page={2}&pagesize=20&platform=android&search_type=musician&ts={3}&";
                url = string.Format(url, ApiHelper.AndroidKey.Appkey, Uri.EscapeDataString(_keyword), _pageUsers, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));
                var m = JsonConvert.DeserializeObject<MusicSearchUserModel>(results);
                if (m.code == 0)
                {
                    if (m.data.num_pages < _pageMenus)
                    {
                        btn_LoadMoreUsers.Visibility = Visibility.Visible;
                    }
                    if (list_Menus.ItemsSource == null)
                    {
                        list_Users.ItemsSource = m.data.result;
                    }
                    else
                    {
                        var ls = list_Users.ItemsSource as ObservableCollection<MusicSearchUserModel>;
                        foreach (var item in m.data.result)
                        {
                            ls.Add(item);
                        }
                    }
                    _pageUsers++;
                }
                else
                {
                    Utils.ShowMessageToast(m.msg);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("搜索歌手失败" + ex.HResult);
            }
            finally
            {
                _UsersLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void sv_Users_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_Users.VerticalOffset == sv_Users.ScrollableHeight)
            {
                if (!_UsersLoading)
                {
                    SearchUsers();
                }
            }
        }
        private void btn_LoadMoreUsers_Click(object sender, RoutedEventArgs e)
        {
            if (!_UsersLoading)
            {
                SearchUsers();
            }
        }

        private void list_Users_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicSearchUserModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicianPage),item.id);
        }
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 1 && list_Menus.ItemsSource == null)
            {
                if (!_MenusLoading)
                {
                    SearchMenus();
                }
            }
            if (pivot.SelectedIndex == 2 && list_Users.ItemsSource == null)
            {
                if (!_UsersLoading)
                {
                    SearchUsers();
                }
            }
        }


    }

    public class MusicSearchSongModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public int num_pages { get; set; }

        public MusicSearchSongModel data { get; set; }


        public ObservableCollection<MusicSearchSongModel> result { get; set; }

        public int id { get; set; }
        private string _title;
        public string title
        {
            get
            {
                return _title.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
            }
            set
            {
                _title = value;
            }
        }
        public string Cover
        {
            get
            {
                return cover + "@200w.jpg";
            }
        }
        public string cover { get; set; }


        public int review_count { get; set; }
        public string reviewStr
        {
            get
            {
                if (review_count >= 10000)
                {
                    return ((double)review_count / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return review_count.ToString();
                }
            }
        }
        public int play_count { get; set; }
        public string playStr
        {
            get
            {
                if (play_count >= 10000)
                {
                    return ((double)play_count / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return play_count.ToString();
                }
            }
        }

        public string author { get; set; }

    }
    public class MusicSearchMenuModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public int num_pages { get; set; }

        public MusicSearchMenuModel data { get; set; }


        public ObservableCollection<MusicSearchMenuModel> result { get; set; }

        public int id { get; set; }

        private string _title;
        public string title
        {
            get
            {
                return _title.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
            }
            set
            {
                _title = value;
            }
        }


        public string Cover
        {
            get
            {
                return cover + "@200w.jpg";
            }
        }
        public string cover { get; set; }

        public int music_count { get; set; }
        public int favor_count { get; set; }
        public string favorStr
        {
            get
            {
                if (favor_count >= 10000)
                {
                    return ((double)favor_count / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return favor_count.ToString();
                }
            }
        }
        public int play_count { get; set; }
        public string playStr
        {
            get
            {
                if (play_count >= 10000)
                {
                    return ((double)play_count / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return play_count.ToString();
                }
            }
        }

        public string author { get; set; }

    }

    public class MusicSearchUserModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public int num_pages { get; set; }

        public MusicSearchUserModel data { get; set; }


        public ObservableCollection<MusicSearchUserModel> result { get; set; }

        public int id { get; set; }

        private string _uname;
        public string uname
        {
            get
            {
                return _uname.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
            }
            set
            {
                _uname = value;
            }
        }


        public string Cover
        {
            get
            {
                return cover + "@200w.jpg";
            }
        }
        public string cover { get; set; }


        public int fans_count { get; set; }
        public string fansStr
        {
            get
            {
                if (fans_count >= 10000)
                {
                    return ((double)fans_count / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return fans_count.ToString();
                }
            }
        }
        public int play_count { get; set; }
        public string playStr
        {
            get
            {
                if (play_count >= 10000)
                {
                    return ((double)play_count / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return play_count.ToString();
                }
            }
        }

        public string author { get; set; }

    }

}
