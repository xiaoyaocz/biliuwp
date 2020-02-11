using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace BiliBili.UWP.Pages.Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicHomePage : Page
    {
        public MusicHomePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;

            }
            LoadMusicHome();

        }
        private async void LoadMusicHome()
        {
            try
            {
                sharkMenus = 1;
                pgcMenus = 1;
                missevan = 1;
                HitSongs = 1;
                pr_load.Visibility = Visibility.Visible;
                string url = string.Format("https://api.bilibili.com/audio/music-service-c/firstpage?appkey={0}&build=5250000&mobi_app=android&platform=android&ts={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                MusicHomeModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<MusicHomeModel>(re);
                if (m.code == 0)
                {
                    this.DataContext = m.data;
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载音频首页失败");
            }
            finally
            {
                pr_load.Visibility = Visibility.Collapsed;
            }

        }


        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as MusicHomeBannerModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.schema);
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            LoadMusicHome();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var width = (availableSize.Width / 3) - 10;
            ViewBox_num.Width = width;
            return base.MeasureOverride(availableSize);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicHomeMenuModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage), item.menuId);
        }

        private void gv_Songs_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicHomeSongModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), item.id);
        }

        private void btn_HotRank_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage), ((sender as HyperlinkButton).DataContext as MusicHomeMenuModel).menuId);
        }

        private void btn_NewRank_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage), ((sender as HyperlinkButton).DataContext as MusicHomeMenuModel).menuId);
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMusicHome();
        }


        int sharkMenus = 1;
        int pgcMenus = 1;
        int missevan = 1;
        int HitSongs = 1;
        private async void btn_RefreshMenu_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).Tag.ToString();

            switch (data)
            {
                case "sharkMenus":
                    (this.DataContext as MusicHomeModel).common = await RefreshMenu(data, sharkMenus, 6);
                    sharkMenus++;
                    break;
                case "shark-pgc-menus":
                    (this.DataContext as MusicHomeModel).pmenu = await RefreshMenu(data, pgcMenus, 6);
                    pgcMenus++;
                    break;
                case "shark-missevan":
                    (this.DataContext as MusicHomeModel).missEvan = await RefreshMenu(data, missevan, 3);
                    missevan++;
                    break;
                case "sharkHitSongs":
                    (this.DataContext as MusicHomeModel).hitSongs = await RefreshSongs(data, HitSongs, 6);
                    HitSongs++;
                    break;
                default:
                    var dt = (sender as HyperlinkButton).DataContext as MusicHomeSongTypeModel;
                    dt.list = await RefreshSongs(dt.cate_type.ToString(), dt.page, 6);
                    dt.page++;
                    break;
            }


        }


        private async Task<ObservableCollection<MusicHomeMenuModel>> RefreshMenu(string data, int time, int pageSize)
        {

            try
            {
                pr_load.Visibility = Visibility.Visible;
                string url = string.Format("https://api.bilibili.com/audio/music-service-c/firstpage/{2}?appkey={0}&build=5250000&mobi_app=android&platform=android&size={3}&time={4}&ts={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, data, pageSize, time);
                url += "&sign=" + ApiHelper.GetSign(url);

                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));

                MusicRefreshMenuModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<MusicRefreshMenuModel>(re);
                if (m.code == 0)
                {
                    return m.data;
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                    return null;
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载音频首页失败");
                return null;
            }
            finally
            {

                pr_load.Visibility = Visibility.Collapsed;
            }

        }


        private async Task<ObservableCollection<MusicHomeSongModel>> RefreshSongs(string data, int time, int pageSize=6)
        {

            try
            {
                pr_load.Visibility = Visibility.Visible;

                var url = "";
                if (data== "sharkHitSongs")
                {
                    url = string.Format("https://api.bilibili.com/audio/music-service-c/firstpage/{2}?appkey={0}&build=5250000&mobi_app=android&platform=android&size={3}&time={4}&ts={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, data, pageSize, time);
                }
                else
                {
                    url = string.Format("https://api.bilibili.com/audio/music-service-c/firstpage/sharkSongs?appkey={0}&build=5250000&cateType={2}&mobi_app=android&platform=android&size={3}&time={4}&ts={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, data, pageSize, time);
                }
                url += "&sign=" + ApiHelper.GetSign(url);

                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));

                MusicRefreshSongModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<MusicRefreshSongModel>(re);
                if (m.code == 0)
                {
                    return m.data;
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                    return null;
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载音频首页失败");
                return null;
            }
            finally
            {

                pr_load.Visibility = Visibility.Collapsed;
            }

        }

        private async void menu_CollectSong_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicCollectPage));
        }

        private void menu_CollectMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicCollectMenuPage),1);
        }
        private async void menu_CollectPMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicCollectMenuPage), 2);
        }
        private async void btn_OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.regions==null)
            {
                await ApiHelper.SetRegions();
            }

            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage),ApiHelper.regions.Find(x=>x.name.Contains("音乐")));

        }

        private void btn_OpenSearch_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicSearchPage));
        }

        private void btn_OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicAllMenuPage), OpenMenuType.Menu);
        }

        private async void menu_CollectUser_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicFollowMusicianPage));
        }

        private void btn_OpenMissEvan_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicAllMenuPage), OpenMenuType.MissEvan);
        }

        private void btn_OpenAlbum_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicAllMenuPage), OpenMenuType.Album);
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            var item= (sender as HyperlinkButton).DataContext as MusicHomeSongTypeModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicAllSongPage), item);
        }
    }
    public class MusicRefreshMenuModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public ObservableCollection<MusicHomeMenuModel> data { get; set; }
    }
    public class MusicRefreshSongModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public ObservableCollection<MusicHomeSongModel> data { get; set; }
    }
    public class MusicHomeModel : INotifyPropertyChanged
    {
        public int code { get; set; }
        public string message { get; set; }
        public MusicHomeModel data { get; set; }

        public MusicHomeModel banner { get; set; }
        public List<MusicHomeBannerModel> bannerList { get; set; }

        public MusicHomeMenuModel hitMenus { get; set; }
        public MusicHomeMenuModel originMenus { get; set; }

        private ObservableCollection<MusicHomeMenuModel> _common;
        public ObservableCollection<MusicHomeMenuModel> common
        {
            get { return _common; }
            set { _common = value; thisPropertyChanged("common"); }
        }


        private ObservableCollection<MusicHomeMenuModel> _pmenu;
        public ObservableCollection<MusicHomeMenuModel> pmenu
        {
            get { return _pmenu; }
            set { _pmenu = value; thisPropertyChanged("pmenu"); }
        }


        private ObservableCollection<MusicHomeMenuModel> _categories;
        public ObservableCollection<MusicHomeMenuModel> categories
        {
            get { return _categories; }
            set { _categories = value; thisPropertyChanged("categories"); }
        }

        private ObservableCollection<MusicHomeMenuModel> _missEvan;

        public ObservableCollection<MusicHomeMenuModel> missEvan
        {
            get { return _missEvan; }
            set { _missEvan = value; thisPropertyChanged("missEvan"); }
        }


        public ObservableCollection<MusicHomeSongTypeModel> songRecommend { get; set; }

        private ObservableCollection<MusicHomeSongModel> _hitSongs;
        public ObservableCollection<MusicHomeSongModel> hitSongs
        {
            get { return _hitSongs; }
            set { _hitSongs = value; thisPropertyChanged("hitSongs"); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

        }


    }
    public class MusicHomeMenuModel
    {
        public int menuId { get; set; }
        public int? collected { get; set; }

        public string title { get; set; }

        private string _coverUrl;
        public string coverUrl
        {
            get { return _coverUrl + "@300w.jpg"; }
            set { _coverUrl = value; }
        }
        public string intro { get; set; }

        public int type { get; set; }

        public Visibility vip
        {
            get
            {
                if (type == 5)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

   
        public string collectNumStr 
        {
            get
            {
                if (collectNum >= 10000)
                {
                    return ((double)collectNum / 10000).ToString("0.0") + "万";
                }
                return collectNum.ToString();
            }
        }
        public string ctimeStr { get; set; }
        public int playNum { get; set; }
        public string palyNum_str
        {
            get
            {
                if (playNum >= 10000)
                {
                    return ((double)playNum / 10000).ToString("0.0") + "万";
                }
                return playNum.ToString();
            }
        }

        public int collectNum { get; set; }
        public int commentNum { get; set; }
        public int songNum { get; set; }
        public string toptitle { get; set; }
        public string schema { get; set; }




    }
    public class MusicHomeSongModel
    {
        public int id { get; set; }

        public string title { get; set; }

        public string author { get; set; }
        private string _coverUrl;
        public string cover_url
        {
            get { return _coverUrl + "@300w.jpg"; }
            set { _coverUrl = value; }
        }

        public string intro { get; set; }

    
        public string uploader_name { get; set; }

        public int play_num { get; set; }
        public string palyNum_str
        {
            get
            {
                if (play_num >= 10000)
                {
                    return ((double)play_num / 10000).ToString("0.0") + "万";
                }
                return play_num.ToString();
            }
        }


        public int comment_num { get; set; }
        public string commentNum_str
        {
            get
            {
                if (comment_num >= 10000)
                {
                    return ((double)comment_num / 10000).ToString("0.0") + "万";
                }
                return comment_num.ToString();
            }
        }
        public int cid { get; set; }


    }
    public class MusicHomeBannerModel
    {
        public int bannerId { get; set; }
        public string bannerTitle { get; set; }
        public string bannerImgUrl { get; set; }
        public int bannerType { get; set; }
        public string schema { get; set; }
    }

    public class MusicHomeSongTypeModel : INotifyPropertyChanged
    {
        public string name { get; set; }
        public int cate_type { get; set; }

        public int page { get; set; } = 1;


        private ObservableCollection<MusicHomeSongModel> _list;
        public ObservableCollection<MusicHomeSongModel> list
        {
            get { return _list; }
            set { _list = value; thisPropertyChanged("list"); }
        }

        public MusicHomeSongTypeModel categories { get; set; }
        public int cateId { get; set; }
        public string cateTitle { get; set; }

        public List<subcateModel> subcate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

        }
    }

    public class subcateModel
    {
        public string cateTitle { get; set; }
        public int cateId { get; set; }
        public int parentId { get; set; }
    }


}
