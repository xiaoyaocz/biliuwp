using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api
{
    public class PlayerAPI
    {
        public ApiModel VideoPlayUrl(string aid, string cid, int qn, bool dash)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://api.bilibili.com/x/player/playurl",
                parameter = ApiUtils.MustParameter(ApiUtils.WebVideoKey, true) + $"&avid={aid}&cid={cid}&qn={qn}&type=&otype=json&mid={(ApiHelper.IsLogin() ? ApiHelper.GetUserId() : "")}"
            };
            if (dash)
            {
                api.parameter += "&fourk=1&fnver=0&fnval=16";
            }
            api.parameter += ApiUtils.GetSign(api.parameter, ApiUtils.WebVideoKey);
            return api;
        }
        public ApiModel SeasonPlayUrl(string aid, string cid, int qn, int season_type, bool dash)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://api.bilibili.com/pgc/player/web/playurl",
                parameter = $"appkey={ApiUtils.WebVideoKey.Appkey}&cid={cid}&qn={qn}&type=&otype=json&module=bangumi&season_type={season_type}"
            };
            if (ApiHelper.IsLogin())
            {
                api.parameter += $"&access_key={ApiHelper.access_key}&mid={ApiHelper.GetUserId()}";
            }
            if (dash)
            {
                api.parameter += "&fourk=1&fnver=0&fnval=16";
            }
            api.parameter += ApiUtils.GetSign(api.parameter, ApiUtils.WebVideoKey);
            return api;
        }
        public ApiModel SeasonPlayUrl23Moe(string animeid, string cid, string epid)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://api.bilibili.com/x/player/playurl",
                parameter = $"animeid={animeid}&cid={cid}&epid={epid}&rnd={Utils.GetTimestampS()}"
            };
            return api;
        }
        public ApiModel SeasonPlayUrlBiliPlus(string aid, string cid, int qn, int season_type, bool dash)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://www.biliplus.com/BPplayurl.php",
                parameter = $"appkey={ApiUtils.WebVideoKey.Appkey}&cid={cid}&qn={qn}&type=&otype=json&module=bangumi&season_type={season_type}"
            };
            if (ApiHelper.IsLogin())
            {
                api.parameter += $"&access_key={ApiHelper.access_key}&mid={ApiHelper.GetUserId()}";
            }
            if (dash)
            {
                api.parameter += "&fnver=0&fnval=16";
            }
            api.parameter += ApiUtils.GetSign(api.parameter, ApiUtils.WebVideoKey);
            return api;
        }

        public ApiModel LivePlayUrl(string cid, int qn = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://api.live.bilibili.com/room/v1/Room/playUrl",
                parameter = $"cid={cid}&qn={qn}&platform=web"
            };
            //api.parameter += ApiUtils.GetSign(api.parameter, ApiUtils.AndroidVideoKey);
            return api;
        }

        /// <summary>
        /// 番剧播放记录上传
        /// </summary>
        /// <param name="aid">AVID</param>
        /// <param name="cid">CID</param>
        /// <param name="sid">SID</param>
        /// <param name="epid">EPID</param>
        /// <param name="type">类型 3=视频，4=番剧</param>
        /// <param name="progress">进度/秒</param>
        /// <returns></returns>
        public ApiModel SeasonHistoryReport(string aid, string cid, int progress, string sid = "0", string epid = "0", int type = 3)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.POST,
                baseUrl = $"https://api.bilibili.com/x/v2/history/report",
                body = ApiUtils.MustParameter(ApiUtils.AndroidVideoKey, true) + $"&aid={aid}&cid={cid}&epid={epid}&sid={sid}&progress={progress}&realtime={progress}&sub_type=1&type={type}"
            };
            api.body += ApiUtils.GetSign(api.body, ApiUtils.AndroidVideoKey);
            return api;
        }
        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="aid">AV</param>
        /// <param name="cid">CID</param>
        /// <param name="color">颜色(10进制)</param>
        /// <param name="msg">内容</param>
        /// <param name="position">位置</param>
        /// <param name="mode">类型</param>
        /// <param name="plat">平台</param>
        /// <returns></returns>
        public ApiModel SendDanmu(string aid, string cid, string color, string msg, int position, int mode = 1, int plat = 2)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.POST,
                baseUrl = $"https://api.bilibili.com/x/v2/dm/post",
                parameter = ApiUtils.MustParameter(ApiUtils.AndroidVideoKey, true) + $"&aid={aid}",
                body = $"msg={Uri.EscapeDataString(msg)}&mode={mode}&screen_state=1&color={color}&pool=0&progress={Convert.ToInt32(position * 1000)}&fontsize=25&rnd={Utils.GetTimestampS()}&from=7&oid={cid}&plat={plat}&type=1"
            };
            api.parameter += ApiUtils.GetSign(api.parameter, ApiUtils.AndroidVideoKey);
            return api;
        }
        /// <summary>
        /// 读取播放信息
        /// </summary>
        /// <param name="aid">AV</param>
        /// <param name="cid">CID</param>
        /// <returns></returns>
        public ApiModel GetPlayerInfo(string aid, string cid)
        {
            var header = new Dictionary<string, string>();
            header.Add("Referer", $"https://www.bilibili.com/video/av{aid}");
            header.Add("user-agent", "Mozilla/5.0 BiliDroid/5.52.0 (bbcallen@gmail.com)");
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = "https://api.bilibili.com/x/player.so",
                parameter = $"id=cid:{cid}&aid={aid}",
                headers = header
            };
            return api;
        }

        /// <summary>
        /// 弹幕关键词
        /// </summary>
        /// <returns></returns>
        public ApiModel GetDanmuFilterWords()
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://api.bilibili.com/x/dm/filter/user",
                parameter = ApiUtils.MustParameter(ApiUtils.AndroidVideoKey, true)
            };
            api.parameter += ApiUtils.GetSign(api.parameter, ApiUtils.AndroidVideoKey);
            return api;
        }
        /// <summary>
        /// 添加弹幕屏蔽关键词
        /// </summary>
        /// <param name="word">关键词</param>
        /// <param name="type">类型，0=关键字，1=正则，2=用户</param>
        /// <returns></returns>
        public ApiModel AddDanmuFilterWord(string word, int type)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.POST,
                baseUrl = $"https://api.bilibili.com/x/dm/filter/user/add",
                body = ApiUtils.MustParameter(ApiUtils.AndroidVideoKey, true) + $"&filter={Uri.EscapeDataString(word)}&type={type}"
            };
            api.body += ApiUtils.GetSign(api.parameter, ApiUtils.AndroidVideoKey);
            return api;
        }


    }
}
