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
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Markup;
using System.Text.RegularExpressions;
using Windows.UI;
using System.ComponentModel;
using BiliBili.UWP.Pages;
using Windows.UI.Xaml.Documents;
using System.Threading.Tasks;
using BiliBili.UWP.Models;
using BiliBili.UWP.Pages.User;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BiliBili.UWP.Controls
{
    public enum CommentMode
    {
        Video,
        Bangumi,
        Dynamic,
        Photo,
        MiniVideo,
        MusicMenu,//歌单
        MusicSong//单曲
    }

    public enum ConmmentSortMode
    {
        All,
        Hot,
        New
    }

    /// <summary>
    /// 新版的评论 18/2/23
    /// 支持头像框显示
    /// 修改评论回复样式
    /// 支持多类型评论
    /// 支持评论删除
    /// 支持评论置顶显示
    /// 支持AV号直接跳转
    /// 19-11-1
    /// B站API更新不在同时返回热门+最新评论
    /// </summary>
    public sealed partial class CommentV2Control : UserControl
    {
        public CommentV2Control()
        {
            this.InitializeComponent();
        }
        ScrollViewer scrollViewer;
        double scrollViewerLocation = 0;
        private void GetScollViewer()
        {
            try
            {
                DependencyObject item = this.Parent;
                while (!(item is ScrollViewer))
                {
                    item = VisualTreeHelper.GetParent(item);
                }
                scrollViewer = item as ScrollViewer;
            }
            catch (Exception)
            {
            }

        }


        private void btn_User_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), (sender as HyperlinkButton).Tag.ToString());
        }

        public void ClearComment()
        {
            //ls_hot.ItemsSource = null;
            ls_new.ItemsSource = null;
            _page = 1;
            hot.Visibility = Visibility.Collapsed;
        }
        public int CommentCount
        {
            get
            {
                return ls_new.Items.Count;
            }
        }

        public bool HasMore
        {
            get
            {
                if (btn_LoadMore.Visibility == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void LoadMore()
        {
            if (!_loading)
            {
                GetComment();
            }
        }

        public void RefreshComment()
        {
            LoadComment();
        }








        int _page = 1;
        int _type = 0;
        public bool _loading = false;
        LoadCommentInfo _loadCommentInfo;

        /// <summary>
        /// 加载评论
        /// </summary>
        public void LoadComment()
        {
            GetScollViewer();

            //if (loadCommentInfo.conmmentSortMode!= ConmmentSortMode.All)
            //{
            //    hot.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    hot.Visibility = Visibility.Visible;
            //}
            switch (_loadCommentInfo.commentMode)
            {
                case CommentMode.Dynamic:
                    _type = 17;
                    break;
                case CommentMode.Photo:
                    _type = 11;
                    break;
                case CommentMode.MiniVideo:
                    _type = 5;
                    break;
                case CommentMode.Video:
                    _type = 1;
                    break;
                case CommentMode.MusicMenu:
                    _type = 19;
                    break;
                case CommentMode.MusicSong:
                    _type = 14;
                    break;
                default:
                    break;
            }

            if (_loadCommentInfo.conmmentSortMode == ConmmentSortMode.Hot)
            {
                hot.Visibility = Visibility.Visible;
                _new.Visibility = Visibility.Collapsed;
            }
            else
            {
                hot.Visibility = Visibility.Collapsed;
                _new.Visibility = Visibility.Visible;
            }
            //_loadCommentInfo = loadCommentInfo;
            _page = 1;
            GetComment();

        }
        /// <summary>
        /// 初始化并加载评论
        /// </summary>
        /// <param name="loadCommentInfo"></param>
        public void LoadComment(LoadCommentInfo loadCommentInfo)
        {
            GetScollViewer();

            switch (loadCommentInfo.commentMode)
            {
                case CommentMode.Dynamic:
                    _type = 17;
                    break;
                case CommentMode.Photo:
                    _type = 11;
                    break;
                case CommentMode.MiniVideo:
                    _type = 5;
                    break;
                case CommentMode.Video:
                    _type = 1;
                    break;
                case CommentMode.MusicMenu:
                    _type = 19;
                    break;
                case CommentMode.MusicSong:
                    _type = 14;
                    break;
                default:
                    break;
            }
            if (loadCommentInfo.conmmentSortMode == ConmmentSortMode.All)
            {
                loadCommentInfo.conmmentSortMode = ConmmentSortMode.Hot;
            }
            if (loadCommentInfo.conmmentSortMode == ConmmentSortMode.Hot)
            {
                hot.Visibility = Visibility.Visible;
                _new.Visibility = Visibility.Collapsed;
            }
            else
            {
                hot.Visibility = Visibility.Collapsed;
                _new.Visibility = Visibility.Visible;
            }

            _loadCommentInfo = loadCommentInfo;
            _page = 1;
            GetComment();

        }


        /// <summary>
        /// 初始化评论
        /// </summary>
        /// <param name="loadCommentInfo"></param>
        public void InitializeComment(LoadCommentInfo loadCommentInfo)
        {
            GetScollViewer();
            _loadCommentInfo = loadCommentInfo;
        }

        private async void GetComment()
        {
            if (_page == 1)
            {
                noRepost.Visibility = Visibility.Collapsed;
                closeRepost.Visibility = Visibility.Collapsed;
                //ls_hot.ItemsSource = null;
                ls_new.ItemsSource = null;
            }
            try
            {
                var sort = 0;
                if (_loadCommentInfo.conmmentSortMode == ConmmentSortMode.Hot)
                {
                    sort = 2;
                }

                btn_LoadMore.Visibility = Visibility.Collapsed;
                _loading = true;
                pr_load.Visibility = Visibility.Visible;
                ObservableCollection<CommentModel> ls = new ObservableCollection<CommentModel>();
                var url = string.Format("https://api.bilibili.com/x/v2/reply?access_key={0}&appkey={1}&build={7}&mobi_app=android&oid={2}&plat=2&platform=android&pn={3}&ps=20&sort={6}&ts={4}&type={5}",
                 ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _loadCommentInfo.oid, _page, ApiHelper.GetTimeSpan_2, _type, sort, ApiHelper.build);
                url += "&sign=" + ApiHelper.GetSign(url);

                //var url = "https://api.bilibili.com/x/v2/reply?oid=2381475&plat=2&pn=1&ps=20&sort=0&type=11";

                var re = await WebClientClass.GetResults(new Uri(url));
                dataCommentModel m = JsonConvert.DeserializeObject<dataCommentModel>(re);
                if (m.code == 0)
                {


                    if (m.data.replies != null && m.data.replies.Count != 0)
                    {

                        if (_page == 1)
                        {
                            if (m.data.upper.top != null)
                            {
                                m.data.upper.top.showTop = Visibility.Visible;
                                m.data.replies.Insert(0, m.data.upper.top);
                            }
                            //ls_hot.ItemsSource = m.data.hots;
                            ls_new.ItemsSource = m.data.replies;
                        }
                        else
                        {
                            foreach (var item in m.data.replies)
                            {
                                (ls_new.ItemsSource as ObservableCollection<CommentModel>).Add(item);
                            }
                        }
                        _page++;

                        if (m.data.replies.Count >= 20)
                        {
                            btn_LoadMore.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (_page == 1)
                        {
                            noRepost.Visibility = Visibility.Visible;

                        }
                        else
                        {
                            Utils.ShowMessageToast("全部加载完了...");
                        }
                    }
                }
                else
                {
                    if (m.code == 12002)
                    {
                        closeRepost.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Utils.ShowMessageToast(m.message);
                    }
                }




            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载评论失败");

            }
            finally
            {
                _loading = false;
                pr_load.Visibility = Visibility.Collapsed;

            }
        }

        private async void GetReply(CommentModel data)
        {
            try
            {
                if (data.replies == null)
                {
                    data.replies = new ObservableCollection<CommentModel>();
                }
                data.showReplyMore = Visibility.Collapsed;
                data.showLoading = Visibility.Visible;
                ObservableCollection<CommentModel> ls = new ObservableCollection<CommentModel>();
                var url = string.Format("https://api.bilibili.com/x/v2/reply/reply?access_key={0}&appkey={1}&build={7}&mobi_app=android&oid={2}&plat=2&platform=android&pn={3}&ps=10&root={6}&ts={4}&type={5}",
                 ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _loadCommentInfo.oid, data.loadpage, ApiHelper.GetTimeSpan_2, _type, data.rpid, ApiHelper.build);
                url += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.GetResults(new Uri(url));
                dataCommentModel m = JsonConvert.DeserializeObject<dataCommentModel>(re);
                if (m.code == 0)
                {
                    if (m.data.replies.Count != 0)
                    {
                        if (m.data.replies.Count >= 10)
                        {
                            data.showReplyMore = Visibility.Visible;
                        }
                        foreach (var item in m.data.replies)
                        {
                            data.replies.Add(item);
                        }
                        data.loadpage++;
                    }
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载评论失败");
                //throw;
            }
            finally
            {
                data.showLoading = Visibility.Collapsed;
            }
        }

        private async void doLike(CommentModel data)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行操作");
                return;
            }
            try
            {
                var action = 0;
                if (data.action == 0)
                {
                    action = 1;
                }
                string url = "https://api.bilibili.com/x/v2/reply/action";
                string content = string.Format("access_key={0}&appkey={1}&platform=android&type={2}&rpid={4}&oid={3}&action={5}&ts={6}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, data.rpid, action, ApiHelper.GetTimeSpan_2);
                content += "&sign=" + ApiHelper.GetSign(content);
                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    if (data.action == 0)
                    {
                        data.action = 1;
                        data.like += 1;
                    }
                    else
                    {
                        data.action = 0;
                        data.like -= 1;
                    }
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("操作失败");
                // throw;
            }



        }

        private void btn_Like_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            doLike(m);
        }

        private void btn_ShowComment_Click(object sender, RoutedEventArgs e)
        {


            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            if (m.showReplies == Visibility.Collapsed)
            {
                if (scrollViewer != null)
                {
                    scrollViewerLocation = scrollViewer.VerticalOffset;
                }
                m.showReplies = Visibility.Visible;
                m.showReplyBtn = Visibility.Collapsed;
                m.showReplyBox = Visibility.Visible;
                m.replies = null;
                m.loadpage = 1;
                if (m.replies == null || m.replies.Count == 0)
                {
                    GetReply(m);
                }

            }
            else
            {
                m.showReplyBtn = Visibility.Collapsed;
                m.showReplies = Visibility.Collapsed;
                if (scrollViewer != null)
                {
                    scrollViewer.ChangeView(null, scrollViewerLocation, null);
                }
            }
        }

        private void btn_ShowReplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            if (m.showReplyBox == Visibility.Visible)
            {
                m.showReplyBox = Visibility.Collapsed;
            }
            else
            {
                m.showReplyBox = Visibility.Visible;
            }

        }

        private void btn_DonotLike_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_LoadMoreReply_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            GetReply(m);
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!_loading)
            {
                GetComment();
            }

        }

        private void btn_HotSort_Click(object sender, RoutedEventArgs e)
        {
            _loadCommentInfo.conmmentSortMode = ConmmentSortMode.Hot;

            LoadComment();
        }

        private void btn_NewSort_Click(object sender, RoutedEventArgs e)
        {
            _loadCommentInfo.conmmentSortMode = ConmmentSortMode.All;
            LoadComment();
        }





        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentModel;

            if (m != null)
            {
                m.replyText += (sender as Button).Content.ToString();
            }
        }


        public async void ShowCommentBox()
        {

            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            CommentDialog commentDialog = new CommentDialog(_type, _loadCommentInfo.oid);


            await commentDialog.ShowAsync();
            if (commentDialog.State)
            {
                RefreshComment();
            }
        }
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void btn_SendReply_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentModel;
            ReplyComment(m);
        }

        private async void ReplyComment(CommentModel m)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("登录后再来回复哦");
                return;
            }
            if (m.replyText.Trim().Length == 0)
            {
                Utils.ShowMessageToast("不能发送空白信息");
                return;
            }
            try
            {
                string url = "https://api.bilibili.com/x/v2/reply/add";

                string content =
                    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&message={5}&root={6}&parent={6}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, ApiHelper.GetTimeSpan_2, Uri.EscapeDataString(m.replyText), m.rpid);
                content += "&sign=" + ApiHelper.GetSign(content);
                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    Utils.ShowMessageToast("回复评论成功");
                    m.loadpage = 1;
                    m.replies.Clear();
                    m.replyText = "";
                    GetReply(m);
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("发送评论失败");
                // throw;
            }






        }
        private void btn_ReplyAt_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentModel;
            ReplyAt(m);
        }
        private async void ReplyAt(CommentModel m)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("登录后再来回复哦");
                return;
            }
            if (m.replyText.Trim().Length == 0)
            {
                Utils.ShowMessageToast("不能发送空白信息");
                return;
            }
            try
            {
                string url = "https://api.bilibili.com/x/v2/reply/add";

                var txt = "回复 @" + m.member.uname + ":" + m.replyText;
                string content =
                    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&message={5}&root={6}&parent={7}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, ApiHelper.GetTimeSpan_2, Uri.EscapeDataString(txt), m.root, m.rpid);
                content += "&sign=" + ApiHelper.GetSign(content);
                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    Utils.ShowMessageToast("回复评论成功");
                    m.replyText = "";
                    m.showReplyBox = Visibility.Collapsed;
                    RefreshComment();
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("发送评论失败");
                // throw;
            }






        }

        private void btn_DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            CommentModel m = null;
            if (sender is HyperlinkButton)
            {
                m = (sender as HyperlinkButton).DataContext as CommentModel;
            }
            if (sender is MenuFlyoutItem)
            {
                m = (sender as MenuFlyoutItem).DataContext as CommentModel;
            }
            DeletComment(m);
        }

        private async void DeletComment(CommentModel m)
        {
            if (m == null)
            {
                return;
            }
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("登录后再来删除哦");
                return;
            }

            try
            {
                string url = "https://api.bilibili.com/x/v2/reply/del";

                string content =
                    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&rpid={5}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, ApiHelper.GetTimeSpan_2, m.rpid);
                content += "&sign=" + ApiHelper.GetSign(content);

                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    Utils.ShowMessageToast("评论删除成功");
                    RefreshComment();
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("删除评论失败");
                // throw;
            }






        }


    }





    public class LoadCommentInfo
    {
        public CommentMode commentMode { get; set; }
        public ConmmentSortMode conmmentSortMode { get; set; }
        public string oid { get; set; }


    }

    public class dataCommentModel
    {

        public int code { get; set; }
        public string message { get; set; }

        public dataCommentModel data { get; set; }

        public dataCommentModel page { get; set; }
        public int acount { get; set; }
        public int count { get; set; }
        public int num { get; set; }
        public int size { get; set; }

        public ObservableCollection<CommentModel> hots { get; set; }
        public ObservableCollection<CommentModel> replies { get; set; }

        public dataCommentModel upper { get; set; }
        public CommentModel top { get; set; }

    }
    public class CommentModel : INotifyPropertyChanged
    {

        public CommentModel()
        {
            ButtonCommand = new DelegateCommand();
            ButtonCommand.MyExecute = new Action<object>(ButtonClick);

        }

        private int _action;//0未点赞,1已经点赞
        public int action
        {
            get { return _action; }
            set { _action = value; thisPropertyChanged("action"); thisPropertyChanged("LikeColor"); }
        }

        public SolidColorBrush LikeColor
        {
            get
            {
                if (action == 0)
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    return (Application.Current.Resources.ThemeDictionaries["Light"] as ResourceDictionary)["Bili-ForeColor"] as SolidColorBrush;
                }
            }
        }


        public long rpid { get; set; }
        public long oid { get; set; }
        public int type { get; set; }
        public long mid { get; set; }
        public long root { get; set; }
        public long parent { get; set; }

        public int count { get; set; }
        private int _rcount;
        public int rcount
        {
            get { return _rcount; }
            set { _rcount = value; thisPropertyChanged("rcount"); }
        }
        public int _like { get; set; }
        public int like
        {
            get { return _like; }
            set { _like = value; thisPropertyChanged("like"); thisPropertyChanged("like_str"); }
        }


        public string rcount_str
        {
            get
            {
                if (rcount > 10000)
                {
                    return ((double)rcount / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return rcount.ToString();
                }
            }

        }
        public string like_str
        {
            get
            {
                if (like > 10000)
                {
                    return ((double)like / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return like.ToString();
                }
            }

        }


        public int floor { get; set; }
        public int state { get; set; }
        public long ctime { get; set; }
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

        public string rpid_str { get; set; }

        public CommentMemberModel member { get; set; }
        public CommentContentModel content { get; set; }


        private ObservableCollection<CommentModel> _replies = new ObservableCollection<CommentModel>();
        public ObservableCollection<CommentModel> replies
        {
            get { return _replies; }
            set { _replies = value; thisPropertyChanged("replies"); }
        }
        //public ObservableCollection<CommentModel> replies { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


        private Visibility _showReplies = Visibility.Collapsed;
        public Visibility showReplies
        {
            get { return _showReplies; }
            set { _showReplies = value; thisPropertyChanged("showReplies"); }
        }

        private Visibility _showReplyBtn = Visibility.Collapsed;
        public Visibility showReplyBtn
        {
            get { return _showReplyBtn; }
            set { _showReplyBtn = value; thisPropertyChanged("showReplyBtn"); }
        }

        private Visibility _showReplyBox = Visibility.Collapsed;
        public Visibility showReplyBox
        {
            get { return _showReplyBox; }
            set { _showReplyBox = value; thisPropertyChanged("showReplyBox"); }
        }


        private Visibility _showReplyMore = Visibility.Collapsed;
        public Visibility showReplyMore
        {
            get { return _showReplyMore; }
            set { _showReplyMore = value; thisPropertyChanged("showReplyMore"); }
        }

        private Visibility _showLoading = Visibility.Collapsed;
        public Visibility showLoading
        {
            get { return _showLoading; }
            set { _showLoading = value; thisPropertyChanged("showLoading"); }
        }


        public Visibility showDelete
        {
            get
            {
                if (mid.ToString() == ApiHelper.GetUserId())
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }



        private int _loadpage = 1;
        public int loadpage
        {
            get { return _loadpage; }
            set { _loadpage = value; thisPropertyChanged("loadpage"); }
        }



        public string replyAt
        {
            get
            {
                return "回复 @" + member.uname;
            }
        }


        private string _replyText;
        public string replyText
        {
            get { return _replyText; }
            set { _replyText = value; thisPropertyChanged("replyText"); }
        }


        private Visibility _showTop = Visibility.Collapsed;
        public Visibility showTop
        {
            get { return _showTop; }
            set { _showTop = value; thisPropertyChanged("showTop"); }
        }



        public DelegateCommand ButtonCommand { get; private set; }

        private void ButtonClick(object paramenter)
        {
            if (paramenter.ToString().Contains("av"))
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), paramenter.ToString().Replace("av", ""));
            }

        }


    }

    public class CommentContentModel
    {

        public string message { get; set; }
        public int plat { get; set; }
        public string plat_str
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
        public string device { get; set; }
        public RichTextBlock text
        {
            get
            {
                try
                {
                    if (message != null)
                    {
                        string input = message;
                        input = input.Replace("\r\n", "<LineBreak/>");
                        input = input.Replace("\n", "<LineBreak/>");
                        //替换表情
                        MatchCollection mc = Regex.Matches(input, @"\[.*?\]");
                        foreach (Match item in mc)
                        {
                            if (emote != null && emote.ContainsKey(item.Groups[0].Value))
                            {
                                var emoji = emote[item.Groups[0].Value];
                                input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Border  Margin=""0 0 0 -4""><Image Source=""{0}"" Width=""{1}"" Height=""{1}"" /></Border></InlineUIContainer>", emoji["url"].ToString(), emoji["meta"]["size"].ToInt32() == 1 ? "20" : "36"));
                            }
                            //var emoji=  ApiHelper.emojis.Where(x => x.name == item.Groups[0].Value);
                            //if (emoji!=null&&emoji.ToList().Count!=0)
                            //{
                            //    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Border><Image Source=""{0}"" Width=""36"" Height=""36""/></Border></InlineUIContainer>", emoji.First().url));
                            //}
                        }

                        //替换av号
                        MatchCollection videos = Regex.Matches(input, @"av(\d+)");
                        foreach (Match item in videos)
                        {
                            var data = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{0}""  CommandParameter=""{0}"" >{0}</HyperlinkButton></InlineUIContainer>", item.Groups[0].Value);
                            input = input.Replace(item.Groups[0].Value, data);
                        }




                        //生成xaml
                        var xaml = string.Format(@"<RichTextBlock HorizontalAlignment=""Stretch"" TextWrapping=""Wrap""  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
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
                catch (Exception)
                {
                    var tx = new RichTextBlock();
                    Paragraph paragraph = new Paragraph();
                    Run run = new Run() { Text = message };
                    paragraph.Inlines.Add(run);
                    tx.Blocks.Add(paragraph);
                    return tx;

                }

            }

        }

        public JObject emote { get; set; }
    }


    public class CommentMemberModel
    {
        public string mid { get; set; }
        public string uname { get; set; }
        public string sex { get; set; }
        public string sign { get; set; }


        //public string avatar { get; set; }
        private string _avatar;
        public string avatar { get { return _avatar; } set { _avatar = value + "@64w_64h.jpg"; } }


        public CommentMemberModel level_info { get; set; }
        public int current_level { get; set; }
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
                        return "ms-appx:///Assets/MiniIcon/transparent.png";
                }
            }
        }


        public string pendant_str
        {
            get
            {
                if (pendant != null)
                {
                    if (pendant.image == "")
                    {
                        return "ms-appx:///Assets/MiniIcon/transparent.png";
                    }
                    return pendant.image;
                }
                else
                {
                    return "ms-appx:///Assets/MiniIcon/transparent.png";
                }
            }
        }
        public CommentMemberModel pendant { get; set; }
        public int pid { get; set; }
        public string name { get; set; }
        public string image { get; set; }

        public CommentMemberModel official_verify { get; set; }
        public int type { get; set; }
        public string desc { get; set; }

        public CommentMemberModel vip { get; set; }
        public int vipType { get; set; }
        public SolidColorBrush vip_co
        {
            get
            {
                if (vip.vipType == 2)
                {
                    return new SolidColorBrush(Colors.DeepPink);
                }
                else
                {
                    if (SettingHelper.Get_Theme() == "Dark")
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

        public string Verify
        {
            get
            {
                if (official_verify == null)
                {
                    return "";
                }
                switch (official_verify.type)
                {
                    case 0:
                        return "ms-appx:///Assets/MiniIcon/ic_authentication_personal.png";
                    case 1:
                        return "ms-appx:///Assets/MiniIcon/ic_authentication_organization.png";
                    default:
                        return "ms-appx:///Assets/MiniIcon/transparent.png";
                }
            }
        }


    }

}
