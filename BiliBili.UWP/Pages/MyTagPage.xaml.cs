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
using Newtonsoft.Json;
using System.Threading.Tasks;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.ChannelModels;
using BiliBili.UWP.Pages.FindMore;
using System.Collections.ObjectModel;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyTagPage : Page
    {
        public MyTagPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            channel = new Channel();
        }
        Channel channel;
        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
      
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (e.NavigationMode == NavigationMode.New)
            {
                await Task.Delay(200);
                LoadList();
            }
            
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            bor_width.Width = (availableSize.Width - 16 - 24 - 12) / 3;
            return base.MeasureOverride(availableSize);
        }
        private async void LoadList()
        {
            pr_Load.Visibility = Visibility.Visible;
            var data =await channel.GetFollowChannel();
            if (data.success)
            {
                grid_myatton.ItemsSource = data.data;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            pr_Load.Visibility = Visibility.Collapsed;
        }
        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadList();
        }

        private void grid_myatton_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Atten_channel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicTopicPage), new object[] {
                 item.name,
                 item.id
            });
        }
        Atten_channel selectItem;
        private void Border_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            selectItem = (sender as Border).DataContext as Atten_channel;
            menu.ShowAt(sender as Border);
        }

        private void Border_Holding(object sender, HoldingRoutedEventArgs e)
        {
            selectItem = (sender as Border).DataContext as Atten_channel;
            menu.ShowAt(sender as Border);
        }

        private async void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            var data=await channel.CancelFollowChannel(selectItem.id);
            if (data.success)
            {
                (grid_myatton.ItemsSource as ObservableCollection<Atten_channel>).Remove(selectItem);
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }
    }


    public class MyTagModel
    {
        public bool status { get; set; }
        public string message { get; set; }
        public MyTagModel data { get; set; }
        public int count { get; set; }
        public List<MyTagModel> tags { get; set; }
        public string name { get; set; }
        public string cover { get; set; }
        public int tag_id { get; set; }
        public int notify { get; set; }
        public int archive_count { get; set; }
        public string updated_ts { get; set; }
    }
    public class TagVideoModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public int total { get; set; }

        public List<TagVideoModel> result { get; set; }
        public string aid { get; set; }
        public string title { get; set; }
        public string play { get; set; }
        public string video_review { get; set; }
        public string pic { get; set; }
    }
}
