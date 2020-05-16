using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules.LiveModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using HtmlAgilityPack;

namespace BiliBili.UWP.Modules
{
    /// <summary>
    /// 直播间模块
    /// </summary>
    public class LiveRoom : IModules
    {
        public enum LiveStatus
        {
            /// <summary>
            /// 闲置中
            /// </summary>
            Stop = 0,
            /// <summary>
            /// 直播中
            /// </summary>
            Live = 1,
            /// <summary>
            /// 轮播
            /// </summary>
            Round = 2
        }
        public static List<TitleItemModel> titleItems;
        public async static Task GetTitleItems()
        {
            try
            {
                var results = await WebClientClass.GetResults(new Uri("https://api.live.bilibili.com/rc/v1/Title/webTitles"));
                var m = JsonConvert.DeserializeObject<TitleModel>(results);
                if (m.code == 0)
                {
                    titleItems = m.data;
                }
            }
            catch (Exception)
            {
            }
        }
        public ObservableCollection<AllGiftsModel> allGifts { get; set; }



        /// <summary>
        /// 取我的礼物
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel> GetAllGifts()
        {
            try
            {
                string url = $"https://api.live.bilibili.com/gift/v3/live/gift_config?actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();

                if (model.code == 0)
                {
                    return new ReturnModel()
                    {
                        success = true,
                        message = "",
                        data = JsonConvert.DeserializeObject<ObservableCollection<AllGiftsModel>>(model.json["data"].ToString()),
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
        /// 读取房间的礼物列表
        /// </summary>
        /// <param name="roomid"></param>
        /// <param name="area_v2_id"></param>
        /// <param name="area_v2_parent_id"></param>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<AllGiftsModel>>> GetRoomGifts(int roomid, int area_v2_id, int area_v2_parent_id)
        {
            try
            {
                if (allGifts == null)
                {
                    var all = await GetAllGifts();
                    if (!all.success)
                    {
                        return new ReturnModel<ObservableCollection<AllGiftsModel>>()
                        {
                            success = false,
                            message = "无法读取礼物"
                        };
                    }
                    allGifts = all.data;
                }
                ObservableCollection<AllGiftsModel> roomGifts = new ObservableCollection<AllGiftsModel>();
                string url = $"https://api.live.bilibili.com/gift/v3/live/room_gift_list?access_key={ApiHelper.access_key}&actionkey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&area_v2_id={area_v2_id}&area_v2_parent_id={area_v2_parent_id}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&roomid={roomid}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    foreach (var item in model.json["data"]["list"])
                    {
                        var id = item["id"].ToInt32();
                        roomGifts.Add(allGifts.First(x => x.id == id));
                    }
                    roomGifts.Add(allGifts.First(x => x.id == 1));
                    return new ReturnModel<ObservableCollection<AllGiftsModel>>()
                    {
                        success = true,
                        message = "",
                        data = roomGifts
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<AllGiftsModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }
            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<AllGiftsModel>>(ex);

            }
        }


        /// <summary>
        /// 取我的礼物
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<LiveMyGiftsModel>>> GetMyGifts()
        {
            try
            {

                if (!ApiHelper.IsLogin())
                {
                    return new ReturnModel<ObservableCollection<LiveMyGiftsModel>>()
                    {
                        message = "请先登录",
                        success = false
                    };
                }

                if (allGifts == null)
                {
                    var all = await GetAllGifts();
                    if (!all.success)
                    {
                        return new ReturnModel<ObservableCollection<LiveMyGiftsModel>>()
                        {
                            success = false,
                            message = "加载我的包裹失败"
                        };
                    }
                    allGifts = all.data;
                }

                string url = $"https://api.live.bilibili.com/gift/v2/gift/bag_list?access_key={ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {

                    ObservableCollection<LiveMyGiftsModel> ls = JsonConvert.DeserializeObject<ObservableCollection<LiveMyGiftsModel>>(model.json["data"]["list"].ToString());

                    foreach (var item in ls)
                    {
                        item.img = allGifts.FirstOrDefault(x => x.id == item.gift_id).img_basic;
                    }
                    return new ReturnModel<ObservableCollection<LiveMyGiftsModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<LiveMyGiftsModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<LiveMyGiftsModel>>(ex);
            }





        }

        /// <summary>
        /// 读取房间信息
        /// </summary>
        /// <param name="roomid"></param>
        /// <returns></returns>
        public async Task<ReturnModel<LiveRoomInfoModel>> GetRoomInfo(int roomid)
        {
            try
            {
                string url = $"https://api.live.bilibili.com/room/v1/room/get_info?access_key={ApiHelper.access_key}&actionkey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&from=room&id={roomid}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {

                    LiveRoomInfoModel m = JsonConvert.DeserializeObject<LiveRoomInfoModel>(model.json["data"].ToString().Replace("0000-00-00 00:00:00", "1990-01-01 00:00:00"));
                    m.UserInfo = await GetUpInfo(m.uid);

                    m.htmldesc = HtmlToRichText(m.description);
                    return new ReturnModel<LiveRoomInfoModel>()
                    {
                        success = true,
                        message = "",
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<LiveRoomInfoModel>()
                    {
                        success = false,
                        message = model.message
                    };
                }
            }
            catch (Exception ex)
            {
                return HandelError<LiveRoomInfoModel>(ex);

            }
        }

        /// <summary>
        /// 读取直播UP信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<LiveUpModel> GetUpInfo(int userid)
        {
            try
            {
                string url = $"https://api.live.bilibili.com/live_user/v1/card/card_up?access_key={ApiHelper.access_key}&actionkey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}&uid={userid}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    LiveUpModel m = JsonConvert.DeserializeObject<LiveUpModel>(model.json["data"].ToString());
                    m.lvColor = new SolidColorBrush(Utils.ToColor(m.level_color));
                    return m;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;

            }
        }

        /// <summary>
        /// 读取直播播放地址
        /// </summary>
        /// <param name="roomid"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public async Task<ReturnModel<LivePlayUrlsModel>> GetRoomPlayurl(int roomid, int quality)
        {
            try
            {
                //string url = $"https://api.live.bilibili.com/room/v1/Room/playUrl?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&cid={roomid}&device=android&https_url_req=0&mobi_app=android&platform=android&quality={quality}&ts={ApiHelper.GetTimeSpan}";
                //url += "&sign=" + ApiHelper.GetSign(url);

                string url = $"https://api.live.bilibili.com/room/v1/Room/playUrl?cid={roomid}&qn={quality}&platform=web";
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    LivePlayUrlsModel m = JsonConvert.DeserializeObject<LivePlayUrlsModel>(model.json["data"].ToString());
                    int i = 1;
                    foreach (var item in m.durl)
                    {
                        item.display = "线路" + i;
                        i++;
                    }
                    return new ReturnModel<LivePlayUrlsModel>()
                    {
                        success = true,
                        message = "",
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<LivePlayUrlsModel>()
                    {
                        success = false,
                        message = model.message
                    };
                }
            }
            catch (Exception ex)
            {
                return HandelError<LivePlayUrlsModel>(ex);

            }
        }


        public async Task<ReturnModel<RoundModel>> GetRoundPlayurl(int roomid)
        {
            try
            {
                string url = $"https://api.live.bilibili.com/live/getRoundPlayVideo?room_id={roomid}&&appkey={ApiHelper.AndroidKey.Appkey}type=flv&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    RoundModel m = JsonConvert.DeserializeObject<RoundModel>(model.json["data"].ToString());
                    var results2 = await WebClientClass.GetResults(new Uri(m.play_url));
                    var play = JsonConvert.DeserializeObject<RoundPlayModel>(results2);
                    m.data = play;
                    if (play.durl.Count != 0)
                    {
                        return new ReturnModel<RoundModel>()
                        {
                            success = true,
                            data = m
                        };
                    }
                    else
                    {
                        return new ReturnModel<RoundModel>()
                        {
                            success = false,
                            message = "读取轮播地址失败"
                        };
                    }
                }
                else
                {
                    return new ReturnModel<RoundModel>()
                    {
                        success = false,
                        message = model.message
                    };
                }
            }
            catch (Exception ex)
            {
                return HandelError<RoundModel>(ex);

            }
        }

        /// <summary>
        /// 取最近弹幕列表
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<LiveMsgModel>>> GetLastLiveMsg(int roomid)
        {
            try
            {

                string url = $"https://live.bilibili.com/AppRoom/msg?_device=android&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&platform=android&room_id={roomid}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    ObservableCollection<LiveMsgModel> ls = JsonConvert.DeserializeObject<ObservableCollection<LiveMsgModel>>(model.json["data"]["room"].ToString());

                    foreach (var v in ls)
                    {
                        if (v.vip == 1)
                        {
                            v.isVip = Visibility.Visible;
                        }
                        if (v.svip == 1)
                        {
                            v.isVip = Visibility.Collapsed;
                            v.isBigVip = Visibility.Visible;
                        }
                        if (v.isadmin == 1)
                        {
                            v.isAdmin = Visibility.Visible;
                        }

                        if (v.medal != null && v.medal.Count != 0)
                        {
                            v.medal_name = v.medal[1].ToString();
                            v.medal_lv = v.medal[0].ToString();
                            v.medalColor = v.medal[4].ToString();
                            v.hasMedal = Visibility.Visible;
                        }
                        if (v.user_level != null && v.user_level.Count != 0)
                        {
                            v.ul = "UL" + v.user_level[0].ToString();
                            v.ulColor = v.user_level[2].ToString();
                        }
                        if (v.user_title != null && v.user_title.Length != 0)
                        {
                            v.hasTitle = Visibility.Visible;
                        }
                        v.uname_color = "Gray";

                    }


                    return new ReturnModel<ObservableCollection<LiveMsgModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<LiveMsgModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<LiveMsgModel>>(ex);
            }





        }
        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="text"></param>
        /// <param name="roomid"></param>
        /// <returns></returns>
        public async Task<ReturnModel> SendDanmu(string text, int roomid)
        {
            try
            {
                if (!ApiHelper.IsLogin())
                {
                    await Utils.ShowLoginDialog();
                    if (!ApiHelper.IsLogin())
                    {
                        return new ReturnModel()
                        {
                            success = false,
                            message = "请先登录"
                        };
                    }
                }

                string sendText = $"cid={roomid}&mid={ApiHelper.GetUserId()}&msg={text}&rnd={ApiHelper.GetTimeSpan}&mode=1&pool=0&type=json&color=16777215&fontsize=25&playTime=0.0";

                var url = $"https://api.live.bilibili.com/api/sendmsg?access_key={ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                string result = await WebClientClass.PostResults(new Uri(url), sendText);
                JObject jb = JObject.Parse(result);
                if ((int)jb["code"] == 0)
                {
                    //AddComment(new TextBlock() { Text= "已发送：" + txt_Comment.Text }, true);
                    //if (LoadDanmu)
                    //{
                    //    danmu.AddGunDanmu(new Controls.MyDanmaku.DanMuModel() { DanText = txt_Comment.Text, DanSize = "25", _DanColor = "16777215" }, true);
                    //}

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
                        message = "发送弹幕失败" + jb["msg"]
                    };

                }


            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }



        }
        /// <summary>
        /// 取免费瓜子时间
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<DateTime>> GetFreeSilverCurrentTask()
        {
            if (!ApiHelper.IsLogin())
            {
                return new ReturnModel<DateTime>()
                {
                    success = false,
                    message = "请先登录"
                };
            }
            try
            {
                string url = $"https://api.live.bilibili.com/mobile/freeSilverCurrentTask?access_key={ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JObject.Parse(results);
                if (model["code"].ToInt32() == 0)
                {

                    return new ReturnModel<DateTime>()
                    {
                        success = true,
                        data = Utils.TimestampToDatetime(Convert.ToInt64(model["data"]["time_end"]))
                    };
                }
                else
                {
                    return new ReturnModel<DateTime>()
                    {
                        success = false,
                        message = model["message"].ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<DateTime>(ex);
            }
        }
        /// <summary>
        /// 领取免费瓜子
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<string>> GetFreeSilverAward()
        {
            if (!ApiHelper.IsLogin())
            {
                return new ReturnModel<string>()
                {
                    success = false,
                    message = "请先登录"
                };
            }
            try
            {
                string url = $"https://api.live.bilibili.com/mobile/freeSilverAward?access_key={ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JObject.Parse(results);
                if (model["code"].ToInt32() == 0)
                {
                    return new ReturnModel<string>()
                    {
                        success = true,
                        message = "领取成功,瓜子+" + model["data"]["awardSilver"],
                        data = model["data"]["silver"].ToString()
                    };
                }
                else
                {
                    return new ReturnModel<string>()
                    {
                        success = false,
                        message = model["message"].ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<string>(ex);
            }
        }

       

        /// <summary>
        /// 七日榜
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<GiftTopListModel>>> GetGiftTop(int roomid)
        {
            try
            {

                string url = $"https://api.live.bilibili.com/AppRoom/getGiftTop?actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&room_id={roomid}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    ObservableCollection<GiftTopListModel> ls = JsonConvert.DeserializeObject<ObservableCollection<GiftTopListModel>>(model.json["data"]["list"].ToString());


                    return new ReturnModel<ObservableCollection<GiftTopListModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<GiftTopListModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<GiftTopListModel>>(ex);
            }
        }
        /// <summary>
        /// 粉丝榜
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<MedalRankListModel>>> GetMedalRankList(int roomid)
        {
            try
            {

                string url = $"https://api.live.bilibili.com/AppRoom/medalRankList?actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&room_id={roomid}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    ObservableCollection<MedalRankListModel> ls = JsonConvert.DeserializeObject<ObservableCollection<MedalRankListModel>>(model.json["data"]["list"].ToString());
                    int i = 1;
                    foreach (var item in ls)
                    {
                        item.rank = i;
                        i++;
                    }

                    return new ReturnModel<ObservableCollection<MedalRankListModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<MedalRankListModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<MedalRankListModel>>(ex);
            }
        }

        /// <summary>
        /// 读取活动榜单
        /// </summary>
        /// <param name="roomid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<OpTopListModel>>> GetOpRank(int roomid, string type)
        {
            try
            {

                string url = $"https://api.live.bilibili.com/AppRoom/opTop?actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&room_id={roomid}&scale=hdpi&ts={ApiHelper.GetTimeSpan}&type={type}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    ObservableCollection<OpTopListModel> ls = JsonConvert.DeserializeObject<ObservableCollection<OpTopListModel>>(model.json["data"]["list"].ToString());
                    return new ReturnModel<ObservableCollection<OpTopListModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<OpTopListModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<OpTopListModel>>(ex);
            }
        }

        /// <summary>
        /// 舰队
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<GuardRankListModel>>> GetGuardRank(int uid)
        {
            try
            {

                string url = $"https://api.live.bilibili.com/AppRoom/guardRank?actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&page=1&page_size=30&platform=android&ruid={uid}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    ObservableCollection<GuardRankListModel> ls = JsonConvert.DeserializeObject<ObservableCollection<GuardRankListModel>>(model.json["data"].ToString());
                    return new ReturnModel<ObservableCollection<GuardRankListModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<GuardRankListModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<GuardRankListModel>>(ex);
            }
        }

        /// <summary>
        /// 读取排行榜
        /// </summary>
        /// <param name="area_v2_id"></param>
        /// <param name="area_v2_parent_id"></param>
        /// <param name="roomid"></param>
        /// <param name="ruid"></param>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<RankActivityModel>>> GetRankActivity(int area_v2_id, int area_v2_parent_id, int roomid, int ruid)
        {
            try
            {

                string url = $"https://api.live.bilibili.com/activity/v1/Common/mobileRoomInfo?actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&area_v2_id={area_v2_id}&area_v2_parent_id={area_v2_parent_id}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&roomid={roomid}&ruid={ruid}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    ObservableCollection<RankActivityModel> ls = JsonConvert.DeserializeObject<ObservableCollection<RankActivityModel>>(model.json["data"]["rankInfo"]["activity"].ToString());
                    ls.Insert(0, new RankActivityModel()
                    {
                        isEvent = 0,
                        type = "粉丝榜",
                        desc = "粉丝榜"
                    });
                    ls.Insert(0, new RankActivityModel()
                    {
                        isEvent = 0,
                        type = "七日榜",
                        desc = "七日榜"
                    });
                    return new ReturnModel<ObservableCollection<RankActivityModel>>()
                    {
                        success = true,
                        message = "",
                        data = ls
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<RankActivityModel>>()
                    {
                        success = true,
                        data = new ObservableCollection<RankActivityModel>() {
                         new RankActivityModel() {
                          isEvent=0,
                          type="七日榜",
                          desc= "七日榜"
                    },new RankActivityModel()
                    {
                        isEvent = 0,
                        type = "粉丝榜",
                        desc = "粉丝榜"
                    }
                     }
                    };
                }


            }
            catch (Exception ex)
            {
                return new ReturnModel<ObservableCollection<RankActivityModel>>()
                {
                    success = true,
                    data = new ObservableCollection<RankActivityModel>() {
                         new RankActivityModel() {
                          isEvent=0,
                          type="七日榜",
                          desc= "七日榜"
                    },new RankActivityModel()
                    {
                        isEvent = 0,
                        type = "粉丝榜",
                        desc = "粉丝榜"
                    }
                     }
                };
            }
        }

        /// <summary>
        /// 赠送我的背包
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="ruid">UP ID</param>
        /// <param name="gift_id">礼物ID</param>
        /// <param name="gift_num">数量</param>
        /// <param name="bag_id">包裹ID</param>
        /// <param name="roomid">房间号</param>
        /// <returns></returns>
        public async Task<ReturnModel> SendMyGift(string uid, string ruid, int gift_id, int gift_num, int bag_id, int roomid)
        {
            try
            {
                string url = $"https://api.live.bilibili.com/gift/v2/live/bag_send?access_key={ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                string data = $"uid={uid}&ruid={ruid}&gift_id={gift_id}&gift_num={gift_num}&bag_id={bag_id}&biz_id={roomid}&rnd={ApiHelper.GetTimeSpan}&biz_code=live";

                var results = await WebClientClass.PostResultsUtf8(new Uri(url), data);
                var json = results.ToDynamicJObject();
                if (json.code == 0)
                {
                    return new ReturnModel()
                    {
                        success = true
                    };
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = json.message
                    };
                }


            }
            catch (Exception ex)
            {

                return HandelError(ex);
            }




        }


        /// <summary>
        /// 赠送礼物
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="ruid">UP ID</param>
        /// <param name="gift_id">礼物ID</param>
        /// <param name="gift_num">数量</param>
        /// <param name="roomid">房间号</param>
        /// <param name="coin_type">瓜子类型</param>
        /// <param name="price">价格</param>
        /// <returns></returns>
        public async Task<ReturnModel> SendGift(string uid, string ruid, int gift_id, int gift_num, int roomid, string coin_type, int price)
        {
            try
            {
                string url = $"https://api.live.bilibili.com/gift/v2/live/send";
                string data = $"access_key={ApiHelper.access_key}&actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&biz_code=live&biz_id={roomid}&build={ApiHelper.build}&coin_type={coin_type}&device=android&gift_id={gift_id}&gift_num={gift_num}&mobi_app=android&platform=android&price={price}&rnd={ApiHelper.GetTimeSpan}&ruid={ruid}&ts={ApiHelper.GetTimeSpan}&uid={uid}";
                data += "&sign=" + ApiHelper.GetSign(data);

                var results = await WebClientClass.PostResultsUtf8(new Uri(url), data);
                var json = results.ToDynamicJObject();
                if (json.code == 0)
                {
                    return new ReturnModel()
                    {
                        success = true
                    };
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = json.message
                    };
                }


            }
            catch (Exception ex)
            {

                return HandelError(ex);
            }

        }



        public RichTextBlock HtmlToRichText(string html)
        {
            try
            {
                var outHtml = html;
                outHtml = Regex.Replace(outHtml, @"<p.*?>", "<Paragraph>", RegexOptions.Singleline);
                outHtml = Regex.Replace(outHtml, @"</p>", "</Paragraph>", RegexOptions.Singleline);
                outHtml = Regex.Replace(outHtml, @"<span.*?>", "", RegexOptions.Singleline);
                outHtml = Regex.Replace(outHtml, @"</span>", "", RegexOptions.Singleline);
                outHtml = outHtml.Replace("<br>", "");
                var mc = Regex.Matches(outHtml, @"<Paragraph>.*?<a.*?href=""(.*?)"".*?>(.*?)</a>.*?</Paragraph>");
                foreach (Match item in mc)
                {
                    outHtml = outHtml.Replace(item.Groups[0].Value, $@"<Paragraph><InlineUIContainer><HyperlinkButton Margin=""0 0 0 -10"" NavigateUri=""{item.Groups[1].Value}"">{item.Groups[2].Value}</HyperlinkButton></InlineUIContainer></Paragraph>");
                }

                var mc1 = Regex.Matches(outHtml, @"<a.*?href=""(.*?)"".*?>(.*?)</a>");
                foreach (Match item in mc1)
                {
                    outHtml = outHtml.Replace(item.Groups[0].Value, $@"<Paragraph><InlineUIContainer><HyperlinkButton Margin=""0 0 0 -10"" NavigateUri=""{item.Groups[1].Value}"">{item.Groups[2].Value}</HyperlinkButton></InlineUIContainer></Paragraph>");
                }

                outHtml = Regex.Replace(outHtml, @"<[a-z].*?>", "", RegexOptions.Singleline);
                outHtml = Regex.Replace(outHtml, @"</[a-z].*?>", "", RegexOptions.Singleline);
                outHtml = System.Net.WebUtility.HtmlDecode(outHtml);
                if (!outHtml.Contains("<Paragraph>"))
                {
                    outHtml += "<Paragraph>" + outHtml + "</Paragraph>";
                }

                outHtml = @"<RichTextBlock  Foreground=""Gray"" FontSize=""14"" Margin=""4"" TextWrapping=""Wrap""  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
    xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"">" + outHtml + "</RichTextBlock>";



                outHtml = outHtml.Replace("&", "&amp;");


                var p = (RichTextBlock)XamlReader.Load(outHtml);
                return p;
            }
            catch (Exception)
            {

                var rich = new RichTextBlock()
                {
                    FontSize = 14,
                    Margin = new Thickness(4),
                    TextWrapping = TextWrapping.Wrap,
                };
                var htmldoc = new HtmlDocument();
                htmldoc.LoadHtml(html);
                var p = new Paragraph();
                p.Inlines.Add(new Run() { Text = htmldoc.DocumentNode.InnerText });
                rich.Blocks.Add(p);
                return rich;
            }


        }

    }


    namespace LiveModels
    {
        public class Count_mapModel
        {
            /// <summary>
            /// Num
            /// </summary>
            public int num { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string text { get; set; }
        }
        public class AllGiftsModel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 亿圆
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Price
            /// </summary>
            public int price { get; set; }
            /// <summary>
            /// Type
            /// </summary>
            public int type { get; set; }
            /// <summary>
            /// silver
            /// </summary>
            public string coin_type { get; set; }
            /// <summary>
            /// Bag_gift
            /// </summary>
            public int bag_gift { get; set; }
            /// <summary>
            /// Effect
            /// </summary>
            public int effect { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string corner_mark { get; set; }
            /// <summary>
            /// Broadcast
            /// </summary>
            public int broadcast { get; set; }
            /// <summary>
            /// Draw
            /// </summary>
            public int draw { get; set; }
            /// <summary>
            /// Stay_time
            /// </summary>
            public int stay_time { get; set; }
            /// <summary>
            /// Animation_frame_num
            /// </summary>
            public int animation_frame_num { get; set; }
            /// <summary>
            /// 虽然只要1元，但是四舍五入之后就值一个亿啊。
            /// </summary>
            public string desc { get; set; }
            /// <summary>
            /// Count_map
            /// </summary>
            public List<Count_mapModel> count_map { get; set; }
            /// <summary>
            /// https://s1.hdslb.com/bfs/live/592e81002d20699c7e4dae4480ada79ab3253eae.png
            /// </summary>
            public string img_basic { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/live/592e81002d20699c7e4dae4480ada79ab3253eae.png
            /// </summary>
            public string img_dynamic { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/live/d97ead07024510e3d0aa9d4e1fdb6a9af8fec2b0.png
            /// </summary>
            public string frame_animation { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/live/e9a7971219a6f6d9ad641dad5019a7ddcef40d47.gif
            /// </summary>
            public string gif { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/live/840aa8e74326905b73453d950ce73871ee5d1818.webp
            /// </summary>
            public string webp { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string full_sc_web { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string full_sc_horizontal { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string full_sc_vertical { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string full_sc_horizontal_svga { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string full_sc_vertical_svga { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string bullet_head { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string bullet_tail { get; set; }


            public Visibility vis
            {
                get
                {
                    return (corner_mark != null && corner_mark.Length != 0) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public SolidColorBrush costColor
            {
                get
                {
                    if (coin_type == "gold")
                    {
                        return new SolidColorBrush(Colors.OrangeRed);
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Gray);
                    }
                }
            }
        }
        public class LiveMyGiftsModel
        {
            /// <summary>
            /// Bag_id
            /// </summary>
            public int bag_id { get; set; }
            /// <summary>
            /// Gift_id
            /// </summary>
            public int gift_id { get; set; }
            /// <summary>
            /// 辣条
            /// </summary>
            public string gift_name { get; set; }
            /// <summary>
            /// Gift_num
            /// </summary>
            public int gift_num { get; set; }
            /// <summary>
            /// Gift_type
            /// </summary>
            public int gift_type { get; set; }
            /// <summary>
            /// Expire_at
            /// </summary>
            public int expire_at { get; set; }
            /// <summary>
            /// Count_map
            /// </summary>
            public List<Count_mapModel> count_map { get; set; }
            /// <summary>
            /// 今天
            /// </summary>
            public string corner_mark { get; set; }


            public Visibility vis
            {
                get
                {
                    if (corner_mark != null && corner_mark != "")
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            public string img { get; set; }
        }

        public class TitleItemModel
        {
            /// <summary>
            /// 
            /// </summary>
            public string identification { get; set; }
            /// <summary>
            /// 枸杞牛奶
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string url { get; set; }
            /// <summary>
            /// 枸杞牛奶
            /// </summary>
            public string source { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string level { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string title_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string web_pic_url { get; set; }
        }

        public class TitleModel
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string msg { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<TitleItemModel> data { get; set; }
        }


        public class Frame
        {
            /// <summary>
            /// frame
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// Expire_time
            /// </summary>
            public int expire_time { get; set; }
            /// <summary>
            /// bls_summer_2018_volleyball
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Area
            /// </summary>
            public int area { get; set; }
            /// <summary>
            /// Area_old
            /// </summary>
            public int area_old { get; set; }
            /// <summary>
            /// Use_old_area
            /// </summary>
            public bool use_old_area { get; set; }
            /// <summary>
            /// Position
            /// </summary>
            public int position { get; set; }
            /// <summary>
            /// Create_time
            /// </summary>
            public int create_time { get; set; }
            /// <summary>
            /// Priority
            /// </summary>
            public int priority { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/album/33c6c91e8603e9ef4c2be910dc39efa3edf4e000.png
            /// </summary>
            public string value { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string bg_color { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string bg_pic { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string desc { get; set; }
        }

        public class Badge
        {
            /// <summary>
            /// v_person
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Position
            /// </summary>
            public int position { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string value { get; set; }
            /// <summary>
            /// bilibili 知名游戏UP主，直播签约主播
            /// </summary>
            public string desc { get; set; }
        }

        public class Mobile_frame
        {
            /// <summary>
            /// mobile_frame
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// Expire_time
            /// </summary>
            public int expire_time { get; set; }
            /// <summary>
            /// bls_summer_2018_volleyball
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Area
            /// </summary>
            public int area { get; set; }
            /// <summary>
            /// Area_old
            /// </summary>
            public int area_old { get; set; }
            /// <summary>
            /// Use_old_area
            /// </summary>
            public bool use_old_area { get; set; }
            /// <summary>
            /// Position
            /// </summary>
            public int position { get; set; }
            /// <summary>
            /// Create_time
            /// </summary>
            public int create_time { get; set; }
            /// <summary>
            /// Priority
            /// </summary>
            public int priority { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/album/33c6c91e8603e9ef4c2be910dc39efa3edf4e000.png
            /// </summary>
            public string value { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string bg_color { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string bg_pic { get; set; }
        }

        public class New_pendants
        {
            /// <summary>
            /// Frame
            /// </summary>
            public Frame frame { get; set; }
            /// <summary>
            /// Badge
            /// </summary>
            public Badge badge { get; set; }
            /// <summary>
            /// Mobile_frame
            /// </summary>
            public Mobile_frame mobile_frame { get; set; }
            /// <summary>
            /// Mobile_badge
            /// </summary>
            public string mobile_badge { get; set; }
        }

        public class LiveRoomInfoModel
        {
            /// <summary>
            /// Uid
            /// </summary>
            public int uid { get; set; }
            /// <summary>
            /// Room_id
            /// </summary>
            public int room_id { get; set; }
            /// <summary>
            /// Short_id
            /// </summary>
            public int short_id { get; set; }
            /// <summary>
            /// Attention
            /// </summary>
            public int attention { get; set; }
            public string Attention { get { return attention.ToW(); } }

            /// <summary>
            /// Online
            /// </summary>
            public int online { get; set; }
            /// <summary>
            /// Is_portrait
            /// </summary>
            public bool is_portrait { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string description { get; set; }
            /// <summary>
            /// Live_status
            /// </summary>
            public int live_status { get; set; }
            /// <summary>
            /// Area_id
            /// </summary>
            public int area_id { get; set; }
            /// <summary>
            /// Parent_area_id
            /// </summary>
            public int parent_area_id { get; set; }
            /// <summary>
            /// 游戏
            /// </summary>
            public string parent_area_name { get; set; }
            /// <summary>
            /// Old_area_id
            /// </summary>
            public int old_area_id { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/live/83d417ad5d62264210220ea5b17b79456e724a35.jpg
            /// </summary>
            public string background { get; set; }
            /// <summary>
            /// 【滚】吃鸡
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/live/e64902520ab6e0aaeb6e2d1b721cccbf241045d3.jpg
            /// </summary>
            public string user_cover { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/live/5096.jpg?07271536
            /// </summary>
            public string keyframe { get; set; }
            /// <summary>
            /// Is_strict_room
            /// </summary>
            public bool is_strict_room { get; set; }
            /// <summary>
            /// 2018-07-27 12:00:46
            /// </summary>
            public DateTime live_time { get; set; }
            /// <summary>
            /// 二五仔,猛男,B站恶霸,吃鸡
            /// </summary>
            public string tags { get; set; }
            /// <summary>
            /// Is_anchor
            /// </summary>
            public int is_anchor { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string room_silent_type { get; set; }
            /// <summary>
            /// Room_silent_level
            /// </summary>
            public int room_silent_level { get; set; }
            /// <summary>
            /// Room_silent_second
            /// </summary>
            public int room_silent_second { get; set; }
            /// <summary>
            /// 绝地求生
            /// </summary>
            public string area_name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string pendants { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string area_pendants { get; set; }
            /// <summary>
            /// Hot_words
            /// </summary>
            public List<string> hot_words { get; set; }
            /// <summary>
            /// bilibili直播签约主播
            /// </summary>
            public string verify { get; set; }
            /// <summary>
            /// New_pendants
            /// </summary>
            public New_pendants new_pendants { get; set; }
            /// <summary>
            /// l:one:live:record:5096:1532664046
            /// </summary>
            public string up_session { get; set; }
            /// <summary>
            /// Pk_status
            /// </summary>
            public int pk_status { get; set; }
            /// <summary>
            /// Pk_id
            /// </summary>
            public int pk_id { get; set; }
            /// <summary>
            /// Allow_change_area_time
            /// </summary>
            public int allow_change_area_time { get; set; }
            /// <summary>
            /// Allow_upload_cover_time
            /// </summary>
            public int allow_upload_cover_time { get; set; }

            public LiveUpModel UserInfo { get; set; }

            public RichTextBlock htmldesc { get; set; }

        }

        public class Glory_info
        {
            /// <summary>
            /// 12
            /// </summary>
            public string gid { get; set; }
            /// <summary>
            /// 镇长荣誉
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 哔哩谷活动
            /// </summary>
            public string activity_name { get; set; }
            /// <summary>
            /// 2017-12-05
            /// </summary>
            public DateTime activity_date { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/vc/e63a0e97ac311b63c93a38a986c90e67c9037229.png
            /// </summary>
            public string pic_url { get; set; }
            /// <summary>
            /// https://live.bilibili.com/pages/1703/plant-act2017.html
            /// </summary>
            public string jump_url { get; set; }
        }

        public class LiveUpModel
        {
            /// <summary>
            /// Uid
            /// </summary>
            public int uid { get; set; }
            /// <summary>
            /// 两仪滚
            /// </summary>
            public string uname { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/face/4a91427ef035836b1937244bc559ed03f244bfa9.jpg
            /// </summary>
            public string face { get; set; }
            /// <summary>
            /// Verify_type
            /// </summary>
            public int verify_type { get; set; }
            /// <summary>
            /// bilibili个人认证:bilibili 知名游戏UP主，直播签约主播
            /// </summary>
            public string desc { get; set; }
            /// <summary>
            /// Level
            /// </summary>
            public int level { get; set; }
            /// <summary>
            /// Level_color
            /// </summary>
            public string level_color { get; set; }
            /// <summary>
            /// Main_vip
            /// </summary>
            public int main_vip { get; set; }
            /// <summary>
            /// Uname_color
            /// </summary>
            public int uname_color { get; set; }
            /// <summary>
            /// Room_id
            /// </summary>
            public int room_id { get; set; }
            /// <summary>
            /// 绝地求生
            /// </summary>
            public string area_name { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/album/33c6c91e8603e9ef4c2be910dc39efa3edf4e000.png
            /// </summary>
            public string pendant { get; set; }
            /// <summary>
            /// Pendant_from
            /// </summary>
            public int pendant_from { get; set; }
            /// <summary>
            /// Glory_info
            /// </summary>
            public List<Glory_info> glory_info { get; set; }
            /// <summary>
            /// Follow_num
            /// </summary>
            public int follow_num { get; set; }
            /// <summary>
            /// Relation_status
            /// </summary>
            public int relation_status { get; set; }

            public SolidColorBrush lvColor { get; set; }

            public Visibility showFollow
            {
                get
                {
                    if (relation_status == 1)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            public Visibility showUnFollow
            {
                get
                {
                    if (relation_status != 1)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }

            public string verify
            {
                get
                {
                    switch (verify_type)
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


        public class Durl
        {
            /// <summary>
            /// Order
            /// </summary>
            public int order { get; set; }
            /// <summary>
            /// Length
            /// </summary>
            public int length { get; set; }
            /// <summary>
            /// http://122.228.77.78/live-bvc/153852/live_50329220_7780332.flv?expires=1532694555&ssig=jX5eMjJc1U-ZbU3OLPP1ig&oi=992662259
            /// </summary>
            public string url { get; set; }
            public string display { get; set; }
        }

        public class LivePlayUrlsModel
        {
            /// <summary>
            /// Durl
            /// </summary>
            private List<Durl> _durl;

            public List<Durl> durl
            {
                get { return _durl; }
                set { _durl = value; }
            }

            /// <summary>
            /// Accept_quality
            /// </summary>
            public List<quality_description_item> quality_description { get; set; }
        
            public int current_qn { get; set; }
            /// <summary>
            /// Current_quality
            /// </summary>
            public int current_quality { get; set; }
        }

        public class quality_description_item
        {
            public string desc { get; set; }
            public int qn { get; set; }
        }


        public class Activity_info
        {
            /// <summary>
            /// 
            /// </summary>
            public string uname_color { get; set; }
        }

        public class LiveMsgModel
        {
            /// <summary>
            /// 哈哈哈哈
            /// </summary>
            public string text { get; set; }
            /// <summary>
            /// Uid
            /// </summary>
            public int uid { get; set; }
            /// <summary>
            /// 叶江霖
            /// </summary>
            public string nickname { get; set; }

            public string username
            {
                get { return nickname+":"; }
            }

            /// <summary>
            /// 
            /// </summary>
            public string uname_color { get; set; }

            /// <summary>
            /// 2018-07-29 16:45:36
            /// </summary>
            public DateTime timeline { get; set; }
            /// <summary>
            /// Isadmin
            /// </summary>
            public int isadmin { get; set; }
            /// <summary>
            /// Vip
            /// </summary>
            public int vip { get; set; }
            /// <summary>
            /// Svip
            /// </summary>
            public int svip { get; set; }
            /// <summary>
            /// Medal
            /// </summary>
            public List<string> medal { get; set; }
            public string medal_name { get; set; }
            /// <summary>
            /// Title
            /// </summary>
            public List<string> title { get; set; }
            /// <summary>
            /// User_level
            /// </summary>
            public List<string> user_level { get; set; }
            /// <summary>
            /// 10000
            /// </summary>
            public string rank { get; set; }
            /// <summary>
            /// Teamid
            /// </summary>
            public int teamid { get; set; }
            /// <summary>
            /// Rnd
            /// </summary>
            public string rnd { get; set; }
            /// <summary>
            /// title-147-1
            /// </summary>
            public string user_title { get; set; }
            /// <summary>
            /// Guard_level
            /// </summary>
            public int guard_level { get; set; }
            /// <summary>
            /// Activity_info
            /// </summary>
            public Activity_info activity_info { get; set; }

            public string medal_lv { get; set; }//勋章
            public string medalColor { get; set; }//勋章颜色
            public string ul { get; set; }//等级
            public string ulColor { get; set; }//等级颜色

            public SolidColorBrush ul_color { get; set; }//勋章颜色
            public SolidColorBrush medal_color { get; set; }//勋章颜色
            public Visibility isAdmin { get; set; } = Visibility.Collapsed;
            public Visibility isVip { get; set; } = Visibility.Collapsed;
            public Visibility isBigVip { get; set; } = Visibility.Collapsed;
            public Visibility hasMedal { get; set; } = Visibility.Collapsed;
            public Visibility hasTitle { get; set; } = Visibility.Collapsed;
            public string titleImg
            {
                get
                {

                    return Modules.LiveRoom.titleItems.FirstOrDefault(x => x.identification == user_title)?.web_pic_url;


                }
            }
        }

        public class RoundModel
        {
            /// <summary>
            /// Cid
            /// </summary>
            public int cid { get; set; }
            /// <summary>
            /// Play_time
            /// </summary>
            public int play_time { get; set; }
            /// <summary>
            /// Sequence
            /// </summary>
            public int sequence { get; set; }
            /// <summary>
            /// Aid
            /// </summary>
            public int aid { get; set; }
            /// <summary>
            /// av17959648-【谷阿莫】5分鐘看完2017讓你一頭霧水的神奇電影《母亲！ Mother!》-P1
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// Pid
            /// </summary>
            public int pid { get; set; }
            /// <summary>
            /// https://interface.bilibili.com/playurl?appkey=fb06a25c6338edbc&buvid=9b2dbd17fe02c6e0b9a5f7cbfe182be2&cid=29320459&otype=json&platform=live&qn=80&sign=90ab916e2065be8de18cd7ad9ddd0441
            /// </summary>
            public string play_url { get; set; }

            public RoundPlayModel data { get; set; }
        }

        public class RoundPlayModel
        {
            /// <summary>
            /// local
            /// </summary>
            public string from { get; set; }
            /// <summary>
            /// suee
            /// </summary>
            public string result { get; set; }
            /// <summary>
            /// Quality
            /// </summary>
            public int quality { get; set; }
            /// <summary>
            /// mp4
            /// </summary>
            public string format { get; set; }
            /// <summary>
            /// Timelength
            /// </summary>
            public int timelength { get; set; }
            /// <summary>
            /// mp4
            /// </summary>
            public string accept_format { get; set; }
            /// <summary>
            /// Accept_quality
            /// </summary>
            public List<int> accept_quality { get; set; }
            /// <summary>
            /// Video_codecid
            /// </summary>
            public int video_codecid { get; set; }
            /// <summary>
            /// Video_project
            /// </summary>
            public bool video_project { get; set; }
            /// <summary>
            /// start
            /// </summary>
            public string seek_param { get; set; }
            /// <summary>
            /// second
            /// </summary>
            public string seek_type { get; set; }
            /// <summary>
            /// Durl
            /// </summary>
            public List<Durl> durl { get; set; }
        }

        public class GiftTopListModel
        {
            /// <summary>
            /// Uid
            /// </summary>
            public int uid { get; set; }
            /// <summary>
            /// 果冻菌
            /// </summary>
            public string uname { get; set; }
            /// <summary>
            /// https://i2.hdslb.com/bfs/face/067b186a5d8927f03ba5eb2a0c57c6efe85d8357.jpg
            /// </summary>
            public string face { get; set; }
            /// <summary>
            /// Rank
            /// </summary>
            public int rank { get; set; }
            /// <summary>
            /// Score
            /// </summary>
            public int score { get; set; }
            /// <summary>
            /// Guard_level
            /// </summary>
            public int guard_level { get; set; }
            /// <summary>
            /// IsSelf
            /// </summary>
            public int isSelf { get; set; }
            /// <summary>
            /// Coin
            /// </summary>
            public int coin { get; set; }

            public string rank_img
            {
                get
                {
                    return "ms-appx:///Assets/LiveRank/ic_live_rank_" + rank + ".png";
                }
            }

            public string info_img
            {
                get
                {
                    return "ms-appx:///Assets/LiveRank/ic_rank_seeds.png";
                }
            }

            public Visibility show_medal { get; set; } = Visibility.Collapsed;
            public Visibility show_info { get; set; } = Visibility.Visible;
        }

        public class GiftTopModel
        {
            /// <summary>
            /// Unlogin
            /// </summary>
            public int unlogin { get; set; }
            /// <summary>
            /// xiaoyaocz
            /// </summary>
            public string uname { get; set; }
            /// <summary>
            /// Rank
            /// </summary>
            public int rank { get; set; }
            /// <summary>
            /// Coin
            /// </summary>
            public int coin { get; set; }
            /// <summary>
            /// List
            /// </summary>
            public ObservableCollection<GiftTopListModel> list { get; set; }
        }

        public class MedalRankListModel
        {
            /// <summary>
            /// Uid
            /// </summary>
            public int uid { get; set; }
            /// <summary>
            /// 召唤师
            /// </summary>
            public string medal_name { get; set; }
            /// <summary>
            /// Level
            /// </summary>
            public int level { get; set; }
            /// <summary>
            /// 青衣才不是御姐呢
            /// </summary>
            public string uname { get; set; }
            /// <summary>
            /// Color
            /// </summary>
            public string color { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/face/4a9f93ec2b4f340e294b5e90ee79943f7f258258.jpg
            /// </summary>
            public string face { get; set; }

            public int rank { get; set; }

            public string rank_img
            {
                get
                {
                    return "ms-appx:///Assets/LiveRank/ic_live_rank_" + rank + ".png";
                }
            }

            public Visibility show_medal { get; set; } = Visibility.Visible;
            public Visibility show_info { get; set; } = Visibility.Collapsed;

            public SolidColorBrush medal_color { get { return new SolidColorBrush(Utils.ToColor(color)); } }
        }


        public class RankActivityModel
        {
            /// <summary>
            /// bls_summer_2018
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// 清爽榜
            /// </summary>
            public string desc { get; set; }
            /// <summary>
            /// IsEvent
            /// </summary>
            public int isEvent { get; set; }
        }


        public class Coin_url
        {
            /// <summary>
            /// https://s1.hdslb.com/bfs/static/blive/live-assets/mobile/android/android/hdpi/rank/rank-1.png
            /// </summary>
            public string src { get; set; }
            /// <summary>
            /// Height
            /// </summary>
            public int height { get; set; }
            /// <summary>
            /// Width
            /// </summary>
            public int width { get; set; }
        }


        public class OpTopListModel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// Score
            /// </summary>
            public int score { get; set; }
            /// <summary>
            /// Rank
            /// </summary>
            public int rank { get; set; }
            /// <summary>
            /// Uid
            /// </summary>
            public int uid { get; set; }
            /// <summary>
            /// 逆时空的朋也
            /// </summary>
            public string uname { get; set; }
            /// <summary>
            /// https://i1.hdslb.com/bfs/face/9cb3c1ffb29c724fc056d1faa7143b06ef0fdff6.jpg
            /// </summary>
            public string face { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string link { get; set; }
            /// <summary>
            /// Coin_url
            /// </summary>
            public Coin_url coin_url { get; set; }
            /// <summary>
            /// Coin1_url
            /// </summary>
            public Coin_url coin1_url { get; set; }


            public string rank_img
            {
                get
                {
                    return coin_url.src;
                }
            }
            public string info_img
            {
                get
                {
                    return coin1_url.src;
                }
            }
            public Visibility show_medal { get; set; } = Visibility.Collapsed;
            public Visibility show_info { get; set; } = Visibility.Visible;

        }

        public class GuardRankListModel
        {
            /// <summary>
            /// Uid
            /// </summary>
            public int uid { get; set; }
            /// <summary>
            /// Ruid
            /// </summary>
            public int ruid { get; set; }
            /// <summary>
            /// Rank
            /// </summary>
            public int rank { get; set; }
            /// <summary>
            /// -AG
            /// </summary>
            public string username { get; set; }
            /// <summary>
            /// https://i0.hdslb.com/bfs/face/b0aae472b96118ccb5a59f92972cb68015553a80.jpg
            /// </summary>
            public string face { get; set; }
            /// <summary>
            /// Is_alive
            /// </summary>
            public int is_alive { get; set; }
            /// <summary>
            /// Guard_level
            /// </summary>
            public int guard_level { get; set; }
            public string rank_img
            {
                get
                {
                    return "ms-appx:///Assets/LiveRank/ic_live_guard_" + guard_level + ".png";
                }
            }
        }





    }

}
