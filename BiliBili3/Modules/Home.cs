using BiliBili3.Helper;
using BiliBili3.Models;
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
using BiliBili3.Modules.HomeModels;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using System.ComponentModel;

namespace BiliBili3.Modules
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
                string url = $"https://app.bilibili.com/x/feed/index?access_key={ApiHelper.access_key}&appkey={ApiHelper._appKey}&build={ApiHelper.build}&flush=0&idx={idx}&login_event=2&mobi_app=android&network=wifi&open_event=&platform=android&pull={(idx == "0").ToString().ToLower()}&qn=32&style=2&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                results = results.Replace("goto", "_goto");
                var model = JsonConvert.DeserializeObject<HomeFeedModel>(results);
                if (model.code == 0)
                {
                    ObservableCollection<HomeDataModel> ls = new ObservableCollection<HomeDataModel>();
                    foreach (var item in model.data)
                    {
                        if (!item.is_ad)
                        {
                            ls.Add(item);
                        }
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
        /// 读取首页信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<HomeDataModel>>> GetHot(string last_param = "", string idx = "0")
        {
            try
            {
                string url = $"https://app.bilibili.com/x/v2/show/popular?access_key={ApiHelper.access_key}&appkey={ApiHelper._appKey}&build={ApiHelper.build}&idx={idx}&last_param={last_param}&login_event=0&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                results = results.Replace("goto", "_goto");
                var model = JsonConvert.DeserializeObject<HomeFeedModel>(results);
                if (model.code == 0)
                {
                    ObservableCollection<HomeDataModel> ls = new ObservableCollection<HomeDataModel>();
                    foreach (var item in model.data)
                    {
                        if (!item.is_ad)
                        {
                            ls.Add(item);
                        }
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
        /// 读取首页信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<TabModel>>> GetTab()
        {
            try
            {
                string url = $"https://app.bilibili.com/x/feed/index/tab?access_key={ApiHelper.access_key}&appkey={ApiHelper._appKey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    return new ReturnModel<ObservableCollection<TabModel>>()
                    {
                        success = true,
                        message = "",
                        data = JsonConvert.DeserializeObject<ObservableCollection<TabModel>>(model.json["data"]["tab"].ToString())
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
                string url = $"https://app.bilibili.com/x/feed/index/tab?access_key={ApiHelper.access_key}&appkey={ApiHelper._appKey}&build={ApiHelper.build}&id={tab_id}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                results = results.Replace("goto", "_goto");
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    return new ReturnModel<TabDataModel>()
                    {
                        success = true,
                        message = "",
                        data = JsonConvert.DeserializeObject<TabDataModel>(model.json["data"].ToString())
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
                string url = $"https://app.bilibili.com/x/feed/dislike?access_key={ApiHelper.access_key}&appkey={ApiHelper._appKey}&build={ApiHelper.build}&goto={_goto}&id={id}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
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
                string url = $"https://app.bilibili.com/x/feed/dislike?access_key={ApiHelper.access_key}&appkey={ApiHelper._appKey}&build={ApiHelper.build}&goto={_goto}&id={id}&mid={mid}&mobi_app=android&platform=android&reason_id={reason_id}&rid={rid}&tag_id={tag_id}&ts={ApiHelper.GetTimeSpan}";
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
        public ReturnModel HandelError(Exception ex)
        {
            if (LogHelper.IsNetworkError(ex))
            {
                return new ReturnModel()
                {
                    success = false,
                    message = "无法连接服务器，请检查网络连接"
                };
            }
            else
            {
                LogHelper.WriteLog(ex);
                return new ReturnModel()
                {
                    success = false,
                    message = "出现了一个未处理错误，已记录"
                };
            }
        }
        public ReturnModel<T> HandelError<T>(Exception ex)
        {
            if (LogHelper.IsNetworkError(ex))
            {
                return new ReturnModel<T>()
                {
                    success = false,
                    message = "无法连接服务器，请检查网络连接"
                };
            }
            else
            {
                LogHelper.WriteLog(ex);
                return new ReturnModel<T>()
                {
                    success = false,
                    message = "出现了一个未处理错误，已记录"
                };
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
            /// <summary>
            /// f274ad57391bd9e3e04479ff1e3e7d52
            /// </summary>
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


        public class Dislike_reasons
        {
            /// <summary>
            /// Reason_id
            /// </summary>
            public int reason_id { get; set; }
            /// <summary>
            /// UP主:嗫告篇帖
            /// </summary>
            public string reason_name { get; set; }
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

        public class HomeDataModel
        {
            public ObservableCollection<Banner_item> banner_item { get; set; }
            /// <summary>
            /// 【刀剑神域】当你用爱情公寓的方法打开刀剑神域
            /// </summary>
            public string title { get; set; }

            public string cover { get; set; }
            public string banner_url { get; set; }
            public Tag tag { get; set; }
            public List<string> covers { get; set; }
            public string Cover
            {
                get
                {
                    if (cover != null && cover.Length != 0)
                    {

                        return cover + ((_goto == "av"||_goto== "bangumi_rcmd") ? "" : "@160w_100h_1e_1c.jpg");
                    }
                    if (banner_url != null && banner_url.Length != 0)
                    {
                        return banner_url;
                    }
                    if (covers != null && covers.Count != 0)
                    {
                        return covers[0];
                    }
                    return "";
                }
            }
            /// <summary>
            /// bilibili://video/28430521?page=1&player_preload=%7B%22cid%22%3A49188836%2C%22expire_time%22%3A1533462819%2C%22file_info%22%3A%7B%2215%22%3A%5B%7B%22timelength%22%3A96234%2C%22filesize%22%3A5834702%7D%5D%2C%2232%22%3A%5B%7B%22timelength%22%3A96234%2C%22filesize%22%3A15015218%7D%5D%2C%2264%22%3A%5B%7B%22timelength%22%3A96234%2C%22filesize%22%3A25867434%7D%5D%2C%2280%22%3A%5B%7B%22timelength%22%3A96234%2C%22filesize%22%3A36424063%7D%5D%7D%2C%22support_quality%22%3A%5B80%2C64%2C32%2C15%5D%2C%22support_formats%22%3A%5B%22flv%22%2C%22flv720%22%2C%22flv480%22%2C%22flv360%22%5D%2C%22support_description%22%3A%5B%22%E9%AB%98%E6%B8%85%201080P%22%2C%22%E9%AB%98%E6%B8%85%20720P%22%2C%22%E6%B8%85%E6%99%B0%20480P%22%2C%22%E6%B5%81%E7%95%85%20360P%22%5D%2C%22quality%22%3A32%2C%22url%22%3A%22http%3A%2F%2Fcn-hbcd2-cu-v-12.acgvideo.com%2Fupgcxcode%2F36%2F88%2F49188836%2F49188836-1-32.flv%3Fexpires%3D1533466200%5Cu0026platform%3Dandroid%5Cu0026ssig%3D4hztzWwnr0NuTEZ7CuW6HA%5Cu0026oi%3D2018869370%5Cu0026nfa%3DuTIiNt%2BAQjcYULykM2EttA%3D%3D%5Cu0026dynamic%3D1%5Cu0026hfa%3D2035673872%5Cu0026hfb%3DNzUxMjI5MWJlMDBjMDY0YTQxNjFjMTJiYWE0MjEwYmQ%3D%5Cu0026trid%3Dd749ddd8e0e84cfb838e56f88f10d803%5Cu0026nfc%3D1%22%2C%22video_codecid%22%3A7%2C%22video_project%22%3Atrue%2C%22fnver%22%3A0%2C%22fnval%22%3A0%7D&player_width=1920&player_height=1080&player_rotate=0
            /// </summary>
            public string uri { get; set; }
            /// <summary>
            /// 28430521
            /// </summary>
            public string param { get; set; }
            /// <summary>
            /// av
            /// </summary>
            public string _goto { get; set; }
            /// <summary>
            /// 249弹幕
            /// </summary>
            public string desc { get; set; }
            /// <summary>
            /// Play
            /// </summary>
            public int play { get; set; }
            /// <summary>
            /// Danmaku
            /// </summary>
            public int danmaku { get; set; }
            /// <summary>
            /// Reply
            /// </summary>
            public int reply { get; set; }
            /// <summary>
            /// Favorite
            /// </summary>
            public int favorite { get; set; }
            /// <summary>
            /// Coin
            /// </summary>
            public int coin { get; set; }
            /// <summary>
            /// Share
            /// </summary>
            public int share { get; set; }
            /// <summary>
            /// Like
            /// </summary>
            public int like { get; set; }
            /// <summary>
            /// Dislike
            /// </summary>
            public int dislike { get; set; }
            /// <summary>
            /// Duration
            /// </summary>
            public int duration { get; set; }
            /// <summary>
            /// Idx
            /// </summary>
            public string idx { get; set; }
            /// <summary>
            /// Cid
            /// </summary>
            public int cid { get; set; }
            /// <summary>
            /// Tid
            /// </summary>
            public int tid { get; set; }
            /// <summary>
            /// MAD·AMV
            /// </summary>
            public string tname { get; set; }
            /// <summary>
            /// Dislike_reasons
            /// </summary>
            public List<Dislike_reasons> dislike_reasons { get; set; }
            /// <summary>
            /// Ctime
            /// </summary>
            public int ctime { get; set; }
            /// <summary>
            /// Autoplay
            /// </summary>
            public int autoplay { get; set; }
            /// <summary>
            /// Mid
            /// </summary>
            public int mid { get; set; }
            /// <summary>
            /// 嗫告篇帖
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/face/a5d995ccad22fcff397d5dfa14913d25cb23b48d.jpg
            /// </summary>
            public string face { get; set; }

            public bool is_ad { get; set; } = false;
            public string online { get; set; }
            public string badge { get; set; }
            public string area { get; set; }
            public Category category { get; set; }

            public Visibility showTag
            {
                get
                {
                    if (badge != null && badge.Length != 0)
                    {
                        return Visibility.Visible;
                    }
                    else if (_goto == "live")
                    {
                        badge = "直播";
                        return Visibility.Visible;
                    }
                    else if (_goto == "article_s")
                    {
                        badge = "专栏";
                        return Visibility.Visible;
                    }
                    else if (_goto == "ad_web_s")
                    {
                        badge = "广告";
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            public Visibility showRight
            {
                get
                {
                    if (_goto == "av")
                    {
                        return Visibility.Visible;
                    }
                    else if (_goto == "live")
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            public Visibility showTname
            {
                get
                {
                    if (_goto == "av")
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            public string hot_bottom
            {
                get
                {
                    return name + " · " + tname;
                }
            }
            public string bottom
            {
                get
                {
                    switch (_goto)
                    {
                        case "av":
                            return tname + ((tag == null) ? "" : " · " + tag.tag_name);
                        case "live":
                            return area;
                        default:
                            return "";
                    }

                }
            }
            public string rightText
            {
                get
                {
                    if (_goto == "av")
                    {
                        var ts = TimeSpan.FromSeconds(duration);
                        return ts.ToString(@"mm\:ss");
                    }
                    else if (_goto == "live")
                    {
                        return name;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            public string text
            {
                get
                {
                    switch (_goto)
                    {
                        case "av":
                            return play.ToW() + "观看 " + danmaku.ToW() + "弹幕";
                        case "live":
                            return online.ToW() + "人气";
                        case "article_s":
                            return play.ToW() + "观看 " + reply.ToW() + "评论";
                        default:
                            return "";
                    }
                }
            }

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
            public ObservableCollection<HomeDataModel> data { get; set; }
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
            public int tab_id { get; set; }
        }


        public class TabVideoItemModel
        {
            public string Cover
            {
                get
                {
                    if (_goto=="web")
                    {
                        return cover + "@100w_100h_1e_1c.jpg";
                    }
                    return cover + "@160w_100h.jpg";

                }
            }
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
            public string cover { get; set; }
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
            public string image { get; set; }
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
                get { return _cover; }
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
