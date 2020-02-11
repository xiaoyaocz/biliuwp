using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    enum OpenMenuType
    {
        Menu,//歌单
        Album,//专辑
        MissEvan//猫耳
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicAllMenuPage : Page
    {
        public MusicAllMenuPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }



        OpenMenuType _opentype;
        MusicCategroiesModel _selectCategroies;
        int _sort = 0;
        int _page = 1;
        bool loading = false;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                _opentype = (OpenMenuType)(e.Parameter as object[])[0];
                btn_SortMenu.Visibility = Visibility.Collapsed;
                btn_SortMissevan.Visibility = Visibility.Collapsed;
                switch (_opentype)
                {
                    case OpenMenuType.Menu:
                        txt_Header.Text = "全部歌单";
                        btn_SortMenu.Visibility = Visibility.Visible;
                        break;
                    case OpenMenuType.Album:
                        txt_Header.Text = "全部专辑";
                        break;
                    case OpenMenuType.MissEvan:
                        txt_Header.Text = "全部猫耳";
                        btn_SortMissevan.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
                _selectCategroies = null;
                _page = 1;
                GetCategroies();
            }
        }



        public async void GetCategroies()
        {
            try
            {
                string url = "";

                switch (_opentype)
                {
                    case OpenMenuType.Menu:
                        url = string.Format("https://api.bilibili.com/audio/music-service-c/categroies/menucate?access_key={0}&appkey={1}&build=5250000&mobi_app=android&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                        break;
                    case OpenMenuType.Album:
                        url = string.Format("https://api.bilibili.com/audio/music-service-c/categroies/pmenucate?access_key={0}&appkey={1}&build=5250000&mobi_app=android&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                        break;
                    case OpenMenuType.MissEvan:
                        url = string.Format("https://api.bilibili.com/audio/music-service-c/categroies/missevan?access_key={0}&appkey={1}&build=5250000&mobi_app=android&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                        break;
                    default:
                        break;
                }
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                MusicCategroiesModel m = JsonConvert.DeserializeObject<MusicCategroiesModel>(results);
                if (m.code == 0)
                {
                    cats.ItemsSource = m.data;

                    LoadMenu();
                }
                else
                {
                    Utils.ShowMessageToast(m.msg);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("读取分类发生错误" + ex.HResult);
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            _page = 1;
            _selectCategroies = e.ClickedItem as MusicCategroiesModel;
            txt_Header.Text = _selectCategroies.itemVal;
            LoadMenu();
        }

        private void btn_All_Click(object sender, RoutedEventArgs e)
        {
            _page = 1;
            _selectCategroies = null;
            switch (_opentype)
            {
                case OpenMenuType.Menu:
                    txt_Header.Text = "全部歌单";
                    break;
                case OpenMenuType.Album:
                    txt_Header.Text = "全部专辑";
                    break;
                case OpenMenuType.MissEvan:
                    txt_Header.Text = "全部猫耳";
                    break;
                default:
                    break;
            }
            LoadMenu();
        }


        private async void LoadMenu()
        {
            try
            {
                loading = true;
                pr_load.Visibility = Visibility.Visible;
                btn_LoadMore.Visibility = Visibility.Collapsed;
                int catId = 0;
                int itemId = 0;
                if (_selectCategroies!=null)
                {
                    catId = _selectCategroies.cateId;
                    itemId = _selectCategroies.itemId;
                }
                if (_page==1)
                {
                    list_menus.ItemsSource = null;
                }
                string url = "";
                switch (_opentype)
                {
                    case OpenMenuType.Menu:
                        url = string.Format("https://api.bilibili.com/audio/music-service-c/menus/filteMenu?appkey={0}&build=5250000&cateId={1}&itemId={2}&mobi_app=android&orderBy={3}&pageNum={4}&pageSize=24&platform=android&ts={5}",  ApiHelper.AndroidKey.Appkey, catId,itemId,_sort,_page, ApiHelper.GetTimeSpan);
                        break;
                    case OpenMenuType.Album:
                        url = string.Format("https://api.bilibili.com/audio/music-service-c/menus/filte-pmenu?appkey={0}&build=5250000&cateId={1}&itemId={2}&mobi_app=android&orderBy={3}&pageNum={4}&pageSize=24&platform=android&ts={5}", ApiHelper.AndroidKey.Appkey, catId, itemId, _sort, _page, ApiHelper.GetTimeSpan);
                        break;
                    case OpenMenuType.MissEvan:
                        url = string.Format("https://api.bilibili.com/audio/music-service-c/menus/missevan?appkey={0}&build=5250000&cateId={1}&itemId={2}&mobi_app=android&orderBy={3}&pageNum={4}&pageSize=24&platform=android&ts={5}", ApiHelper.AndroidKey.Appkey, catId, itemId, _sort, _page, ApiHelper.GetTimeSpan);
                        break;
                    default:
                        break;
                }
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                MusicCateMenusModel obj = JsonConvert.DeserializeObject<MusicCateMenusModel>(results);

                if (obj.code == 0)
                {
                    if (obj.data.list!=null&& obj.data.list.Count!=0)
                    {
                        if (_page==1)
                        {
                            list_menus.ItemsSource = obj.data.list;
                        }
                        else
                        {
                            var ls= list_menus.ItemsSource as ObservableCollection<MusicHomeMenuModel>;
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
                Utils.ShowMessageToast("加载失败歌单");
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
            LoadMenu();


        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset==sv.ScrollableHeight)
            {
                if (!loading)
                {
                    LoadMenu();
                }
            }
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!loading)
            {
                LoadMenu();
            }
        }

        private void list_menus_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage),(e.ClickedItem as MusicHomeMenuModel).menuId);
        }
    }

    public class MusicCateMenusModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public MusicCateMenusModel data { get; set; }

        public int pages { get; set; }
        public ObservableCollection<MusicHomeMenuModel> list { get; set; }
    }


    public class MusicCategroiesModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public List<MusicCategroiesModel> data { get; set; }

        public int cateId { get; set; }

        public string cateVal { get; set; }

        public List<MusicCategroiesModel> cateItemList { get; set; }
        public int itemId { get; set; }
        public string itemVal { get; set; }
        public int attr { get; set; }

    }



}
