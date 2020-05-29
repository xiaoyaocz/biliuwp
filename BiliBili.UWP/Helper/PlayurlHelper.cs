using BiliBili.UWP.Controls;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SYEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace BiliBili.UWP.Helper
{
    //TODO 需要重写此类


    /// <summary>
    /// 视频地址解析类
    /// </summary>
    public static class PlayurlHelper
    {
        /// <summary>
        /// 读取番剧播放地址
        /// </summary>
        /// <param name="model"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static async Task<ReturnPlayModel> GetBangumiUrl(PlayerModel model, int qn)
        {
            try
            {
                //兼容旧代码
                if (qn < 10)
                {
                    switch (qn)
                    {
                        case 1:
                            qn = 32;
                            break;
                        case 2:
                            qn = 64;
                            break;
                        case 3:
                            qn = 80;
                            break;
                        case 4:
                            qn = 116;
                            break;
                        default:
                            qn = 64;
                            break;
                    }
                }

                if (SettingHelper.Get_UseDASH())
                {
                    var bilidash = await GetBilibiliBangumiUrlDash(model, qn);
                    if (bilidash != null)
                    {
                        return bilidash;
                    }
                }


                //官方API
                var bili = await GetBilibiliBangumiUrl(model, qn);
                if (bili != null)
                {
                    return bili;
                }
                ////官方WEBAPI
                //var biliweb = await GetBilibiliBangumiWebUrl(model, qn);
                //if (biliweb != null)
                //{
                //    return biliweb;
                //}
                if (SettingHelper.Get_PriorityBiliPlus())
                {
                    //biliplus API
                    var biliplus = await GetBiliPlus(model, qn);
                    if (biliplus != null)
                    {
                        return biliplus;
                    }
                    var moe = await Get23MoeUrl(model, qn);
                    if (moe != null)
                    {
                        return moe;
                    }
                }
                else
                {
                    //23moe API
                    var moe = await Get23MoeUrl(model, qn);
                    if (moe != null)
                    {
                        return moe;
                    }
                    var biliplus = await GetBiliPlus(model, qn);
                    if (biliplus != null)
                    {
                        return biliplus;
                    }
                }

                return null;

            }
            catch (Exception)
            {

                return null;
            }

        }
        public static async Task<ReturnPlayModel> GetBiliPlus(PlayerModel model, int qn)
        {
            if (SettingHelper.Get_UseDASH())
            {
                var biliplusdash = await GetBiliPlusDashUrl(model.Mid, qn, "https://www.bilibili.com/bangumi/play/ep" + model.episode_id, model.season_type);
                if (biliplusdash != null)
                {
                    return biliplusdash;
                }
            }

            var biliplus = await GetBiliPlusUrl(model.Mid, qn, "https://www.bilibili.com/bangumi/play/ep" + model.episode_id, model.season_type);
            if (biliplus != null)
            {
                return biliplus;
            }

            //biliplus API
            var biliplus2 = await GetBiliPlusUrl2(model);
            if (biliplus2 != null)
            {
                return biliplus2;
            }
            return null;
        }
        public static async Task<ReturnPlayModel> GetBilibiliBangumiUrl(PlayerModel model, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type={4}&qn={2}&ts={3}", ApiHelper.WebVideoKey.Appkey, model.Mid, qn, ApiHelper.GetTimeSpan_2, model.season_type);
                url2 += "&sign=" + ApiHelper.GetSign(url2, ApiHelper.WebVideoKey);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                //是否遇到了地区限制
                if (m.code == 0 && !re.Contains("8986943"))
                {
                    foreach (var item in m.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, 0, item.length / 1000);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com/bangumi/play/ep" + model.episode_id);
                    return new ReturnPlayModel()
                    {
                        playlist = playList,
                        usePlayMode = UsePlayMode.SYEngine,
                        urls = urls,
                        from = "bilibili"
                    };
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
        public static async Task<ReturnPlayModel> GetBilibiliBangumiUrlDash(PlayerModel model, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    "https://api.bilibili.com/pgc/player/web/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type={4}&qn={2}&ts={3}&fourk=1&fnver=0&fnval=16", ApiHelper.WebVideoKey.Appkey, model.Mid, qn, ApiHelper.GetTimeSpan_2, model.season_type);
                if (ApiHelper.IsLogin())
                {
                    url2 += $"&access_key={ApiHelper.access_key}&mid={ApiHelper.GetUserId()}";
                }
                url2 += "&sign=" + ApiHelper.GetSign(url2, ApiHelper.WebVideoKey);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                var obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    if (obj["result"]["dash"] != null)
                    {
                        int codecid = 7;
                        if (SettingHelper.Get_DASHUseHEVC())
                        {
                            codecid = 12;
                        }
                        var videos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["result"]["dash"]["video"].ToString());
                        var audios = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["result"]["dash"]["audio"].ToString());
                        var video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                        if (video == null && codecid == 12)
                        {
                            codecid = 7;
                            video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                            if (video == null)
                            {
                                video = videos.FirstOrDefault(x => x.codecid == codecid);
                            }
                        }

                        var audio = audios.FirstOrDefault();

                        return new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.Dash,
                            mediaSource = await CreateAdaptiveMediaSource(video, audio),
                            from = "bilibili_dash_" + codecid
                        };
                    }
                    else
                    {
                        return null;
                    }
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
        public static async Task<ReturnPlayModel> GetBilibiliBangumiWebUrl(PlayerModel model, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    $"https://api.bilibili.com/pgc/player/web/playurl?avid={ model.Aid}&cid={model.Mid}&qn={qn}&type=&otype=json&ep_id={model.episode_id}&&module=bangumi&access_key={ApiHelper.access_key}");
                //url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                JObject obj = JObject.Parse(re);

                //是否遇到了地区限制
                if (obj["code"].ToInt32() == 0 && !re.Contains("8986943"))
                {
                    foreach (var item in obj["result"]["durl"])
                    {
                        urls.Add(item["url"].ToString());
                        playList.Append(item["url"].ToString(), 0, 0);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com");
                    return new ReturnPlayModel()
                    {
                        playlist = playList,
                        usePlayMode = UsePlayMode.SYEngine,
                        urls = urls,
                        from = "bilibili"
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }


        public static async Task<ReturnPlayModel> Get23MoeUrl(PlayerModel model, int qn)
        {
            try
            {
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                List<string> urls = new List<string>();
                string rnd = ApiHelper.GetTimeSpan.ToString();
                if (SettingHelper.Get_UseVIP())
                {
                    rnd = "true" + rnd;
                }
                var re3 = await WebClientClass.GetResults(new Uri(string.Format("https://moe.nsapps.cn/api/v1/BiliAnimeUrl?animeid={0}&cid={1}&epid={2}&rnd={3}", model.banId, model.Mid, model.banInfo.episode_id, rnd)));
                JObject obj = JObject.Parse(re3);
                if (Convert.ToInt32(obj["code"].ToString()) == 0)
                {
                    if (obj["mode"].ToString() == "mp4")
                    {
                        urls.Add(obj["data"][0]["url"].ToString());
                        return new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.System,
                            url = obj["data"][0]["url"].ToString(),
                            urls = urls,
                            from = "server"
                        };
                    }
                    else
                    {
                        var returnPlayModel = new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.SYEngine,
                            from = "server"
                        };
                        foreach (var item in obj["data"])
                        {
                            playList.Append(item["url"].ToString(), item["size"].ToInt32(), item["length"].ToInt32() / 1000);
                            urls.Add(item["url"].ToString());
                        }
                        playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com/bangumi/play/ep" + model.episode_id);
                        returnPlayModel.playlist = playList;
                        returnPlayModel.urls = urls;
                        return returnPlayModel;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }
        /// <summary>
        /// 使用biliplus的接口，支持番剧/视频
        /// 需要授权
        /// </summary>
        /// <param name="model"></param>
        /// <param name="qn"></param>
        /// <returns></returns>
        public static async Task<ReturnPlayModel> GetBiliPlusUrl(string cid, int qn, string referer, int season_type, string cookie = "")
        {
            try
            {
                var season = "";
                if (season_type != 0)
                {
                    season = $"&module=bangumi&season_type={ season_type}";
                }
                string url = "https://www.biliplus.com/BPplayurl.php?cid=" + cid + $"&otype=json&type=&quality={qn}&qn={qn}{season}&access_key={ApiHelper.access_key}";
                Dictionary<string, string> header = new Dictionary<string, string>();
                //if (SettingHelper.Get_BiliplusCookie() != "")
                //{
                //    if (cookie == "")
                //    {
                //        cookie = SettingHelper.Get_BiliplusCookie();
                //    }
                //    header.Add("Cookie", cookie);
                //}

                string re = await WebClientClass.GetResults(new Uri(url));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                if (m.code == 0)
                {
                    List<string> urls = new List<string>();
                    var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                    foreach (var item in m.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, 0, item.length / 1000);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(referer);
                    return new ReturnPlayModel()
                    {
                        playlist = playList,
                        usePlayMode = UsePlayMode.SYEngine,
                        urls = urls,
                        from = "biliplus"
                    };
                }
                else if (m.code == -403)
                {
                    //if (ApiHelper.IsLogin())
                    //{
                    //    ReturnPlayModel returnPlayModel = null;
                    //    MessageDialog messageDialog = new MessageDialog("读取视频地址失败了，是否授权Biliplus后再试一次?");
                    //    messageDialog.Commands.Add(new UICommand("授权", async (e) =>
                    //    {
                    //        var _cookie = await Account.AuthBiliPlus();
                    //        if (_cookie != "")
                    //        {
                    //            returnPlayModel = await GetBiliPlusUrl(cid, qn, referer, season_type, _cookie);
                    //        }
                    //        else
                    //        {
                    //            Utils.ShowMessageToast("授权失败了");
                    //        }
                    //    }));
                    //    messageDialog.Commands.Add(new UICommand("取消"));
                    //    await messageDialog.ShowAsync();
                    //    return returnPlayModel;
                    //}
                    //else
                    //{
                    return null;
                    //}
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }



        }
        public static async Task<ReturnPlayModel> GetBiliPlusDashUrl(string cid, int qn, string referer, int season_type, string cookie = "")
        {
            try
            {
                var season = "";
                if (season_type != 0)
                {
                    season = $"&module=bangumi&season_type={ season_type}";
                }
                string url = "https://www.biliplus.com/BPplayurl.php?cid=" + cid + $"&otype=json&type=&quality={qn}&qn={qn}{season}&access_key={ApiHelper.access_key}&fourk=1&fnver=0&fnval=16";
                Dictionary<string, string> header = new Dictionary<string, string>();
                if (SettingHelper.Get_BiliplusCookie() != "")
                {
                    if (cookie == "")
                    {
                        cookie = SettingHelper.Get_BiliplusCookie();
                    }
                    header.Add("Cookie", cookie);
                }

                string re = await WebClientClass.GetResults(new Uri(url));
                var obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    if (obj["dash"] != null)
                    {
                        int codecid = 7;
                        if (SettingHelper.Get_DASHUseHEVC())
                        {
                            codecid = 12;
                        }
                        var videos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["dash"]["video"].ToString());
                        var audios = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["dash"]["audio"].ToString());
                        var video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                        if (video == null && codecid == 12)
                        {
                            codecid = 7;
                            video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                        }
                        var audio = audios.FirstOrDefault();

                        return new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.Dash,
                            mediaSource = await CreateAdaptiveMediaSource(video, audio),
                            from = "bilibili_dash_" + codecid
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (obj["code"].ToInt32() == -403)
                {
                    //if (ApiHelper.IsLogin())
                    //{
                    //    ReturnPlayModel returnPlayModel = null;
                    //    MessageDialog messageDialog = new MessageDialog("读取视频地址失败了，是否授权Biliplus后再试一次?");
                    //    messageDialog.Commands.Add(new UICommand("授权", async (e) =>
                    //    {
                    //        var _cookie = await Account.AuthBiliPlus();
                    //        if (_cookie != "")
                    //        {
                    //            returnPlayModel = await GetBiliPlusDashUrl(cid, qn, referer, season_type, _cookie);
                    //        }
                    //        else
                    //        {
                    //            Utils.ShowMessageToast("授权失败了");
                    //        }
                    //    }));
                    //    messageDialog.Commands.Add(new UICommand("取消"));
                    //    await messageDialog.ShowAsync();
                    //    return returnPlayModel;
                    //}
                    //else
                    //{
                    return null;
                    //}
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }



        }

        public static async Task<ReturnPlayModel> GetBiliPlusUrl2(PlayerModel model)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    $"https://www.biliplus.com//BPplayurl.php?cid={model.Mid}&otype=json");
                //url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                JObject obj = JObject.Parse(re);

                if (obj["code"].ToInt32() == 10004)
                {
                    if (ApiHelper.IsLogin())
                    {
                        ReturnPlayModel returnPlayModel = null;
                        MessageDialog messageDialog = new MessageDialog("读取视频地址失败了，是否授权Biliplus后再试一次?");
                        messageDialog.Commands.Add(new UICommand("授权", async (e) =>
                        {
                            var _cookie = await Account.AuthBiliPlus();
                            if (_cookie != "")
                            {
                                returnPlayModel = await GetBiliPlusUrl2(model);
                            }
                            else
                            {
                                Utils.ShowMessageToast("授权失败了");
                            }
                        }));
                        messageDialog.Commands.Add(new UICommand("取消"));
                        await messageDialog.ShowAsync();
                        return returnPlayModel;
                    }
                    else
                    {
                        return null;
                    }
                }

                //是否遇到了地区限制
                if (obj["code"].ToInt32() == 0 && !re.Contains("8986943"))
                {
                    foreach (var item in obj["durl"])
                    {
                        urls.Add(item["url"].ToString());
                        playList.Append(item["url"].ToString(), item["size"].ToInt32(), item["length"].ToInt32() / 1000);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com/bangumi/play/ep" + model.episode_id);
                    return new ReturnPlayModel()
                    {
                        playlist = playList,
                        usePlayMode = UsePlayMode.SYEngine,
                        urls = urls,
                        from = "bilibili"
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }



        /// <summary>
        /// 读取视频播放地址
        /// </summary>
        /// <param name="model"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static async Task<ReturnPlayModel> GetVideoUrl(string aid, string cid, int qn)
        {
            try
            {
                if (qn < 10)
                {
                    switch (qn)
                    {
                        case 1:
                            qn = 32;
                            break;
                        case 2:
                            qn = 64;
                            break;
                        case 3:
                            qn = 80;
                            break;
                        case 4:
                            qn = 116;
                            break;
                        default:
                            qn = 64;
                            break;
                    }
                }

                if (SettingHelper.Get_UseDASH())
                {
                    var bilidash = await GetVideoUrlDASH(aid, cid, qn);
                    if (bilidash != null)
                    {
                        return bilidash;
                    }
                }

                var biliv1 = await GetVideoUrlV1(aid, cid, qn);
                if (biliv1 != null)
                {
                    return biliv1;
                }
                var biliplus = await GetBiliPlusUrl(cid, qn, "https://www.bilibili.com/video/av" + aid + "/", 0);
                if (biliplus != null)
                {
                    return biliplus;
                }
                return null;

            }
            catch (Exception)
            {
                return null;
            }

        }
        public static async Task<ReturnPlayModel> GetVideoUrlDASH(string aid, string cid, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                //string url = $"https://api.bilibili.com/x/player/playurl?appkey={ApiHelper.AndroidKey.Appkey}&avid={ aid}&cid={cid}&qn={qn}&type=&otype=json&fnver=0&fnval=16";

                string url = $"https://api.bilibili.com/x/player/playurl?avid={aid}&cid={cid}&qn={qn}&type=&otype=json&fourk=1&fnver=0&fnval=16&appkey={ ApiHelper.WebVideoKey.Appkey}";
                //url += "&sign=" + ApiHelper.GetSign(url, ApiHelper.WebVideoKey);
                if (ApiHelper.IsLogin())
                {
                    url += $"&access_key={ApiHelper.access_key}&mid={ApiHelper.GetUserId()}";
                }
                url = ApiHelper.GetSignWithUrl(url, ApiHelper.WebVideoKey);
                string re = await WebClientClass.GetResults(new Uri(url));
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    if (obj["data"]["dash"] != null)
                    {
                        int codecid = 7;
                        if (SettingHelper.Get_DASHUseHEVC())
                        {
                            codecid = 12;
                        }
                        var videos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["data"]["dash"]["video"].ToString());
                        var audios = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DashItem>>(obj["data"]["dash"]["audio"].ToString());
                        var video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                        if (video == null && codecid == 12)
                        {
                            codecid = 7;
                            video = videos.FirstOrDefault(x => x.id == qn && x.codecid == codecid);
                        }
                        if (video == null)
                        {
                            video = videos.OrderByDescending(x => x.id).FirstOrDefault(x => x.codecid == 7);
                        }
                        var audio = audios.FirstOrDefault();

                        return new ReturnPlayModel()
                        {
                            usePlayMode = UsePlayMode.Dash,
                            mediaSource = await CreateAdaptiveMediaSource(video, audio),
                            from = "bilibili_dash_" + codecid
                        };
                    }
                    else
                    {
                        return null;
                    }
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

        public static async Task<ReturnPlayModel> GetVideoUrlV1(string aid, string cid, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                string url = $"https://api.bilibili.com/x/player/playurl?avid={aid}&cid={cid}&qn={qn}&type=&otype=json&appkey={ApiHelper.WebVideoKey.Appkey}";
                url += "&sign=" + ApiHelper.GetSign(url, ApiHelper.WebVideoKey);
                string re = await WebClientClass.GetResults(new Uri(url));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                if (m.code == 0)
                {
                    var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                    foreach (var item in m.data.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, 0, item.length / 1000);
                    }
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs("https://www.bilibili.com/video/av" + aid + "/");

                    return new ReturnPlayModel()
                    {
                        usePlayMode = UsePlayMode.SYEngine,
                        playlist = playList,
                        urls = urls,
                        from = "bilibili"
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }


        }


        /// <summary>
        /// 读取搜狐源的播放地址
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static async Task<string> GetSoHuPlayInfo(string mid, int quality)
        {
            try
            {
                string[] str = mid.Split('|');
                //http://bangumi.bilibili.com/player/web_api/playurl?cid=10506396&module=movie&player=1&quality=4&ts=1475587467&sign=12b256ad5510d558d07ddf5c4430cd56
                // string url = string.Format("http://bangumi.bilibili.com/player/web_api/playurl?cid={0}&module=movie&player=1&quality=4&ts={1}&appkey={2}", mid,ApiHelper.GetTimeSpen,ApiHelper._appkey_DONTNOT);

                string url = string.Format("http://api.tv.sohu.com/v4/video/info/{0}.json?api_key=1820c56b9d16bbe3381766192e134811&uid=ad99774cfadfe5ecf12457ec5085359a&poid=1&plat=12&sver=3.7.0&partner=419&sysver=10.0.10586.318&ts={1}&verify=43026f88247fcbe0c56411624bd1531e&passport=&aid={2}&program_id=", str[1], ApiHelper.GetTimeSpan_2, str[0]);

                string results = await WebClientClass.GetResults(new Uri(url));
                SohuModel model = JsonConvert.DeserializeObject<SohuModel>(results);

                if (model.status == 200)
                {

                    switch (quality)
                    {
                        case 1:
                            return model.data.url_nor + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
                        case 2:
                            return model.data.url_super + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
                        case 3:
                            return model.data.url_original + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
                        default:
                            return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";

            }
        }

        /// <summary>
        /// 读取下载地址
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetVideoUrl_Download(DownloadTaskModel m)
        {
            try
            {
                List<string> urls = new List<string>();
                var qn = 32;
                switch (m.quality)
                {
                    case 1:
                        qn = 32;
                        break;
                    case 2:
                        qn = 64;
                        break;
                    case 3:
                        qn = 80;
                        break;
                    case 4:
                        qn = 112;
                        break;
                    default:
                        break;
                }

                //https://interface.bilibili.com/v2/playurl?cid=31452468&appkey=84956560bc028eb7&otype=json&type=&quality=112&qn=112&sign=38b1355368ee65d055c12b57bdb26e3a

                if (m.downloadMode == DownloadMode.Video)
                {
                    var videos = await GetVideoUrl(m.avid, m.cid, qn);
                    if (videos != null)
                    {
                        return videos.urls;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var ban = await GetBangumiUrl(new PlayerModel()
                    {
                        Aid = m.avid,
                        banId = m.sid,
                        episode_id = m.epid,
                        banInfo = new Models.episodesModel()
                        {
                            episode_id = m.epid
                        },
                        index = m.epIndex,
                        Mid = m.cid
                    }, qn);
                    if (ban != null)
                    {
                        return ban.urls;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            catch (Exception)
            {
                return null;
            }





        }


        public static async Task<List<QualityModel>> GetVideoQualities(PlayerModel model,bool down=false)
        {
            List<QualityModel> qualities = new List<QualityModel>();
            try
            {
                var qn = 64;

                string url = $"https://api.bilibili.com/x/player/playurl?avid={model.Aid}&cid={model.Mid}&qn={qn}&type=&otype=json&appkey={ ApiHelper.WebVideoKey.Appkey}";
                if ((!down&&SettingHelper.Get_UseDASH())||(down && !SettingHelper.Get_DownFLV()))
                {
                    url += "&fourk=1&fnver=0&fnval=16";
                }
                if (ApiHelper.IsLogin())
                {
                    url += $"&access_key={ApiHelper.access_key}&mid={ApiHelper.GetUserId()}";
                }
                url += "&sign=" + ApiHelper.GetSign(url, ApiHelper.WebVideoKey);
                string re = await WebClientClass.GetResults(new Uri(url));

                //var mc = Regex.FlvPlyaerUrlModel(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);

                foreach (var item in m.data.accept_description)
                {
                    qualities.Add(new QualityModel()
                    {
                        description = item,
                        qn = m.data.accept_quality[m.data.accept_description.IndexOf(item)]
                    });
                }
                qualities = qualities.OrderBy(x => x.qn).ToList();
                if (!ApiHelper.IsLogin())
                {
                    qualities = qualities.Where(x => x.qn <= 32).ToList();
                    foreach (var item in qualities)
                    {
                        item.description += "(登录享受更多清晰度)";
                    }
                }
                else
                {
                    if (!SettingHelper.Get_UserIsVip())
                    {
                        qualities = qualities.Where(x => x.qn != 74 && x.qn <= 80).ToList();
                    }
                }
                return qualities;
            }
            catch (Exception ex)
            {

                return GetDefaultQualities();
            }


        }


        public static async Task<List<QualityModel>> GetAnimeQualities(PlayerModel model)
        {
            List<QualityModel> qualities = new List<QualityModel>();
            try
            {
                var qn = 64;
                List<string> urls = new List<string>();

                string url2 = string.Format(
                    "https://api.bilibili.com/pgc/player/web/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type={4}&qn={2}&ts={3}", ApiHelper.WebVideoKey.Appkey, model.Mid, qn, ApiHelper.GetTimeSpan_2, model.season_type);
                if (SettingHelper.Get_UseDASH())
                {
                    url2 += "&fourk=1&fnver=0&fnval=16";
                }
                if (ApiHelper.IsLogin())
                {
                    url2 += $"&access_key={ApiHelper.access_key}&mid={ApiHelper.GetUserId()}";
                }
                url2 += "&sign=" + ApiHelper.GetSign(url2, ApiHelper.WebVideoKey);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                var jobj = JObject.Parse(re);
                if (jobj["code"].ToInt32() == 0 && !re.Contains("8986943"))
                {
                    var dashModel = JsonConvert.DeserializeObject<DashModel>(jobj["result"].ToString());
                    foreach (var item in dashModel.accept_description)
                    {
                        qualities.Add(new QualityModel()
                        {
                            description = item,
                            qn = dashModel.accept_quality[dashModel.accept_description.IndexOf(item)]
                        });
                    }
                    if (!ApiHelper.IsLogin())
                    {
                        qualities = qualities.Where(x => x.qn <= 32).ToList();
                        foreach (var item in qualities)
                        {
                            item.description += "(登录享受更多清晰度)";
                        }
                    }
                    else
                    {
                        if (!SettingHelper.Get_UserIsVip())
                        {
                            qualities = qualities.Where(x => x.qn != 74 && x.qn <= 80).ToList();
                        }
                    }
                }
                else
                {
                    if (SettingHelper.Get_PriorityBiliPlus())
                    {
                        return GetDefaultQualities();
                    }
                    else
                    {
                        var re3 = await WebClientClass.GetResults(new Uri(string.Format("https://moe.nsapps.cn/api/v1/BiliAnimeUrl?animeid={0}&cid={1}&epid={2}&rnd={3}", model.banId, model.Mid, model.banInfo.episode_id, ApiHelper.GetTimeSpan)));
                        JObject obj = JObject.Parse(re3);
                        if (Convert.ToInt32(obj["code"].ToString()) == 0)
                        {
                            qualities.Add(new QualityModel()
                            {
                                description = "默认",
                                qn = 80
                            });
                        }
                        else
                        {
                            return GetDefaultQualities();
                        }
                    }
                }
                qualities = qualities.OrderBy(x => x.qn).ToList();
                return qualities;
            }
            catch (Exception)
            {

                return GetDefaultQualities();
            }


        }

        public static List<QualityModel> GetDefaultQualities()
        {
            if (ApiHelper.IsLogin())
            {
                return new List<QualityModel>() {
                new QualityModel(){description="流畅", qn=16},
                new QualityModel(){description="清晰", qn=32},
                new QualityModel(){description="高清",qn=64},
                new QualityModel(){description="超清",qn=80}
            };
            }
            else
            {
                return new List<QualityModel>() {
                new QualityModel(){description="流畅(登录享受更多清晰度)", qn=16},
                new QualityModel(){description="清晰(登录享受更多清晰度)", qn=32},
            };
            }
           
        }

        public static async Task<HasSubtitleModel> GetHasSubTitle(string aid, string cid)
        {
            try
            {
                var url = $"https://api.bilibili.com/x/player.so?id=cid:{cid}&aid={aid}";
                var results = await WebClientClass.GetResults(new Uri(url));
                if (results.Contains("subtitle"))
                {
                    var json = Regex.Match(results, @"<subtitle>(.*?)</subtitle>").Groups[1].Value;
                    return JsonConvert.DeserializeObject<HasSubtitleModel>(json);
                }
                else
                {
                    return new HasSubtitleModel() { allow_submit = false };
                }
            }
            catch (Exception)
            {
                return new HasSubtitleModel() { allow_submit = false };
            }
        }
        public static async Task<SubtitleModel> GetSubtitle(string url)
        {
            try
            {
                if (!url.Contains("http:") || !url.Contains("https:"))
                {
                    url = "https:" + url;
                }
                var results = await WebClientClass.GetResults(new Uri(url));


                return JsonConvert.DeserializeObject<SubtitleModel>(results);

            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<IMediaSource> CreateAdaptiveMediaSource(DashItem video, DashItem audio)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Referer = new Uri("https://www.bilibili.com");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
                var mpdStr = $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011""  profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
  <Period  start=""PT0S"">
    <AdaptationSet>
      <ContentComponent contentType=""video"" id=""1"" />
      <Representation bandwidth=""{video.bandwidth}"" codecs=""{video.codecs}"" height=""{video.height}"" id=""{video.id}"" mimeType=""{video.mimeType}"" width=""{video.width}"">
        <BaseURL></BaseURL>
        <SegmentBase indexRange=""{video.SegmentBase.indexRange}"">
          <Initialization range=""{video.SegmentBase.Initialization}"" />
        </SegmentBase>
      </Representation>
    </AdaptationSet>
    <AdaptationSet>
      <ContentComponent contentType=""audio"" id=""2"" />
      <Representation bandwidth=""{audio.bandwidth}"" codecs=""{audio.codecs}"" id=""{audio.id}"" mimeType=""{audio.mimeType}"" >
        <BaseURL></BaseURL>
        <SegmentBase indexRange=""{audio.SegmentBase.indexRange}"">
          <Initialization range=""{audio.SegmentBase.Initialization}"" />
        </SegmentBase>
      </Representation>
    </AdaptationSet>
  </Period>
</MPD>
";

                var stream = new MemoryStream(Encoding.UTF8.GetBytes(mpdStr)).AsInputStream();
                var soure = await AdaptiveMediaSource.CreateFromStreamAsync(stream, new Uri(video.baseUrl), "application/dash+xml", httpClient);
                var s = soure.Status;
                soure.MediaSource.DownloadRequested += (sender, args) =>
                {
                    if (args.ResourceContentType == "audio/mp4")
                    {
                        args.Result.ResourceUri = new Uri(audio.baseUrl);
                    }
                };
                return soure.MediaSource;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private static PlaylistNetworkConfigs CreatePlaylistNetworkConfigs(string referer)
        {
            SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
            config.DownloadRetryOnFail = true;
            config.HttpCookie = string.Empty;
            config.UniqueId = string.Empty;
            config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            config.HttpReferer = referer;
            return config;
        }
    }
    class RUModel
    {
        public string url { get; set; }
        public long length { get; set; }
        public long size { get; set; }
    }


    public class H5PlyaerUrlModel
    {
        public string from { get; set; }

        public List<H5PlyaerUrlModel> durl { get; set; }
        public int order { get; set; }
        public string url { get; set; }




    }
    public class FlvPlyaerUrlModel
    {
        public string format { get; set; }
        public int code { get; set; }
        public int status { get; set; }
        public int vip_status { get; set; }
        public List<string> accept_description { get; set; }
        public List<int> accept_quality { get; set; }
        public FlvPlyaerUrlModel data { get; set; }
        public List<FlvPlyaerUrlModel> durl { get; set; }
        public int order { get; set; }
        public int length { get; set; }
        public long size { get; set; }
        public string url { get; set; }
        public string[] backup_url { get; set; }

    }
    public class DashModel
    {
        public string format { get; set; }
        public List<string> accept_description { get; set; }
        public List<int> accept_quality { get; set; }
        /// <summary>
        /// 时长，毫秒
        /// </summary>
        public int timelength { get; set; }
        public int video_codecid { get; set; }
        public DashDashModel dash { get; set; }
    }
    public class DashDashModel
    {
        public List<DashItemModel> video { get; set; }
        public List<DashItemModel> audio { get; set; }
        /// <summary>
        /// 时长，秒
        /// </summary>
        public int duration { get; set; }

    }
    public class DashItemModel
    {
        public int id { get; set; }
        public int bandwidth { get; set; }
        public string baseUrl { get; set; }
        public string base_url { get; set; }
        public List<string> backupUrl { get; set; }
        public List<string> backup_url { get; set; }
        public string mime_type { get; set; }
        public string mimeType { get; set; }
        public string codecs { get; set; }
        public int codecid { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string frameRate { get; set; }
        public string frame_rate { get; set; }
        /// <summary>
        /// 计算平均帧数
        /// </summary>
        public string fps
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(frameRate))
                    {
                        var values = frameRate.Split('/');
                        if (values.Length == 1)
                        {
                            return frameRate;
                        }
                        double r = Convert.ToDouble(values[0]);
                        double d = Convert.ToDouble(values[1]);
                        return (r / d).ToString("0.0");
                    }
                    else if (!string.IsNullOrEmpty(frame_rate))
                    {
                        var values = frame_rate.Split('/');
                        if (values.Length == 1)
                        {
                            return frame_rate;
                        }
                        double r = Convert.ToDouble(values[0]);
                        double d = Convert.ToDouble(values[1]);
                        return (r / d).ToString("0.0");
                    }
                    else
                    {
                        return "0";
                    }
                }
                catch (Exception)
                {
                    return "0";
                }

            }
        }

        public SegmentBase SegmentBase { get; set; }
        public SegmentBase segment_base { get; set; }
    }

    public class SegmentBase
    {
        public string Initialization { get; set; }
        public string indexRange { get; set; }

        public string initialization { get; set; }
        public string index_range { get; set; }
    }

    public class SetPlayMp4Model
    {
        public string type { get; set; }
        public int duration { get; set; }
        public string url { get; set; }
    }
    public class SetPlayModel
    {
        public string type { get; set; }
        public int duration { get; set; }
        public List<SetPlayUrlModel> segments { get; set; }
    }
    public class SetPlayUrlModel
    {
        public int duration { get; set; }
        public int filesize { get; set; }
        public string url { get; set; }
    }

    public class PlayParModel
    {
        public List<PlayerModel> viedeolist { get; set; }
        public int play { get; set; }
        public bool laod { get; set; }

    }
    public class SohuModel
    {
        public int status { get; set; }
        public string statusText { get; set; }
        public SohuModel data { get; set; }
        public string url_blue { get; set; }

        public string download_url { get; set; }
        public string url_high { get; set; }
        public string url_nor { get; set; }
        public string url_original { get; set; }
        public string url_super { get; set; }

        public string url_high_mp4 { get; set; }
        public string url_nor_mp4 { get; set; }
        public string url_original_mp4 { get; set; }
        public string url_super_mp4 { get; set; }
    }
    public class PlayerModel
    {
        public PlayMode Mode { get; set; }
        public string No { get; set; }
        public int index { get; set; }
        public string ImageSrc { get; set; }
        public string rich_vid { get; set; }
        public string Aid { get; set; }
        public string Mid { get; set; }
        public string Title { get; set; }
        public string VideoTitle { get; set; }
        public string episode_id { get; set; }
        public string Path { get; set; }
        public object Parameter { get; set; }

        public PlayMode playMode { get; set; }
        public List<string> videoList { get; set; }

        public string banId { get; set; }
        public episodesModel banInfo { get; set; }
        /// <summary>
        /// 是否互动视频
        /// </summary>
        public bool isInteraction { get; set; } = false;
        /// <summary>
        /// 互动视频分支ID
        /// </summary>
        public int node_id { get; set; } = 0;

        public int season_type { get; set; } = 1;

        public int? graph_version { get; set; } = 467;

        public bool is_dash { get; set; } = false;
    }

    public class DashItem
    {
        public string baseUrl { get; set; }
        public string bandwidth { get; set; }
        public string codecs { get; set; }
        public int codecid { get; set; }
        public string height { get; set; }
        public string width { get; set; }
        public int id { get; set; }
        public string mimeType { get; set; }
        public SegmentBase SegmentBase { get; set; }
    }

    public class HasSubtitleModel
    {
        public bool allow_submit { get; set; }
        public List<HasSubtitleItemModel> subtitles { get; set; }
    }

    public class HasSubtitleItemModel
    {
        public long id { get; set; }
        public string lan { get; set; }
        public string lan_doc { get; set; }
        public string subtitle_url { get; set; }
    }
    public class SubtitleModel
    {
        public double font_size { get; set; }
        public string font_color { get; set; }
        public double background_alpha { get; set; }
        public string background_color { get; set; }
        public string Stroke { get; set; }

        public List<SubtitleItemModel> body { get; set; }
    }
    public class SubtitleItemModel
    {
        public double from { get; set; }
        public double to { get; set; }
        public int location { get; set; }
        public string content { get; set; }
    }
    public class QualityModel
    {
        public int qn { get; set; }
        public string description { get; set; }
    }


    public enum UsePlayMode
    {
        /// <summary>
        /// 使用SYEngine播放
        /// </summary>
        SYEngine,
        /// <summary>
        /// 使用系统控件播放
        /// </summary>
        System,
        /// <summary>
        /// 使用Html5播放器播放，暂时无效
        /// </summary>
        Html5,
        /// <summary>
        /// 使用VLC播放
        /// </summary>
        VLC,
        Dash
    }
    public class ReturnPlayModel
    {
        public IMediaSource mediaSource { get; set; }
        public UsePlayMode usePlayMode { get; set; }
        public SYEngine.Playlist playlist { get; set; }
        public string url { get; set; }

        public string from { get; set; }

        /// <summary>
        /// 暂时用于测试
        /// </summary>
        public List<string> urls { get; set; }
    }
}
