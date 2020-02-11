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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AddTagPage : Page
    {
        public AddTagPage()
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
            if (e.NavigationMode== NavigationMode.New )
            {
                ls_tag.SelectedIndex = 0;
            }
        }
        private async void LoadTags(string rid)
        {
            //gv_Tags
            try
            {
                gv_Tags.Items.Clear();
                pr_Load.Visibility = Visibility.Visible;
                string results = await WebClientClass.GetResults(new Uri(string.Format("http://api.bilibili.com/x/tag/hots?rid={0}&type=0&jsonp=json", rid)));
                AllTagsModel my = JsonConvert.DeserializeObject<AllTagsModel>(results);
                if (my.code==0)
                {
                    //List<AllTagsModel> ls = new List<AllTagsModel>();
                    my.data.ForEach(x=>x.tags.ForEach(y=> {
                        if (y.is_atten==0)
                        {
                            gv_Tags.Items.Add(y);
                            //ls.Add(y);
                        }
                        }));
                   // gv_Tags.ItemsSource = ls;
                }
                else
                {
                    Utils.ShowMessageToast("读取失败 " + my.message, 3000);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("读取失败 \r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }

        }

        private void ls_tag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ls_tag.SelectedIndex==-1)
            {
                return;
            }

            if ((ls_tag.SelectedItem as ListViewItem).Tag!=null)
            {
                not_my.Visibility = Visibility.Visible;
                my.Visibility = Visibility.Collapsed;
                LoadTags((ls_tag.SelectedItem as ListViewItem).Tag.ToString());
            }
            else
            {
                GetTag();
                //GetLikeTag();
                not_my.Visibility = Visibility.Collapsed;
                my.Visibility = Visibility.Visible;
            }
           
        }

        private async void GetTag()
        {
            try
            {
                gv.Items.Clear();
                pr_Load.Visibility = Visibility.Visible;
                string results = await WebClientClass.GetResults(new Uri("http://space.bilibili.com/ajax/tags/getSubList?mid=" + ApiHelper.GetUserId()));
                MyTagModel my = JsonConvert.DeserializeObject<MyTagModel>(results);
                if (my.status)
                {
                    my.data.tags.ForEach(x=> gv.Items.Add(x));
                   // gv.ItemsSource = my.data.tags;
                    
                }
                else
                {
                    Utils.ShowMessageToast("读取失败 " + my.message, 3000);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("读取失败 \r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async void gv_Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var info = e.ClickedItem as AllTagsModel;
                string results = await WebClientClass.PostResults(new Uri("http://api.bilibili.com/x/tag/subscribe/add"), "jsonp=jsonp&tag_id="+info.tag_id, "http://www.bilibili.com/");
                JObject obj = JObject.Parse(results);
                if ((int)obj["code"]==0)
                {
                    Utils.ShowMessageToast("订阅成功", 3000);
                    gv_Tags.Items.Remove(e.ClickedItem);
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString(), 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("订阅失败",3000);
            }
           
            
        }

        private async void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var info = e.ClickedItem as MyTagModel;
                string results = await WebClientClass.PostResults(new Uri("http://api.bilibili.com/x/tag/subscribe/cancel"), "jsonp=jsonp&tag_id=" + info.tag_id, "http://www.bilibili.com/");
                JObject obj = JObject.Parse(results);
                if ((int)obj["code"] == 0)
                {
                    Utils.ShowMessageToast("已取消订阅", 3000);
                    gv.Items.Remove(e.ClickedItem);
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString(), 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("取消订阅失败", 3000);
            }
        }


        private async void GetLikeTag()
        {
            try
            {
                gv_like.Items.Clear();
                pr_Load.Visibility = Visibility.Visible;
                string url = string.Format("http://app.bilibili.com/x/feed/subscribe/tags?access_key={0}&appkey={1}&build=434300&mobi_app=android&platform=wp&pn=1&ps=60&ts={2}000",ApiHelper.access_key,ApiHelper.AndroidKey.Appkey,ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                LikeTagsModel my = JsonConvert.DeserializeObject<LikeTagsModel>(results);
                if (my.code==0)
                {
                    my.data.recommend.ForEach(x => gv_like.Items.Add(x));
                    // gv.ItemsSource = my.data.tags;
                  
                }
                else
                {
                    Utils.ShowMessageToast("读取失败 " + my.message, 3000);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("读取失败 \r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async void gv_like_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var info = e.ClickedItem as LikeTagsModel;
                string results = await WebClientClass.PostResults(new Uri("http://api.bilibili.com/x/tag/subscribe/add"), "jsonp=jsonp&tag_id=" + info.tag_id, "http://www.bilibili.com/");
                JObject obj = JObject.Parse(results);
                if ((int)obj["code"] == 0)
                {
                    Utils.ShowMessageToast("订阅成功", 3000);
                    MyTagModel m = new MyTagModel() { tag_id = info.tag_id,name=info.tag_name};
                    gv_like.Items.Remove(e.ClickedItem);
                    gv.Items.Add(m);
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString(), 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("订阅失败", 3000);
            }
        }

        private void btn_refresh_Click_1(object sender, RoutedEventArgs e)
        {
           // GetLikeTag();
        }
    }

    public class AllTagsModel
    {
        public int code { get; set; }
        public string message { get; set; }

        public List<AllTagsModel> data { get; set; }

        public int rid { get; set; }
        public List<AllTagsModel> tags { get; set; }
        public int tag_id { get; set; }
        public string tag_name { get; set; }
        public int is_atten { get; set; }
    }
    public class LikeTagsModel
    {
        public int code { get; set; }
        public string message { get; set; }

        public LikeTagsModel data { get; set; }

        public List<LikeTagsModel> recommend { get; set; }
        public int tag_id { get; set; }
        public string tag_name { get; set; }
    }

}
