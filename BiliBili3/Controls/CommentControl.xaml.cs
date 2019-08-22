using BiliBili3.Helper;
using BiliBili3.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BiliBili3.Controls
{
    public sealed partial class CommentControl : UserControl
    {
        public delegate void OpenUserHandle(string id);
        public event OpenUserHandle OpenUser;
        public CommentControl()
        {
            this.InitializeComponent();
          
        }
        public void InitializeComment(int _pageNum, int _pageNum_Reply, string aid)
        {
            sv.ChangeView(null, 0, null);
            sv_Reply.ChangeView(null, 0, null);
            pageNum = _pageNum;
            pageNum_Reply = _pageNum_Reply;
            _aid = aid;
            LoadComment();
            
        }
        public void DisposableComment()
        {
            ListView_Comment_Hot.ItemsSource = null;
            ListView_Comment_New.Items.Clear();
            ListView_Comment_Reply.Items.Clear();
        }
        public void InitializeCommentHot(string aid)
        {
            pageNum = 1;
            pageNum_Reply = 1;
            //txt_Hot.Visibility = Visibility.Collapsed;
            //txt_New.Visibility = Visibility.Collapsed;
            ListView_Comment_New.Visibility = Visibility.Collapsed;
            grid_Send.Visibility = Visibility.Collapsed;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            _aid = aid;
            LoadComment_JustHot();
        }
        public async void LoadComment_JustHot()
        {
            try
            {
                GetEmojis();
                _Loading = true;
                pr_load.Visibility = Visibility.Visible;
                string url = "http://api.bilibili.com/x/reply?type=1&sort=2&oid=" + _aid + "&pn=" + pageNum + "&ps=5&platform=wp&appkey=" + ApiHelper.AndroidKey.Appkey + "&access_key=" + ApiHelper.access_key + "&rnd" + new Random().Next(1, 9999);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                NewCommentModel model = JsonConvert.DeserializeObject<NewCommentModel>(results);
                ListView_Comment_Hot.ItemsSource = model.data.replies;
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    Utils.ShowMessageToast("检查你的网络连接！", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("读取评论失败!\r\n" + ex.Message, 3000);
                }
            }
            finally
            {
                pr_load.Visibility = Visibility.Collapsed;
                _Loading = false;
            }

        }



        public int pageNum = 1;
        public int pageNum_Reply = 1;
        public string _aid = "";
        public async void LoadComment()
        {
            try
            {
              
                GetEmojis();
                _Loading = true;
                pr_load.Visibility = Visibility.Visible;
                string url = "http://api.bilibili.com/x/reply?type=1&sort=0&oid=" + _aid + "&pn=" + pageNum + "&ps=20&platform=wp&appkey=" + ApiHelper.AndroidKey.Appkey + "&access_key=" + ApiHelper.access_key + "&rnd" + new Random().Next(1, 9999);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                NewCommentModel model = JsonConvert.DeserializeObject<NewCommentModel>(results);
                if (model.code==0)
                {
                    if (pageNum == 1)
                    {
                        ListView_Comment_Hot.ItemsSource = model.data.hots;
                        ListView_Comment_New.Items.Clear();
                    }
                    if (model.data.replies.Count != 0)
                    {
                        model.data.replies.ForEach(x => ListView_Comment_New.Items.Add(x));
                        pageNum++;
                    }
                    else
                    {
                        Utils.ShowMessageToast("加载完了...", 3000);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(model.message, 3000);
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
                    Utils.ShowMessageToast("读取评论失败!\r\n" + ex.Message, 3000);
                }
            }
            finally
            {
                pr_load.Visibility = Visibility.Collapsed;
                _Loading = false;
            }

        }
        public async void LoadComment_Reply(string aid)
        {
            try
            {
              
                _Loading_Reply = true;
                pr_load.Visibility = Visibility.Visible;

            
                string url = "http://api.bilibili.com/x/reply/reply?oid=" + aid + "&pn=" + pageNum_Reply + "&ps=20&root=" + rpid + "&type=1&access_key=" + ApiHelper.access_key + "&appkey=" + ApiHelper.AndroidKey.Appkey + "&rnd" + new Random().Next(1, 9999); ;
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                NewCommentModel model = JsonConvert.DeserializeObject<NewCommentModel>(results);
                if (model.code==0)
                {
                    if (pageNum_Reply == 1)
                    {
                        ListView_Comment_Reply.Items.Clear();
                    }
                    if (model.data.replies.Count != 0)
                    {
                        model.data.replies.ForEach(x => ListView_Comment_Reply.Items.Add(x));
                        pageNum_Reply++;
                    }
                    else
                    {
                        Utils.ShowMessageToast("加载完了...", 3000);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(model.message, 3000);
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
                    Utils.ShowMessageToast("读取评论失败!\r\n" + ex.Message, 3000);
                }
            }
            finally
            {
                pr_load.Visibility = Visibility.Collapsed;
                _Loading_Reply = false;
            }

        }
        public async void LoadHotComment(string aid)
        {
            try
            {
                pr_load.Visibility = Visibility.Visible;

                string url = "http://api.bilibili.com/x/reply?type=1&sort=1&oid=" + aid + "&pn=1&ps=20&platform=wp&appkey=" + ApiHelper.AndroidKey.Appkey + "&access_key=" + ApiHelper.access_key + "&rnd" + new Random().Next(1, 9999); ;
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                NewCommentModel model = JsonConvert.DeserializeObject<NewCommentModel>(results);
                if (model.data.replies.Count != 0)
                {
                    model.data.replies.ForEach(x => ls_Hot.Items.Add(x));
                }
                else
                {
                    Utils.ShowMessageToast("加载完了...", 3000);
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
                    Utils.ShowMessageToast("读取热门评论失败!\r\n" + ex.Message, 3000);
                }
            }
            finally
            {
                pr_load.Visibility = Visibility.Collapsed;
            }

        }
        bool _Loading = false;
        bool _Loading_Reply = false;
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!_Loading)
                {
                    LoadComment();
                }
            }
        }
        private void GetEmojis()
        {
            if (ApiHelper.emoji == null)
            {
                ApiHelper.SetEmojis();
                //GetEmojis();
            }
            else
            {
                pivot.ItemsSource = ApiHelper.emoji;
                pivot_Reply.ItemsSource = ApiHelper.emoji;
            }
        }
        //List<EmojiModel> emojis;
        //private async void GetEmojis()
        //{
        //    try
        //    {
        //        wc = new WebClientClass();
        //        string url = "http://api.bilibili.com/x/v2/reply/emojis";
        //        string results = await wc.GetRvesults(new Uri(url));
        //        FaceModel model = Jso nConvert.DeserializeObject<FaceModel>(results);
        //        pivot.ItemsSource = model.data;
        //        ApiHelper.emojis = new List<EmojiModel>();
        //        model.data.ForEach(x => x.emojis.ForEach(y => ApiHelper.emojis.Add(y)));
        //    }
        //    catch (Exception)
        //    {
        //        Utils.ShowMessageToast("读取图片表情失败！",3000);
        //    }

        //}

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            txt_Comment.Text += (sender as Button).Content.ToString();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            txt_Comment.Text += (e.ClickedItem as EmojiModel).name;
        }

        private void ListView_Comment_Hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            //string str = ((FrameworkElement)e.OriginalSource).DataContext.ToString();
            //Copy.Text = str;

            //menuFlyout.ShowAt(ListView_Comment_Hot, e.GetPosition(this.lvVerses));
        }
        string rpid = "";
        string userId = "";
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            menu_Zan.Text = ((sender as Grid).DataContext as NewCommentModel).Action;
            rpid = ((sender as Grid).DataContext as NewCommentModel).rpid.ToString();
            userId = ((sender as Grid).DataContext as NewCommentModel).mid;
            menuFlyout.ShowAt(ListView_Comment_Hot, e.GetPosition(ListView_Comment_Hot));
        }

        private async void btn_Send_Click(object sender, RoutedEventArgs e)
        {
           
            if (UserManage.IsLogin())
            {
                try
                {
                    string uri = string.Format("http://api.bilibili.com/x/reply/add?_device=wp&build={2}&platform=wp&appkey={0}&access_key={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.access_key,ApiHelper.build);
                    uri += "&sign=" + ApiHelper.GetSign(uri);
                    Uri ReUri = new Uri(uri);
                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    string QuStr = "plat=6&jsonp=jsonp&message=" + Uri.EscapeDataString(txt_Comment.Text) + "&type=1&oid=" + _aid;
                    var response = await hc.PostAsync(ReUri, new HttpStringContent(QuStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        pageNum = 1;
                        LoadComment();
                        Utils.ShowMessageToast("已发送评论!", 3000);
                        txt_Comment.Text = "";
                    }
                    else
                    {
                        Utils.ShowMessageToast(json["message"].ToString(), 3000);
                    }

                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("评论时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);

            }
        }

        private async void menu_Zan_Click(object sender, RoutedEventArgs e)
        {
            //string rpid = ((sender as HyperlinkButton).DataContext as CommentModel).rpid;
           
            if (UserManage.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/reply/action");

                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    string sendString = "";
                    if (menu_Zan.Text == "赞同")
                    {
                        sendString = "jsonp=jsonp&oid=" + _aid + "&type=1&rpid=" + rpid + "&action=1";
                    }
                    else
                    {
                        sendString = "jsonp=jsonp&oid=" + _aid + "&type=1&rpid=" + rpid + "&action=0";
                    }
                    var response = await hc.PostAsync(ReUri, new HttpStringContent(sendString, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        if (menu_Zan.Text == "赞同")
                        {
                            menu_Zan.Text = "取消赞";
                        }
                        else
                        {
                            menu_Zan.Text = "赞同";
                        }
                        Utils.ShowMessageToast("成功", 3000);
                    }
                    else
                    {
                        if ((int)json["code"] == 12007)
                        {
                            Utils.ShowMessageToast("已经点赞了", 3000);
                        }
                        else
                        {
                            Utils.ShowMessageToast("点赞失败" + result, 3000);
                        }
                    }

                }
                catch (Exception)
                {
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录!", 3000);
            }
        }

        private void menu_Reply_Click(object sender, RoutedEventArgs e)
        {

            if (this.ActualWidth <= 500)
            {
                sp_View.OpenPaneLength = this.ActualWidth;
            }
            else
            {
                sp_View.OpenPaneLength = 500;
            }

            sp_View.IsPaneOpen = true;
            grid_reply.Visibility = Visibility.Visible;
            grid_hot.Visibility = Visibility.Collapsed;
            pageNum_Reply = 1;
            sv_Reply.ChangeView(null, 0, null);
            LoadComment_Reply(_aid);
        }

        private void btn_HotMore_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                sp_View.OpenPaneLength = this.ActualWidth;

            }
            else
            {
                sp_View.OpenPaneLength = 500;
            }
            sp_View.IsPaneOpen = true;
            grid_reply.Visibility = Visibility.Collapsed;
            grid_hot.Visibility = Visibility.Visible;
            LoadHotComment(_aid);
        }

        #region 回复
        private void Grid_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            menu_Zan_Reply.Text = ((sender as Grid).DataContext as NewCommentModel).Action;
            rpid = ((sender as Grid).DataContext as NewCommentModel).rpid.ToString();
            reply_name = ((sender as Grid).DataContext as NewCommentModel).member.uname;
            //reply_id = info.rpid;
            userId = ((sender as Grid).DataContext as NewCommentModel).mid;
            reply_id = ((sender as Grid).DataContext as NewCommentModel).root_str;
            menuFlyout_Reply.ShowAt(ListView_Comment_Reply, e.GetPosition(ListView_Comment_Reply));
        }

        private void sv_Reply_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_Reply.VerticalOffset == sv_Reply.ScrollableHeight)
            {
                if (!_Loading_Reply)
                {
                    LoadComment_Reply(_aid);
                }
            }
        }

        private async void menu_Zan_Reply_Click(object sender, RoutedEventArgs e)
        {
            //string rpid = ((sender as HyperlinkButton).DataContext as CommentModel).rpid;
          
            if (UserManage.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/reply/action");

                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    string sendString = "";
                    if (menu_Zan_Reply.Text == "赞同")
                    {
                        sendString = "jsonp=jsonp&oid=" + _aid + "&type=1&rpid=" + rpid + "&action=1";
                    }
                    else
                    {
                        sendString = "jsonp=jsonp&oid=" + _aid + "&type=1&rpid=" + rpid + "&action=0";
                    }
                    var response = await hc.PostAsync(ReUri, new HttpStringContent(sendString, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        if (menu_Zan_Reply.Text == "赞同")
                        {
                            menu_Zan_Reply.Text = "取消赞";
                        }
                        else
                        {
                            menu_Zan_Reply.Text = "赞同";
                        }
                        Utils.ShowMessageToast("成功", 3000);
                    }
                    else
                    {
                        if ((int)json["code"] == 12007)
                        {
                            Utils.ShowMessageToast("已经点赞了", 3000);
                        }
                        else
                        {
                            Utils.ShowMessageToast("点赞失败" + result, 3000);
                        }
                    }

                }
                catch (Exception)
                {
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录!", 3000);
            }
        }
        #endregion
        bool at = false;
        string reply_name = "";
        string reply_id = "";
        private async void btn_Send_Reply_Click(object sender, RoutedEventArgs e)
        {
           
            if (UserManage.IsLogin())
            {
                try
                {
                    string uri = string.Format("http://api.bilibili.com/x/reply/add?_device=wp&build={2}&platform=wp&appkey={0}&access_key={1}", ApiHelper.AndroidKey.Appkey, ApiHelper.access_key,ApiHelper.build);
                    uri += "&sign=" + ApiHelper.GetSign(uri);
                    Uri ReUri = new Uri(uri);
                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    at = txt_Comment_Reply.Text.Contains("回复 @");
                    string QuStr = "parent=" + rpid + "&root=" + rpid + "&plat=6&message=" + Uri.EscapeDataString(txt_Comment_Reply.Text) + "&type=1&oid=" + _aid;
                    if (at)
                    {
                        QuStr = "parent=" + reply_id + "&root=" + reply_id + "&plat=6&message=" + Uri.EscapeDataString(txt_Comment_Reply.Text) + "&type=1&oid=" + _aid;
                    }
                    var response = await hc.PostAsync(ReUri, new HttpStringContent(QuStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        pageNum_Reply = 1;
                        LoadComment_Reply(_aid);
                        Utils.ShowMessageToast("已发送评论!", 3000);
                        txt_Comment_Reply.Text = "";
                    }
                    else
                    {
                        Utils.ShowMessageToast(json["message"].ToString(), 3000);
                    }

                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("评论时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txt_Comment_Reply.Text += (sender as Button).Content.ToString();
        }

        private void GridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            txt_Comment_Reply.Text += (e.ClickedItem as EmojiModel).name;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

            GetEmojis();
        }

        private void menu_Reply_Reply_Click(object sender, RoutedEventArgs e)
        {

            txt_Comment_Reply.Text = string.Format("回复 @{0} :", reply_name);
        }

        private void menu_User_Reply_Click(object sender, RoutedEventArgs e)
        {
            OpenUser(userId);
        }

        private void menu_User_Click(object sender, RoutedEventArgs e)
        {
            OpenUser(userId);
        }

        private void sp_View_PaneClosed(SplitView sender, object args)
        {
        }

    }
    public class NewCommentModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public NewCommentModel data { get; set; }
        public List<NewCommentModel> hots { get; set; }

        public List<NewCommentModel> replies { get; set; }
        public NewCommentModel member { get; set; }
        public NewCommentModel content { get; set; }

        public NewCommentModel level_info { get; set; }
        public NewCommentModel vip { get; set; }
        public int vipType { get; set; }
        public SolidColorBrush vip_co
        {
            get
            {
                if (vipType == 2)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    if (SettingHelper.Get_Theme()=="Dark")
                    {
                        return new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }
        public string avatar { get; set; }
        public string uname { get; set; }
        public string floor { get; set; }
        public string rpid { get; set; }
        public long ctime { set; get; }
        public int plat { get; set; }
        public string root_str { get; set; }
        public string Plat
        {
            get
            {
                switch (plat)
                {
                    case 2:
                        return "来自 Android";
                    case 3:
                        return "来自 IOS";
                    case 4:
                        return "来自 WindowsPhone";
                    case 6:
                        return "来自 Windows";
                    default:
                        return "";
                }
            }
        }
        public string mid { get; set; }
        public string time
        {
            get
            {
                //DateTime dtStart = new DateTime(1970, 1, 1);
                //long lTime = long.Parse(ctime + "0000000");
                ////long lTime = long.Parse(textBox1.Text);
                //TimeSpan toNow = new TimeSpan(lTime);
                //return dtStart.Add(toNow).ToLocalTime().ToString();

                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(ctime + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = TimeSpan.FromSeconds(ctime);
                DateTime dt = dtStart.Add(toNow).ToLocalTime();
                TimeSpan span = DateTime.Now - dt;
                if (span.TotalDays > 7)
                {
                    return dt.ToString("yyyy-MM-dd");
                }
                else
                if (span.TotalDays > 1)
                {
                    return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
                }
                else
                if (span.TotalHours > 1)
                {
                    return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                }
                else
                if (span.TotalMinutes > 1)
                {
                    return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                }
                else
                if (span.TotalSeconds >= 1)
                {
                    return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                }
                else
                {
                    return "1秒前";
                }


            }
        }
        public string rcount { get; set; }
        public string like { get; set; }
        public int current_level { get; set; }
        public int action { get; set; }
        public string Action
        {
            get
            {
                if (action == 0)
                {
                    return "赞同";
                }
                else
                {
                    return "取消赞";
                }
            }
        }

        public string LV
        {
            get
            {
                switch (level_info.current_level)
                {
                    case 0:
                        return "ms-appx:///Assets/MiniIcon/ic_lv0_large.png";
                    case 1:
                        return "ms-appx:///Assets/MiniIcon/ic_lv1_large.png";
                    case 2:
                        return "ms-appx:///Assets/MiniIcon/ic_lv2_large.png";
                    case 3:
                        return "ms-appx:///Assets/MiniIcon/ic_lv3_large.png";
                    case 4:
                        return "ms-appx:///Assets/MiniIcon/ic_lv4_large.png";
                    case 5:
                        return "ms-appx:///Assets/MiniIcon/ic_lv5_large.png";
                    case 6:
                        return "ms-appx:///Assets/MiniIcon/ic_lv6_large.png";
                    default:
                        return "";
                }
            }
        }

        public RichTextBlock text
        {
            get
            {
                if (message != null)
                {
                    string input = message;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    MatchCollection mc = Regex.Matches(input, @"\[(.*?)\]");
                    foreach (Match item in mc)
                    {

                        input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Border><Image Source=""{0}"" Width=""36"" Height=""36""/></Border></InlineUIContainer>", ApiHelper.emojis.First(x => x.name == item.Groups[0].Value).url));
                    }

                    //生成xaml
                    var xaml = string.Format(@"<RichTextBlock HorizontalAlignment=""Stretch"" TextWrapping=""Wrap"" Margin=""5"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
    xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"" >
                                          <Paragraph>{0}</Paragraph>
                                      </RichTextBlock>", input);
                    var p = (RichTextBlock)XamlReader.Load(xaml);
                    return p;
                }
                else
                {
                    return null;
                }
            }

        }



    }
    //public class FaceModel
    //{
    //    public int code
    //    {
    //        get; set;
    //    }
    //    public List<FaceModel> data { get; set; }
    //    public int pid { get; set; }
    //    public string pname { get; set; }
    //    public int pstate { get; set; }
    //    public string purl { get; set; }
    //    public List<EmojiModel> emojis { get; set; }
    //}
    //public class EmojiModel
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //    public int state { get; set; }
    //    public string url { get; set; }
    //    public string remark { get; set; }
    //    public string vname
    //    {
    //        get
    //        {
    //            return Regex.Match(name, @"\[.*?_(.*?)\]").Groups[1].Value;
    //        }
    //    }
    //}
}
