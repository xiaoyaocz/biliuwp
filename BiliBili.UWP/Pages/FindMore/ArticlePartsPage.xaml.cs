using Newtonsoft.Json.Linq;
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
    public sealed partial class ArticlePartsPage : Page
    {
        public ArticlePartsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        int _typeId = 0;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New)
            {
                if (e.Parameter != null && (e.Parameter as object[]).Length != 0)
                {
                    _typeId = (e.Parameter as object[])[0].ToInt32();
                    LoadCategories();
                }
            }
        }

        private async void LoadCategories()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = string.Format( "https://api.bilibili.com/x/article/categories?appkey={0}&build=5250000&mobi_app=android&platform=android&ts={1}",ApiHelper.AndroidKey.Appkey,ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                JObject obj = JObject.Parse(results);
                if (obj["code"].ToInt32()==0)
                {
                    List<ArticleCategoriesModel> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArticleCategoriesModel>>(obj["data"].ToString());

                    //foreach (var item in list)
                    //{
                    //    //item.children.Insert(0, new ArticleCategoriesModel() {
                    //    //     id=item.id,
                    //    //     name="全部"
                    //    //});
                    //    item.selectIndex = -1;
                    //}

                    pivot.ItemsSource = list;
                   
                    if (_typeId!=0)
                    {
                        pivot.SelectedItem = list.Find(x => x.id == _typeId);
                    }
                    else
                    {
                        pivot.SelectedIndex = 0;

                    }
                    LoadArticle(pivot.SelectedItem as ArticleCategoriesModel);
                    _typeId = 0;
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());

                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载失败");
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }


        private async void LoadArticle(ArticleCategoriesModel m)
        {
            try
            {
                m.loadVis = Visibility.Visible;
                m.loading=true;
                var cid = m.id;
                if (m.selectIndex!=-1)
                {
                    cid = m.children[m.selectIndex].id;
                }
                string url = "https://api.bilibili.com/x/article/recommends?access_key={0}&appkey={1}&build=5250000&cid={2}&from=2&mobi_app=android&platform=android&pn={3}&ps=20&sort=0&ts={4}";
                url = string.Format(url,ApiHelper.access_key,ApiHelper.AndroidKey.Appkey,cid,m.page,ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                string re = await WebClientClass.GetResults(new Uri(url));

                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32()==0)
                {
                    if (obj["data"] != null)
                    {
                        var ls = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<ArticlesModel>>(obj["data"].ToString());
                        if (ls.Count != 0)
                        {
                            if (m.page == 1)
                            {
                                m.articles = ls;
                            }
                            else
                            {
                                foreach (var item in ls)
                                {
                                    m.articles.Add(item);
                                }
                            }
                            m.page++;
                        }
                    }
                    else {
                        Utils.ShowMessageToast("加载完了");
                    }
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载失败");
            }
            finally
            {
                m.loadVis = Visibility.Collapsed;
                m.loading = false;
            }
        }


        private void cb_Change_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item= (sender as ComboBox).DataContext as ArticleCategoriesModel;
            item.page = 1;
            item.articles = null;
            LoadArticle(item);

        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as ArticleCategoriesModel;
            if (!item.loading)
            {
                LoadArticle(item);
            }



        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Width>=700)
            {
                int i = Convert.ToInt32(availableSize.Width / 400);
                if (i > 3)
                {
                    i = 3;
                }
                bor_Width.Width = availableSize.Width / i - 24;
            }
            else
            {
                bor_Width.Width = availableSize.Width - 24;
            }
            return base.MeasureOverride(availableSize);
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
          
            var sv = sender as ScrollViewer;
            if (sv.VerticalOffset== sv.ScrollableHeight)
            {
                var item = sv.DataContext as ArticleCategoriesModel;
                if (!item.loading)
                {
                    LoadArticle(item);
                }
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ArticlesModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage),item.id);
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item= pivot.SelectedItem as ArticleCategoriesModel;
            if (item.selectIndex==-1)
            {
                item.selectIndex = 0;
                LoadArticle(item);
            }
        }
    }

    public class ArticleCategoriesModel : INotifyPropertyChanged
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string name { get; set; }

      
        public List<ArticleCategoriesModel> children { get; set; }

      
        public int page { get; set; } = 1;
        public bool loading { get; set; } = false;


        private int _selectIndex = -1;
        public int selectIndex
        {
            get { return _selectIndex; }
            set { _selectIndex = value; thisPropertyChanged("selectIndex"); }
        }

        private Visibility _loadVis= Visibility.Collapsed;
        public Visibility loadVis {
            get { return _loadVis; }
            set { _loadVis = value; thisPropertyChanged("loadVis"); }
        }
        private ObservableCollection<ArticlesModel> _articles;
        public ObservableCollection<ArticlesModel> articles
        {
            get { return _articles; }
            set { _articles = value; thisPropertyChanged("articles"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }


}
