using BiliBili.UWP.Controls;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.ChannelModels;
using BiliBili.UWP.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.FindMore
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DynamicTopicPage : Page
    {
        public DynamicTopicPage()
        {
            this.InitializeComponent();
          
            channel = new Channel();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        string tag = "";
        bool _loadDynamic = false;
    
        int page = 1;
        int channel_id = 0;
        string channel_name;
        Channel channel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New)
            {
                base.OnNavigatedTo(e);
                var par = (e.Parameter as object[])[0].ToString();
                if ((e.Parameter as object[]).Length>=2)
                {
                    channel_id = (e.Parameter as object[])[1].ToInt32();
                }
                channel_name = par;
                channel_id = 0;
                txt_Header.Text = "#" + par + "#";
                tag = Uri.EscapeDataString(par);

                ls_dynamic.ClearData();
                ls_videos.ItemsSource = null;
                page = 1;
                pr_Load.Visibility = Visibility.Visible;
                await Task.Delay(200);
                await GetTab();
                GetFeed();
                GetDynamic();
                pr_Load.Visibility = Visibility.Collapsed;
            }
         
        }
       
        private async void GetDynamic()
        {
            try
            {

                pr_Load.Visibility = Visibility.Visible;
                _loadDynamic = true;
             
                string url = string.Format("https://api.vc.bilibili.com/topic_svr/v1/topic_svr/topic_new?topic_name={0}&_device=android&access_key={1}&appkey={2}&build=5250000&mobi_app=android&platform=android&qn=32&src=bilih5&ts={3}",
                tag,ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan_2);

                if (ls_dynamic.Count() != 0)
                {
                    url = string.Format("https://api.vc.bilibili.com/topic_svr/v1/topic_svr/topic_history?topic_name={0}&offset_dynamic_id={4}&_device=android&access_key={1}&appkey={2}&build=5250000&mobi_app=android&platform=android&qn=32&src=bilih5&ts={3}",
               tag, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan_2,ls_dynamic.GetLastDynamicId());
                }

                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                results = results.Replace("default", "_default");
                DynamicModel dynamicModel = JsonConvert.DeserializeObject<DynamicModel>(results);
                if (dynamicModel.code == 0)
                {
                    if (dynamicModel.data.cards == null)
                    {
                        Utils.ShowMessageToast("没有更多动态了");
                        return;
                    }
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
        private async Task GetTab()
        {
            var data = await channel.GetChannelTab(channel_id, channel_name);
            if (data.success)
            {
                channel_id = data.data.id;
                if (data.data.is_atten==1)
                {
                    btn_Follow.Visibility = Visibility.Collapsed;
                    btn_UnFollow.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_Follow.Visibility = Visibility.Visible;
                    btn_UnFollow.Visibility = Visibility.Collapsed;
                }
            }


        }
      
        private async void GetFeed()
        {
            var data = await channel.GetChannelFeeds(channel_id, channel_name,page);
            if (data.success)
            {
                if (data.data.Count==0)
                {
                    Utils.ShowMessageToast("没有更多了");
                    return;
                }
                if (ls_videos.ItemsSource==null)
                {
                    ls_videos.ItemsSource = data.data;
                }
                else
                {
                    foreach (var item in data.data)
                    {
                        (ls_videos.ItemsSource as ObservableCollection<ChannelFeedModel>).Add(item);
                    }
                }
                page++;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            int num = 2;
            if (availableSize.Width>500)
            {
                num = (int)availableSize.Width / 160;
            }
            bor_width3.Width = (availableSize.Width - (num * 18)) / num;
            bor_height.Width = bor_width3.Width * (10 / 16);
            return base.MeasureOverride(availableSize);
        }

        private void PullToRefreshBox_RefreshInvoked_1(DependencyObject sender, object args)
        {
            ls_dynamic.ClearData();
            GetDynamic();
        }

        private async void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            RepostDialog repostDialog = new RepostDialog(txt_Header.Text);
            await repostDialog.ShowAsync();
        }

        private void ls_videos_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item= e.ClickedItem as ChannelFeedModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), item.param);
        }

        private void btn_loadMore_Click(object sender, RoutedEventArgs e)
        {
            GetFeed();
        }

        private async void btn_Follow_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            var data = await channel.FollowChannel(channel_id);
            if (data.success)
            {
                btn_Follow.Visibility = Visibility.Collapsed;
                btn_UnFollow.Visibility = Visibility.Visible;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }

        private async void btn_UnFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            var data = await channel.CancelFollowChannel(channel_id);
            if (data.success)
            {
                btn_Follow.Visibility = Visibility.Visible;
                btn_UnFollow.Visibility = Visibility.Collapsed;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }

        private void ls_dynamic_LoadMore(object sender, string e)
        {
            if (_loadDynamic)
            {
                return;
            }
            GetDynamic();
        }

        private void ls_dynamic_Refresh(object sender, EventArgs e)
        {

        }
    }
}
