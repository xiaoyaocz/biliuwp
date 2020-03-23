using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http.Filters;
using BiliBili.UWP.Modules.HomeModels;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using System.ComponentModel;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace BiliBili.UWP.Modules
{
    public class Home : IModules
    {
        /// <summary>
        /// 读取首页信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<HomeDataModel>>> GetHome(string idx = "0")
        {
            try
            {
                string url = $"https://app.bilibili.com/x/v2/feed/index?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&flush=0&idx={idx}&login_event=2&mobi_app=android&network=wifi&open_event=&platform=android&pull={(idx == "0").ToString().ToLower()}&qn=32&style=2&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<HomeFeedModel>(results);
                if (model.code == 0)
                {
                    
                    ObservableCollection<HomeDataModel> ls = new ObservableCollection<HomeDataModel>();
                    foreach (var item in model.data.items)
                    {
                        if(!item.card_goto.Contains("ad_web")) ls.Add(item);
                    }
                    return new ReturnModel<ObservableCollection<HomeDataModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<HomeDataModel>>()
                    {
                        success = false,
                        message = model.message.ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<ObservableCollection<HomeDataModel>>(ex);
            }
        }
        /// <summary>
        /// 读取热门信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<object[]>> GetHot(string last_param = "", string idx = "0")
        {
            try
            {
                string url = $"https://app.bilibili.com/x/v2/show/popular/index?appkey={ApiHelper.AndroidKey.Appkey }&build={ApiHelper.build}&fnval=16&fnver=0&force_host=0&fourk=1&idx={idx}&last_param={last_param}&mobi_app=android&platform=android&qn=32&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                results = results.Replace("goto", "_goto");
                var model = JsonConvert.DeserializeObject<HotModel>(results);
                if (model.code == 0)
                {
                    ObservableCollection<HotItemModel> ls = new ObservableCollection<HotItemModel>();
                    foreach (var item in model.data)
                    {
                        if (item._goto=="av")
                        {
                            ls.Add(item);
                        }
                    }
                    return new ReturnModel<object[]>()
                    {
                        success = true,
                        message = "",
                        data = new object[] {
                            ls,
                            model.config.top_items
                        }
                    };
                }
                else
                {
                    return new ReturnModel<object[]>()
                    {
                        success = false,
                        message = model.message.ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<object[]>(ex);
            }
        }

        /// <summary>
        /// 读取首页TAB信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<TabModel>>> GetTab()
        {
            try
            {
                string url = $"https://app.bilibili.com/x/resource/show/tab?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    var tabs = JsonConvert.DeserializeObject<List<TabModel>>(model.json["data"]["tab"].ToString());
                    ObservableCollection<TabModel> ls = new ObservableCollection<TabModel>();
                    foreach (var item in tabs.Where(x=>x.tab_id.ToInt32()!=0))
                    {
                        ls.Add(item);
                    }
                    return new ReturnModel<ObservableCollection<TabModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<TabModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<TabModel>>(ex);
            }
        }

        /// <summary>
        /// 读取首页信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<TabDataModel>> GetTabData(int tab_id)
        {
            try
            {
                string url = $"https://app.bilibili.com/x/feed/index/tab?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&id={tab_id}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                results = results.Replace("goto", "_goto");
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    var data = JsonConvert.DeserializeObject<TabDataModel>(model.json["data"].ToString());
                    var banner = data.item.FirstOrDefault(x => x._goto == "banner");
                    if (banner!=null)
                    {
                        var list= banner.banner_item.Where(x => string.IsNullOrEmpty(x.image)).ToList();
                        foreach (var item in list)
                        {
                            banner.banner_item.Remove(item);
                        }
                        if (banner.banner_item.Count == 0)
                        {
                            data.item.Remove(banner);
                        }
                    }
                    return new ReturnModel<TabDataModel>()
                    {
                        success = true,
                        message = "",
                        data = data
                    };
                }
                else
                {
                    return new ReturnModel<TabDataModel>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {
                return HandelError<TabDataModel>(ex);
            }
        }
        /// <summary>
        /// 不感兴趣
        /// </summary>
        /// <param name="_goto"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ReturnModel> UnLike(string _goto, string id)
        {
            try
            {
                string url = $"https://app.bilibili.com/x/feed/dislike?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&goto={_goto}&id={id}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<HomeFeedModel>(results);
                if (model.code == 0)
                {
                    return new ReturnModel()
                    {
                        success = true,
                        message = ""
                    };
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = model.message.ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError(ex);
            }
        }
        /// <summary>
        /// 不感兴趣，需要理由
        /// </summary>
        /// <param name="_goto"></param>
        /// <param name="id"></param>
        /// <param name="mid"></param>
        /// <param name="reason_id"></param>
        /// <param name="rid"></param>
        /// <param name="tag_id"></param>
        /// <returns></returns>
        public async Task<ReturnModel> UnLikeNeedReason(string _goto, string id, string mid, int reason_id, string rid, string tag_id)
        {
            try
            {
                string url = $"https://app.bilibili.com/x/feed/dislike?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&goto={_goto}&id={id}&mid={mid}&mobi_app=android&platform=android&reason_id={reason_id}&rid={rid}&tag_id={tag_id}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<HomeFeedModel>(results);
                if (model.code == 0)
                {

                    return new ReturnModel()
                    {
                        success = true,
                        message = ""
                    };
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = model.message.ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError(ex);
            }
        }
       
    }

    namespace HomeModels
    {
        public class Banner_item
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 关于全面进行内容整改的公告
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/archive/939bc05bfe3598d5ad1e759ff022e0bc0c99a16b.png
            /// </summary>

            public string image { get; set; }
            public string hash { get; set; }
            /// <summary>
            /// https://www.bilibili.com/blackboard/activity-BJ0jN5PN7.html
            /// </summary>
            public string uri { get; set; }
            /// <summary>
            /// 1533453333803
            /// </summary>
            public string request_id { get; set; }
            /// <summary>
            /// Server_type
            /// </summary>
            public int server_type { get; set; }
            /// <summary>
            /// Resource_id
            /// </summary>
            public int resource_id { get; set; }
            /// <summary>
            /// Index
            /// </summary>
            public int index { get; set; }
            /// <summary>
            /// Cm_mark
            /// </summary>
            public int cm_mark { get; set; }
        }
        public class HomeFeedRcmd_reason_style
        {
            public string text { get; set; }
            public string text_color { get; set; }
            public SolidColorBrush textColor
            {
                get
                {
                    return new SolidColorBrush(Utils.ToColor2(text_color));
                }
            }
            public string bg_color { get; set; }
            public SolidColorBrush bgColor
            {
                get
                {
                    return new SolidColorBrush(Utils.ToColor2(bg_color));
                }
            }
            public string border_color { get; set; }
            public SolidColorBrush borderColor
            {
                get
                {
                    return new SolidColorBrush(Utils.ToColor2(border_color));
                }
            }
            public int bg_style { get; set; }

        }
        public class HomeFeedDesc_button
        {
            public string text { get; set; }
        }
        public class Dislike_reasons
        {
            /// <summary>
            /// Reason_id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// UP主:嗫告篇帖
            /// </summary>
            public string name { get; set; }
        }
        public class Children
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 日常
            /// </summary>
            public string name { get; set; }
        }

        public class Category
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 生活
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Children
            /// </summary>
            public Children children { get; set; }
        }

        public class Tag
        {
            /// <summary>
            /// Is_atten
            /// </summary>
            public int is_atten { get; set; }
            /// <summary>
            /// Tag_id
            /// </summary>
            public int tag_id { get; set; }
            /// <summary>
            /// 英雄联盟
            /// </summary>
            public string tag_name { get; set; }
        }
        public class HotModel
        {
            public int code { get; set; }
            public string message { get; set; }
            public ObservableCollection<HotItemModel> data { get; set; }

            public HotConfigModel config { get; set; }
        }
        public class HotConfigModel
        {
            public string item_title { get; set; }
            public ObservableCollection<HotTopItemModel> top_items { get; set; }


        }
        public class HotTopItemModel
        {
            public string icon { get; set; }
            public string title { get; set; }
            public string module_id { get; set; }
            public string uri { get; set; }
        }
        public class HotItemModel
        {
            public string _goto { get; set; }
            public string param { get; set; }
            public string idx { get; set; }
            public string cover { get; set; }

            public string title { get; set; }
            public string right_desc_1 { get; set; }
            public string right_desc_2 { get; set; }
            public RcmdReasonStyle rcmd_reason_style { get; set; }
        }
        public class RcmdReasonStyle
        {
            public string text { get; set; }
            public string text_color { get; set; }
            public SolidColorBrush Text_color
            {
                get
                {
                    if (text_color!=null)
                    {
                        return new SolidColorBrush(text_color.ToColor2());
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
            public string bg_color { get; set; }
            public SolidColorBrush Bg_color
            {
                get
                {
                    if (bg_color != null)
                    {
                        return new SolidColorBrush(bg_color.ToColor2());
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Transparent);
                    }
                  
                }
            }
            public string border_color { get; set; }
            public SolidColorBrush Border_color
            {
                get
                {
                    if (border_color != null)
                    {
                        return new SolidColorBrush(border_color.ToColor2());
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Transparent);
                    }
                }
            }

            public int bg_style { get; set; }
        }
        public class Three_point
        {
            public List<Dislike_reasons> dislike_reasons { get; set; }
        }
        public class HomeDataModel
        {
            public ObservableCollection<Banner_item> banner_item { get; set; }
            private string _title="";
            public string title {
                get {
                    if (_title==""&&uri=="")
                    {
                        return "你追的番剧更新啦~";
                    }
                    return _title;
                }
                set { _title = value; }
            }

            public string cover { get; set; }
           
            public string uri { get; set; }
            public string param { get; set; }
            public string card_goto { get; set; }
          
            public string idx { get; set; }


            public Three_point three_point { get; set; }
            public HomeFeedArgs args { get; set; }

            public HomeFeedRcmd_reason_style rcmd_reason_style { get; set; }
            public HomeFeedDesc_button desc_button { get; set; }
            public string cover_left_text_1 { get; set; }
            public string cover_left_text_2 { get; set; }
            public int cover_left_icon_1 { get; set; }
            public int cover_left_icon_2 { get; set; }
            public string left_text
            {
                get
                {
                    return $"{iconToText(cover_left_icon_1)}{cover_left_text_1??""} {iconToText(cover_left_icon_2)}{cover_left_text_2??""}";
                }
            }
            public string cover_right_text { get; set; }

            public string badge { get; set; }
            public bool showBadge
            {
                get
                {
                    return !string.IsNullOrEmpty(badge);
                }
            }

            public bool showCoverText
            {
                get
                {
                    return !string.IsNullOrEmpty(cover_left_text_1) || !string.IsNullOrEmpty(cover_left_text_2) || !string.IsNullOrEmpty(cover_right_text);
                }
            }
            public bool showRcmd
            {
                get
                {
                    return rcmd_reason_style != null;
                }
            }
            public string bottomText
            {
                get
                {
                    if (desc_button!=null)
                    {
                        return desc_button.text;
                    }
                    return "";
                }
            }
       
            public string iconToText(int icon)
            {
                switch (icon)
                {
                    case 1:
                    case 6:
                        return "观看:";
                    case 2:
                        return "人气:";
                    case 3:
                        return "弹幕:";
                    case 4:
                        return "追番:";
                    case 7:
                        return "评论:";
                    default:
                        return "";
                }
            }
        }
        public class HomeFeedArgs
        {
            public string up_id { get; set; }
            public string up_name { get; set; }
            public int rid { get; set; }
            public int tid { get; set; }
            public string tname { get; set; }
            public string rname { get; set; }
            public int aid { get; set; }
        }
        public class HomeFeedModel
        {
            /// <summary>
            /// Code
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 0
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// Ttl
            /// </summary>
            public int ttl { get; set; }

            /// <summary>
            /// Data
            /// </summary>
            public HomeFeedDataModel data { get; set; }
        }
        public class HomeFeedDataModel
        {
            public ObservableCollection<HomeDataModel> items { get; set; }
        }
        public class TabModel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 动画透镜
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Tab_id
            /// </summary>
            public string tab_id { get; set; }
        }


        public class TabVideoItemModel
        {
           
            public string rightText
            {
                get
                {

                    var ts = TimeSpan.FromSeconds(duration);
                    return ts.ToString(@"mm\:ss");

                }
            }
            public string text
            {
                get
                {
                    return play.ToW() + "观看 " + danmaku.ToW() + "弹幕";
                }
            }

            /// <summary>
            /// 《人生一串》第六集之《朝圣之地》
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string cover { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string uri { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string param { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string _goto { get; set; }
            /// <summary>
            /// 104951弹幕
            /// </summary>
            public string desc { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int play { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int danmaku { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int reply { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int favorite { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int coin { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int share { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int like { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int dislike { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int duration { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int cid { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int tid { get; set; }
            /// <summary>
            /// 社会·美食·旅行
            /// </summary>
            public string tname { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int ctime { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int mid { get; set; }
            /// <summary>
            /// bilibili纪录片
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string face { get; set; }
        }

        public class TabItemModel
        {
            public Visibility showMore
            {
                get
                {
                    if (_goto== "tag_rcmd")
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public string _goto { get; set; }
            /// <summary>
            /// 独乐串不如众乐串~
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/archive/855bfc0a07e498cb16a520c62f86048a236cbbc8.png
            /// </summary>
            private string _cover;

            public string cover
            {
                get { return _cover+"@300h.jpg"; }
                set { _cover = value; }
            }


            /// <summary>
            /// https://www.bilibili.com/blackboard/activity-Byw_ZqWbZm.html
            /// </summary>
            public string uri { get; set; }
            /// <summary>
            /// 1469
            /// </summary>
            public string param { get; set; }
            public string content { get; set; }
            /// <summary>
            /// 最强烧烤在哪里？
            /// </summary>
            public string desc { get; set; }
            /// <summary>
            /// 视频征集
            /// </summary>
            public string badge { get; set; }
            /// <summary>
            /// Hide_badge
            /// </summary>
            public bool hide_badge { get; set; }
            /// <summary>
            /// Ratio
            /// </summary>
            public int ratio { get; set; }
            public ObservableCollection<TabVideoItemModel> item { get; set; }
            public ObservableCollection<TabBannerItem> banner_item { get; set; }
        }
        public class TabBannerItem
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 骨头骨头
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/archive/8ce99a09b5639ac5ff74b5736536fabf8cfd5a54.jpg
            /// </summary>
            private string _image;

            public string image
            {
                get { return _image; }
                set { _image = value; }
            }


            /// <summary>
            /// 
            /// </summary>
            public string hash { get; set; }
            /// <summary>
            /// https://h5.dianping.com/app/h5-ranklist-static/list_poi.html
            /// </summary>
            public string uri { get; set; }
            /// <summary>
            /// Server_type
            /// </summary>
            public int server_type { get; set; }
            /// <summary>
            /// Cm_mark
            /// </summary>
            public int cm_mark { get; set; }
        }

        public class TabDataModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public void ThisPropertyChanged(string name)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(null, new PropertyChangedEventArgs(name));
                }
            }


            private string _cover;

            public string cover
            {
                get { return _cover+"@300h.jpg"; }
                set { _cover = value; ThisPropertyChanged("cover"); }
            }
            private ObservableCollection<TabItemModel> _item;

            public ObservableCollection<TabItemModel> item
            {
                get { return _item; }
                set { _item = value; ThisPropertyChanged("item"); }
            }

           
        }


    }

}
