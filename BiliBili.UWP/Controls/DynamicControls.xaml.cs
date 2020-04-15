using BiliBili.UWP.Pages;
using BiliBili.UWP.Pages.FindMore;
using BiliBili.UWP.Pages.User;
using BiliBili.UWP.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliBili.UWP.Controls
{
    public sealed partial class DynamicControls : UserControl
    {
        public event EventHandler<string> LoadMore;
        public event EventHandler Refresh;
        public event EventHandler OpenComment;
        public DynamicControls()
        {
            this.InitializeComponent();
            dynamicItemDataTemplateSelector.resource = this.Resources;
            photoItemDataTemplateSelector.resource = this.Resources;
        }
        public void DoRefresh()
        {
            list_dynamic.ItemsSource = null;
            Refresh?.Invoke(this, null);
        }
        public string GetLastDynamicId()
        {
            if (list_dynamic.ItemsSource != null)
            {
                return (list_dynamic.ItemsSource as ObservableCollection<DynamicCardsModel>).LastOrDefault()?.desc.dynamic_id.ToString();
            }
            else
            {
                return "";
            }
        }
        public int Count()
        {
            if (list_dynamic.ItemsSource == null)
            {
                return 0;
            }
            return list_dynamic.Items.Count;
        }
        public void ClearData()
        {
            list_dynamic.ItemsSource = null;
        }

        public void HideLoadMoreButton()
        {
            btn_LoadMore.Visibility = Visibility.Collapsed;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Width != 0)
            {
                bor_width.Width = (availableSize.Width / 3) - 10;
                bor_Width1.Width = (availableSize.Width / 3);
            }
            return base.MeasureOverride(availableSize);
        }
        public void LoadData(ObservableCollection<DynamicCardsModel> dynamicCards, bool needClear = false)
        {
            if (needClear)
            {
                list_dynamic.ItemsSource = null;
            }
            if (list_dynamic.ItemsSource == null)
            {
                list_dynamic.ItemsSource = dynamicCards;
            }
            else
            {
                dynamicCards.ToList().ForEach(x => (list_dynamic.ItemsSource as ObservableCollection<DynamicCardsModel>).Add(x));
            }
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            DoRefresh();
        }
        private void sv_dynamic_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_dynamic.VerticalOffset >= sv_dynamic.ScrollableHeight - 200)
            {

                LoadMore?.Invoke(this, GetLastDynamicId());
            }
        }
        private async void btn_Repost_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as DynamicCardsModel;

            RepostDialog repostDialog = new RepostDialog(item);
            await repostDialog.ShowAsync();
            //Utils.ShowMessageToast(item.desc.type.ToString());
        }

        private void btn_Comment_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as DynamicCardsModel;

            //if (item.desc.type == 2)
            //{
            //    //http://h.bilibili.com/ywh/h5/2369391
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), "http://h.bilibili.com/ywh/h5/" + item.feed1.item.id);
            //    return;
            //}

            if (item.desc.type == 512)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), item.bangumi.apiSeasonInfo.season_id.ToString());
                return;
            }

            if (item.desc.type == 8)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), item.video.aid.ToString());
                return;
            }
            if (item.desc.type == 64)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), "https://www.bilibili.com/read/app/" + item.article.id);
                return;
            }
            if (OpenComment != null)
            {
                OpenComment(this, null);
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicInfoPage), item);

        }

        private async void btn_Like_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as DynamicCardsModel;
            var d = await doLike(item);
            if (item.desc.is_liked == 0 && d)
            {
                item.desc.is_liked = 1;
                item.desc.like += 1;

                item.desc.likeColor = (Application.Current.Resources.ThemeDictionaries["Light"] as ResourceDictionary)["Bili-ForeColor"] as SolidColorBrush;
                return;
            }
            if (item.desc.is_liked == 1 && d)
            {
                item.desc.is_liked = 0;
                item.desc.like -= 1;
                item.desc.likeColor = new SolidColorBrush(Colors.Gray);
                return;
            }
            //Utils.ShowMessageToast(item.desc.type.ToString());
        }

        private void list_dynamic_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as DynamicCardsModel;
            if (item.desc.type == 256)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), item.music.id);
                //Utils.ShowMessageToast("暂不支持音频播放");
                return;
            }

            //if (item.desc.type==2)
            //{
            //    //http://h.bilibili.com/ywh/h5/2369391
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), "http://h.bilibili.com/ywh/h5/" + item.feed1.item.id);
            //    return;
            //}

            if (item.desc.type == 512)
            {
                var seasonid = 0;
                if (item.bangumi.apiSeasonInfo == null)
                {
                    seasonid = item.bangumi.season.season_id;
                }
                else
                {
                    seasonid = item.bangumi.apiSeasonInfo.season_id;
                }
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), seasonid);
                return;
            }
            if (item.desc.type == 2048)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), item.web.sketch.target_url);
                return;
            }
            if (item.desc.type == 8)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), item.video.aid.ToString());
                return;
            }
            if (item.desc.type == 64)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), "https://www.bilibili.com/read/app/" + item.article.id);
                return;
            }



            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicInfoPage), item);


        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var ls = new List<string>();
            var index = 0;
            if (((sender as GridView).DataContext as DynamicCardsModel).feed1 != null)
            {
                var item = ((sender as GridView).DataContext as DynamicCardsModel).feed1;
                foreach (var item1 in item.item.pictures)
                {
                    ls.Add(item1.img_src);
                }
                index = item.item.showpictures.IndexOf(e.ClickedItem as picturesModel);

            }
            if (((sender as GridView).DataContext as DynamicCardsModel).feed != null)
            {
                var item = ((sender as GridView).DataContext as DynamicCardsModel).feed;
                foreach (var item1 in item.photo.item.pictures)
                {
                    ls.Add(item1.img_src);
                }
                index = ls.IndexOf((e.ClickedItem as picturesModel).img_src);
            }
            Controls.ImagePreview imagePreview = new Controls.ImagePreview(ls, index);
            imagePreview.Show();
        }

        private void btn_OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DynamicCardsModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), item.feed.video.aid);
        }

        private void btn_OpenAtricle_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DynamicCardsModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), "https://www.bilibili.com/read/app/" + item.feed.article.id);
        }

        private void btn_OpenPhoto_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DynamicCardsModel;




            //DynamicCardsModel m = new DynamicCardsModel() {
            //    desc=new DynamicCardsModel()
            //    {
            //        timestamp=item.feed.photo.item.upload_time,
            //        type =item.desc.orig_type,
            //        dynamic_id=item.desc.orig_dy_id,
            //        uid=item.feed.photo.user.uid,
            //        user_profile=new user_profileModel()
            //        {
            //            info =new user_profileModel() { 
            //                uid = item.feed.photo.user.uid,
            //                face = item.feed.photo.user.head_url,
            //                uname = item.feed.photo.user.name
            //            }
            //        },
            //        rid=item.feed.photo.item.id
            //    },
            //    card = item.feed.origin
            //};
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicInfoPage), item.desc.orig_dy_id);
        }

        private void btn_OpenWeb_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DynamicCardsModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), item.feed.web.sketch.target_url);
        }

        private void btn_OpenUser_Click(object sender, RoutedEventArgs e)
        {

            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), (sender as HyperlinkButton).Tag.ToString());

        }

        private void btn_OpenMusic_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DynamicCardsModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), item.feed.music.id);
        }

        private void btn_OpenBnagumi_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DynamicCardsModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), item.feed.bangumi.apiSeasonInfo.season_id);
        }


        private async Task<bool> doLike(DynamicCardsModel model)
        {
            try
            {
                string url = "https://api.vc.bilibili.com/dynamic_like/v1/dynamic_like/like";
                string content = string.Format("_device=android&access_key={0}&appkey={1}&build=5250000&dynamic_id={2}&mobi_app=android&platform=android&rid={3}&spec_type=0&src=bilih5&ts={4}&type=8&uid={5}",
                     ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, model.desc.dynamic_id, model.desc.rid, ApiHelper.GetTimeSpan_2, ApiHelper.GetUserId());
                content += "&sign=" + ApiHelper.GetSign(content);
                var results = await WebClientClass.PostResults(new Uri(url), content);

                JObject obj = JObject.Parse(results);
                if (obj["code"].ToInt32() == 0)
                {
                    return true;
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                    return false;
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("点赞失败");
                return false;
            }
        }

        private void btn_User_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as DynamicCardsModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), item.desc.user_profile.info.uid);
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            LoadMore?.Invoke(this, GetLastDynamicId());
        }

        private void btn_vote_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://t.bilibili.com/vote/h5/index/#/result?vote_id=" + (sender as HyperlinkButton).Tag.ToString());
        }
    }

}
namespace BiliBili.UWP.Views
{
    public class DynamicItemDataTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary resource;
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var card = item as DynamicCardsModel;
            switch (card.desc.type)
            {
                case 1:
                    card.feed = JsonConvert.DeserializeObject<DynamicFeedModel>(card.card);
                    if (card.display != null && card.display.emoji_info != null)
                    {
                        card.feed.item.emoji_details = card.display.emoji_info.emoji_details;
                    }
                    if (card.feed.item.orig_type == 4)
                    {
                        card.feed.showText = Visibility.Visible;
                        card.feed.text.item.dynamic_id = card.feed.orig_dy_id;
                    }
                    if (card.feed.item.orig_type == 8)
                    {
                        card.feed.showVideo = Visibility.Visible;
                    }
                    if (card.feed.item.orig_type == 64)
                    {
                        card.feed.showArticle = Visibility.Visible;
                    }
                    if (card.feed.item.orig_type == 2)
                    {
                        card.feed.showPhoto = Visibility.Visible;

                        if (card.feed.photo != null)
                        {
                            if (card.feed.photo.item.pictures_count > 9)
                            {
                                var ls = card.feed.photo.item.pictures.Take(9).ToList();
                                ls.Last().count = card.feed.photo.item.pictures_count;
                                card.feed.photo.item.showpictures = ls;
                            }
                            else
                            {
                                card.feed.photo.item.showpictures = card.feed.photo.item.pictures;
                            }
                        }

                    }
                    if (card.feed.item.orig_type == 2048)
                    {
                        card.feed.showWeb = Visibility.Visible;
                    }
                    if (card.feed.item.orig_type == 512)
                    {
                        card.feed.showBangumi = Visibility.Visible;
                    }
                    if (card.feed.item.orig_type == 256)
                    {
                        card.feed.showMusic = Visibility.Visible;
                    }
                    if (card.feed.item.orig_type == 16)
                    {
                        card.feed.showMiniVideo = Visibility.Visible;
                    }
                    return resource["Feed0"] as DataTemplate;
                case 2:
                case 4:
                    if (card.feed1 == null)
                    {
                        card.feed1 = JsonConvert.DeserializeObject<DynamicFeed1Model>(card.card);
                        if (card.display!=null&& card.display.emoji_info!=null)
                        {
                            card.feed1.item.emoji_details = card.display.emoji_info.emoji_details;
                        }
                        if (card.desc != null && card.desc.dynamic_id != 0)
                        {
                            card.feed1.item.dynamic_id = card.desc.dynamic_id;
                        }
                        if (card.desc != null && card.desc.orig_dy_id != 0)
                        {
                            card.feed1.item.dynamic_id = card.desc.orig_dy_id;
                        }
                    }

                    if (card.feed1.item.pictures != null)
                    {
                        card.feed1.item.pictures.ForEach(x => x.c = card.feed1.item.pictures_count);
                    }
                    if (card.feed1.item.pictures_count > 9)
                    {
                        var ls = card.feed1.item.pictures.Take(9).ToList();
                        ls.Last().count = card.feed1.item.pictures_count;
                        card.feed1.item.showpictures = ls;
                    }
                    else
                    {
                        card.feed1.item.showpictures = card.feed1.item.pictures;
                    }

                    return resource["Feed1"] as DataTemplate;
                case 8:
                    card.video = JsonConvert.DeserializeObject<DynamicVideoModel>(card.card);
                    if (card.display != null && card.display.emoji_info != null)
                    {
                        card.video.emoji_details = card.display.emoji_info.emoji_details;
                    }
                    return resource["FeedVideo"] as DataTemplate;
                case 16:
                    card.minivideo = JsonConvert.DeserializeObject<DynamicMiniVideoModel>(card.card);
                    if (card.display != null && card.display.emoji_info != null)
                    {
                        card.minivideo.emoji_details = card.display.emoji_info.emoji_details;
                    }
                    return resource["FeedMiniVideo"] as DataTemplate;
                case 64:
                    card.article = JsonConvert.DeserializeObject<DynamicArticleModel>(card.card);
                  
                    return resource["FeedArticle"] as DataTemplate;
                case 256:
                    card.music = JsonConvert.DeserializeObject<DynamicMusicModel>(card.card);
                    if (card.display != null && card.display.emoji_info != null)
                    {
                        card.music.emoji_details = card.display.emoji_info.emoji_details;
                    }
                    return resource["FeedMusic"] as DataTemplate;
                case 512:
                case 4099:
                    card.bangumi = JsonConvert.DeserializeObject<DynamicBangumiModel>(card.card);
                    
                    return resource["FeedBangumi"] as DataTemplate;
                case 2048:
                    card.web = JsonConvert.DeserializeObject<DynamicWebModel>(card.card);
                    if (card.display != null && card.display.emoji_info != null)
                    {
                        card.web.emoji_details = card.display.emoji_info.emoji_details;
                    }
                    return resource["FeedWeb"] as DataTemplate;
                default:
                    return resource["FeedUnkonw"] as DataTemplate;
            }

        }
    }


    public class PhotoItemDataTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary resource;
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {

            if ((item as picturesModel).c == 4)
            {
                return resource["Photos22"] as DataTemplate;
            }
            else
            {
                return resource["Photos33"] as DataTemplate;
            }
        }
    }



    public class DynamicModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public DynamicModel data { get; set; }
        public ObservableCollection<DynamicCardsModel> cards { get; set; }

    }
    public class DynamicCardsModel : INotifyPropertyChanged
    {
        public DynamicCardsModel()
        {
            ButtonCommand = new DelegateCommand();
            TagCommand = new DelegateCommand();
            lotteryCommand = new DelegateCommand();
            ButtonCommand.MyExecute = new Action<object>(ButtonClick);
            TagCommand.MyExecute = new Action<object>(TagCommandClick);
            lotteryCommand.MyExecute = new Action<object>(LotteryCommandClick);

        }

        private DynamicCardsModel _desc;
        public DynamicCardsModel desc
        {
            get { return _desc; }
            set { _desc = value; thisPropertyChanged("desc"); }
        }

        public long uid { get; set; }
        //2 普通动态，1 转发动态 ,64 文章，8 投稿视频，512番剧更新,32 番剧更新 已弃用,256 音频,4 文字,16小视频，2048网页,10001推荐视频
        private int _type;
        public int type
        {
            get
            {
                return _type;
            }
            set { _type = value; }
        }
        public long rid { get; set; }

        public user_profileModel user_profile { get; set; }
        public long dynamic_id { get; set; }


        private int _repost;//转发
        public int repost
        {
            get { return _repost; }
            set { _repost = value; thisPropertyChanged("repost"); }
        }
        //public int repost { get; set; }


        //public int like { get; set; }

        private int _like;//是否点赞
        public int like
        {
            get { return _like; }
            set { _like = value; thisPropertyChanged("like"); thisPropertyChanged("likeColor"); }
        }

        private int _is_liked;//是否点赞
        public int is_liked
        {
            get { return _is_liked; }
            set { _is_liked = value; thisPropertyChanged("is_liked"); thisPropertyChanged("likeColor"); }
        }


        public int comment_count { get; set; }
        public int comment { get; set; }
        public long orig_dy_id { get; set; }
        public int orig_type { get; set; }

        public string card { get; set; }


        public string mode
        {
            get
            {
                switch (desc.type)
                {
                    case 512:
                        return "番剧";
                    case 4099:
                        return "影视";
                    default:
                        return "番剧";
                }
            }
        }

        public DynamicVideoModel video { get; set; }
        public DynamicBangumiModel bangumi { get; set; }
        public DynamicMusicModel music { get; set; }
        public DynamicArticleModel article { get; set; }

        public DynamicFeed1Model feed1 { get; set; }

        public DynamicWebModel web { get; set; }
        public DynamicFeedModel feed { get; set; }

        public DynamicMiniVideoModel minivideo { get; set; }

        //包含表情信息
        public displayModel display { get; set; }


        private int _reply;
        public int reply
        {
            get
            {
                if (desc.comment != 0)
                {
                    return desc.comment;
                }
                //replyCnt
                if (_reply != 0)
                {
                    return _reply;
                }
                if (card == "")
                {
                    return 0;
                }
                var re = Regex.Match(card, @"""reply\"":(.*?),").Groups[1].Value.Trim().ToInt32();
                if (re != 0)
                {
                    return re;
                }
                else
                {
                    var re1 = Regex.Match(card, @"""reply_count\"":(.*?),").Groups[1].Value.Trim().ToInt32();
                    if (re1 != 0)
                    {
                        return re1;
                    }
                    else
                    {
                        return Regex.Match(card, @"""replyCnt\"":(.*?),").Groups[1].Value.Trim().ToInt32();
                    }
                }
            }
            set
            {
                _reply = value;
            }
        }

        public long timestamp { get; set; }

        public string time
        {
            get
            {


                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(timestamp + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = TimeSpan.FromSeconds(timestamp);
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



        private SolidColorBrush _likeColor;
        public SolidColorBrush likeColor
        {
            get
            {
                if (is_liked == 0)
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    return (Application.Current.Resources.ThemeDictionaries["Light"] as ResourceDictionary)["Bili-ForeColor"] as SolidColorBrush;
                }
            }
            set
            {
                _likeColor = value;
                thisPropertyChanged("likeColor");
            }
        }


        public DelegateCommand ButtonCommand { get; private set; }
        public DelegateCommand TagCommand { get; private set; }
        public DelegateCommand lotteryCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ButtonClick(object paramenter)
        {
            //Command="{Binding ButtonCommand}"
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), paramenter);
        }

        private void TagCommandClick(object paramenter)
        {
            //Command="{Binding ButtonCommand}"
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicTopicPage), paramenter);
        }
        private async void LotteryCommandClick(object paramenter)
        {
            //Command="{Binding ButtonCommand}"
            if (paramenter.ToString() == "0")
            {
                await new Controls.LotteryDialog(desc.orig_dy_id.ToString()).ShowAsync();
            }
            else
            {
                await new Controls.LotteryDialog(paramenter.ToString()).ShowAsync();
            }
        }
        private void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public DynamicCardsModel extension { get; set; }
        public vote_cfgModel vote_cfg { get; set; }
        public string lott { get; set; }

        public Visibility showExtension
        {
            get
            {
                if (extension != null && extension.vote_cfg != null)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public Visibility showLott
        {
            get
            {
                if (extension != null && extension.lott != null)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
    }
    public class lott
    {
        public int lottery_id { get; set; }
    }
    public class user_profileModel
    {
        public user_profileModel info { get; set; }
        public long uid { get; set; }
        public string uname { get; set; }

        private string _face;
        public string face { get { return _face; } set { _face = value + "@64w_64h.jpg"; } }


        public user_profileModel card { get; set; }
        public user_profileModel official_verify { get; set; }
        public int type { get; set; }//-1未认证,0个人认证,1企业认证
        public string desc { get; set; }

        public string verify
        {
            get
            {
                if (official_verify != null)
                {
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
                else
                {
                    return "";
                }
            }
        }


    }

    public class displayModel
    {
        public emoji_infoModel emoji_info { get; set; }

    }
    public class emoji_infoModel
    {
        public List<emoji_detailsModel> emoji_details { get; set; }
    }
    public class emoji_detailsModel
    {
        public string emoji_name { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }
    public class DynamicVideoModel
    {
        public long aid { get; set; }

        public List<emoji_detailsModel> emoji_details { get; set; }
        public string desc { get; set; }
        public string title { get; set; }
        private string _pic;

        public string pic
        {
            get { return _pic; }
            set { _pic = value + "@200w.jpg"; }
        }


        public string dynamic { get; set; }
        public double duration { get; set; }
        public string Duration
        {
            get
            {
                var ts = TimeSpan.FromSeconds(duration);
                return ts.ToString();
            }
        }


        public string ctrl { get; set; }
        public FrameworkElement Content
        {
            get
            {
                if (dynamic == null || dynamic.Length == 0)
                {
                    return new TextBlock();
                }
                try
                {
                    string input = dynamic;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    if (emoji_details!=null&& emoji_details.Count!=0)
                    {
                        foreach (var item in emoji_details)
                        {
                            input = input.Replace(item.emoji_name, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", item.url));
                        }
                    }
                    //MatchCollection mc = Regex.Matches(input, @"\[(.*?)\]");
                    //foreach (Match item in mc)
                    //{
                    //    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", "ms-appx:///Assets/Emoji/" + item.Groups[1].Value + ".png"));
                    //}
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
                                    var d = dynamic.Substring(item.location, item.length);
                                    var index = input.IndexOf(d);

                                    //var s = input.Substring(index,item.length);
                                    input = input.Remove(index, item.length);
                                    var test = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.data);
                                    input = input.Insert(index, test);
                                }
                            }
                        }
                    }

                    input = input.Replace("^x$%^", "@");
                    input = input.Replace("&", "&amp;");
                    MatchCollection tags = Regex.Matches(input, @"\#(.*?)\#");
                    foreach (Match item in tags)
                    {
                        input = input.Replace(item.Groups[0].Value, @"<InlineUIContainer><HyperlinkButton Command=""{Binding TagCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", item.Groups[0].Value, item.Groups[1].Value));
                    }




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

                    return new TextBlock()
                    {
                        Text = dynamic,
                        IsTextSelectionEnabled = true,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                }

            }
        }



        public DynamicVideoModel stat { get; set; }
        public long danmaku { get; set; }
        public string Danmaku
        {
            get
            {
                if (danmaku > 10000)
                {
                    return ((double)danmaku / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return danmaku.ToString();
                }
            }
        }
        public long view { get; set; }
        public string View
        {
            get
            {
                if (view > 10000)
                {
                    return ((double)view / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return view.ToString();
                }
            }
        }

        public Visibility showdynamic
        {
            get
            {
                if (dynamic != null && dynamic.Length != 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }



        public DynamicVideoModel owner { get; set; }
        public long mid { get; set; }
        public string name { get; set; }
        public string face { get; set; }
    }

    public class DynamicBangumiModel
    {
        public long aid { get; set; }

        public DynamicBangumiModel apiSeasonInfo { get; set; }
        public string cover { get; set; }
        public int season_id { get; set; }
        public string title { get; set; }

        public string new_desc { get; set; }



        public string index_title { get; set; }
        public string url { get; set; }
        public DynamicBangumiModel season { get; set; }
    }


    public class DynamicMusicModel
    {
        public long id { get; set; }

        public List<emoji_detailsModel> emoji_details { get; set; }

        public string cover { get; set; }

        public string title { get; set; }

        public string upperAvatar { get; set; }
        public string upId { get; set; }

        public string upper { get; set; }
        public string intro { get; set; }

        public string ctrl { get; set; }
        public RichTextBlock Content
        {
            get
            {
                if (intro == null || intro.Length == 0)
                {
                    RichTextBlock richTextBlock = new RichTextBlock();
                    var p = new Paragraph();
                    p.Inlines.Add(new Run() { Text = intro });
                    richTextBlock.Blocks.Add(p);
                    return richTextBlock;
                }
                try
                {
                    string input = intro;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    if (emoji_details != null && emoji_details.Count != 0)
                    {
                        foreach (var item in emoji_details)
                        {
                            input = input.Replace(item.emoji_name, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", item.url));
                        }
                    }
                    //MatchCollection mc = Regex.Matches(input, @"\[(.*?)\]");
                    //foreach (Match item in mc)
                    //{
                    //    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", "ms-appx:///Assets/Emoji/" + item.Groups[1].Value + ".png"));
                    //}
                    if (ctrl != null && ctrl.Length != 0)
                    {
                        var re = ctrl;
                        List<ctrlModel> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ctrlModel>>(re);
                        if (list != null && list.Count != 0)
                        {
                            foreach (var item in list.OrderBy(x => x.location))
                            {
                                //if (item.type == 1)
                                //{
                                var d = intro.Substring(item.location, item.length);
                                var index = input.IndexOf(d);

                                //var s = input.Substring(index,item.length);
                                input = input.Remove(index, item.length);
                                var test = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.data);
                                input = input.Insert(index, test);
                                // }
                            }
                        }
                    }

                    input = input.Replace("^x$%^", "@");
                    input = input.Replace("&", "&amp;");
                    MatchCollection tags = Regex.Matches(input, @"\#(.*?)\#");
                    foreach (Match item in tags)
                    {
                        input = input.Replace(item.Groups[0].Value, @"<InlineUIContainer><HyperlinkButton Command=""{Binding TagCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", item.Groups[0].Value, item.Groups[1].Value));
                    }

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

                    RichTextBlock richTextBlock = new RichTextBlock();
                    var p = new Paragraph();
                    p.Inlines.Add(new Run() { Text = intro });
                    richTextBlock.Blocks.Add(p);
                    return richTextBlock;
                }

            }
        }


        public Visibility showdynamic
        {
            get
            {
                if (intro != null && intro.Length != 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
    }
    public class DynamicMiniVideoModel
    {
        public List<emoji_detailsModel> emoji_details { get; set; }
        public DynamicMiniVideoModel item { get; set; }

        public DynamicMiniVideoModel cover { get; set; }
        public string _default { get; set; }

        public long id { get; set; }


        public string video_playurl { get; set; }



        public string description { get; set; }
        public string ctrl { get; set; }
        public RichTextBlock Content
        {
            get
            {
                if (description == null || description.Length == 0)
                {
                    RichTextBlock richTextBlock = new RichTextBlock();
                    var p = new Paragraph();
                    p.Inlines.Add(new Run() { Text = description });
                    richTextBlock.Blocks.Add(p);
                    return richTextBlock;
                }
                try
                {
                    string input = description;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    if (emoji_details != null && emoji_details.Count != 0)
                    {
                        foreach (var item in emoji_details)
                        {
                            input = input.Replace(item.emoji_name, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", item.url));
                        }
                    }
                    //MatchCollection mc = Regex.Matches(input, @"\[(.*?)\]");
                    //foreach (Match item in mc)
                    //{
                    //    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", "ms-appx:///Assets/Emoji/" + item.Groups[1].Value + ".png"));
                    //}
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
                                    var d = description.Substring(item.location, item.length);
                                    var index = input.IndexOf(d);

                                    //var s = input.Substring(index,item.length);
                                    input = input.Remove(index, item.length);
                                    var test = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.data);
                                    input = input.Insert(index, test);
                                }
                            }
                        }
                    }

                    input = input.Replace("^x$%^", "@");
                    input = input.Replace("&", "&amp;");
                    MatchCollection tags = Regex.Matches(input, @"\#(.*?)\#");
                    foreach (Match item in tags)
                    {
                        input = input.Replace(item.Groups[0].Value, @"<InlineUIContainer><HyperlinkButton Command=""{Binding TagCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", item.Groups[0].Value, item.Groups[1].Value));
                    }

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

                    RichTextBlock richTextBlock = new RichTextBlock();
                    var p = new Paragraph();
                    p.Inlines.Add(new Run() { Text = description });
                    richTextBlock.Blocks.Add(p);
                    return richTextBlock;
                }

            }
        }

        public DynamicMiniVideoModel user { get; set; }
        public long uid { get; set; }
        public string head_url { get; set; }
        public string name { get; set; }
    }

    public class DynamicArticleModel
    {


        private string _banner_url;
        public string banner_url
        {
            get { return _banner_url; }
            set { _banner_url = value; }
        }




        public string summary { get; set; }

        public string title { get; set; }
        public long id { get; set; }

        public DynamicArticleModel stats { get; set; }
        public DynamicArticleModel author { get; set; }
        public string name { get; set; }
        public long mid { get; set; }
        public string face { get; set; }

        public long view { get; set; }
        public string View
        {
            get
            {
                if (view > 10000)
                {
                    return ((double)view / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return view.ToString();
                }
            }
        }
    }

    public class DynamicFeed1Model : INotifyPropertyChanged
    {
        public List<emoji_detailsModel> emoji_details { get; set; }
        public long doc_id { get; set; }
        public long upload_timestamp { get; set; }
        public long upload_time { get; set; }
        public long id { get; set; }
        public DynamicFeed1Model item { get; set; }

        public DynamicFeed1Model user { get; set; }
        public long uid { get; set; }
        public string head_url { get; set; }

        public string name { get; set; }

        public string content { get; set; }
        public long dynamic_id { get; set; }
        private string _description;
        public string description
        {
            get
            {
                if (_description == null || _description == "")
                {
                    if (content != null && content.Length != 0)
                    {
                        return content;

                    }
                    else
                    {
                        return title;
                    }


                }
                else
                {
                    return _description;
                }
            }
            set { _description = value; }
        }

        public string at_control { get; set; }
        public RichTextBlock Content
        {
            get
            {

                if (description == null || description.Length == 0)
                {
                    var txt = new RichTextBlock()
                    {
                        Margin = new Thickness(0, 4, 0, 4),
                        IsTextSelectionEnabled = true
                    };
                    var p = new Paragraph();
                    p.Inlines.Add(new Run() { Text = description });
                    txt.Blocks.Add(p);
                    return txt;
                }
                try
                {
                    string input = description;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    if (emoji_details != null && emoji_details.Count != 0)
                    {
                        foreach (var item in emoji_details)
                        {
                            input = input.Replace(item.emoji_name, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", item.url));
                        }
                    }
                    //MatchCollection mc = Regex.Matches(input, @"\[(.*?)\]");
                    //foreach (Match item in mc)
                    //{
                    //    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", "ms-appx:///Assets/Emoji/" + item.Groups[1].Value + ".png"));
                    //}
                    if (at_control != null && at_control.Length != 0)
                    {
                        var re = at_control;
                        List<ctrlModel> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ctrlModel>>(re);
                        if (list != null && list.Count != 0)
                        {
                            foreach (var item in list.OrderBy(x => x.location))
                            {

                                if (item.type == 1)
                                {
                                    var d = description.Substring(item.location, item.length);
                                    var index = input.IndexOf(d);

                                    //var s = input.Substring(index,item.length);
                                    input = input.Remove(index, item.length);
                                    var test = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.data);
                                    input = input.Insert(index, test);
                                }

                            }
                        }
                    }

                    input = input.Replace("^x$%^", "@");
                    input = input.Replace("&", "&amp;");
                    MatchCollection tags = Regex.Matches(input, @"\#(.*?)\#");
                    foreach (Match item in tags)
                    {
                        input = input.Replace(item.Groups[0].Value, @"<InlineUIContainer><HyperlinkButton Command=""{Binding TagCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", item.Groups[0].Value, item.Groups[1].Value));
                    }
                    input = input.Replace("互动抽奖", $@"<InlineUIContainer><HyperlinkButton Command=""{{Binding lotteryCommand}}""  CommandParameter=""{dynamic_id}"" IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" ><TextBlock>🎁互动抽奖</TextBlock></HyperlinkButton></InlineUIContainer>");
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
                    p.Inlines.Add(new Run() { Text = description });
                    txt.Blocks.Add(p);
                    return txt;
                }

            }
        }


        public string title { get; set; }

        public int comment_count { get; set; }
        public int vote_count { get; set; }

        public int pictures_count { get; set; }
        public string uname { get; set; }

        private List<picturesModel> _pictures;
        public List<picturesModel> pictures
        {
            get
            {
                if (_pictures != null)
                {
                    _pictures.ForEach(x => x.c = pictures_count);
                }
                return _pictures;
            }
            set { _pictures = value; }
        }


        public Visibility showPic
        {
            get
            {
                if (pictures == null || pictures.Count == 0)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public List<picturesModel> pics
        {
            get
            {
                if (pictures == null)
                {
                    return null;
                }
                else
                {
                    if (pictures.Count <= 9)
                    {
                        return pictures;
                    }
                    else
                    {
                        var ls = pictures.Take(9).ToList();
                        ls.Last().count = pictures.Count;
                        return ls;
                    }
                }

            }
        }



        private List<picturesModel> _showpictures;
        public List<picturesModel> showpictures
        {
            get
            {
                return _showpictures;
            }
            set
            {
                _showpictures = value;
                mPropertyChanged("showpictures");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void mPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }





    }

    public class vote_cfgModel
    {
        public int vote_id { get; set; }
        public string desc { get; set; }
        public int join_num { get; set; }
    }

    public class picturesModel
    {


        private string _img_src;
        public string img_src
        {
            get { return _img_src + "@300w_300h_1e_1c.jpg"; }
            set { _img_src = value; }
        }
        public string img_width { get; set; }
        public string img_height { get; set; }
        public string img_size { get; set; }

        public int count { get; set; }

        public int c { get; set; }

        public Visibility show
        {
            get
            {

                if (count > 9)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public string plus
        {
            get
            {
                if (count > 9)
                {
                    return "+" + (count - 9);
                }
                else
                {
                    return "+0";
                }
            }
        }



    }
    public class DynamicFeedModel
    {


        public List<emoji_detailsModel> emoji_details { get; set; }
        public DynamicFeedModel item { get; set; }
        public long dynamic_id { get; set; }
        public string content { get; set; }
        public string ctrl { get; set; }
        public RichTextBlock Content
        {
            get
            {
                if (content == null || content.Length == 0)
                {
                    return new RichTextBlock();
                }
                try
                {
                    string input = content;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    if (emoji_details != null && emoji_details.Count != 0)
                    {
                        foreach (var item in emoji_details)
                        {
                            input = input.Replace(item.emoji_name, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", item.url));
                        }
                    }
                    //MatchCollection mc = Regex.Matches(input, @"\[(.*?)\]");
                    //foreach (Match item in mc)
                    //{
                    //    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", "ms-appx:///Assets/Emoji/" + item.Groups[1].Value + ".png"));
                    //}
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
                                    var d = content.Substring(item.location, item.length);
                                    var index = input.IndexOf(d);

                                    //var s = input.Substring(index,item.length);
                                    input = input.Remove(index, item.length);
                                    var test = @"<InlineUIContainer><HyperlinkButton Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.data);
                                    input = input.Insert(index, test);
                                }
                            }
                        }
                    }

                    input = input.Replace("^x$%^", "@");
                    input = input.Replace("&", "&amp;");
                    MatchCollection tags = Regex.Matches(input, @"\#(.*?)\#");
                    foreach (Match item in tags)
                    {
                        input = input.Replace(item.Groups[0].Value, @"<InlineUIContainer><HyperlinkButton Command=""{Binding TagCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", item.Groups[0].Value, item.Groups[1].Value));
                    }

                    input = input.Replace("互动抽奖", $@"<InlineUIContainer><HyperlinkButton Command=""{{Binding lotteryCommand}}"" CommandParameter=""{orig_dy_id}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" ><TextBlock>🎁互动抽奖</TextBlock></HyperlinkButton></InlineUIContainer>");

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

                    return new RichTextBlock()
                    {
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                }

            }
        }


        public string title { get; set; }

        public long uid { get; set; }

        public int orig_type { get; set; }
        public string origin { get; set; }
        public long orig_dy_id { get; set; }
        public Visibility showVideo { get; set; } = Visibility.Collapsed;
        public Visibility showArticle { get; set; } = Visibility.Collapsed;
        public Visibility showPhoto { get; set; } = Visibility.Collapsed;
        public Visibility showWeb { get; set; } = Visibility.Collapsed;
        public Visibility showBangumi { get; set; } = Visibility.Collapsed;
        public Visibility showMusic { get; set; } = Visibility.Collapsed;
        public Visibility showText { get; set; } = Visibility.Collapsed;

        public Visibility showMiniVideo { get; set; } = Visibility.Collapsed;
        public DynamicVideoModel video
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 8)
                {
                    return JsonConvert.DeserializeObject<DynamicVideoModel>(origin);
                }
                else
                {
                    return null;
                }
            }
        }

        public DynamicArticleModel article
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 64)
                {
                    return JsonConvert.DeserializeObject<DynamicArticleModel>(origin);
                }
                else
                {
                    return null;
                }
            }
        }
        public DynamicFeed1Model photo
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 2)
                {
                    return JsonConvert.DeserializeObject<DynamicFeed1Model>(origin);
                }
                else
                {
                    return null;
                }
            }
        }

        public DynamicFeed1Model text
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 4)
                {
                    return JsonConvert.DeserializeObject<DynamicFeed1Model>(origin);
                }
                else
                {
                    return null;
                }
            }
        }

        public DynamicWebModel web
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 2048)
                {
                    return JsonConvert.DeserializeObject<DynamicWebModel>(origin);
                }
                else
                {
                    return null;
                }
            }
        }
        public DynamicBangumiModel bangumi
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 512)
                {
                    return JsonConvert.DeserializeObject<DynamicBangumiModel>(origin);
                }
                else
                {
                    return null;
                }
            }
        }
        public DynamicMusicModel music
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 256)
                {
                    return JsonConvert.DeserializeObject<DynamicMusicModel>(origin);
                }
                else
                {
                    return null;
                }
            }
        }

        public DynamicMiniVideoModel minivideo
        {
            get
            {
                if (origin != null && origin.Length != 0 && item.orig_type == 16)
                {
                    return JsonConvert.DeserializeObject<DynamicMiniVideoModel>(origin);
                }
                else
                {
                    return null;
                }
            }
        }


        public DynamicFeedModel origin_extension { get; set; }
        public vote_cfgModel vote_cfg { get; set; }
        public string lott { get; set; }

        public Visibility showExtension { get { if (origin_extension != null && origin_extension.vote_cfg != null) { return Visibility.Visible; } else { return Visibility.Collapsed; } } }
        public Visibility showLott
        {
            get
            {
                if (origin_extension != null && origin_extension.lott != null)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
    }
    public class ctrlModel
    {
        public string data { get; set; }
        public int length { get; set; }
        public int location { get; set; }
        public int type { get; set; }

    }
    public class DynamicWebModel
    {
        public DynamicWebModel user { get; set; }
        public DynamicWebModel vest { get; set; }
        public DynamicWebModel sketch { get; set; }
        public List<emoji_detailsModel> emoji_details { get; set; }
        public string content { get; set; }

        public string desc_text { get; set; }

        public string title { get; set; }
        public string cover_url { get; set; }
        public string target_url { get; set; }
        public string face { get; set; }
        public string uname { get; set; }
        public string uid { get; set; }

        public string ctrl { get; set; }
        public RichTextBlock Content
        {
            get
            {
                if (content == null || content.Length == 0)
                {
                    return new RichTextBlock();
                }
                try
                {
                    string input = content;
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    if (emoji_details != null && emoji_details.Count != 0)
                    {
                        foreach (var item in emoji_details)
                        {
                            input = input.Replace(item.emoji_name, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", item.url));
                        }
                    }
                    //MatchCollection mc = Regex.Matches(input, @"\[(.*?)\]");
                    //foreach (Match item in mc)
                    //{
                    //    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Image Source=""{0}"" Width=""24"" Height=""24""/></InlineUIContainer>", "ms-appx:///Assets/Emoji/" + item.Groups[1].Value + ".png"));
                    //}
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
                                    var d = content.Substring(item.location, item.length);
                                    var index = input.IndexOf(d);

                                    //var s = input.Substring(index,item.length);
                                    input = input.Remove(index, item.length);
                                    var test = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding ButtonCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.data);
                                    input = input.Insert(index, test);
                                }
                            }
                        }
                    }

                    input = input.Replace("^x$%^", "@");
                    input = input.Replace("&", "&amp;");
                    MatchCollection tags = Regex.Matches(input, @"\#(.*?)\#");
                    foreach (Match item in tags)
                    {
                        input = input.Replace(item.Groups[0].Value, @"<InlineUIContainer><HyperlinkButton Command=""{Binding TagCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", item.Groups[0].Value, item.Groups[1].Value));
                    }

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

                    return new RichTextBlock()
                    {
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                }

            }
        }
        public Visibility showweb
        {
            get
            {
                if (content != null && content.Length != 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

    }
}