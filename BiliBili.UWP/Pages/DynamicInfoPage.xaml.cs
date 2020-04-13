using BiliBili.UWP.Controls;
using BiliBili.UWP.Pages.FindMore;
using BiliBili.UWP.Pages.User;
using BiliBili.UWP.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DynamicInfoPage : Page
    {
        public DynamicInfoPage()
        {
            this.InitializeComponent();
          
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        DynamicCardsModel _data;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //if (e.NavigationMode== NavigationMode.New)
            //{
            if ((e.Parameter as object[])[0] is DynamicCardsModel)
            {
                _data = (e.Parameter as object[])[0] as DynamicCardsModel;
                ObservableCollection<DynamicCardsModel> ls = new ObservableCollection<DynamicCardsModel>();
                ls.Add(_data);
                dynamic.HideLoadMoreButton();
                dynamic.LoadData(ls,true);
                ls_repost.ItemsSource = null;
                if (_data.desc.user_profile != null)
                {
                    if (_data.desc.user_profile.info.uid.ToString() == ApiHelper.GetUserId())
                    {
                        btn_Delete.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btn_Delete.Visibility = Visibility.Collapsed;
                    }
                }
                LoadRepost();

                comment.ClearComment();
                InitializeComment();
            }
            else
            {
                var hid= (e.Parameter as object[])[0].ToString();

                LoadAlbumData(hid);

            }
           
            //}


        }

        private async void LoadAlbumData(string id)
        {
            string dynamic_id = id;
            if (id.Length<15)
            {
                 dynamic_id = await GetDynamicId(id);
                if (dynamic_id == "")
                {
                    return;
                }



            }
            
            LoadDynamicData(dynamic_id);
            //try
            //{
            //    DynamicCardsModel dynamicCardsModel = new DynamicCardsModel();

            //    string url = string.Format("http://api.vc.bilibili.com/link_draw/v1/doc/detail?access_key={0}&appkey={1}&build=5250000&doc_id={2}&mobi_app=android&platform=android&src=bilih5&trace_id=20180225170000026&ts={3}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, id, ApiHelper.GetTimeSpan);
            //    url += "&sign=" + ApiHelper.GetSign(url);

            //    string re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
            //    re = re.Replace("upload_time", "_upload_time");
            //    JObject obj = JObject.Parse(re);
            //    if (obj["code"].ToInt32() == 0)
            //    {
            //        DynamicFeed1Model dynamicFeed1Model = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicFeed1Model>(obj["data"].ToString());
            //        _data = new DynamicCardsModel() {
            //             feed1= dynamicFeed1Model,
            //             desc=new DynamicCardsModel()
            //             {
            //                 timestamp = dynamicFeed1Model.item.upload_timestamp,
            //                 type =2,
            //                 uid= dynamicFeed1Model.user.uid,
            //                 dynamic_id=Convert.ToInt64( dynamic_id),
            //                 user_profile = new user_profileModel()
            //                 {
            //                     info = new user_profileModel()
            //                     {
            //                         uid = dynamicFeed1Model.user.uid,
            //                         face = dynamicFeed1Model.user.head_url,
            //                         uname = dynamicFeed1Model.user.name
            //                     }
            //                 },
            //                 rid = dynamicFeed1Model.item.doc_id,

            //                 like= dynamicFeed1Model.item.vote_count,
            //             },
            //            reply = dynamicFeed1Model.item.comment_count,
            //            card = obj["data"].ToString()
            //        };
            //        ObservableCollection<DynamicCardsModel> ls = new ObservableCollection<DynamicCardsModel>();
            //        ls.Add(_data);
            //        list_dynamic.ItemsSource = ls;
            //        ls_repost.ItemsSource = null;
            //        if (_data.desc.user_profile != null)
            //        {
            //            if (_data.desc.user_profile.info.uid.ToString() == ApiHelper.GetUserId())
            //            {
            //                btn_Delete.Visibility = Visibility.Visible;
            //            }
            //            else
            //            {
            //                btn_Delete.Visibility = Visibility.Collapsed;
            //            }
            //        }
            //        LoadRepost();

            //        comment.ClearComment();
            //        InitializeComment();
            //    }
            //    else
            //    {
            //        obj["message"].ToString();
            //    }




            //}
            //catch (Exception)
            //{
            //    Utils.ShowMessageToast("无法读取动态");
            //}



        }


        private async void LoadDynamicData(string dynamic_id)
        {
            
            try
            {
                string url = string.Format("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?access_key={0}&appkey={1}&dynamic_id={2}&build=5250000&mobi_app=android&platform=android&src=bilih5&ts={3}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, dynamic_id, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                string re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
              
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    DynamicCardsModel dynamicCardsModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicCardsModel>(obj["data"]["card"].ToString());
                    _data =  dynamicCardsModel;
                    //_data = new DynamicCardsModel()
                    //{
                    //    feed1 = dynamicFeed1Model,
                    //    desc = new DynamicCardsModel()
                    //    {
                    //        timestamp = dynamicFeed1Model.item.upload_timestamp,
                    //        type = 2,
                    //        uid = dynamicFeed1Model.user.uid,
                    //        dynamic_id = Convert.ToInt64(dynamic_id),
                    //        user_profile = new user_profileModel()
                    //        {
                    //            info = new user_profileModel()
                    //            {
                    //                uid = dynamicFeed1Model.user.uid,
                    //                face = dynamicFeed1Model.user.head_url,
                    //                uname = dynamicFeed1Model.user.name
                    //            }
                    //        },
                    //        rid = dynamicFeed1Model.item.doc_id,

                    //        like = dynamicFeed1Model.item.vote_count,
                    //    },
                    //    reply = dynamicFeed1Model.item.comment_count,
                    //    card = obj["data"].ToString()
                    //};
                    ObservableCollection<DynamicCardsModel> ls = new ObservableCollection<DynamicCardsModel>();
                    ls.Add(_data);
                    dynamic.LoadData(ls,true);
                    ls_repost.ItemsSource = null;
                    if (_data.desc.user_profile != null)
                    {
                        if (_data.desc.user_profile.info.uid.ToString() == ApiHelper.GetUserId())
                        {
                            btn_Delete.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            btn_Delete.Visibility = Visibility.Collapsed;
                        }
                    }
                    LoadRepost();

                    comment.ClearComment();
                    InitializeComment();
                }
                else
                {
                    obj["message"].ToString();
                }




            }
            catch (Exception)
            {
                Utils.ShowMessageToast("无法读取动态");
            }



        }


        private async Task<string> GetDynamicId(string doc_id)
        {
            try
            {
                string url = string.Format("http://api.vc.bilibili.com/link_draw/v2/doc/dynamic_id?_device=android&appkey={0}&build=5250000&doc_id={1}&platform=android&src=bilih5", ApiHelper.AndroidKey.Appkey, doc_id);
                url += "&sign=" + ApiHelper.GetSign(url);

                string re = await WebClientClass.GetResults(new Uri(url));
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    return obj["data"]["dynamic_id"].ToString();
                }
                else
                {
                    Utils.ShowMessageToast("无法读取动态");
                }
                return "";
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("无法读取动态");
                return "";
            }
            
        }

        private void InitializeComment()
        {
            if (_data.desc.type == 1 || _data.desc.type == 4 || _data.desc.type == 2048)
            {
                comment.InitializeComment(new Controls.LoadCommentInfo()
                {
                    commentMode = Controls.CommentMode.Dynamic,
                    conmmentSortMode = Controls.ConmmentSortMode.Hot,
                    oid = _data.desc.dynamic_id.ToString()
                });
            }
            if (_data.desc.type == 2)
            {
                comment.InitializeComment(new Controls.LoadCommentInfo()
                {
                    commentMode = Controls.CommentMode.Photo,
                    conmmentSortMode = Controls.ConmmentSortMode.Hot,
                    oid = _data.desc.rid.ToString()
                });
            }
            if (_data.desc.type == 16)
            {
                comment.InitializeComment(new Controls.LoadCommentInfo()
                {
                    commentMode = Controls.CommentMode.MiniVideo,
                    conmmentSortMode = Controls.ConmmentSortMode.Hot,
                    oid = _data.desc.rid.ToString()
                });
            }

        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 1&&comment.CommentCount==0)
            {
                comment.LoadComment();
            }

        }

        bool _repostLoading = false;
        private async void LoadRepost()
        {
            //_data
            try
            {
                _repostLoading = true;
                pr_Load.Visibility = Visibility.Visible;
                noRepost.Visibility = Visibility.Collapsed;
                btn_LoadMoreRepost.Visibility = Visibility.Collapsed;

                string offset = "0";
                if (ls_repost.Items.Count != 0)
                {
                    offset = ls_repost.Items.Count.ToString();
                }
                string url = "https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/view_repost";

                string content = string.Format("_device=android&access_key={0}&appkey={1}&build=5250000&dynamic_id={2}&mobi_app=android&offset={5}&platform=android&src=bilih5&ts={3}&uid={4}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _data.desc.dynamic_id, ApiHelper.GetTimeSpan_2, ApiHelper.GetUserId(), offset);
                content += "&sign=" + ApiHelper.GetSign(content);
                var re = await WebClientClass.PostResultsUtf8(new Uri(url), content);

                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    if (obj["data"]["comments"] == null)
                    {
                        if (ls_repost.Items.Count != 0)
                        {
                            Utils.ShowMessageToast("没有更多转发了");
                        }
                        else
                        {
                            noRepost.Visibility = Visibility.Visible;
                        }
                        return;
                    }
                  
                    ObservableCollection<RepostModel> ls = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<RepostModel>>(obj["data"]["comments"].ToString());
                    if (ls_repost.Items.Count == 0)
                    {
                        ls_repost.ItemsSource = ls;
                       
                    }
                    else
                    {
                        ls.ToList().ForEach(x => (ls_repost.ItemsSource as ObservableCollection<RepostModel>).Add(x));
                    }
                    btn_LoadMoreRepost.Visibility = Visibility.Visible;


                    _data.desc.repost = obj["data"]["total_count"].ToInt32();

                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }

               

            }
            catch (Exception)
            {

                Utils.ShowMessageToast("无法读取转发信息");
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                pivot.SelectedIndex = 0;
                _repostLoading = false;
            }




        }

        private void btn_LoadMoreRepost_Click(object sender, RoutedEventArgs e)
        {
            if (!_repostLoading)
            {

                LoadRepost();
            }
        }

        private void btn_User_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as HyperlinkButton).DataContext is DynamicCardsModel)
            {
                var item = (sender as HyperlinkButton).DataContext as DynamicCardsModel;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), item.desc.user_profile.info.uid);
            }
            else
            {
                var item = (sender as HyperlinkButton).DataContext as RepostModel;
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), item.uid);
            }

        }

        private async void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                return;
            }

            try
            {
                string url = "https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/rm_rp_dyn?access_key={0}&appkey={1}&build=5250000&platform=android&ts={2}";
                url = string.Format(url,ApiHelper.access_key,ApiHelper.AndroidKey.Appkey,ApiHelper.GetTimeSpan_2);
                url += "&sign" + ApiHelper.GetSign(url);

                string content = "uid={0}&dynamic_id={1}";
                content = string.Format(content, ApiHelper.GetUserId(), _data.desc.dynamic_id,  ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan_2);
                

                var re = await WebClientClass.PostResultsUtf8(new Uri(url), content);
                Newtonsoft.Json.Linq.JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    Utils.ShowMessageToast("已经删除");
                    this.Frame.GoBack();
                }
                else
                {
                    Utils.ShowMessageToast("操作失败" + obj["message"].ToString());
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("操作发生错误");
            }


        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard("https://t.bilibili.com/"+_data.desc.dynamic_id);

            Utils.ShowMessageToast("已将地址复制到剪切板", 3000);


        }

        private void dynamic_OpenComment(object sender, EventArgs e)
        {
            comment.ShowCommentBox();
        }
    }

    public class RepostModel
    {
        public RepostModel()
        {
            ButtonCommand = new DelegateCommand();
            ButtonCommand.MyExecute = new Action<object>(ButtonClick);

        }
        public DelegateCommand ButtonCommand { get; private set; }
        private void ButtonClick(object paramenter)
        {
            //Command="{Binding ButtonCommand}"
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), paramenter);
        }

        public long uid { get; set; }
        public string comment { get; set; }
        public long ts { get; set; }
        public string face_url { get; set; }
        public string uname { get; set; }
        public string rp_dyn_id { get; set; }
        public string ctrl { get; set; }
        public displayModel display { get; set; }
      
        public RichTextBlock Content
        {
            get
            {

                if (comment == null || comment.Length == 0)
                {
                    return new RichTextBlock();
                }
                try
                {
                    string input = comment;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    if (display!=null&& display.emoji_info!=null&&display.emoji_info.emoji_details != null && display.emoji_info.emoji_details.Count != 0)
                    {
                        foreach (var item in display.emoji_info.emoji_details)
                        {
                            input = input.Replace(item.emoji_name, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", item.url));
                        }
                    }
                  
                    if (ctrl != null && ctrl.Length != 0)
                    {
                        var re = ctrl;
                        List<ctrlModel> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ctrlModel>>(re);
                        if (list != null && list.Count != 0)
                        {
                            foreach (var item in list.OrderBy(x => x.location))
                            {
                                if (item.type == 1)
                                {
                                    var d = comment.Substring(item.location, item.length);
                                    var index = input.IndexOf(d);

                                    //var s = input.Substring(index,item.length);
                                    input = input.Remove(index, item.length);
                                    var test = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" >{0}</HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.data);
                                    input = input.Insert(index, test);
                                }
                            }
                        }
                    }

                    input = input.Replace("^x$%^", "@");

                    var xaml = string.Format(@"<RichTextBlock HorizontalAlignment=""Stretch"" TextWrapping=""Wrap"" Margin=""5"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
    xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"" >
                                          <Paragraph>{0}</Paragraph>
                                      </RichTextBlock>", input);
                    var p = (RichTextBlock)XamlReader.Load(xaml);

                    p.IsTextSelectionEnabled = true;
                    p.Margin = new Thickness(0, 4, 0, 4);

                    return p;
                }
                catch (Exception)
                {
                    var txt = new RichTextBlock()
                    {
                        Margin = new Thickness(0, 4, 0, 4),
                        IsTextSelectionEnabled = true
                    };
                    var p = new Paragraph();
                    p.Inlines.Add(new Run() { Text = comment });
                    txt.Blocks.Add(p);
                    return txt;
                }

            }
        }


        public string time
        {
            get
            {


                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(ts + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = TimeSpan.FromSeconds(ts);
                DateTime dt = dtStart.Add(toNow).ToLocalTime();
                TimeSpan span = DateTime.Now - dt;
                if (span.TotalDays > 3)
                {
                    return dt.ToString("MM-dd");
                }
                else
                if (span.TotalDays > 2)
                {
                    return string.Format("前天");
                }
                else
                if (span.TotalDays > 1)
                {
                    return string.Format("昨天");
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

    }

  
}
