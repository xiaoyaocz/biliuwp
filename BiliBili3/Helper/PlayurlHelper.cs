using BiliBili3.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SYEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BiliBili3.Helper
{
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
                //官方API
                var bili = await GetBilibiliBangumiUrl(model,qn);
                if (bili != null)
                {
                    return bili;
                }
                //官方WEBAPI
                var biliweb = await GetBilibiliBangumiWebUrl(model, qn);
                if (biliweb != null)
                {
                    return biliweb;
                }


                //23moe API
                var moe = await Get23MoeUrl(model, qn);
                if (moe != null)
                {
                    return moe;
                }

                //biliplus API
                var biliplus = await GetBiliPlusUrl(model.Mid, qn, "https://www.bilibili.com/bangumi/play/ep" + model.episode_id);
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
            catch (Exception)
            {

                return null;
            }

        }

        public static async Task<ReturnPlayModel> GetBilibiliBangumiUrl(PlayerModel model, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type=1&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, model.Mid, qn, ApiHelper.GetTimeSpan_2);
                url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                //是否遇到了地区限制
                if (m.code == 0 && !re.Contains("8986943"))
                {
                    foreach (var item in m.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, item.size, item.length / 1000);
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

        public static async Task<ReturnPlayModel> GetBilibiliBangumiWebUrl(PlayerModel model, int qn)
        {
            try
            {
                List<string> urls = new List<string>();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                string url2 = string.Format(
                    $"https://api.bilibili.com/pgc/player/web/playurl?avid={model.Aid}&cid={model.Mid}&qn={qn}&type=flv&otype=json&ep_id={model.episode_id}&module=bangumi");
                //url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                JObject obj = JObject.Parse(re);
              
                //是否遇到了地区限制
                if (obj["code"].ToInt32()==0 && !re.Contains("8986943"))
                {
                    foreach (var item in obj["result"]["durl"])
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
        public static async Task<ReturnPlayModel> GetBiliPlusUrl(string cid, int qn,string referer,string cookie="")
        {
            try
            {
                string url = "https://www.biliplus.com/BPplayurl.php?cid=" + cid + $"&otype=json&type=&quality={qn}&module=bangumi&season_type=1&qn={qn}&access_key={ApiHelper.access_key}";
                Dictionary<string, string> header = new Dictionary<string, string>();
                if (SettingHelper.Get_BiliplusCookie()!="")
                {
                    if (cookie=="")
                    {
                        cookie = SettingHelper.Get_BiliplusCookie();
                    }
                    header.Add("Cookie", cookie);
                }
                
                string re = await WebClientClass.GetResults(new Uri(url));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                if (m.code == 0)
                {
                    List<string> urls = new List<string>();
                    var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                    foreach (var item in m.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, item.size, item.length / 1000);
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
                    if (ApiHelper.IsLogin())
                    {
                        ReturnPlayModel returnPlayModel = null;
                        MessageDialog messageDialog = new MessageDialog("读取视频地址失败了，是否授权Biliplus后再试一次?");
                        messageDialog.Commands.Add(new UICommand("授权",async (e) => {
                            var _cookie =await Account.AuthBiliPlus();
                            if (_cookie != "")
                            {
                                returnPlayModel =await GetBiliPlusUrl(cid, qn, referer, _cookie);
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
                }else
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
                        messageDialog.Commands.Add(new UICommand("授权", async (e) => {
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
                List<string> urls = new List<string>();
                string url = $"https://api.bilibili.com/x/player/playurl?avid={aid}&cid={cid}&qn={qn}&type=&otype=json&appkey={ApiHelper._appKey_VIDEO}";
                url += "&sign=" + ApiHelper.GetSign_VIDEO(url);
                string re = await WebClientClass.GetResults(new Uri(url));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                if (m.code == 0)
                {
                    var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                    foreach (var item in m.data.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, item.size, item.length / 1000);
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
                var biliplus = await GetBiliPlusUrl(cid, qn, "https://www.bilibili.com/video/av" + aid + "/");
                if (biliplus!=null)
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
                    var videos =await GetVideoUrl(m.avid, m.cid, qn);
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
                    var ban=await GetBangumiUrl(new PlayerModel() {
                        Aid=m.avid,
                        banId=m.sid,
                        episode_id=m.epid,
                        banInfo=new Models.episodesModel() {
                            episode_id = m.epid
                        },
                        index=m.epIndex,
                        Mid=m.cid
                    },qn);
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


        public static async Task<List<QualityModel>> GetVideoQualities(PlayerModel model)
        {
            List<QualityModel> qualities = new List<QualityModel>();
            try
            {
                var qn = 64;

                string url = $"https://api.bilibili.com/x/player/playurl?avid={model.Aid}&cid={model.Mid}&qn={qn}&type=&otype=json&appkey={ApiHelper._appKey_VIDEO}";
                url += "&sign=" + ApiHelper.GetSign_VIDEO(url);
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
                return qualities;
            }
            catch (Exception ex)
            {

                return new List<QualityModel>() {
                     new QualityModel(){description="流畅", qn=32},
                     new QualityModel(){description="清晰",qn=64},
                     new QualityModel(){description="高清",qn=80},
                     new QualityModel(){description="超清",qn=112},
                };
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
                    "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type=1&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, model.Mid, qn, ApiHelper.GetTimeSpan_2);
                url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                if (m.code == 0 && !re.Contains("8986943"))
                {

                    foreach (var item in m.accept_description)
                    {
                        qualities.Add(new QualityModel()
                        {
                            description = item,
                            qn = m.accept_quality[m.accept_description.IndexOf(item)]
                        });
                    }

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
                        return new List<QualityModel>() {
                     new QualityModel(){description="流畅", qn=32},
                     new QualityModel(){description="清晰",qn=64},
                     new QualityModel(){description="高清",qn=80},
                     new QualityModel(){description="超清",qn=112},
                };
                    }



                }


                qualities = qualities.OrderBy(x => x.qn).ToList();
                return qualities;
            }
            catch (Exception)
            {

                return new List<QualityModel>() {
                     new QualityModel(){description="流畅", qn=32},
                     new QualityModel(){description="清晰",qn=64},
                     new QualityModel(){description="高清",qn=80},
                     new QualityModel(){description="超清",qn=112},
                };
            }


        }

        public static List<QualityModel> GetDefaultQualities()
        {
            return new List<QualityModel>() {
                     new QualityModel(){description="流畅", qn=1},
                     new QualityModel(){description="清晰",qn=2},
                     new QualityModel(){description="高清",qn=3},
                     new QualityModel(){description="超清",qn=4},
           };
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


        private static PlaylistNetworkConfigs CreatePlaylistNetworkConfigs(string referer)
        {
            SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
            config.DownloadRetryOnFail = true;
            config.HttpCookie = string.Empty;
            config.UniqueId = string.Empty;
            config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            config.HttpReferer = referer;
            return config;
        }
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
        VLC
    }
    public class ReturnPlayModel
    {
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
