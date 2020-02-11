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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SubjectPage : Page
    {
        public SubjectPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
            if (e.NavigationMode == NavigationMode.New)
            {
                SPID = int.Parse((e.Parameter as object[])[0].ToString());
                grid_Info.DataContext = null;
                pivot.ItemsSource = null;
                txt_Desc.MaxLines = 2;
                GetSpInfo();
            }

        }
        int SPID = 0;
        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        private async void GetSpInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
               
                string url = string.Format("http://api.bilibili.com/sp?spid={0}&type=json&appkey={1}", SPID, ApiHelper.AndroidKey.Appkey);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                SpInfoModel model = JsonConvert.DeserializeObject<SpInfoModel>(results);
                grid_Info.DataContext = model;
                List<SpSeasonModel> list = new List<SpSeasonModel>();
                if (model.isbangumi == 0)
                {
                    list.Add(new SpSeasonModel() { Header = "相关视频", list = await GetList(SPID.ToString(), string.Empty) });
                }
                else
                {
                    if (model.isbangumi == 2 && model.season == null)
                    {
                        list.Add(new SpSeasonModel() { Header = "剧集", list = await GetList(SPID.ToString(), "0") });
                    }
                    else
                    {
                        foreach (var item in model.season)
                        {
                            list.Add(new SpSeasonModel() { Header = item.season_name, list = await GetList(SPID.ToString(), item.season_id.ToString()) });
                        }
                    }
                    list.Add(new SpSeasonModel() { Header = "相关视频", list = await GetList(SPID.ToString(), string.Empty) });
                }
                pivot.ItemsSource = list;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取信息失败", 3000);
                //throw;
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<List<SpVideoModel>> GetList(string sid, string seasonId)
        {
            try
            {
                List<SpVideoModel> list = new List<SpVideoModel>();
                // pr_Load.Visibility = Visibility.Visible;

              
                string url = string.Empty;
                if (seasonId.Length == 0)
                {
                    url = string.Format("http://api.bilibili.com/spview?_device=android&appkey={0}&build=418000&mobi_app=android&platform=android&spid={1}", ApiHelper.AndroidKey.Appkey, sid);
                    url += "&sign=" + ApiHelper.GetSign(url);
                }
                else
                {
                    url = string.Format(" http://api.bilibili.com/spview?_device=android&appkey={0}&bangumi=2&build=418000&mobi_app=android&platform=android&season_id={1}&spid={2}", ApiHelper.AndroidKey.Appkey, seasonId, sid);
                    url += "&sign=" + ApiHelper.GetSign(url);
                }
                string results = await WebClientClass.GetResults(new Uri(url));
                SpVideoModel model = JsonConvert.DeserializeObject<SpVideoModel>(results);
                JObject jo = JObject.Parse(model.list.ToString());
                for (int i = 0; i < model.count; i++)
                {
                    list.Add(JsonConvert.DeserializeObject<SpVideoModel>(jo[i.ToString()].ToString()));
                }
                //List<SpVideoModel> ls= JsonConvert.DeserializeObject<List<SpVideoModel>>(model.list.ToString());
                return list;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取信息失败", 3000);
                return null;
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), (e.ClickedItem as SpVideoModel).aid);
           // this.Frame.Navigate(typeof(VideoViewPage), (e.ClickedItem as SpVideoModel).aid);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private void txt_Desc_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (txt_Desc.MaxLines != 0)
            {
                txt_Desc.MaxLines = 0;

            }
            else
            {
                txt_Desc.MaxLines = 2;
            }
        }
    }

    public class SpInfoModel
    {
        public int spid { get; set; }
        public string title { get; set; }
        public string lastupdate_at { get; set; }
        public string create_at { get; set; }
        public string alias { get; set; }
        public string cover { get; set; }
        public string description { get; set; }

        public long view { get; set; }
        public long attention { get; set; }
        public int isbangumi { get; set; }//0为False,2为电视剧
        public List<SpInfoModel> season { get; set; }
        public int season_id { get; set; }
        public string season_name { get; set; }
        public string index_cover { get; set; }
        //http://api.bilibili.com/spview?_device=android&_hwid=bd2e7034b953cffe&_ulv=10000&access_key=95d163e95853c1eeb96a74cbaddaa073&appkey=c1b107428d337928&build=418000&mobi_app=android&platform=android&spid=25154&sign=4cb832a0a9f0348b469c74de1c2a58df

    }

    public class SpSeasonModel
    {
        public string Header { get; set; }
        public List<SpVideoModel> list { get; set; }
        public Visibility IsNull
        {
            get
            {
                if (list == null || list.Count == 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

    }

    public class SpVideoModel
    {
        public int code { get; set; }
        public object list { get; set; }
        public int count { get; set; }
        public string aid { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public string cid { get; set; }
    }

}
