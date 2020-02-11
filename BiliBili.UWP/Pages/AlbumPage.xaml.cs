using BiliBili.UWP.Views;
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

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
       //是否首次加载
        bool isload = true;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            isload = true;
            cb_cos.SelectedIndex = 0;
            isload = false;
            if (e.NavigationMode==NavigationMode.New)
            {
                _drawPage = 0;
                _cosPage = 0;
                gv_cos.ItemsSource = null;
                gv_draw.ItemsSource = null;
                pivot.SelectedIndex = 0;
                cb_draw.SelectedIndex = 0;
            }
           
        }


        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Width <= 500)
            {
                bor_Width.Width = availableSize.Width / 2 - 18;
            }
            else
            {
                int i = Convert.ToInt32(availableSize.Width / 240);
                bor_Width.Width = availableSize.Width / i - 14;
            }

            return base.MeasureOverride(availableSize);
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex==0)
            {
                cb_draw.Visibility = Visibility.Visible;
                cb_cos.Visibility = Visibility.Collapsed;
            }
            else
            {
                cb_draw.Visibility = Visibility.Collapsed;
                cb_cos.Visibility = Visibility.Visible;
                if (gv_cos.ItemsSource==null)
                {
                    _cosPage = 0;
                    GetCos();
                }
            }
            
        }

        private void cb_draw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isload)
            {
                return;
            }
            _drawPage = 0;
            GetDraw();
        }

        private void cb_cos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isload)
            {
                return;
            }
            _cosPage = 0;
            GetCos();
        }


        int _drawPage = 0;
        bool _drawLoad = false;
        private async void GetDraw()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                btn_loadMoreDraw.Visibility = Visibility.Collapsed;
                _drawLoad = true;
                if (_drawPage==0)
                {
                    gv_draw.ItemsSource = null;
                }
                var url = "http://api.vc.bilibili.com/link_draw/v2/doc/index?appkey={1}&page_num={1}&page_size=20&platform=android&src=bilih5&ts={2}&type=recommend";
                switch (cb_draw.SelectedIndex)
                {
                    case 0:
                        url = string.Format(url, ApiHelper.AndroidKey.Appkey, _drawPage, ApiHelper.GetTimeSpan);
                        break;
                    case 1:
                        url = "http://api.vc.bilibili.com/link_draw/v2/doc/list?appkey={0}&category={3}&page_num={1}&page_size=20&platform=android&type=hot&ts={2}";
                        url = string.Format(url, ApiHelper.AndroidKey.Appkey, _drawPage, ApiHelper.GetTimeSpan, "illustration");
                        break;
                    case 2:
                        url = "http://api.vc.bilibili.com/link_draw/v2/doc/list?appkey={0}&category={3}&page_num={1}&page_size=20&platform=android&type=hot&ts={2}";
                        url = string.Format(url, ApiHelper.AndroidKey.Appkey, _drawPage, ApiHelper.GetTimeSpan, "comic");
                        break;
                    case 3:
                        url = "http://api.vc.bilibili.com/link_draw/v2/doc/list?appkey={0}&category={3}&page_num={1}&page_size=20&platform=android&type=hot&ts={2}";
                        url = string.Format(url, ApiHelper.AndroidKey.Appkey, _drawPage, ApiHelper.GetTimeSpan, "draw");
                        break;
                    default:
                        break;
                }
                url += "&sign=" + ApiHelper.GetSign(url);

                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                AblumModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<AblumModel>(results);
                if (m.code==0)
                {
                    if (m.data.items!=null&&m.data.items.Count!=0)
                    {
                        btn_loadMoreDraw.Visibility = Visibility.Visible;
                        if (gv_draw.ItemsSource==null)
                        {
                            gv_draw.ItemsSource = m.data.items;
                        }
                        else
                        {
                            var ls = gv_draw.ItemsSource as ObservableCollection<AblumModel>;
                            foreach (var item in m.data.items)
                            {
                                ls.Add(item);
                            }
                        }
                        _drawPage++;
                        btn_loadMoreDraw.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Utils.ShowMessageToast("加载完了");
                        btn_loadMoreDraw.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载插画失败");
            }
            finally
            {
                _drawLoad = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }



        }

        int _cosPage = 0;
        bool _cosLoad = false;
        private async void GetCos()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                btn_loadMoreDraw.Visibility = Visibility.Collapsed;
                _cosLoad = true;
                if (_cosPage == 0)
                {
                    gv_cos.ItemsSource = null;
                }
                var url = "http://api.vc.bilibili.com/link_draw/v2/Photo/index?appkey={1}&page_num={1}&page_size=20&platform=android&src=bilih5&ts={2}&type=recommend";
                switch (cb_draw.SelectedIndex)
                {
                    case 0:
                        url = string.Format(url, ApiHelper.AndroidKey.Appkey, _cosPage, ApiHelper.GetTimeSpan);
                        break;
                    case 1:
                        url = "http://api.vc.bilibili.com/link_draw/v2/Photo/list?appkey={0}&category={3}&page_num={1}&page_size=20&platform=android&type=hot&ts={2}";
                        url = string.Format(url, ApiHelper.AndroidKey.Appkey, _cosPage, ApiHelper.GetTimeSpan, "cos");
                        break;
                    case 2:
                        url = "http://api.vc.bilibili.com/link_draw/v2/Photo/list?appkey={0}&category={3}&page_num={1}&page_size=20&platform=android&type=hot&ts={2}";
                        url = string.Format(url, ApiHelper.AndroidKey.Appkey, _cosPage, ApiHelper.GetTimeSpan, "sifu");
                        break;
                    default:
                        break;
                }
                url += "&sign=" + ApiHelper.GetSign(url);

                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                AblumModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<AblumModel>(results);
                if (m.code == 0)
                {
                    if (m.data.items != null && m.data.items.Count != 0)
                    {
                        btn_loadMoreCos.Visibility = Visibility.Visible;
                        if (gv_cos.ItemsSource == null)
                        {
                            gv_cos.ItemsSource = m.data.items;
                        }
                        else
                        {
                            var ls = gv_cos.ItemsSource as ObservableCollection<AblumModel>;
                            foreach (var item in m.data.items)
                            {
                                ls.Add(item);
                            }
                        }
                        _cosPage++;
                        btn_loadMoreCos.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Utils.ShowMessageToast("加载完了");
                        btn_loadMoreCos.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载插画失败");
            }
            finally
            {
                _cosLoad = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }



        }

        private void btn_loadMoreDraw_Click(object sender, RoutedEventArgs e)
        {
            if (!_drawLoad)
            {
                GetDraw();
            }
        }

        private void sv_draw_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_draw.VerticalOffset==sv_draw.ScrollableHeight)
            {
                if (!_drawLoad)
                {
                    GetDraw();
                }
            }
            
        }

        private void gv_draw_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item= e.ClickedItem as AblumModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicInfoPage),item.item.doc_id.ToString());
        }



        private void btn_loadMoreCos_Click(object sender, RoutedEventArgs e)
        {
            if (!_cosLoad)
            {
                GetCos();
            }
        }

        private void sv_cos_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_cos.VerticalOffset == sv_cos.ScrollableHeight)
            {
                if (!_cosLoad)
                {
                    GetCos();
                }
            }
        }
    }


    public class AblumModel
    {
        public int code { get; set;}
        public string message { get; set; }

        public AblumModel data { get; set; }

        public ObservableCollection<AblumModel> items { get; set; }

        public AblumModel user { get; set; }
        public AblumItemModel item { get; set; }
        public int uid { get; set; }
        public string head_url { get; set; }
        public string name { get; set; }



    }

    public class AblumItemModel
    {
        public int doc_id { get; set; }
        public int poster_uid { get; set; }

        public string img
        {
            get
            {
                if (pictures!=null&& pictures.Count!=0)
                {
                    return pictures[0].img_src;
                }
                else
                {
                    return "ms-appx:///Assets/MiniIcon/transparent.png";
                }
            }
        }
        public List<picturesModel> pictures { get; set; }
        public string title { get; set; }
        public string category { get; set; }
        public long upload_time { get; set; }

        public int already_liked { get; set; }//已经点赞
        public int already_voted { get; set; }
    }






}


