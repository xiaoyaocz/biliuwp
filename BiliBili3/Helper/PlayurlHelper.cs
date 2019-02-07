using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={0}&player=1&quality={1}&qn={1}&ts={2}", model.Mid, qn, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign_PlayUrl(url);

                string re = await WebClientClass.GetResults(new Uri(url));
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);

                SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
                config.DownloadRetryOnFail = true;
                config.HttpCookie = string.Empty;
                config.UniqueId = string.Empty;
                config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

                if (re.Contains("<code>"))
                {
                    //string url2 = string.Format("https://bangumi.bilibili.com/player/web_api/playurl/?access_key={3}&cid={0}&module=bangumi&player=1&otype=json&type=flv&quality={1}&ts={2}", model.Mid, quality, GetTimeSpan_2,access_key);
                    //url2 += "&sign=" + ApiHelper.GetSign_VIP(url2);
                    //
                    string url2 = string.Format(
                        "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type=1&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, model.Mid, qn, ApiHelper.GetTimeSpan_2);
                    url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                    re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                    FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                    if (m.code == 0 && !re.Contains("8986943") )
                    {
                        //if (m.durl.Count==1)
                        //{
                        //    return new ReturnPlayModel()
                        //    {
                        //        usePlayMode = UsePlayMode.System,
                        //        url = m.durl[0].url,
                        //        urls = urls
                        //    };
                        //}

                        foreach (var item in m.durl)
                        {
                            urls.Add(item.url);
                            playList.Append(item.url, item.size, item.length / 1000);
                        }
                    }
                    else
                    {
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
                                returnPlayModel.playlist = playList;
                                returnPlayModel.urls = urls;
                                return returnPlayModel;
                            }
                        }

                        if (SettingHelper.Get_UseOtherSite())
                        {
                            var playurl = await _5DMHelper.GetUrl(model.banId, Convert.ToInt32(model.No));

                            if (playurl == "")
                            {
                                return null;
                            }
                            else
                            {
                                urls.Add(playurl);
                                return new ReturnPlayModel()
                                {
                                    usePlayMode = UsePlayMode.System,
                                    url = playurl,
                                    urls = urls,
                                    from = "other_site"
                                };
                            }
                        }
                        else
                        {
                            return null;
                        }

                    }

                }
                else
                {
                    var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);

                    foreach (Match item in mc)
                    {

                        playList.Append(item.Groups[3].Value, Convert.ToInt32(item.Groups[2].Value), Convert.ToInt64(item.Groups[1].Value) / 1000);
                    }

                }
                config.HttpReferer = "https://www.bilibili.com/bangumi/play/ep" + model.episode_id;
                //config.HttpReferer = "";
                //config.HttpCookie = "sid=aj08qul1; pgv_pvi=2726422528; fts=1498031012; rpdid=ikxqxlpildoplqwkwlqw; buvid3=0916A88B-F02E-46E2-95B1-BEF35678E1EE37229infoc; LIVE_BUVID=9b2dbd17fe02c6e0b9a5f7cbfe182be2; LIVE_BUVID__ckMd5=54bf74d417f1dfe6; OUTFOX_SEARCH_USER_ID_NCOO=301070296.3442316; uTZ=-480; biliMzIsnew=1; biliMzTs=0; UM_distinctid=16072fe8c3138-01d5f1e1fd27a6-5d4e211f-1fa400-16072fe8c32d6; _ga=GA1.2.701916902.1513903771; im_notify_type_7251681=0; BANGUMI_SS_21728_REC=164986; finger=81df3ec0; 21680=183802; 22843=173309; BANGUMI_SS_22843_REC=173309; BANGUMI_SS_21680_REC=183802; balh_server=https://biliplus.ipcjs.win; purl_token=bilibili_1518001366; balh_season_21680=1; pgv_si=s8821510144; DedeUserID=7251681; DedeUserID__ckMd5=e2742b2aff29c1cf; SESSDATA=83ace795%2C1520595578%2Ca2c615ce; bili_jct=a0f037944a8423a37efb566011d0a84b; _dfcaptcha=42d9ae3eecffafaf47b08effeef54128";
                playList.NetworkConfigs = config;
                //FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                // var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);



                //foreach (var item in m.durl)
                //{
                //    playList.Append(item.url, item.size, item.length / 1000);
                //}


                // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
                return new ReturnPlayModel()
                {
                    playlist = playList,
                    usePlayMode = UsePlayMode.SYEngine,
                    urls = urls,
                    from = "bilibili"
                };
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

                //https://interface.bilibili.com/v2/playurl?cid=31452468&appkey=84956560bc028eb7&otype=json&type=&quality=112&qn=112&sign=38b1355368ee65d055c12b57bdb26e3a

                //string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, cid, qn, ApiHelper.GetTimeSpan_2);
                string url = $"https://api.bilibili.com/x/player/playurl?avid={aid}&cid={cid}&qn={qn}&type=&otype=json&appkey={0}";
                url += "&sign=" + ApiHelper.GetSign_VIDEO(url);


                string re = await WebClientClass.GetResults(new Uri(url));

                //var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                // FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                if (m.code == 0)
                {
                    foreach (var item in m.data.durl)
                    {
                        urls.Add(item.url);
                        playList.Append(item.url, item.size, item.length / 1000);
                    }
                }
                else
                {
                    var re3 = await WebClientClass.GetResults(new Uri(string.Format("https://moe.nsapps.cn/api/v1/BiliVideoUrl?cid={0}&qn={1}&rnd={2}", cid, qn, ApiHelper.GetTimeSpan)));
                    JObject obj = JObject.Parse(re3);
                    if (Convert.ToInt32(obj["code"].ToString()) == 0)
                    {

                        if (obj["mode"].ToString() == "mp4")
                        {
                            return new ReturnPlayModel()
                            {
                                urls = new List<string>() { obj["data"][0]["url"].ToString() },
                                usePlayMode = UsePlayMode.System,
                                url = obj["data"][0]["url"].ToString(),
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
                                urls.Add(item["url"].ToString());
                                playList.Append(item["url"].ToString(), item["size"].ToInt32(), item["length"].ToInt32() / 1000);

                            }
                            returnPlayModel.playlist = playList;
                            returnPlayModel.urls = urls;
                            return returnPlayModel;
                        }


                    }

                }



                SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
                config.DownloadRetryOnFail = true;
                config.HttpCookie = string.Empty;

                config.UniqueId = string.Empty;
                config.HttpReferer = "https://www.bilibili.com/video/av" + aid + "/";
                config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

                playList.NetworkConfigs = config;

                // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
                return new ReturnPlayModel()
                {
                    usePlayMode = UsePlayMode.SYEngine,
                    playlist = playList,
                    urls = urls,
                    from = "bilibili"
                };
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
                    string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, m.cid, qn, ApiHelper.GetTimeSpan_2);
                    url += "&sign=" + ApiHelper.GetSign_VIDEO(url);
                    string re = await WebClientClass.GetResults(new Uri(url));
                    FlvPlyaerUrlModel flvPlyaerUrlModel = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                    if (flvPlyaerUrlModel.code == 0)
                    {
                        foreach (var item in flvPlyaerUrlModel.durl)
                        {
                            urls.Add(item.url);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {

                    string url = string.Format("https://interface.bilibili.com/playurl?cid={0}&player=1&quality={1}&qn={1}&ts={2}", m.cid, qn, ApiHelper.GetTimeSpan);
                    url += "&sign=" + ApiHelper.GetSign_PlayUrl(url);
                    string re = await WebClientClass.GetResults(new Uri(url));

                    if (re.Contains("<code>"))
                    {

                        string url2 = string.Format(
                            "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type=1&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, m.cid, qn, ApiHelper.GetTimeSpan_2);
                        url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                        re = await WebClientClass.GetResults(new Uri(url2));
                        FlvPlyaerUrlModel flvPlyaerUrlModel = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                        if (flvPlyaerUrlModel.code == 0 && !re.Contains("8986943"))
                        {
                            foreach (var item in flvPlyaerUrlModel.durl)
                            {
                                urls.Add(item.url);
                            }

                        }
                        else
                        {
                            var re3 = await WebClientClass.GetResults(new Uri(string.Format("https://moe.nsapps.cn/api/v1/BiliAnimeUrl?animeid={0}&cid={1}&epid={2}&rnd={3}", m.sid, m.cid, m.epid, ApiHelper.GetTimeSpan)));
                            JObject obj = JObject.Parse(re3);
                            if (Convert.ToInt32(obj["code"].ToString()) == 0)
                            {
                                urls.Add(obj["data"][0]["url"].ToString());
                            }
                            else
                            {
                                var playurl = await _5DMHelper.GetUrl(m.sid, Convert.ToInt32(m.epIndex) - 1);
                                if (playurl == "")
                                {
                                    return null;
                                }
                                else
                                {
                                    urls.Add(playurl);
                                }
                            }
                        }

                    }
                    else
                    {
                        var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);
                        foreach (Match item in mc)
                        {
                            urls.Add(item.Groups[3].Value);
                        }
                    }
                }


                return urls;
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

                string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, model.Mid, qn, ApiHelper.GetTimeSpan_2);
                url += "&sign=" + ApiHelper.GetSign_VIDEO(url);
                string re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));

                //var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);
                FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);

                foreach (var item in m.accept_description)
                {
                    qualities.Add(new QualityModel()
                    {
                        description = item,
                        qn = m.accept_quality[m.accept_description.IndexOf(item)]
                    });
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


        public static async Task<List<QualityModel>> GetAnimeQualities(PlayerModel model)
        {
            List<QualityModel> qualities = new List<QualityModel>();
            try
            {
                var qn = 64;
                List<string> urls = new List<string>();
                string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={0}&player=1&quality={1}&qn={1}&ts={2}", model.Mid, qn, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign_PlayUrl(url);

                string re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                if (re.Contains("<code>"))
                {
                    string url2 = string.Format(
                        "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type=1&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, model.Mid, qn, ApiHelper.GetTimeSpan_2);
                    url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
                    re = await WebClientClass.GetResultsUTF8Encode(new Uri(url2));
                    FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
                    if (m.code == 0 && !re.Contains("8986943") )
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
                            var playurl = await _5DMHelper.GetUrl(model.banId, Convert.ToInt32(model.No) - 1);

                            if (playurl != "")
                            {
                                qualities.Add(new QualityModel()
                                {
                                    description = "默认",
                                    qn = 80
                                });
                            }
                        }



                    }

                }
                else
                {
                    qualities = new List<QualityModel>() {
                     new QualityModel(){description="流畅", qn=32},
                     new QualityModel(){description="清晰",qn=64},
                     new QualityModel(){description="高清",qn=80},
                     new QualityModel(){description="超清",qn=112},
                     };

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
