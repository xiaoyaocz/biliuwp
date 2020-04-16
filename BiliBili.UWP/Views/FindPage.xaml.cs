using BiliBili.UWP.Pages;
using BiliBili.UWP.Pages.FindMore;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FindPage : Page
    {
        public FindPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.New)
            {
                if (list_Hot.Items.Count==0)
                {
                    GetHotKeyword();
                }
               
            }
           
        }

        public async void GetHotKeyword()
        {
            try
            {

                string results = await WebClientClass.GetResults(new Uri("https://s.search.bilibili.com/main/hotword"));
                HotModel model = JsonConvert.DeserializeObject<HotModel>(results);
                List<HotModel> ban = JsonConvert.DeserializeObject<List<HotModel>>(model.list.ToString());

                list_Hot.ItemsSource = ban;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取搜索热词失败", 3000);
            }

        }
        private void btn_setting_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo( NavigateMode.Info,typeof(SettingPage));
        }

        private void Find_btn_Part_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ChannelPage));
        }

        private void list_Hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(SearchV2Page),new object[] { (e.ClickedItem as HotModel).keyword });
            
        }

        private void autoSug_Box_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            txt_auto_Find.Text = args.SelectedItem as string;
        }

        public async Task<ObservableCollection<String>> GetSugges(string text)
        {
            try
            {
                string results = await WebClientClass.GetResults(new Uri("http://s.search.bilibili.com/main/suggest?suggest_type=accurate&sub_type=tag&main_ver=v1&term=" + text));
                JObject json = JObject.Parse(results);
                // json["result"]["tag"].ToString();
                List<SuggesModel> list = JsonConvert.DeserializeObject<List<SuggesModel>>(json["result"]["tag"].ToString());
                ObservableCollection<String> suggestions = new ObservableCollection<string>();
                foreach (SuggesModel item in list)
                {
                    suggestions.Add(item.value);
                }
                return suggestions;
            }
            catch (Exception)
            {
                return new ObservableCollection<string>();
            }

        }
        public class SuggesModel
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        private async void autoSug_Box_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.Length != 0)
            {
                sender.ItemsSource = await GetSugges(sender.Text);
            }
            else
            {
                sender.ItemsSource = null;
            }
        }

        private void autoSug_Box_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(txt_auto_Find.Text))
            {
                Utils.ShowMessageToast("关键字不能为空");
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(SearchV2Page), new object[] { txt_auto_Find .Text});
        }

        private void Find_btn_Rank_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(RankPage));
        }

        private void Find_btn_Random_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), new Random().Next(10000, 4999999).ToString());
        }

        private void Find_btn_Topic_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(TopicPage));
        }

        private void Find_btn_Activity_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ActivityPage));
        }

        private void Find_btn_Shop_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "https://show.bilibili.com/m/platform/home.html");
        }

        private void btn_localHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LocalHistoryPage));
        }

     
        private async void btn_Tag_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录！",3000);
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MyTagPage));
        }

        private void btn_QR_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(QRPage));
        }

        private void Find_btn_Article_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticlePage));
        }
    }
    public class HotModel
    {
        public object list { get; set; }
        public string keyword { get; set; }
        public string status { get; set; }
    }
}
