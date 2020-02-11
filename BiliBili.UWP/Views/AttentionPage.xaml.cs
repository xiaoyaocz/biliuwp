using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using BiliBili.UWP.Pages;
using Newtonsoft.Json;
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
using Windows.Data.Xml.Dom;
using System.Net;
using Windows.UI.Notifications;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Windows.UI;
using BiliBili.UWP.Pages.FindMore;
using System.ComponentModel;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Documents;
using BiliBili.UWP.Controls;
// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AttentionPage : Page
    {
        public AttentionPage()
        {
            this.InitializeComponent();
            //dynamicItemDataTemplateSelector.resource = this.Resources;
            //photoItemDataTemplateSelector.resource = this.Resources;
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                await Task.Delay(200);
                MessageCenter.Logined += MessageCenter_Logined;
                if (!UserManage.IsLogin())
                {
                    DT_noLogin.Visibility = Visibility.Visible;
                    pivot.Visibility = Visibility.Collapsed;
                    b_btn_Refresh.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pivot.Visibility = Visibility.Visible;
                    DT_noLogin.Visibility = Visibility.Collapsed;
                    if (ls_video.Count() == 0)
                    {
                        _page = 1;
                        GetDynamicVideos();
                    }

                }
            }

        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            MessageCenter.Logined -= MessageCenter_Logined;
        }
        private void MessageCenter_Logined()
        {
            if (UserManage.IsLogin())
            {
                DT_noLogin.Visibility = Visibility.Collapsed;
                pivot.Visibility = Visibility.Visible;

                GetDynamicVideos();
            }
        }
        int _page = 1;
        bool _Loading = false;
        private async void GetDynamicVideos()
        {
            try
            {
                _Loading = true;
                pr_Load.Visibility = Visibility.Visible;
                string url = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_new?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&qn=32&rsp_type=2&src=bili&ts={ApiHelper.GetTimeSpan_2}&type_list=8%2C512%2C4099&uid={ApiHelper.GetUserId()}&update_num_dy_id=0";
                if (ls_video.Count()!=0)
                {
                    url = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_history?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&offset_dynamic_id={ls_video.GetLastDynamicId()}&page={_page}&platform=android&qn=32&rsp_type=2&src=bili&ts={ApiHelper.GetTimeSpan_2}&type_list=8%2C512%2C4099&uid={ApiHelper.GetUserId()}";
                }
                else
                {
                    _page = 0;
                }
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                results = results.Replace("default", "_default");
                DynamicModel dynamicModel = JsonConvert.DeserializeObject<DynamicModel>(results);
                if (dynamicModel.code == 0)
                {
                    ObservableCollection<DynamicCardsModel> cards = new ObservableCollection<DynamicCardsModel>();
                    foreach (var item in dynamicModel.data.cards)
                    {
                        if (item.desc.type != 32)
                        {
                            cards.Add(item);
                        }
                    }

                    ls_video.LoadData(cards);
                    _page++;
                }
                else
                {
                    Utils.ShowMessageToast(dynamicModel.msg);
                }

            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867 || ex.HResult == -2147012889)
                {
                    Utils.ShowMessageToast("无法连接服务器，请检查你的网络连接", 3000);
                }
                else
                {

                    Utils.ShowMessageToast("动态加载错误", 3000);
                }
            }
            finally
            {
                _Loading = false;
           
                pr_Load.Visibility = Visibility.Collapsed;
              
            }
        }


        private async void User_Login_Click(object sender, RoutedEventArgs e)
        {
            LoginDialog loginDialog = new LoginDialog();
            await loginDialog.ShowAsync();
            //MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LoginPage));
        }
     
       
        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedIndex == 0)
            {
                ls_video.ClearData();
                GetDynamicVideos();
            }
            if (pivot.SelectedIndex == 1 )
            {
                ls_dynamic.ClearData();
                GetDynamic();
            }
            if (pivot.SelectedIndex == 2 )
            {
                ls_hot.ClearData();
                GetHot();
            }
        }

      
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (pivot.SelectedIndex == 1&&ls_dynamic.Count()==0)
            {
                GetDynamic();
            }
            if (pivot.SelectedIndex == 2 & ls_hot.Count() == 0)
            {
                GetHot();
            }



        }
        bool _loadDynamic = false;
        private async void GetDynamic()
        {
            try
            {

                pr_Load.Visibility = Visibility.Visible;
                _loadDynamic = true;
                var next = "";
                var u = "dynamic_new";
                if (ls_dynamic.Count() != 0)
                {
                    u = "dynamic_history";
                    next = "&offset_dynamic_id=" + ls_dynamic.GetLastDynamicId();
                }

                string url = string.Format("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/{5}?access_key={0}&appkey={1}&build=5250000&mobi_app=android&platform=android&qn=32&rsp_type=2&src=bilih5&ts={2}&type=268435455&uid={3}{4}",
                ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan_2, ApiHelper.GetUserId(), next, u);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                results = results.Replace("default", "_default");
                DynamicModel dynamicModel = JsonConvert.DeserializeObject<DynamicModel>(results);
                if (dynamicModel.code == 0)
                {
                    ObservableCollection<DynamicCardsModel> cards = new ObservableCollection<DynamicCardsModel>();
                    foreach (var item in dynamicModel.data.cards)
                    {
                        if (item.desc.type != 32)
                        {
                            cards.Add(item);
                        }
                    }

                    ls_dynamic.LoadData(cards);
                }
                else
                {
                    Utils.ShowMessageToast(dynamicModel.msg);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取综合动态失败");
            }
            finally
            {
                _loadDynamic = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        int _pageHot = 1;
        bool _loadHot = false;
        private async void GetHot()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                _loadHot = true;

                string url = string.Format("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/recommend?_device=android&access_key={0}&appkey={1}&build=5250000&mobi_app=android&page={2}&platform=android&qn=32&src=bilih5&ts={3}&uid={4}",
                ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _pageHot, ApiHelper.GetTimeSpan_2, ApiHelper.GetUserId());
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));


                results = results.Replace("default", "_default");
                DynamicModel dynamicModel = JsonConvert.DeserializeObject<DynamicModel>(results);
                if (dynamicModel.code == 0)
                {
                    ObservableCollection<DynamicCardsModel> cards = new ObservableCollection<DynamicCardsModel>();
                    foreach (var item in dynamicModel.data.cards)
                    {
                        if (item.desc.type != 32)
                        {
                            cards.Add(item);
                        }
                    }

                    ls_hot.LoadData(cards, _pageHot == 1);

                    _pageHot++;
                }
                else
                {
                    Utils.ShowMessageToast(dynamicModel.msg);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("读取热门动态失败");
            }
            finally
            {
                _loadHot = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private async void btn_sendDynamic_Click(object sender, RoutedEventArgs e)
        {
            RepostDialog repostDialog = new RepostDialog();
            await repostDialog.ShowAsync();
            if (repostDialog.SendState)
            {
                ls_dynamic.ClearData();
                GetDynamic();
            }
        }

        private void DynamicControls_Refresh(object sender, EventArgs e)
        {
            ls_dynamic.ClearData();
            GetDynamic();
        }

        private void DynamicControls_LoadMore(object sender, string e)
        {
            if (_loadDynamic)
            {
                return;
            }
            GetDynamic();
        }

        private void ls_hot_Refresh(object sender, EventArgs e)
        {
            ls_hot.ClearData();
            GetHot();
        }

        private void ls_hot_LoadMore(object sender, string e)
        {
            if (_loadHot)
            {
                return;
            }
            GetHot();
        }

        private void ls_video_Refresh(object sender, EventArgs e)
        {

        }

        private void ls_video_LoadMore(object sender, string e)
        {
            if (_Loading)
            {
                return;
            }
            GetDynamicVideos();
        }
    }


  

}
