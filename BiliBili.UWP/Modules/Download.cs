using BiliBili.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Modules
{
    public class Download : IModules
    {
        /// <summary>
        /// 读取番剧/影视可下载的清晰度
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="cid"></param>
        /// <param name="season_type"></param>
        /// <param name="access_key"></param>
        /// <param name="mid"></param>
        /// <returns></returns>
        public async Task<ReturnModel<List<QualityInfo>>> GetSeasonQualitys(string aid, string cid, int season_type, string access_key = "", string mid = "")
        {
            try
            {
                string url = $"https://api.bilibili.com/pgc/player/api/playurl?aid={ aid}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&cid={cid}&fnval=0&fnver=0&fourk=1{ ((access_key == "") ? "" : $"&access_key={access_key}&mid={mid}")}&mobi_app=android&module=bangumi&npcybs=0&otype=json&platform=android&qn=0&season_type={season_type}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<SeasonUrlInfo>(results);
                if (model.code == 0)
                {
                    List<QualityInfo> list = new List<QualityInfo>();

                    for (int i = 0; i < model.accept_quality.Count; i++)
                    {
                        list.Add(new QualityInfo()
                        {
                            Description = model.accept_description[i],
                            Qn = model.accept_quality[i]
                        });
                    }
                    //未登录不给下载1080+的视频
                    if (access_key == "")
                    {
                        list.RemoveAll(x => x.Qn > 80);
                    }
                    return new ReturnModel<List<QualityInfo>>()
                    {
                        success = true,
                        message = "",
                        data = list
                    };
                }
                else if (model.code == -10403 && model.message == "大会员专享限制")
                {
                    return new ReturnModel<List<QualityInfo>>()
                    {
                        success = true,
                        message = "",
                        data = new List<QualityInfo>() {
                            new QualityInfo() { Description = "高清 1080P", Qn = 80 },
                            new QualityInfo() { Description = "高清 720P", Qn = 64 },
                            new QualityInfo() { Description = "清晰 480P", Qn = 32 },
                            new QualityInfo() { Description = "流畅 360P", Qn = 16 }
                        }
                    };

                }
                else
                {
                    return await GetBiliPlusSeasonQualitys(aid,cid,season_type,access_key,mid);
                }
               
            }
            catch (Exception ex)
            {
                return new ReturnModel<List<QualityInfo>>()
                {
                    success = true,
                    message = "",
                    data = new List<QualityInfo>() { new QualityInfo() { Description = "默认", Qn = 0 } }
                };
            }
        }

        public async Task<ReturnModel<List<QualityInfo>>> GetBiliPlusSeasonQualitys(string aid, string cid, int season_type, string access_key = "", string mid = "")
        {
            try
            {
                string url = $"https://www.biliplus.com/BPplayurl.php?avid={ aid}&cid={cid}&qn=0&type=&otype=json&module=bangumi{ ((access_key == "") ? "" : $"&access_key={access_key}&mid={mid}")}&season_type={season_type}&ts={ApiHelper.GetTimeSpan}";
               
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<SeasonUrlInfo>(results);
                if (model.code == 0)
                {
                    List<QualityInfo> list = new List<QualityInfo>();

                    for (int i = 0; i < model.accept_quality.Count; i++)
                    {
                        list.Add(new QualityInfo()
                        {
                            Description = model.accept_description[i],
                            Qn = model.accept_quality[i]
                        });
                    }
                    //未登录不给下载1080+的视频
                    if (access_key == "")
                    {
                        list.RemoveAll(x => x.Qn > 80);
                    }
                    return new ReturnModel<List<QualityInfo>>()
                    {
                        success = true,
                        message = "",
                        data = list
                    };
                }
                else
                {
                    return new ReturnModel<List<QualityInfo>>()
                    {
                        success = true,
                        message = "",
                        data = new List<QualityInfo>() { new QualityInfo() { Description = "默认", Qn = 0 } }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ReturnModel<List<QualityInfo>>()
                {
                    success = true,
                    message = "",
                    data = new List<QualityInfo>() { new QualityInfo() { Description = "默认", Qn = 0 } }
                };
            }
        }

        /// <summary>
        /// 读取番剧/影视可下载地址
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="cid"></param>
        /// <param name="season_id"></param>
        /// <param name="season_type"></param>
        /// <param name="quality"></param>
        /// <param name="access_key"></param>
        /// <param name="mid"></param>
        /// <returns></returns>
        public async Task<ReturnModel<List<DownloadUrlInfo>>> GetSeasonDownloadUrl(string aid, string cid, int season_id, int season_type, QualityInfo quality, string access_key = "", string mid = "")
        {

            if (quality.Qn != 0)
            {
                var andorid_api = await GetSeasonDownloadUrlAndroidApi(aid, cid, season_type, quality, access_key, mid);
                if (andorid_api.success || andorid_api.message == "大会员专享限制")
                {
                    return andorid_api;
                }
                if (SettingHelper.Get_PriorityBiliPlus())
                {
                    var biliplus = await GetSeasonDownloadUrlBiliplusApi(cid, aid, quality, season_type, access_key, mid);
                    if (biliplus.success)
                    {
                        return biliplus;
                    }
                }
            }
           
            var moe_api = await GetSeasonDownloadUrlMoeApi(season_id, cid, aid, quality, "");
            if (moe_api.success)
            {
                return moe_api;
            }

            return new ReturnModel<List<DownloadUrlInfo>>()
            {
                success = false,
                message = "无法读取下载地址"
            };
        }
        private async Task<ReturnModel<List<DownloadUrlInfo>>> GetSeasonDownloadUrlAndroidApi(string aid, string cid, int season_type, QualityInfo quality, string access_key = "", string mid = "")
        {
            try
            {
                var url = $"https://api.bilibili.com/pgc/player/api/playurl?access_key={ access_key}&aid={aid}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&cid={cid}&fnval=0&fnver=0&fourk=1&mid={mid}&mobi_app=android&module=bangumi&npcybs=0&otype=json&platform=android&qn={quality.Qn}&season_type={season_type}&ts={ApiHelper.GetTimeSpan}";
                //string url = $"https://api.bilibili.com/pgc/player/api/playurl?aid={ aid}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&cid={cid}&fnval=0&fnver=0&fourk=1{ ((access_key == "") ? "" : $"&access_key={access_key}&mid={mid}")}&mobi_app=android&module=bangumi&npcybs=1&otype=json&platform=android&qn={quality.Qn}&season_type={season_type}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<SeasonUrlInfo>(results);
                if (model.code == 0)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("User-Agent", "Bilibili Freedoooooom/MarkII");
                    List<DownloadUrlInfo> downloadUrls = new List<DownloadUrlInfo>();
                    foreach (var item in model.durl)
                    {
                        downloadUrls.Add(new DownloadUrlInfo()
                        {
                            Aid = aid,
                            Cid = cid,
                            Codecid = model.video_codecid,
                            Format = model.format,
                            From = "bilibili_season",
                            Headers = headers,
                            Length = item.length,
                            Order = item.order,
                            QualityInfo = quality,
                            Size = item.size,
                            Url = item.url
                        });
                    }
                    return new ReturnModel<List<DownloadUrlInfo>>()
                    {
                        success = true,
                        message = "",
                        data = downloadUrls
                    };
                }
                else
                {
                    return new ReturnModel<List<DownloadUrlInfo>>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception)
            {
                return new ReturnModel<List<DownloadUrlInfo>>()
                {
                    success = false,
                    message = "读取下载地址错误"
                };
            }
        }
        private async Task<ReturnModel<List<DownloadUrlInfo>>> GetSeasonDownloadUrlMoeApi(int season_id, string cid, string aid, QualityInfo quality, string epid = "")
        {
            try
            {
                string url = $"https://moe.nsapps.cn/api/v1/BiliAnimeUrl?animeid={season_id}&cid={cid}&epid=&rnd={ApiHelper.GetTimeSpan}";
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    //headers.Add("User-Agent", "Bilibili Freedoooooom/MarkII");
                    List<DownloadUrlInfo> downloadUrls = new List<DownloadUrlInfo>();
                    foreach (var item in model.json["data"])
                    {
                        downloadUrls.Add(new DownloadUrlInfo()
                        {
                            Aid = aid,
                            Cid = cid,
                            Codecid = 7,
                            Format = "mp4",
                            From = "server_season",
                            Headers = headers,
                            Length = item["length"].ToInt32(),
                            Order = 0,
                            QualityInfo = quality,
                            Size = item["size"].ToInt32(),
                            Url = item["url"].ToString()
                        });
                    }
                    return new ReturnModel<List<DownloadUrlInfo>>()
                    {
                        success = true,
                        message = "",
                        data = downloadUrls
                    };

                }
                else
                {
                    return new ReturnModel<List<DownloadUrlInfo>>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {
                return new ReturnModel<List<DownloadUrlInfo>>()
                {
                    success = false,
                    message = "读取下载地址错误"
                };
            }
        }
        private async Task<ReturnModel<List<DownloadUrlInfo>>> GetSeasonDownloadUrlBiliplusApi(string cid, string aid, QualityInfo quality, int season_type,string access_key = "", string mid = "")
        {
            try
            {
                string url = $"https://www.biliplus.com/BPplayurl.php?avid={ aid}&cid={cid}&qn={quality.Qn}&type=&otype=json&module=bangumi{ ((access_key == "") ? "" : $"&access_key={access_key}&mid={mid}")}&season_type={season_type}&ts={ApiHelper.GetTimeSpan}";
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<SeasonUrlInfo>(results);
                if (model.code == 0)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3891.0 Safari/537.36 Edg/78.0.268.3");
                    List<DownloadUrlInfo> downloadUrls = new List<DownloadUrlInfo>();
                    foreach (var item in model.durl)
                    {
                        downloadUrls.Add(new DownloadUrlInfo()
                        {
                            Aid = aid,
                            Cid = cid,
                            Codecid = model.video_codecid,
                            Format = model.format,
                            From = "biliplus_season",
                            Headers = headers,
                            Length = item.length,
                            Order = item.order,
                            QualityInfo = quality,
                            Size = item.size,
                            Url = item.url
                        });
                    }
                    return new ReturnModel<List<DownloadUrlInfo>>()
                    {
                        success = true,
                        message = "",
                        data = downloadUrls
                    };
                }
                else
                {
                    return new ReturnModel<List<DownloadUrlInfo>>()
                    {
                        success = false,
                        message = model.message
                    };
                }

            }
            catch (Exception ex)
            {
                return new ReturnModel<List<DownloadUrlInfo>>()
                {
                    success = false,
                    message = "读取下载地址错误"
                };
            }
        }

        /// <summary>
        /// 读取视频可下载的清晰度
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="cid"></param>
        /// <param name="season_type"></param>
        /// <param name="access_key"></param>
        /// <param name="mid"></param>
        /// <returns></returns>
        public async Task<ReturnModel<List<QualityInfo>>> GetVideoQualitys(string aid, string cid, string access_key = "", string mid = "")
        {

            try
            {
                string url = ApiHelper.GetSignWithUrl($"https://app.bilibili.com/x/playurl?npcybs=0&mobi_app=android&fnval=0&fnver=0&platform=android&fourk=1&build={ ApiHelper.build }&actionkey=appkey&appkey={ApiHelper.AndroidVideoKey.Appkey }&otype=json&qn=0&device=android&aid={aid}&cid={cid}&force_host=0&ts={ApiHelper.GetTimeSpan_2}{ ((access_key == "") ? "" : $"&access_key={access_key}&mid={mid}")}", ApiHelper.AndroidVideoKey);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<VideoUrlInfo>(results);
                if (model.code == 0)
                {
                    //未登录只能下载480P视频
                    if (access_key == "")
                    {
                        return new ReturnModel<List<QualityInfo>>()
                        {
                            success = true,
                            message = "",
                            data = new List<QualityInfo>() {
                                new QualityInfo(){
                                    Description="清晰 480P(登录下载更多清晰度)",
                                    Qn=32
                                }
                            }
                        };
                    }

                    List<QualityInfo> list = new List<QualityInfo>();

                    for (int i = 0; i < model.data.accept_quality.Count; i++)
                    {
                        list.Add(new QualityInfo()
                        {
                            Description = model.data.accept_description[i],
                            Qn = model.data.accept_quality[i]
                        });
                    }
                    
                    return new ReturnModel<List<QualityInfo>>()
                    {
                        success = true,
                        message = "",
                        data = list
                    };
                }
                else
                {
                    return new ReturnModel<List<QualityInfo>>()
                    {
                        success = true,
                        message = "",
                        data = new List<QualityInfo>() { }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ReturnModel<List<QualityInfo>>()
                {
                    success = true,
                    message = "",
                    data = new List<QualityInfo>() { }
                };
            }
        }
        public async Task<ReturnModel<List<DownloadUrlInfo>>> GetVideoDownloadUrl(string aid, string cid, QualityInfo quality, string access_key = "", string mid = "")
        {

            var andorid_api = await GetVideoDownloadUrlAndroidApi(aid, cid, quality, access_key, mid);
            if (andorid_api != null)
            {
                return new ReturnModel<List<DownloadUrlInfo>>()
                {
                    success = true,
                    message = "",
                    data = andorid_api
                };
            }


            return new ReturnModel<List<DownloadUrlInfo>>()
            {
                success = false,
                message = "无法读取下载地址"
            };
        }
        private async Task<List<DownloadUrlInfo>> GetVideoDownloadUrlAndroidApi(string aid, string cid, QualityInfo quality, string access_key = "", string mid = "")
        {
            try
            {
                string url = ApiHelper.GetSignWithUrl($"https://app.bilibili.com/x/playurl?npcybs=1&mobi_app=android&fnval=0&fnver=0&platform=android&fourk=1&build={ ApiHelper.build }&actionkey=appkey&appkey={ApiHelper.AndroidVideoKey.Appkey }&otype=json&qn={quality.Qn}&device=android&aid={aid}&cid={cid}&force_host=0&ts={ApiHelper.GetTimeSpan_2}{ ((access_key == "") ? "" : $"&access_key={access_key}&mid={mid}")}", ApiHelper.AndroidVideoKey);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = JsonConvert.DeserializeObject<VideoUrlInfo>(results);
                if (model.code == 0)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("User-Agent", "Bilibili Freedoooooom/MarkII");
                    List<DownloadUrlInfo> downloadUrls = new List<DownloadUrlInfo>();
                    foreach (var item in model.data.durl)
                    {
                        downloadUrls.Add(new DownloadUrlInfo()
                        {
                            Aid = aid,
                            Cid = cid,
                            Codecid = model.data.video_codecid,
                            Format = model.data.format,
                            From = "bilibili_season",
                            Headers = headers,
                            Length = item.length,
                            Order = item.order,
                            QualityInfo = quality,
                            Size = item.size,
                            Url = item.url
                        });
                    }
                    return downloadUrls;
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

    }

    public class QualityInfo
    {
        public int Qn { get; set; }
        public string Description { get; set; }
    }

    public class DownloadUrlInfo
    {
        public string Cid { get; set; }
        public string Aid { get; set; }
        public QualityInfo QualityInfo { get; set; }
        public string Url { get; set; }
        public int Order { get; set; }
        public long Length { get; set; }
        public long Size { get; set; }
        public int Codecid { get; set; }
        public string Format { get; set; }
        public string From { get; set; }
        public IDictionary<string, string> Headers { get; set; }
    }
    public class VideoUrlInfo
    {
        public int code { get; set; }
        public string message { get; set; }
        public SeasonUrlInfo data { get; set; }

    }

    public class SeasonUrlInfo
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<durl> durl { get; set; }
        public int is_preview { get; set; }
        public string type { get; set; }
        public int video_codecid { get; set; }
        public List<int> accept_quality { get; set; }
        public List<string> accept_description { get; set; }
        public string format { get; set; }

    }

    public class durl
    {
        public int order { get; set; }
        public long size { get; set; }
        public long length { get; set; }
        public string url { get; set; }
    }


}
