using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BiliBili3.Helper
{
    class _5DMHelper
    {
        public async static Task<string> GetUrl(string sid, int index)
        {
            try
            {
                string playUrl = "";
                var data = await GetData(sid);
                if (data == null)
                {
                    return "";
                }

                string url1 = "https://www.5dm.tv/bangumi/" + data.toid + "?link=" + index;
                string re1 = await GetResults(new Uri(url1), "https://www.5dm.tv");

                string url2 = Regex.Match(re1, @"<iframe.*?src=""(.*?)""").Groups[1].Value;
                string re2 = await GetResults(new Uri(url2), url1);

                var m = Regex.Matches(re2, @"<source.*?src=""(.*?)""", RegexOptions.Singleline);
                foreach (Match item in m)
                {
                    if (!item.Groups[1].Value.Contains("http"))
                    {
                        playUrl = "https://5mplayer.duapp.com" + item.Groups[1].Value;
                    }
                    else
                    {
                        playUrl = item.Groups[1].Value;
                    }
                }
                return playUrl;
            }
            catch (Exception)
            {
                return "";
            }



        }

        private static async Task<BILIDMModel> GetData(string sid)
        {
            try
            {
                List<BILIDMModel> list = new List<BILIDMModel>();
                var re = await WebClientClass.GetResults(new Uri("https://moe.nsapps.cn/api/v1/OtherSiteAnimes?rnd=" + ApiHelper.GetTimeSpan));
                JObject jObject = JObject.Parse(re);
                list = JsonConvert.DeserializeObject<List<BILIDMModel>>(jObject["data"].ToString());
                if (list.Exists(x => x.sid == sid))
                {
                    return list.Find(x => x.sid == sid);
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
        public static async Task<string> GetResults(Uri url, string Referer)
        {
            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            using (HttpClient hc = new HttpClient(fiter))
            {
                hc.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                hc.DefaultRequestHeaders.Referer = new Uri(Referer);

                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                string results = await hr.Content.ReadAsStringAsync();
                return results;
            }
        }


        public class BILIDMModel
        {
            public string title { get; set; }
            public string sid { get; set; }
            public string toid { get; set; }
        }
    }

}
