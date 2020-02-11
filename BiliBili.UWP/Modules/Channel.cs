using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules.ChannelModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BiliBili.UWP.Modules
{
    public class Channel : IModules
    {
        /// <summary>
        /// 读取个人信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ChannelModel>> GetChannel()
        {
            try
            {
                string url = $"https://app.bilibili.com/x/channel/list?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    string re = model.json["data"].ToString();
                    var m = JsonConvert.DeserializeObject<ChannelModel>(re.Replace("goto","_goto"));
                    return new ReturnModel<ChannelModel>()
                    {
                        success = true,
                        message = "",
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<ChannelModel>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<ChannelModel>(ex);
            }
        }

        /// <summary>
        /// 订阅频道
        /// </summary>
        /// <param name="channel_id">频道ID</param>
        /// <returns></returns>
        public async Task<ReturnModel> FollowChannel(int channel_id)
        {
            try
            {
                string url = "https://app.bilibili.com/x/channel/add";
                string data = $"access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&channel_id={channel_id}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                data += "&sign=" + ApiHelper.GetSign(data);
                var results = await WebClientClass.PostResults(new Uri(url), data);
                var model = results.ToDynamicJObject();
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
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }
        /// <summary>
        /// 取消订阅频道
        /// </summary>
        /// <param name="channel_id">频道ID</param>
        /// <returns></returns>
        public async Task<ReturnModel> CancelFollowChannel(int channel_id)
        {
            try
            {
                string url = "https://app.bilibili.com/x/channel/cancel";
                string data = $"access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&channel_id={channel_id}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                data += "&sign=" + ApiHelper.GetSign(data);
                var results = await WebClientClass.PostResults(new Uri(url), data);
                var model = results.ToDynamicJObject();
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
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }

        /// <summary>
        /// 读取我的全部订阅频道
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<Atten_channel>>> GetFollowChannel()
        {
            try
            {
                string url = $"https://app.bilibili.com/x/channel/subscribe?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    var m = JsonConvert.DeserializeObject<ObservableCollection<Atten_channel>>(model.json["data"].ToString());
                    return new ReturnModel<ObservableCollection<Atten_channel>>()
                    {
                        success = true,
                        message = "",
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<Atten_channel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<Atten_channel>>(ex);
            }
        }


        /// <summary>
        /// 读取频道视频
        /// </summary>
        /// <param name="channel_id">频道ID</param>
        /// <param name="name">频道名称</param>
        /// <param name="page">频道页数</param>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<ChannelFeedModel>>> GetChannelFeeds(int channel_id,string name,int page=1)
        {
            try
            {
                string url = $"https://app.bilibili.com/x/channel/feed?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&channel_id={channel_id}&channel_name={Uri.EscapeDataString(name)}&display_id={page}&login_event=0&mobi_app=android&platform=android&pull={(page==1).ToString().ToLower()}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                results = results.Replace("goto", "_goto");
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {

                    ChannelFeedRootModel m = JsonConvert.DeserializeObject<ChannelFeedRootModel>(model.json["data"].ToString());
                    return new ReturnModel<ObservableCollection<ChannelFeedModel>>()
                    {
                        success = true,
                        message = "",
                        data = m.feed
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<ChannelFeedModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<ChannelFeedModel>>(ex);
            }
        }

        /// <summary>
        /// 读取频道信息
        /// </summary>
        /// <param name="channel_id">频道ID</param>
        /// <param name="name">频道名称</param>
        /// <returns></returns>
        public async Task<ReturnModel<FeedTabModel>> GetChannelTab(int channel_id,string name)
        {
            try
            {
                string url = $"https://app.bilibili.com/x/channel/feed/tab?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&channel_id={channel_id}&channel_name={Uri.EscapeDataString(name)}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {

                    var m = JsonConvert.DeserializeObject<FeedTabModel>(model.json["data"].ToString());
                    return new ReturnModel<FeedTabModel>()
                    {
                        success = true,
                        message = "",
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<FeedTabModel>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<FeedTabModel>(ex);
            }
        }

    }
    namespace ChannelModels
    {
        public class Region_top
        {
            /// <summary>
            /// Tid
            /// </summary>
            public int tid { get; set; }
            /// <summary>
            /// Reid
            /// </summary>
            public int reid { get; set; }
            /// <summary>
            /// 番剧
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/archive/6f629bd0dcd71d7b9911803f8e4f94fd0e5b4bfd.png
            /// </summary>
            public string logo { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string _goto { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string param { get; set; }
            /// <summary>
            /// Type
            /// </summary>
            public int type { get; set; }
            /// <summary>
            /// bilibili://pgc/bangumi
            /// </summary>
            public string uri { get; set; }
        }

        public class Region_bottom
        {
            /// <summary>
            /// Tid
            /// </summary>
            public int tid { get; set; }
            /// <summary>
            /// Reid
            /// </summary>
            public int reid { get; set; }
            /// <summary>
            /// 直播
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/archive/1b0ac7eafd51b03a0dc5b2390eec2fbffb25adf7.png
            /// </summary>
            public string logo { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string _goto { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string param { get; set; }
            /// <summary>
            /// Type
            /// </summary>
            public int type { get; set; }
            /// <summary>
            /// bilibili://livearea
            /// </summary>
            public string uri { get; set; }
        }

        public class Rec_channel : INotifyPropertyChanged
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 甜点
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/archive/6713fd135f064bd361bbdb30a27c79d2bc1f375f.jpg
            /// </summary>
            public string cover { get; set; }
            /// <summary>
            /// Atten
            /// </summary>
            public int atten { get; set; }
            private Visibility _showCancel = Visibility.Collapsed;
            public Visibility showCancel
            {
                get
                {
                    return _showCancel;
                }
                set
                {
                    _showCancel = value;
                    ThisPropertyChanged("showCancel");
                }
            }

            private Visibility _showAtten = Visibility.Visible;
            public Visibility showAtten
            {
                get
                {
                    return _showAtten;
                }
                set
                {
                    _showAtten = value;
                    ThisPropertyChanged("showAtten");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void ThisPropertyChanged(string name)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(null, new PropertyChangedEventArgs(name));
                }
            }


        }
        public class Atten_channel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 手机评测
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Is_atten
            /// </summary>
            public int is_atten { get; set; }
            /// <summary>
            /// http://i0.hdslb.com/bfs/archive/c5289e87883077079f675994f6f8e16559a59325.jpg
            /// </summary>
            public string cover { get; set; }
            /// <summary>
            /// Atten
            /// </summary>
            public int atten { get; set; }
        }
        public class ChannelModel
        {
            /// <summary>
            /// Region_top
            /// </summary>
            public ObservableCollection<Region_top> region_top { get; set; }
            /// <summary>
            /// Region_bottom
            /// </summary>
            public ObservableCollection<Region_bottom> region_bottom { get; set; }
            /// <summary>
            /// Rec_channel
            /// </summary>
            public ObservableCollection<Rec_channel> rec_channel { get; set; }
            public ObservableCollection<Atten_channel> atten_channel { get; set; }

            public Visibility showAtten_channel
            {
                get
                {
                    if (atten_channel != null && atten_channel.Count != 0)
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
            /// 14033727683789547391
            /// </summary>
            public string ver { get; set; }
        }



        public class Dislike_reasons
        {
            /// <summary>
            /// Reason_id
            /// </summary>
            public int reason_id { get; set; }
            /// <summary>
            /// UP主:我是Javi
            /// </summary>
            public string reason_name { get; set; }
        }

        public class ChannelFeedModel
        {
            /// <summary>
            /// 花4000多在闲鱼买iPhoneX差点翻车
            /// </summary>
            public string title { get; set; }
            private string _cover;
            /// <summary>
            /// https://i0.hdslb.com/bfs/archive/147c3c0625231250ae57cd3bc2da369b1c8d202b.jpg
            /// </summary>
            public string cover {
                get { return _cover + "@160w_100h.jpg"; }
                set { _cover = value; }
            }


            /// <summary>
            /// bilibili://video/27108593?player_width=1920&player_height=1080&player_rotate=0
            /// </summary>
            public string uri { get; set; }
            /// <summary>
            /// 27108593
            /// </summary>
            public string param { get; set; }
            /// <summary>
            /// av
            /// </summary>
            public string _goto { get; set; }
            /// <summary>
            /// 337弹幕
            /// </summary>
            public string desc { get; set; }
            private string _play;
            /// <summary>
            /// Play
            /// </summary>
            public string play {
                get { return _play.ToW(); }
                set { _play = value; }
            }

            private string _danmaku;
            /// <summary>
            /// Danmaku
            /// </summary>
            public string danmaku
            {
                get { return _danmaku.ToW(); }
                set { _danmaku = value; }
            }
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
            /// Cid
            /// </summary>
            public int cid { get; set; }
            /// <summary>
            /// Tid
            /// </summary>
            public int tid { get; set; }
            /// <summary>
            /// 数码
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
            /// Duration
            /// </summary>
            public int duration { get; set; }
            /// <summary>
            /// Mid
            /// </summary>
            public int mid { get; set; }
            /// <summary>
            /// 我是Javi
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// http://i2.hdslb.com/bfs/face/bbcf32523c416627638b23278de07b46cd476996.jpg
            /// </summary>
            public string face { get; set; }
            /// <summary>
            /// recommend
            /// </summary>
            public string from_type { get; set; }
        }

        public class ChannelFeedRootModel
        {
            /// <summary>
            /// Feed
            /// </summary>
            public ObservableCollection<ChannelFeedModel> feed { get; set; }
        }

        public class FeedTabModel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// TV动画
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Is_atten
            /// </summary>
            public int is_atten { get; set; }
            /// <summary>
            /// Atten
            /// </summary>
            public int atten { get; set; }
        }


    }
}
