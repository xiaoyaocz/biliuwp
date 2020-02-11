using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    public sealed partial class MusicCollectMenuPage : Page
    {
        public MusicCollectMenuPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }



        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode== NavigationMode.New)
            {
                _type = (e.Parameter as object[])[0].ToInt32();
                LoadMenus();
            }
        }


        private async void LoadMenus()
        {

            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = "https://api.bilibili.com/audio/music-service-c/users/{0}/menus?access_key={1}&appkey={2}&build=5250000&mobi_app=android&page_index=1&page_size=1000&platform=android&ts={3}&type={4}";
                url = string.Format(url, ApiHelper.GetUserId(), ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, _type);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                JObject obj = JObject.Parse(results);
               
                if (obj["code"].ToInt32() == 0)
                {
                    if (obj["data"]["list"]!=null)
                    {
                        List<MusicHomeMenuModel> m = JsonConvert.DeserializeObject<List<MusicHomeMenuModel>>(obj["data"]["list"].ToString());
                        list_menus.ItemsSource = m;
                    }
                }
                else
                {
                    Utils.ShowMessageToast(obj["msg"].ToString());
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载收藏失败");
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }



        int _type = 1;
        private void list_menus_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item= e.ClickedItem as MusicHomeMenuModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicMenuPage), item.menuId);
        }



    }
}
