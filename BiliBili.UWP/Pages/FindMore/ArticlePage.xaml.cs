using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace BiliBili.UWP.Pages.FindMore
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ArticlePage : Page
    {
        public ArticlePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadBanner();
            }

        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (g.ActualWidth >= 680)
            {
                bor_SetBanner.Width = 200;
            }
            else
            {
                var d = g.ActualWidth * ((double)212 / 680);
                bor_SetBanner.Width = d;
            }
        }


        private async void LoadBanner()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                IsLoading = true;
                _page = 1;
                var url = string.Format("https://api.bilibili.com/x/article/home?appkey={0}&build=515000&cid=0&mobi_app=android&platform=android&pn=1&ps=20&ts={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                ArticleModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<ArticleModel>(results);
                if (m.code == 0)
                {
                    ls_Banner.ItemsSource = m.data.banners;
                    ls_article.ItemsSource = m.data.articles;
                    _page++;
                }
                else
                {
                    Utils.ShowMessageToast("加载失败", 2000);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("加载失败", 2000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                IsLoading = false;
            }

        }
        int _page = 1;
        bool IsLoading = false;
        private async void AddArticle()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                var url = string.Format("https://api.bilibili.com/x/article/home?appkey={0}&build=515000&cid=0&mobi_app=android&platform=android&pn={2}&ps=20&ts={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, _page);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                ArticleModel m = Newtonsoft.Json.JsonConvert.DeserializeObject<ArticleModel>(results);
                if (m.code == 0)
                {
                    foreach (var item in m.data.articles)
                    {
                        (ls_article.ItemsSource as ObservableCollection<ArticlesModel>).Add(item);
                    }
                    _page++;
                    //ls_article.ItemsSource = m.data.articles;
                }
                else
                {
                    Utils.ShowMessageToast("加载失败", 2000);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("加载失败", 2000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                IsLoading = false;
            }

        }
        private void ls_Banner_ItemClick(object sender, ItemClickEventArgs e)
        {

            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), (e.ClickedItem as ArticleModel).url);
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!IsLoading)
                {
                    AddArticle();
                }
            }
        }

        private void ls_article_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), "https://www.bilibili.com/read/app/" + (e.ClickedItem as ArticlesModel).id);
        }

        private void btn_Banner_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), ((sender as HyperlinkButton).DataContext as ArticleModel).url);
        }

        private void btn_ArticlePart_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticlePartsPage), (sender as HyperlinkButton).Tag.ToInt32());
        }
    }

    public class ArticleModel
    {
        public int code { get; set; }

        public ArticleModel data { get; set; }

        public List<ArticleModel> banners { get; set; }

        public ObservableCollection<ArticlesModel> articles { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string url { get; set; }



    }
    public class ArticlesModel
    {

        public int id { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string banner_url { get; set; }
        public string logo
        {
            get
            {
                return banner_url + "@234w_176h.png";
            }
        }
        public string bannerImage
        {
            get
            {
                return banner_url + "@500w.jpg";
            }
        }

        public ArticlesModel category { get; set; }

        public string name { get; set; }


        public ArticlesModel author { get; set; }
        public string face { get; set; }


        public ArticlesModel stats { get; set; }
        public int view { get; set; }
        public int like { get; set; }
        public int reply { get; set; }

    }



}

