using BiliBili.UWP.Api;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
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
    public sealed partial class LiveCenterPage : Page
    {
        LiveCenter  liveCenter;
        public LiveCenterPage()
        {
            this.InitializeComponent();
            liveCenter = new LiveCenter();
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
           
            LoadInfo();
        }
        private async void LoadInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;

                var m = await liveCenter.GetUserInfo();
                if (m.success)
                {

                    //m.data.uname = ApiHelper.userInfo.name;
                    //m.data.pic = ApiHelper.userInfo.face;
                    if (m.data.vip==1)
                    {
                        isvip.Visibility = Visibility.Visible;
                        novip.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        isvip.Visibility = Visibility.Collapsed;
                        novip.Visibility = Visibility.Visible;
                    }

                    this.DataContext = m.data;
                    //bor_UL.Background = GetColor(m.data.user_level_color);
                    if (m.data.wearTitle!=null)
                    {
                        bor_title.Visibility = Visibility.Visible;
                    }
                    if (m.data.medal != null)
                    {
                        bor_Medal.Visibility = Visibility.Visible;
                        //bor_Medal.Background = GetColor(m.data.medal.color);
                    }
                    else
                    {
                        bor_Medal.Visibility = Visibility.Collapsed;
                    }

                    if (m.data.isSign==1)
                    {
                        btn_sign.Visibility = Visibility.Collapsed;
                        signed.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btn_sign.Visibility = Visibility.Visible;
                        signed.Visibility = Visibility.Collapsed;
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
                    Utils.ShowMessageToast("发生错误\r\n" , 3000);
                }
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                
            }
        }
     
        private void btn_myroom_Click(object sender, RoutedEventArgs e)
        {
            var info = this.DataContext as LiveCenterModel;
            if (info==null)
            {
                return;
            }
            if (info.room_id=="0")
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), info.room_id);
        }

        private void btn_myfeed_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveFeedPage));
        }

        private void btn_myhistory_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveHistoryPage));
        }

        private async void btn_sign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var api = new Api.Live.LiveCenterAPI();
                var result =await api.DoSign().Request();
                SignModel model = JsonConvert.DeserializeObject<SignModel>(result.results);
                if (model.code == 0)
                {
                    SignModel data = JsonConvert.DeserializeObject<SignModel>(model.data.ToString());

                    btn_sign.Visibility = Visibility.Collapsed;
                    signed.Visibility = Visibility.Visible;

                    await new MessageDialog(data.text).ShowAsync();
                    
                }
                else
                {
                    await new MessageDialog(model.msg).ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("签到时发生错误\r\n"+ ex.Message).ShowAsync();
            }

        }

        private void btn_buyVIP_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://live.bilibili.com/i#to-vip");
        }

        private void btn_Capsuletoy_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://live.bilibili.com/pages/playground/index#!/capsule-toy");
        }

        private void btn_DHH_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://live.bilibili.com/hd/guard-desc?menu=0");
        }

        private void btn_myMedal_Click(object sender, RoutedEventArgs e)
        {
            //http://link.bilibili.com/p/center/index#/user-center/wearing-center/my-medal
           // MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://link.bilibili.com/p/center/index#/user-center/wearing-center/my-medal");
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveMyMedalPage));
        }

        private void btn_myTitle_Click(object sender, RoutedEventArgs e)
        {
           // MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://link.bilibili.com/p/center/index#/user-center/wearing-center/library");
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveMyTitlePage));
        }

        private void btn_Hjjv_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info,typeof(WebPage), "https://link.bilibili.com/p/center/index?#/user-center/my-info/award");
        }

        private void btn_buyGold_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("暂时没开发，请到其它平台操作",3000);
        }

        private void btn_buySlider_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://live.bilibili.com/exchange");
            //http://live.bilibili.com/exchange
           // Utils.ShowMessageToast("暂时没开发，请到其它平台操作", 3000);
        }
    }
}
