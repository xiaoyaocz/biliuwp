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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BiliBili3.Models;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili3.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LivePartInfoPage : Page
    {
        public LivePartInfoPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.New)
            {
                await Task.Delay(200);
                _pid = (e.Parameter as object[])[0].ToString();
              
                switch (int.Parse(_pid))
                {
                    case 1:
                        top_txt_Header.Text = "单机联机";
                        break;
                    case 2:
                        top_txt_Header.Text = "御宅文化";
                        break;
                    case 3:
                        top_txt_Header.Text = "网络游戏";
                        break;
                    case 4:
                        top_txt_Header.Text = "电子竞技";
                        break;
                  
                    case 6:
                        top_txt_Header.Text = "生活娱乐";
                        break;
                    case 7:
                        top_txt_Header.Text = "放映厅";
                        break;
                    case 8:
                        top_txt_Header.Text = "萌宅推荐";
                        break;
                    case 9:
                        top_txt_Header.Text = "绘画专区";
                        break;
                    case 10:
                        top_txt_Header.Text = "唱见舞见";
                        break;
                    case 11:
                        top_txt_Header.Text = "手机直播";
                        break;
                    case 12:
                        top_txt_Header.Text = "手游直播";
                        break;
                    case 99:
                        top_txt_Header.Text = "精彩轮播";
                        break;
                    default:
                        break;
                }
                grid_tag.ItemsSource = null;
                _TJPage = 1;
                _sort = "recommend";
                gv.Items.Clear();
                await  LoadType(_pid);
                GetTJ();
            }
        }
        string _pid;
        bool _loadingTag = false;
        private async Task LoadType(string pid)
        {
            pr_Load.Visibility = Visibility.Visible;
            List<LivePartTagModel> ls = new List<LivePartTagModel>();
            ls.Add(new LivePartTagModel() { tag_name = "全部" });
            try
            {
                string url = string.Format("http://live.bilibili.com/AppIndex/tags?access_key={0}&appkey={1}&build=434000&mobi_app=android&platform=android",ApiHelper.access_key,ApiHelper.AndroidKey.Appkey);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results =await WebClientClass.GetResults(new Uri(url));
               
                JObject obj = JObject.Parse(results);
                if ((int)obj["code"]==0)
                {
                    List<string> str = JsonConvert.DeserializeObject<List<string>>(obj["data"][pid].ToString());
                    str.ForEach(x => ls.Add(new LivePartTagModel() { tag_name= x}));
                    
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString(),3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("标签读取失败", 3000);
            }
            finally
            {
                grid_tag.ItemsSource = ls;
                _loadingTag = true;
                grid_tag.SelectedIndex = 0;
                _loadingTag = false;
            }

        }
        string _sort = "recommend";
        int _TJPage = 1;
        bool _TJLoading = false;
        private async void GetTJ()
        {
            try
            {
                _TJLoading = true;
                pr_Load.Visibility = Visibility.Visible;
               

                string url = string.Format("http://live.bilibili.com/mobile/rooms?access_key={0}&appkey={1}&area_id={2}&build=434000&mobi_app=android&page={3}&platform=android&sort={4}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey,_pid, _TJPage, _sort);
                if (grid_tag.SelectedIndex!=0&& grid_tag.SelectedIndex!=-1)
                {
                    url += "&tag="+Uri.EscapeDataString((grid_tag.SelectedItem as LivePartTagModel).tag_name);
                }
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                AllLiveModel m = JsonConvert.DeserializeObject<AllLiveModel>(results);
                if (m.code == 0)
                {
                    if (m.data.Count != 0)
                    {
                        m.data.ForEach(x => gv.Items.Add(x));
                        _TJPage++;
                    }
                    else
                    {
                        Utils.ShowMessageToast("加载完了...", 3000);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(m.message, 3000);
                }

            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    Utils.ShowMessageToast("检查你的网络连接！", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("发生错误\r\n" + ex.Message, 3000);
                }
            }
            finally
            {
                _TJLoading = false;
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_TJLoading)
                {
                    GetTJ();
                }
            }
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!_TJLoading)
            {
                GetTJ();
            }
        }

        private void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), (e.ClickedItem as AllLiveModel).room_id);
        }

        private void btn_TJ_Click(object sender, RoutedEventArgs e)
        {

          
            foreach (ToggleMenuFlyoutItem item in menu.Items)
            {
                item.IsChecked = false;
            }
            _sort = (sender as ToggleMenuFlyoutItem).Tag.ToString();
            (sender as ToggleMenuFlyoutItem).IsChecked = true;
            _TJPage = 1;
            gv.Items.Clear();
            GetTJ();
        }

        private  void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            _TJPage = 1;
            gv.Items.Clear();
            GetTJ();
        }

        private void btn_Type_Checked(object sender, RoutedEventArgs e)
        {
            grid_tag.Visibility = Visibility.Visible;
        }

        private void btn_Type_Unchecked(object sender, RoutedEventArgs e)
        {
            grid_tag.Visibility = Visibility.Collapsed;
        }

        private  void grid_tag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingTag|| grid_tag.ItemsSource==null)
            {
                return;
            }
            _TJPage = 1;
            gv.Items.Clear();
            GetTJ();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                //ViewBox_num.Width = ActualWidth / 2 - 18;
                //ViewBox2_num.Width = ActualWidth / 2 - 18;
                bor_Width2.Width = ActualWidth / 2 - 20;
                Grid.SetRow(com_bar, 2);

                com_bar.HorizontalAlignment = HorizontalAlignment.Stretch;
                com_bar.VerticalAlignment = VerticalAlignment.Bottom;

            }
            else
            {
                //int i = Convert.ToInt32(ActualWidth / 200);
                //ViewBox_num.Width = ActualWidth / i - 13;
                //ViewBox2_num.Width = ActualWidth / i - 13;
                int i = Convert.ToInt32(ActualWidth / 200);
                bor_Width2.Width = ActualWidth / i - 15;
                Grid.SetRow(com_bar, 0);
                Grid.SetRowSpan(com_bar, 2);
                com_bar.HorizontalAlignment = HorizontalAlignment.Right;
                com_bar.VerticalAlignment = VerticalAlignment.Top;
            }


           
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }

    public class LivePartTagModel
    {
        public string tag_name { get; set; }
        public string tag_value { get; set; }
    }

}
