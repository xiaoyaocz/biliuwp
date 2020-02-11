using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class LiveMyMedalPage : Page
    {
        public LiveMyMedalPage()
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
                LoadData();
            }
        }
        private async void LoadData()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;

                string url = $"https://api.live.bilibili.com/AppUser/medal?access_key={ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                LiveMedalModel m = JsonConvert.DeserializeObject<LiveMedalModel>(results);
                if (m.code == 0)
                {
                    if (m.data.Count == 0)
                    {
                        NoDT.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        NoDT.Visibility = Visibility.Collapsed;
                    }
                    list.ItemsSource = m.data;
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
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }

       

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.ToString()=="卸下")
            {
                Cancel();
            }
            else
            {
                Add(((sender as Button).DataContext as LiveMedalModel).medal_id);
            }
        }
        private async void Cancel()
        {

            try
            {
                pr_Load.Visibility = Visibility.Visible;

                string url = $"http://live.bilibili.com/AppUser/canelMedal?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                JObject m = JObject.Parse(results);
                if ( (int)m["code"] == 0)
                {
                    Utils.ShowMessageToast("操作成功", 3000);
                    LoadData();
                }
                else
                {
                    Utils.ShowMessageToast(m["message"].ToString(), 3000);
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
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }
        private async void Add(int id)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;

                string url = $"http://live.bilibili.com/AppUser/wearMedal?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&platform=android";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.PostResults(new Uri(url), "medal_id="+ id + "&");
                JObject m = JObject.Parse(results);
                if ((int)m["code"] == 0)
                {
                    Utils.ShowMessageToast("操作成功", 3000);
                    LoadData();
                }
                else
                {
                    Utils.ShowMessageToast(m["message"].ToString(), 3000);
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
                pr_Load.Visibility = Visibility.Collapsed;

            }
        }

    }

    public class LiveMedalModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<LiveMedalModel> data{ get; set; }

        public int medal_id { get; set; }
        public string medal_name { get; set; }
        public string level { get; set; }
        public string uname { get; set; }
        public string intimacy { get; set; }
        public string next_intimacy { get; set; }
        public int status { get; set; }
        public string Status
        {
            get
            {
                if (status==1)
                {
                    return "卸下";
                }
                else
                {
                    return "佩戴";
                }
            }
        }
        public string color { get; set; }
        public SolidColorBrush _Color
        {
            get
            {
                try
                {
                    color = Convert.ToInt32(color).ToString("X2");
                    if (color.StartsWith("#"))
                        color = color.Replace("#", string.Empty);
                    int v = int.Parse(color, System.Globalization.NumberStyles.HexNumber);
                    SolidColorBrush solid = new SolidColorBrush(new Color()
                    {
                        A = Convert.ToByte(255),
                        R = Convert.ToByte((v >> 16) & 255),
                        G = Convert.ToByte((v >> 8) & 255),
                        B = Convert.ToByte((v >> 0) & 255)
                    });
                    // color = solid;
                    return solid;
                }
                catch (Exception)
                {
                    SolidColorBrush solid = new SolidColorBrush(new Color()
                    {
                        A = 255,
                        R = 255,
                        G = 255,
                        B = 255
                    });
                    // color = solid;
                    return solid;
                }
            }
        }
        public int guard_type { get; set; }
        public string buff_msg { get; set; }
    }


}
