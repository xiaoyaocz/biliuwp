using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BiliBili.Background
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //通知
            ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
            bool Update = SettingHelper.Get_DTCT();
            
            if (Update)
            {
                var deferral = taskInstance.GetDeferral();
                await GetNews();
                deferral.Complete();
            }
            else
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
            }
        }

       

        private async Task GetNews()
        {
            try
            {
                var response = await GetUserAttentionUpdate();

                if (response != null)
                {
                    //var news = response.Data.Take(5).ToList();
                    UpdatePrimaryTile(response);
                    //UpdateSecondaryTile(response);
                }

            }
            catch (Exception)
            {
                // ignored
            }
          
        }


        private void UpdatePrimaryTile(List<GetAttentionUpdate> news)
        {
            ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
            if (news == null || !news.Any())
            {
                return;
            }

            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueueForWide310x150(true);
                updater.EnableNotificationQueueForSquare150x150(true);
                updater.EnableNotificationQueueForSquare310x310(true);
                updater.EnableNotificationQueue(true);

                List<string> oldList = new List<string>();
                updater.Clear();
                bool updateVideo = SettingHelper.Get_DT();
                bool updateBnagumi = SettingHelper.Get_FJ();
                try
                {



                    if (SettingHelper.Get_TsDt().Length!=0)
                    {
                        oldList = SettingHelper.Get_TsDt().ToString().Split(',').ToList();
                        //oldList.RemoveAt(0);
                    }
                    else
                    {
                        string s1 = "";
                        foreach (var item in news)
                        {
                            s1 += item.addition.aid + ",";
                        }
                        s1 = s1.Remove(s1.Length - 1);
                        SettingHelper.Set_TsDt(s1);
                        //container.Values["TsDt"] = s1;
                    }
                }
                catch (Exception)
                {
                }


                foreach (var n in news)
                {
                    if (news.IndexOf(n) <= 4)
                    {
                        var doc = new XmlDocument();
                        var xml = string.Format(TileTemplateXml, n.addition.pic, n.addition.title, n.addition.description);
                        doc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
                        {
                            ProhibitDtd = false,
                            ValidateOnParse = false,
                            ElementContentWhiteSpace = false,
                            ResolveExternals = false
                        });
                        updater.Update(new TileNotification(doc));
                    }

                    //通知
                    if (oldList != null && oldList.Count != 0)
                    {
                        if (!oldList.Contains(n.addition.aid))
                        {
                            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
                            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
                            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                            ((XmlElement)toastNode).SetAttribute("duration", "short");
                            ((XmlElement)toastNode).SetAttribute("launch", n.addition.aid);
                            if (n.type == 3)
                            {
                                if (updateBnagumi)
                                {
                                    toastTextElements[0].AppendChild(toastXml.CreateTextNode("您关注的《" + n.source.title + "》" + "更新了第" + n.content.index + "话"));
                                    ToastNotification toast = new ToastNotification(toastXml);
                                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                                }
                            }
                            else
                            {
                                if (updateVideo)
                                {
                                    toastTextElements[0].AppendChild(toastXml.CreateTextNode(n.source.uname + "" + "上传了《" + n.addition.title + "》"));
                                    ToastNotification toast = new ToastNotification(toastXml);
                                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                                }
                            }

                        }
                    }
                }
                //container.Values["Ts"] = news;



            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                string s = "";
                foreach (var item in news)
                {
                    s += item.addition.aid + ",";
                }
                s = s.Remove(s.Length - 1);
                SettingHelper.Set_TsDt(s);
            }
        }

        /// <summary>
        /// 关注动态
        /// </summary>
        /// <returns></returns>
        private async Task<List<GetAttentionUpdate>> GetUserAttentionUpdate()
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                    ApiHelper.access_key = await FileIO.ReadTextAsync(file);
                    string url = string.Format("http://api.bilibili.com/x/feed/pull?ps=10&type=0&pn={0}&_={1}", 1, ApiHelper.GetTimeSpen);
                   // url += "&sign=" + ApiHelper.GetSign(url);
                    HttpResponseMessage hr = await hc.GetAsync(new Uri(url));
                    hr.EnsureSuccessStatusCode();
                    string results = await hr.Content.ReadAsStringAsync();

                    //一层
                    GetAttentionUpdate model1 = JsonConvert.DeserializeObject<GetAttentionUpdate>(results);
                    if (model1.code == 0)
                    {
                        GetAttentionUpdate model2 = JsonConvert.DeserializeObject<GetAttentionUpdate>(model1.data.ToString());
                        return model2.feeds;
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

        private const string TileTemplateXml = @"
<tile branding='name'> 
  <visual version='3'>
    <binding template='TileMedium'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileWide'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileLarge'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
  </visual>
</tile>";
        private class GetAttentionUpdate
        {
            //必须有登录Cookie
            //Josn：http://api.bilibili.com/x/feed/pull?jsonp=jsonp&ps=20&type=1&pn=1
            //第一层
            public int code { get; set; }//状态，0为正常
            public object data { get; set; }//数据，包含第二层

            public List<GetAttentionUpdate> feeds { get; set; }

            public GetAttentionUpdate page { get; set; }
            public int count { get; set; }
            public int num { get; set; }
            public int size { get; set; }


            public string id { get; set; }//视频ID
            public string src_id { get; set; }//作者信息，包含第四层
            public string add_id { get; set; }//视频信息，包含第四层
            public int type { get; set; }
            public string mcid { get; set; }
            public GetAttentionUpdate source { get; set; }
            public string mid { get; set; }
            public string uname { get; set; }
            public string sex { get; set; }
            public string avatar { get; set; }
            public string sign { get; set; }
            public GetAttentionUpdate new_ep { get; set; }
            public string av_id { get; set; }
            public string index { get; set; }

            public GetAttentionUpdate addition { get; set; }
            public string description { get; set; }
            public string aid { get; set; }
            public string title { get; set; }//标题
            public string typename { get; set; }//播放数
            public int typeid { get; set; }//播放数

            public string play { get; set; }//弹幕数
            public string video_review { get; set; }//上传时间
            public string pic { get; set; }//封面
            public long ctime { get; set; }

            public GetAttentionUpdate content { get; set; }


            public string Create
            {
                get
                {
                    DateTime dtStart = new DateTime(1970, 1, 1);
                    //long lTime = long.Parse(ctime + "000");
                    //long lTime = long.Parse(textBox1.Text);
                    TimeSpan toNow = TimeSpan.FromSeconds(ctime);
                    DateTime dt = dtStart.Add(toNow).ToLocalTime();
                    TimeSpan span = DateTime.Now - dt;
                    if (span.TotalDays > 7)
                    {
                        return dt.ToString("MM-dd");
                    }
                    else
                    if (span.TotalDays > 1)
                    {
                        return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
                    }
                    else
                    if (span.TotalHours > 1)
                    {
                        return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                    }
                    else
                    if (span.TotalMinutes > 1)
                    {
                        return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                    }
                    else
                    if (span.TotalSeconds >= 1)
                    {
                        return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                    }
                    else
                    {
                        return "1秒前";
                    }
                }
            }

        }
    }
    class ApiHelper
    {
        //九幽反馈
        public const string JyAppkey = @"1afd8ae4b933daa51a39573a5719bba5";
        public const string JySecret = @"d9e7262e70801e795c18dc20e0972df6";

        public const string _appSecret_Wp = "ba3a4e554e9a6e15dc4d1d70c2b154e3";//Wp
        public const string _appSecret_IOS = "8cb98205e9b2ad3669aad0fce12a4c13";//Ios
        public const string _appSecret_Android = "ea85624dfcf12d7cc7b2b3a94fac1f2c";//Android
        public const string _appSecret_DONTNOT = "2ad42749773c441109bdc0191257a664";//Android

        public const string _appKey = "422fd9d7289a1dd9";//Wp
        public const string _appKey_IOS = "4ebafd7c4951b366";
        public const string _appKey_Android = "c1b107428d337928";
        public const string _appkey_DONTNOT = "85eb6835b0a1034e";//e5b8ba95cab6104100be35739304c23a
        //85eb6835b0a1034e,2ad42749773c441109bdc0191257a664
        public static string access_key = string.Empty;

        public static string GetSign(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret_Wp);
            result = GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static string GetSign_Android(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret_Android);
            result = GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }

        public static long GetTimeSpen
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); }
        }

        public static string GetMd5String(string result)
        {
            //可以选择MD5 Sha1 Sha256 Sha384 Sha512
            string strAlgName = HashAlgorithmNames.Md5;

            // 创建一个 HashAlgorithmProvider 对象
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);

            // 创建一个可重用的CryptographicHash对象           
            CryptographicHash objHash = objAlgProv.CreateHash();

            IBuffer buffMsg1 = CryptographicBuffer.ConvertStringToBinary(result, BinaryStringEncoding.Utf16BE);
            objHash.Append(buffMsg1);
            IBuffer buffHash1 = objHash.GetValueAndReset();
            string strHash1 = CryptographicBuffer.EncodeToHexString(buffHash1);
            return strHash1;
        }



        public static string GetUserId()
        {
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            Dictionary<string, string> ls = new Dictionary<string, string>();
            string a = string.Empty;
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name, item.Value);
                if (item.Name == "DedeUserID")
                {
                    a = item.Value;
                }
            }
            return a;

        }


    }
    class SettingHelper
    {
        static ApplicationDataContainer container;
        public static bool Get_DTCT()
        {
            container = ApplicationData.Current.LocalSettings;
            if (container.Values["DTCT"] != null)
            {
                return (bool)container.Values["DTCT"];
            }
            else
            {
                Set_DTCT(true);
                return true;
            }
        }
        public static void Set_DTCT(bool value)
        {
            container = ApplicationData.Current.LocalSettings;
            container.Values["DTCT"] = value;
        }



        public static bool Get_DT()
        {
            container = ApplicationData.Current.LocalSettings;
            if (container.Values["DT"] != null)
            {
                return (bool)container.Values["DT"];
            }
            else
            {
                Set_DT(true);
                return true;
            }
        }

        public static void Set_DT(bool value)
        {
            container = ApplicationData.Current.LocalSettings;
            container.Values["DT"] = value;
        }

        public static bool Get_FJ()
        {
            container = ApplicationData.Current.LocalSettings;
            if (container.Values["FJ"] != null)
            {
                return (bool)container.Values["FJ"];
            }
            else
            {
                Set_FJ(true);
                return true;
            }
        }

        public static void Set_FJ(bool value)
        {
            container = ApplicationData.Current.LocalSettings;
            container.Values["FJ"] = value;
        }
        public static string Get_TsDt()
        {
            container = ApplicationData.Current.LocalSettings;
            if (container.Values["TsDt"] != null)
            {
                return (string)container.Values["TsDt"];
            }
            else
            {
                Set_TsDt("");
                return "";
            }
        }

        public static void Set_TsDt(string value)
        {
            container = ApplicationData.Current.LocalSettings;
            container.Values["TsDt"] = value;
        }
    }
}
