using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.Web.Http;
using Windows.UI.Core;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RankPage : Page
    {
        public RankPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.Frame.Name == "bg_Frame")
            {
                g.Background = null;
            }
        }
        private void btn_back_Click(object sender, RoutedEventArgs e)
        {

            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            
        }
        public async Task GetQZRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-03.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_QZ.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }
        }
        public async Task GetYCRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/origin-03.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_YC.ItemsSource = ReList;

            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }
        }
        public async Task GetFJRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("https://api.bilibili.com/pgc/web/rank/list?day=3&season_type=1"));
                JObject obj = JObject.Parse(results);
                if (obj["code"].ToInt32()==0)
                {
                    List<BangumiRankModel> ls = JsonConvert.DeserializeObject<List<BangumiRankModel>>(obj["result"]["list"].ToString());
                    QQ_Rank_FJ.ItemsSource = ls;
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }
               
              
               

            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }
        }
        public async Task GetDHRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-1.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_DH.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }

        public async Task GetYYRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-3.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_YY.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }
        public async Task GetWDRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-129.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }

                QQ_Rank_WD.ItemsSource = ReList;

            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }

        public async Task GetYXRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-4.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_YX.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }
        public async Task GetKJRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-36.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_KJ.ItemsSource = ReList;


            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }

        public async Task GetYLRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-5.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_YL.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }
        public async Task GetSHRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-160.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_SH.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }
        public async Task GetGCRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-119.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_GC.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }


        public async Task GetDYRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-23.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_DY.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }
        public async Task GetDSJRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-11.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_DSJ.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }
        public async Task GetSSRank()
        {
            try
            {
               
                string results = await WebClientClass.GetResults(new Uri("http://www.bilibili.com/index/rank/all-3-155.json"));
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                InfoModel model1 = JsonConvert.DeserializeObject<InfoModel>(model.rank.ToString());
                List<InfoModel> ls = JsonConvert.DeserializeObject<List<InfoModel>>(model1.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 30; i++)
                {
                    if (i < 3)
                    {
                        ls[i].forColor = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;;
                    }
                    ls[i].num = i + 1;
                    ReList.Add(ls[i]);
                }
                QQ_Rank_SS.ItemsSource = ReList;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }

        }


        private void QQ_Rank_YC_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), ((InfoModel)e.ClickedItem).aid);
        }
        bool QZLoad = false;
        bool YCLoad = false;
        bool FJLoad = false;
        bool DHLoad = false;
        bool YYLoad = false;
        bool WDLoad = false;
        bool YXLoad = false;
        bool KJLoad = false;
        bool YLLoad = false;
        bool GCLoad = false;
        bool DYLoad = false;
        bool DSJLoad = false;
        bool SSLoad = false;
        bool SHLoad = false;
        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    if (!YCLoad)
                    {
                        await Task.Delay(200);
                        pr_loading.Visibility = Visibility.Visible;
                        await GetYCRank();
                        YCLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 1:
                    if (!QZLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetQZRank();
                        QZLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 2:
                    if (!FJLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetFJRank();
                        FJLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 3:
                    if (!DHLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetDHRank();
                        DHLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 4:
                    if (!YYLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetYYRank();
                        YYLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 5:
                    if (!WDLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetWDRank();
                        WDLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 6:
                    if (!YXLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetYXRank();
                        YXLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 7:
                    if (!KJLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetKJRank();
                        KJLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 8:
                    if (!SHLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetSHRank();
                        SHLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 9:
                    if (!YLLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetYLRank();
                        YLLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 10:
                    if (!GCLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetGCRank();
                        GCLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 11:
                    if (!DYLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetDYRank();
                        DYLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 12:
                    if (!DSJLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetDSJRank();
                        DSJLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 13:
                    if (!SSLoad)
                    {
                        pr_loading.Visibility = Visibility.Visible;
                        await GetSSRank();
                        SSLoad = true;
                        pr_loading.Visibility = Visibility.Collapsed;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int d = Convert.ToInt32(this.ActualWidth / 400);
            if (d > 3)
            {
                d = 3;
            }
            bor_Width.Width = this.ActualWidth / d - 22;
        }

        private void QQ_Rank_FJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as BangumiRankModel).season_id.ToString());
            
        }
    }

    public class InfoModel
    {
        public object list { get; set; }
        public object rank { get; set; }
        private string _pic;

        public string pic
        {
            get { return _pic+"@200w.jpg"; }
            set { _pic = value; }
        }


        public string title { get; set; }
        public string play { get; set; }
        public string author { get; set; }
        public string video_review { get; set; }
        public string description { get; set; }
        public string mid { get; set; }
        public string aid { get; set; }
        public int num { get; set; }
        public SolidColorBrush forColor { get; set; }

        //用于直播
        public object data { get; set; }
        public string room_id { get; set; }
        public string online { get; set; }
        public string uname { get; set; }
        public string cover { get; set; }
        public string face { get; set; }
        public string roomid { get; set; }
    }
    public class BangumiRankModel
    {
        public string badge { get; set; }
        private string _cover;

        public string cover
        {
            get { return _cover + "@120h.jpg"; }
            set { _cover = value; }
        }
        public string title { get; set; }
        public int season_id { get; set; }
        public int rank { get; set; }
        public int pts { get; set; }
        public BangumiRankStatModel stat { get; set; }
    }
    public class BangumiRankStatModel
    {
       
        public long danmaku { get; set; }
        public long follow { get; set; }
        public long view { get; set; }
    }

}
