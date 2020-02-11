using BiliBili.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    enum ChatType
    {
        New,
        Old
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyMessagePage : Page
    {
        public MyMessagePage()
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
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            timer.Stop();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            GetMessage();
        }

        DispatcherTimer timer;
        
        private void btn_HF_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = Convert.ToInt32((sender as Button).Tag);
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            switch (pivot.SelectedIndex)
            {
                case 0:
                    GetReply();
                    break;
                case 1:
                    GetAtme();
                    break;
                case 2:
                    GetZan();
                    break;
                case 3:
                    GetNotify();
                    break;
                case 4:
                    GetChatMe();
                    break;
                default:
                    break;
            }
        }

        private async void GetReply()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
             
                string url = string.Format("http://message.bilibili.com/api/notify/query.replyme.list.do?_device=wp&&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=410005&data_type=1&page_size=40&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                MessageReplyModel model = JsonConvert.DeserializeObject<MessageReplyModel>(results);
                if (model.code == 0)
                {
                    List<MessageReplyModel> list = JsonConvert.DeserializeObject<List<MessageReplyModel>>(model.data.ToString());
                    foreach (var item in list)
                    {
                        MessageReplyModel models = JsonConvert.DeserializeObject<MessageReplyModel>(item.publisher.ToString());
                        item.mid = models.mid;
                        item.name = models.name;
                        item.face = models.face;
                    }
                    list_Reply.ItemsSource = list;
                }
                else
                {
                    Utils.ShowMessageToast("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private async void GetAtme()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
               
                string url = string.Format("http://message.bilibili.com/api/notify/query.atme.list.do?_device=android&&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=518000&data_type=1&page_size=40&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                MessageReplyModel model = JsonConvert.DeserializeObject<MessageReplyModel>(results);
                if (model.code == 0)
                {
                    List<MessageReplyModel> list = JsonConvert.DeserializeObject<List<MessageReplyModel>>(model.data.ToString());
                    foreach (var item in list)
                    {
                        MessageReplyModel models = JsonConvert.DeserializeObject<MessageReplyModel>(item.publisher.ToString());
                        item.mid = models.mid;
                        item.name = models.name;
                        item.face = models.face;
                    }
                    list_AtMe.ItemsSource = list;
                }
                else
                {
                    Utils.ShowMessageToast("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private async void GetZan()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
               
                string url = string.Format("http://message.bilibili.com/api/notify/query.praiseme.list.do?_device=android&&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=518000&data_type=1&page_size=40&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                MessageReplyModel model = JsonConvert.DeserializeObject<MessageReplyModel>(results);
                if (model.code == 0)
                {
                    List<MessageReplyModel> list = JsonConvert.DeserializeObject<List<MessageReplyModel>>(model.data.ToString());
                    foreach (var item in list)
                    {
                        MessageReplyModel models = JsonConvert.DeserializeObject<MessageReplyModel>(item.publisher.ToString());
                        item.mid = models.mid;
                        item.name = models.name;
                        item.face = models.face;
                    }
                    list_Zan.ItemsSource = list;
                }
                else
                {
                    Utils.ShowMessageToast("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private async void GetNotify()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
               
                //http://message.bilibili.com/api/notify/query.sysnotify.list.do?_device=android&_hwid=68fc5d795c256cd1&_ulv=10000&access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&data_type=1&page_size=40&platform=android&ts=1461404973000&sign=fc3b4e26348a1204e2064e7712d10179
                string url = string.Format("https://message.bilibili.com/api/notify/query.sysnotify.list.do?_device=android&&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=518000&data_type=1&page_size=40&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                MessageReplyModel model = JsonConvert.DeserializeObject<MessageReplyModel>(results);
                if (model.code == 0)
                {
                    List<MessageReplyModel> list = JsonConvert.DeserializeObject<List<MessageReplyModel>>(model.data.ToString());
                    list_Notify.ItemsSource = list;
                }
                else
                {
                    Utils.ShowMessageToast("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async void GetChatMe()
        {
            try
            {
                pr_Load.Visibility = Visibility.Collapsed;
               
                // http://message.bilibili.com/api/msg/query.room.list.do?access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&page_size=100&platform=android&ts=1461404884000&sign=5e212e424761aa497a75b0fb7fbde775
                string url = string.Format("http://message.bilibili.com/api/msg/query.room.list.do?access_key={0}&actionKey=appkey&appkey={1}&build=518000&page_size=100&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                MessageChatModel model = JsonConvert.DeserializeObject<MessageChatModel>(results);
                if (model.code == 0)
                {
                    List<MessageChatModel> list = JsonConvert.DeserializeObject<List<MessageChatModel>>(model.data.ToString());
                    list_ChatMe.ItemsSource = list;
                }
                else
                {
                    Utils.ShowMessageToast("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }


        private async void GetMessage()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
               
                // http://message.bilibili.com/api/msg/query.room.list.do?access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&page_size=100&platform=android&ts=1461404884000&sign=5e212e424761aa497a75b0fb7fbde775
                string url = string.Format("https://message.bilibili.com/api/notify/query.notify.count.do?_device=android&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=518000&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                MessageModel model = JsonConvert.DeserializeObject<MessageModel>(results);
                if (model.code == 0)
                {
                    MessageModel list = JsonConvert.DeserializeObject<MessageModel>(model.data.ToString());
                    if (list.reply_me != 0)
                    {
                        bor_HF.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bor_HF.Visibility = Visibility.Collapsed;
                    }

                    if (list.at_me != 0)
                    {
                        bor_At.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bor_At.Visibility = Visibility.Collapsed;
                    }

                    if (list.praise_me != 0)
                    {
                        bor_Zan.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bor_Zan.Visibility = Visibility.Collapsed;
                    }

                    if (list.notify_me != 0)
                    {
                        bor_TZ.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bor_TZ.Visibility = Visibility.Collapsed;
                    }

                    if (list.chat_me != 0)
                    {
                        bor_SX.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bor_SX.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    Utils.ShowMessageToast("读取通知失败," + model.message, 3000);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("读取通知失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }


        private void list_Reply_ItemClick(object sender, ItemClickEventArgs e)
        {
            string aid = Regex.Match((e.ClickedItem as MessageReplyModel).link, @"http://www.bilibili.com/video/av(.*?)/").Groups[1].Value;
            if (aid.Length != 0)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), aid);
                //this.Frame.Navigate(typeof(VideoInfoPage), aid);
                return;
            }
            Match quan = Regex.Match((e.ClickedItem as MessageReplyModel).link, @"^http://www.im9.com/post.html\?community_id=(.*?)&post_id=(.*?)$");
            if (quan.Groups.Count == 3)
            {

                //this.Frame.Navigate(typeof(QuanInfoPage), new QuanziListModel() { community_id = Convert.ToInt32(quan.Groups[1].Value), post_id = Convert.ToInt32(quan.Groups[2].Value) });
                return;
            }


        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            // MessageReplyModel model = (sender as HyperlinkButton).DataContext as MessageReplyModel;
            //this.Frame.Navigate(typeof(UserInfoPage), model.mid);
        }

        private void list_Notify_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as MessageReplyModel).link != null)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), (e.ClickedItem as MessageReplyModel).link);
                //this.Frame.Navigate(typeof(WebViewPage), (e.ClickedItem as MessageReplyModel).link);
            }

        }

        private void list_ChatMe_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ChatPage), new object[] { (e.ClickedItem as MessageChatModel).rid, ChatType.Old });
           // this.Frame.Navigate(typeof(ChatPage), new object[] { (e.ClickedItem as MessageChatModel).rid, ChatType.Old });
            // Utils.ShowMessageToast("聊天功能待完成...",3000);
        }



    }
}
