using BiliBili3.Class;
using BiliBili3.Controls;
using BiliBili3.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using System.Xml.Linq;
using BiliBili3.Helper;

namespace BiliBili3
{
    enum LoginStatus
    {
        Succeed,
        Failure,
        Error
    }
    class ApiHelper
    {
        //九幽反馈
        public const string JyAppkey = @"afaaf76fbe62a275d4dc309d6151d3c3";



        //public const string _appSecret_Wp = "ba3a4e554e9a6e15dc4d1d70c2b154e3";//Wp
        public const string _appSecret_Wp = "560c52ccd288fed045859ed18bffd973";
        public const string _appSecret_IOS = "8cb98205e9b2ad3669aad0fce12a4c13";//Ios
        public const string _appSecret_Android = "560c52ccd288fed045859ed18bffd973";//Android
        public const string _appSecret_DONTNOT = "2ad42749773c441109bdc0191257a664";
        public const string _appSecret_Android2 = "jr3fcr8w7qey8wb0ty5bofurg2cmad8x";
        public const string _appSecret_VIP = "9b288147e5474dd2aa67085f716c560d";
        public const string _appSecret_VIDEO = "94aba54af9065f71de72f5508f1cd42e";
        public const string _appSecret_PlayUrl = "1c15888dc316e05a15fdd0a02ed6584f";
        public const string _appSecret_WebLogin = "c2ed53a74eeefe3cf99fbd01d8c9c375";
        public const string _appKey_Android2 = "1d8b6e7d45233436";
        public const string _appKey_VIP = "iVGUTjsxvpLeuDCf";

        // public const string _appKey = "422fd9d7289a1dd9";//Wp

        public const string _appKey = "1d8b6e7d45233436";//Wp
        public const string _appKey_WebLogin = "27eb53fc9058f8c3";//Wp
        public const string _appKey_VIDEO = "84956560bc028eb7";
        public const string _appKey_IOS = "4ebafd7c4951b366";
        public const string _appKey_Android = "1d8b6e7d45233436";
        public const string _appkey_DONTNOT = "85eb6835b0a1034e";//e5b8ba95cab6104100be35739304c23a
                                                                 //85eb6835b0a1034e,2ad42749773c441109bdc0191257a664
        public static string _buvid = "B3CC4714-C1D3-4010-918B-8E5253E123C16133infoc";
        public static string _hwid = "03008c8c0300d6d1";

        public const string build = "5442100";

        private static string _access_key;
        public static string access_key {
            get
            {
                if (_access_key == "")
                {
                    return SettingHelper.Get_Access_key();
                }
                else {
                    return _access_key;
                }; }
            set {  _access_key = value; }
        }
        //public static List<string> followList;


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
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }

        public static string GetSign_WebLogin(string url)
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
            stringBuilder.Append(_appKey_WebLogin);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
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
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }


        public static string GetSign_Ios(string url)
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
            stringBuilder.Append(_appSecret_IOS);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }


        public static string GetSign_Android2(string url)
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
            stringBuilder.Append(_appSecret_Android2);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static string GetSign_VIP(string url)
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
            stringBuilder.Append(_appSecret_VIP);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static string GetSign_VIDEO(string url)
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
            stringBuilder.Append(_appSecret_VIDEO);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }

        public static string GetSign_PlayUrl(string url)
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
            stringBuilder.Append(_appSecret_PlayUrl);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }


        public static string GetSign_DN(string url)
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
            stringBuilder.Append(_appSecret_DONTNOT);
            result = GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static long GetTimeSpan
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalSeconds); }
        }
        public static long GetTimeSpan_2
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalMilliseconds); }
        }

        public static List<EmojiModel> emojis;
        public static List<FaceModel> emoji;
        public static async void SetEmojis()
        {
            try
            {
                string url = "http://api.bilibili.com/x/v2/reply/emojis";
                string results = await WebClientClass.GetResults(new Uri(url));
                FaceModel model = JsonConvert.DeserializeObject<FaceModel>(results);
                emoji = model.data;
                emojis = new List<EmojiModel>();
                model.data.ForEach(x => x.emojis.ForEach(y => emojis.Add(y)));
            }
            catch (Exception)
            {
            }

        }


        public static List<RegionModel> regions;
        public static async Task SetRegions()
        {
            try
            {
                string url = string.Format("https://app.bilibili.com/x/v2/region/index?appkey={0}&build={2}&mobi_app=android&platform=android&ts={1}", ApiHelper._appKey,GetTimeSpan,ApiHelper.build);
                url += "&sign=" + ApiHelper.GetSign(url);

                string results = await WebClientClass.GetResults(new Uri(url));
                RegionModel model = JsonConvert.DeserializeObject<RegionModel>(results);
                if (model.code==0)
                {
                    model.data.RemoveAll(x =>(x.name == "会员购" || x.name == "游戏中心")|| x.logo=="");
                
                    regions = model.data;

                   // model.data.ForEach(x => x.emojis.ForEach(y => emojis.Add(y)));
                }
            }
            catch (Exception)
            {
            }

        }



        public static async Task<string> GetEncryptedPassword(string passWord)
        {
            string base64String;
            try
            {
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);
                string url = "https://passport.bilibili.com/login?act=getkey&_=" + GetTimeSpan_2;
                string stringAsync = await WebClientClass.GetResults(new Uri(url));
                JObject jObjects = JObject.Parse(stringAsync);
                string str = jObjects["hash"].ToString();
                string str1 = jObjects["key"].ToString();
                string str2 = string.Concat(str, passWord);
                string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
                byte[] numArray = Convert.FromBase64String(str3);
                AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
                CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(WindowsRuntimeBufferExtensions.AsBuffer(numArray), 0);
                IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, WindowsRuntimeBufferExtensions.AsBuffer(Encoding.UTF8.GetBytes(str2)), null);
                base64String = Convert.ToBase64String(WindowsRuntimeBufferExtensions.ToArray(buffer));
            }
            catch (Exception)
            {
                //throw;
                base64String = passWord;
            }
            return base64String;
        }

        //public static async Task<string> LoginBilibili(string UserName, string Password)
        //{
        //    try
        //    {
        //        //https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=JPJclVQpH4jwouRcSnngNnuPEq1S1rizxVJjLTg%2FtdqkKOizeIjS4CeRZsQg4%2F500Oye7IP4gWXhCRfHT6pDrboBNNkYywcrAhbOPtdx35ETcPfbjXNGSxteVDXw9Xq1ng0pcP1burNnAYtNRSayEKC1jiugi1LKyWbXpYE6VaM%3D&type=json&userid=xiaoyaocz&sign=74e4c872ec7b9d83d3a8a714e7e3b4b3
        //        //发送第一次请求，得到access_key
        //        string url = "https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=" + WebUtility.UrlEncode(await GetEncryptedPassword(Password)) + "&type=json&userid=" + WebUtility.UrlEncode(UserName);
        //        url += "&sign="+GetSign(url);

        //        string results = await WebClientClass.GetResults(new Uri(url));
        //        //Json解析及数据判断
        //        LoginModel model = new LoginModel();
        //        model = JsonConvert.DeserializeObject<LoginModel>(results);
        //        if (model.code == -627)
        //        {
        //            return "登录失败，密码错误！";
        //        }
        //        if (model.code == -626)
        //        {
        //            return "登录失败，账号不存在！";
        //        }
        //        if (model.code == -625)
        //        {
        //            return "密码错误多次";
        //        }
        //        if (model.code == -628)
        //        {
        //            return "未知错误";
        //        }
        //        if (model.code == -1)
        //        {
        //            return "登录失败，程序注册失败！请联系作者！";
        //        }

        //        if (model.code == 0)
        //        {
        //            access_key = model.access_key;
        //            string urlgo = "http://api.bilibili.com/login/sso?gourl=http%3A%2F%2Fwww.bilibili.com&access_key=" + model.access_key + "&appkey=422fd9d7289a1dd9&platform=android&scale=xhdpi";
        //            urlgo += "&sign=" + ApiHelper.GetSign(urlgo);
        //            WebView WB = new WebView();
        //            WB.Navigate(new Uri(urlgo));

        //           // await WebClientClass.GetResults(new Uri(urlgo));

        //            StorageFolder folder = ApplicationData.Current.LocalFolder;
        //            StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
        //            await FileIO.WriteTextAsync(file, model.access_key);
        //        }
        //        //看看存不存在Cookie
        //        HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
        //        HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

        //        List<string> ls = new List<string>();
        //        foreach (HttpCookie item in cookieCollection)
        //        {
        //            ls.Add(item.Name);
        //        }
        //        if (ls.Contains("DedeUserID"))
        //        {
        //            return "登录成功";
        //        }
        //        else
        //        {
        //            return "登录失败";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.HResult == -2147012867)
        //        {
        //            return "登录失败，检查你的网络连接！";
        //        }
        //        else
        //        {
        //            return "登录发生错误";
        //        }

        //    }

        //}
        public static async Task<LoginModel> LoginBilibili(string UserName, string Password,string captcha="")
        {
            try
            {
               
                //https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=JPJclVQpH4jwouRcSnngNnuPEq1S1rizxVJjLTg%2FtdqkKOizeIjS4CeRZsQg4%2F500Oye7IP4gWXhCRfHT6pDrboBNNkYywcrAhbOPtdx35ETcPfbjXNGSxteVDXw9Xq1ng0pcP1burNnAYtNRSayEKC1jiugi1LKyWbXpYE6VaM%3D&type=json&userid=xiaoyaocz&sign=74e4c872ec7b9d83d3a8a714e7e3b4b3
                //发送第一次请求，得到access_key
                string url = "https://passport.bilibili.com/api/oauth2/login"; //+ WebUtility.UrlEncode(await GetEncryptedPassword(Password)) + " &type=json&userid=" + WebUtility.UrlEncode(UserName);
                //url += "&sign=" + GetSign(url);

                string content = string.Format("appkey={0}&platform=android&password={1}&username={2}&ts={3}", _appKey_Android, WebUtility.UrlEncode(await GetEncryptedPassword(Password)), WebUtility.UrlEncode(UserName), GetTimeSpan);
                if (captcha!="")
                {
                    content += "&captcha="+ captcha;
                }

                content += "&sign=" + GetSign_Android(content);
                string results = await WebClientClass.PostResults(new Uri(content), "");
                //Json解析及数据判断
                LoginModel model = new LoginModel();
                model = JsonConvert.DeserializeObject<LoginModel>(results);
                //if (model.code == -627|| model.code == -629)
                //{
                //    return model.message;
                //}
                //if (model.code == -626)
                //{
                //    return "登录失败，账号不存在！";
                //}
                //if (model.code == -625)
                //{
                //    return "密码错误多次";
                //}
                //if (model.code == -628)
                //{
                //    return "未知错误";
                //}
                //if (model.code == -1)
                //{
                //    return "登录失败，程序注册失败！请联系作者！";
                //}
                //if (model.code==-105)
                //{
                //    return "需要验证码";
                //}

                if (model.code == 0)
                {
                    access_key = model.data. access_token;
                    string urlgo = string.Format("https://passport.bilibili.com/api/login/sso?access_key={0}&appkey={1}&build=511000&gourl=http%3A%2F%2Fbangumi.bilibili.com%2Fmoe%2F2017%2Fjp%2Fmobile%2F&mobi_app=android&platform=android&ts={2}", model.data.access_token, _appKey_Android, GetTimeSpan);
                    urlgo += "&sign=" + ApiHelper.GetSign(urlgo);
                    try
                    {
                        //WebView wc = new WebView();
                        //wc.Navigate(new Uri(urlgo));
                        await WebClientClass.GetResults(new Uri(urlgo));
                    }
                    catch (Exception ex)
                    {
                        //throw;
                    }

                    // await WebClientClass.GetResults(new Uri(urlgo));
                    SettingHelper.Set_Access_key(model.data.access_token);
                    //StorageFolder folder = ApplicationData.Current.LocalFolder;
                    //StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                    //await FileIO.WriteTextAsync(file, model.data.access_token);
                }
                else
                {
                    return model;
                }
                //看看存不存在Cookie
                HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

                List<string> ls = new List<string>();
                foreach (HttpCookie item in cookieCollection)
                {
                    ls.Add(item.Name);
                }

                if (ls.Contains("DedeUserID") || SettingHelper.Get_Access_key().Length != 0)
                {
                   // return "登录成功";
                }
                else
                {
                    model.code = -233;
                    model.message = "登录失败，请重新登录";
                    //return "登录失败";
                }
                return model;
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    return new LoginModel() {
                         code=-223,
                         message="检查你的网络连接"
                    };
                }
                else
                {
                    return new LoginModel()
                    {
                        code = ex.HResult,
                        message = "登录发生错误"
                    };
                }

            }

        }


        public async static Task<LoginStatus> QRLogin(string oauthKey)
        {
            try
            {
                string url = string.Format("https://passport.bilibili.com/qrcode/login?access_key={0}&appkey={1}&build=5250000&mobi_app=android&oauthKey={2}&platform=android", ApiHelper.access_key, ApiHelper._appKey_Android, oauthKey);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                JObject obj = JObject.Parse(results);
                if ((int)obj["code"] == 0)
                {
                    return LoginStatus.Succeed;
                }
                else
                {
                    return LoginStatus.Failure;
                }
            }
            catch (Exception)
            {
                return LoginStatus.Error;
            }
        }


        public static string GetUserId()
        {
            if (IsLogin())
            {
                return SettingHelper.Get_UserID().ToString();
            }
            else
            {
                return "0";
            }

        }
        public static string GetHwid()
        {
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            Dictionary<string, string> ls = new Dictionary<string, string>();
            string a = string.Empty;
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name, item.Value);
                if (item.Name == "buvid3")
                {
                    a = item.Value;
                }
            }
            return a;

        }
        public  static bool IsLogin()
        {
            if (SettingHelper.Get_Access_key() != "")
            {
                return true;
            }
            else
            {
               
                return false;
            }
        }
        public static string GetCookies()
        {
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            string cookie = "";
            foreach (HttpCookie item in cookieCollection)
            {
                cookie += item.Name + "=" + item.Value + ";";
            }



            return cookie;


        }

        //public static async Task<string> GetVideoUrl(PlayerModel model, int quality)
        //{
        //    switch (model.Mode)
        //    {
        //        case PlayMode.Sohu:
        //            return await GetSoHuPlayInfo(model.rich_vid, quality);

        //        default:
        //            //if (model.Mode== PlayMode.Bangumi)
        //            //{
        //            //    return await GetBiliUrl_Ban(model, quality);
        //            //}
        //            //else
        //            //{
        //            //    return await GetBiliUrl(model, quality);
        //            //}
        //            return await GetVideoUrl_MP4(model, quality);
        //    }

        //}

        //private static async Task<string> GetVideoUrl_MP4(PlayerModel model, int quality)
        //{
        //    try
        //    {
        //        var qn = 112;
        //        switch (quality)
        //        {
        //            case 1:
        //                qn = 32;
        //                break;
        //            case 2:
        //                qn = 64;
        //                break;
        //            case 3:
        //                qn = 48;
        //                break;
        //            default:
        //                break;
        //        }
        //        if (quality == 3)
        //        {
        //            quality = 2;
        //        }
        //        string url = string.Format("http://interface.bilibili.com/playurl?cid={0}&player=1&quality={1}&ts={2}", model.Mid, quality, GetTimeSpan_2);
        //        url += "&sign=" + ApiHelper.GetSign_VIDEO(url);


        //        string re = await WebClientClass.GetResults(new Uri(url));

        //        var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);


        //        //FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);


        //        // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
        //        return mc[0].Groups[3].Value;
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }
        //}


        //public static async Task<SYEngine.Playlist> GetVideoUrl_FLV(PlayerModel model, int quality)
        //{
        //    try
        //    {

        //        var qn = 32;
        //        switch (quality)
        //        {
        //            case 1:
        //                qn = 32;
        //                break;
        //            case 2:
        //                qn = 64;
        //                break;
        //            case 3:
        //                qn = 80;
        //                break;
        //            case 4:
        //                qn = 112;
        //                break;
        //            default:
        //                break;
        //        }
        //        //https://interface.bilibili.com/v2/playurl?cid=31452468&appkey=84956560bc028eb7&otype=json&type=&quality=112&qn=112&sign=38b1355368ee65d055c12b57bdb26e3a

        //        string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&qn={2}&ts={3}", ApiHelper._appKey_VIDEO,model.Mid, qn, GetTimeSpan_2);
        //        url += "&sign=" + ApiHelper.GetSign_VIDEO(url);


        //        string re = await WebClientClass.GetResults(new Uri(url));

        //        //var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);
        //        FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
        //        // FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
        //        var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
        //        if (m.code == 0)
        //        {
        //            foreach (var item in m.durl)
        //            {
        //                playList.Append(item.url, item.size, item.length / 1000);
        //            }
        //        }
        //        else
        //        {
        //            var re3 = await WebClientClass.GetResults(new Uri(string.Format("http://120.92.50.146/23moe/api/v1/BiliAnimeUrl?animeid={0}&cid={1}&epid={2}&rnd={3}", model.banId, model.Mid, model.banInfo.episode_id, GetTimeSpan)));
        //            JObject obj = JObject.Parse(re3);
        //            if (Convert.ToInt32(obj["code"].ToString()) == 0)
        //            {
                        


                      
        //            }



        //            return null;
        //        }
        //        // playList.Append("http://cn-gdfs4-dx-v-05.acgvideo.com/vg5/7/41/22127332-1.flv?expires=1503063600&platform=pc&ssig=PK_rnmzW87H2KAPUpunQxQ&oi=1901471098&nfa=fcKlFxz1QpHo9Foo1/OG3A==&dynamic=1&hfa=2074472869&hfb=Yjk5ZmZjM2M1YzY4ZjAwYTMzMTIzYmIyNWY4ODJkNWI=", 0, 0);
        //        // playList.Append("http://cn-gdfs4-dx-v-05.acgvideo.com/vg5/7/41/22127332-2.flv?expires=1503063600&platform=pc&ssig=ZsHXmsEKDQldCEIjpQ0w-Q&oi=1901471098&nfa=ejKFeH33a+U2uARxtCj9GQ==&dynamic=1&hfb=Yjk5ZmZjM2M1YzY4ZjAwYTMzMTIzYmIyNWY4ODJkNWI=", 0, 0);




        //        SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
        //        config.DownloadRetryOnFail = true;
        //        config.HttpCookie = string.Empty;

        //        config.UniqueId = string.Empty;
        //        config.HttpReferer = "https://www.bilibili.com/video/av" + model.Aid + "/";
        //        config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

        //        playList.NetworkConfigs = config;

        //        // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
        //        return playList;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //}
        //public static async Task<List<string>> GetVideoUrl_Download(DownloadTaskModel m)
        //{
        //    try
        //    {
        //        List<string> urls = new List<string>();
        //        var qn = 32;
        //        switch (m.quality)
        //        {
        //            case 1:
        //                qn = 32;
        //                break;
        //            case 2:
        //                qn = 64;
        //                break;
        //            case 3:
        //                qn = 80;
        //                break;
        //            case 4:
        //                qn = 112;
        //                break;
        //            default:
        //                break;
        //        }

        //        //https://interface.bilibili.com/v2/playurl?cid=31452468&appkey=84956560bc028eb7&otype=json&type=&quality=112&qn=112&sign=38b1355368ee65d055c12b57bdb26e3a

        //        if (m.downloadMode== DownloadMode.Video)
        //        {
        //            string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, m.cid, qn, GetTimeSpan_2);
        //            url += "&sign=" + ApiHelper.GetSign_VIDEO(url);
        //            string re = await WebClientClass.GetResults(new Uri(url));
        //            FlvPlyaerUrlModel flvPlyaerUrlModel = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
        //            if (flvPlyaerUrlModel.code == 0)
        //            {
        //                foreach (var item in flvPlyaerUrlModel.durl)
        //                {
        //                    urls.Add(item.url);
        //                }
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //        else{

        //            string url = string.Format("https://interface.bilibili.com/playurl?cid={0}&player=1&quality={1}&qn={1}&ts={2}", m.cid, qn, GetTimeSpan);
        //            url += "&sign=" + GetSign_PlayUrl(url);
        //            string re = await WebClientClass.GetResults(new Uri(url));
                  
        //            if (re.Contains("<code>"))
        //            {
                        
        //                string url2 = string.Format(
        //                    "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type=1&qn={2}&ts={3}", ApiHelper._appKey_VIDEO, m.cid, qn, GetTimeSpan_2);
        //                url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
        //                re = await WebClientClass.GetResults(new Uri(url2));
        //                FlvPlyaerUrlModel flvPlyaerUrlModel = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
        //                if (flvPlyaerUrlModel.code == 0 && !re.Contains("8986943"))
        //                {
        //                    foreach (var item in flvPlyaerUrlModel.durl)
        //                    {
        //                        urls.Add(item.url);
        //                    }

        //                }
        //                else
        //                {
        //                    var re3 = await WebClientClass.GetResults(new Uri(string.Format("http://120.92.50.146/23moe/api/v1/BiliAnimeUrl?animeid={0}&cid={1}&epid={2}&rnd={3}", m.sid, m.cid, m.epid, GetTimeSpan)));
        //                    JObject obj = JObject.Parse(re3);
        //                    if (Convert.ToInt32(obj["code"].ToString()) == 0)
        //                    {
        //                        urls.Add(obj["data"][0]["url"].ToString());
        //                    }
        //                    else{
        //                        var playurl = await _5DMHelper.GetUrl(m.sid, Convert.ToInt32(m.epIndex) - 1);
        //                        if (playurl == "")
        //                        {
        //                            return null;
        //                        }
        //                        else
        //                        {
        //                            urls.Add(playurl);
        //                        }
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);
        //                foreach (Match item in mc)
        //                {
        //                    urls.Add(item.Groups[3].Value);
        //                }
        //            }
        //        }
                
              
        //        return urls;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }


            


        //}


        ////public static async Task<SYEngine.Playlist> GetVideoUrl_FLV(PlayerModel model, int quality)
        ////{
        ////    try
        ////    {

        ////        var qn = 80;
        ////        switch (quality)
        ////        {
        ////            case 1:
        ////                qn = 32;
        ////                break;
        ////            case 2:
        ////                qn = 64;
        ////                break;
        ////            case 3:
        ////                qn = 80;
        ////                break;
        ////            default:
        ////                break;
        ////        }

        ////        string url = string.Format("http://interface.bilibili.com/playurl?cid={0}&player=1&otype=json&type=flv&quality={2}&qn={3}&ts={1}", model.Mid,GetTimeSpan_2,qn,qn);
        ////        url += "&sign=" + ApiHelper.GetSign_VIDEO(url);


        ////        string re  = await WebClientClass.GetResults(new Uri(url));


        ////        FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
        ////        var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
        ////        foreach (var item in m.durl)
        ////        {
        ////            //if (item.backup_url!=null&&item.backup_url.Length!=0)
        ////            //{
        ////            //    playList.Append(item.backup_url[0], item.size, item.length / 1000);
        ////            //}
        ////            //else{
        ////                playList.Append(item.url, item.size, item.length / 1000);
        ////            //}
        ////        }

        ////        SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
        ////        config.DownloadRetryOnFail = true;
        ////        config.HttpCookie = GetCookies();
        ////        config.UniqueId = string.Empty;
        ////        config.HttpReferer = "https://www.bilibili.com/video/av"+model.Aid+"/";
        ////        config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

        ////        playList.NetworkConfigs = config;

        ////        // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
        ////        return playList;
        ////    }
        ////    catch (Exception)
        ////    {
        ////        return null;
        ////    }

        ////}
        //public static async Task<ReturnPlayModel> GetBangumiUrl_FLV(PlayerModel model, int quality)
        //{
        //    try
        //    {
        //        var qn = 80;
        //        switch (quality)
        //        {
        //            case 1:
        //                qn = 32;
        //                break;
        //            case 2:
        //                qn = 64;
        //                break;
        //            case 3:
        //                qn = 80;
        //                break;
        //            case 4:
        //                qn = 112;
        //                break;
        //            default:
        //                break;
        //        }


        //        string url = string.Format("https://interface.bilibili.com/v2/playurl?cid={0}&player=1&quality={1}&qn={1}&ts={2}", model.Mid, qn, GetTimeSpan);
        //        url += "&sign=" + GetSign_PlayUrl(url);

        //        string re = await WebClientClass.GetResults(new Uri(url));
        //        var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);

        //        SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
        //        config.DownloadRetryOnFail = true;
        //        config.HttpCookie = string.Empty;
        //        config.UniqueId = string.Empty;
        //        config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
             
        //        if (re.Contains("<code>"))
        //        {
        //            //string url2 = string.Format("https://bangumi.bilibili.com/player/web_api/playurl/?access_key={3}&cid={0}&module=bangumi&player=1&otype=json&type=flv&quality={1}&ts={2}", model.Mid, quality, GetTimeSpan_2,access_key);
        //            //url2 += "&sign=" + ApiHelper.GetSign_VIP(url2);
        //            //
        //            string url2 = string.Format(
        //                "https://bangumi.bilibili.com/player/web_api/v2/playurl?cid={1}&appkey={0}&otype=json&type=&quality={2}&module=bangumi&season_type=1&qn={2}&ts={3}", ApiHelper._appKey_VIDEO,model.Mid, qn, GetTimeSpan_2);
        //            url2 += "&sign=" + ApiHelper.GetSign_VIDEO(url2);
        //            re = await WebClientClass.GetResults(new Uri(url2));
        //            FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
        //            if (m.code == 0 && !re.Contains("8986943"))
        //            {
        //                foreach (var item in m.durl)
        //                {
        //                    playList.Append(item.url, item.size, item.length / 1000);
        //                }
                        
        //            }
        //            else
        //            {
        //                var re3 = await WebClientClass.GetResults(new Uri(string.Format("http://120.92.50.146/23moe/api/v1/BiliAnimeUrl?animeid={0}&cid={1}&epid={2}&rnd={3}",model.banId, model.Mid, model.banInfo.episode_id,GetTimeSpan)));
        //                JObject obj = JObject.Parse(re3);
        //                if (Convert.ToInt32(obj["code"].ToString()) == 0)
        //                {
        //                    return new ReturnPlayModel()
        //                    {
        //                        usePlayMode = UsePlayMode.System,
        //                        url = obj["data"][0]["url"].ToString()
        //                    };
        //                }


        //                var playurl = await _5DMHelper.GetUrl(model.banId,Convert.ToInt32( model.No)-1);

        //                if (playurl=="")
        //                {
        //                    return null;
        //                }
        //                else{
        //                    return new ReturnPlayModel() {
        //                        usePlayMode = UsePlayMode.System,
        //                        url = playurl
        //                    };
        //                }
        //            }
                    
        //        }
        //        else
        //        {
        //            var mc = Regex.Matches(re, @"<length>(.*?)</length>.*?<size>(.*?)</size>.*?<url><!\[CDATA\[(.*?)\]\]></url>", RegexOptions.Singleline);

        //            foreach (Match item in mc)
        //            {
        //                playList.Append(item.Groups[3].Value, Convert.ToInt32(item.Groups[2].Value), Convert.ToInt64(item.Groups[1].Value) / 1000);
        //            }
                    
        //        }
        //        config.HttpReferer = "https://www.bilibili.com/bangumi/play/ep"+model.episode_id;
        //        //config.HttpReferer = "";
        //        //config.HttpCookie = "sid=aj08qul1; pgv_pvi=2726422528; fts=1498031012; rpdid=ikxqxlpildoplqwkwlqw; buvid3=0916A88B-F02E-46E2-95B1-BEF35678E1EE37229infoc; LIVE_BUVID=9b2dbd17fe02c6e0b9a5f7cbfe182be2; LIVE_BUVID__ckMd5=54bf74d417f1dfe6; OUTFOX_SEARCH_USER_ID_NCOO=301070296.3442316; uTZ=-480; biliMzIsnew=1; biliMzTs=0; UM_distinctid=16072fe8c3138-01d5f1e1fd27a6-5d4e211f-1fa400-16072fe8c32d6; _ga=GA1.2.701916902.1513903771; im_notify_type_7251681=0; BANGUMI_SS_21728_REC=164986; finger=81df3ec0; 21680=183802; 22843=173309; BANGUMI_SS_22843_REC=173309; BANGUMI_SS_21680_REC=183802; balh_server=https://biliplus.ipcjs.win; purl_token=bilibili_1518001366; balh_season_21680=1; pgv_si=s8821510144; DedeUserID=7251681; DedeUserID__ckMd5=e2742b2aff29c1cf; SESSDATA=83ace795%2C1520595578%2Ca2c615ce; bili_jct=a0f037944a8423a37efb566011d0a84b; _dfcaptcha=42d9ae3eecffafaf47b08effeef54128";
        //        playList.NetworkConfigs = config;
        //        //FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);
        //        // var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);



        //        //foreach (var item in m.durl)
        //        //{
        //        //    playList.Append(item.url, item.size, item.length / 1000);
        //        //}


        //        // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
        //        return new ReturnPlayModel()
        //        {
        //            playlist = playList,
        //            usePlayMode = UsePlayMode.SYEngine
        //        };
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //}
        //public static async Task<SYEngine.Playlist> GetVideoUrl_FFJ(PlayerModel model, int quality)
        //{
        //    try
        //    {
        //        model.index = Convert.ToInt32(model.No) - 1;
        //        string re = await WebClientClass.PostResults(new Uri("http://www.shokdown.com/parse.php"), "url=" + Uri.EscapeDataString("http://www.bilibili.com/video/av" + model.Aid + "/index_" + model.index + ".html"));

        //        var mc = Regex.Matches(re, "<br>.*?<br>", RegexOptions.Singleline);
        //        List<string> l1 = new List<string>();
        //        List<string> l2 = new List<string>();
        //        List<string> l3 = new List<string>();
        //        for (int i = 0; i < mc.Count; i++)
        //        {
        //            foreach (Match item in Regex.Matches(mc[i].Groups[0].Value, @"<a href=""(.*?)"" target=""_blank"" >(.*?)</a>", RegexOptions.Singleline))
        //            {
        //                if (i == 0)
        //                {
        //                    l1.Add(item.Groups[1].Value);
        //                }
        //                if (i == 1)
        //                {
        //                    l2.Add(item.Groups[1].Value);
        //                }
        //                if (i == 2)
        //                {
        //                    l3.Add(item.Groups[1].Value);
        //                }
        //            }
        //        }

        //        var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
        //        playList.Clear();
        //        int c = 0;
        //        if (quality == 1)
        //        {
        //            foreach (var item in l1)
        //            {
        //                playList.Append(item, 0, 0);
        //                c++;
        //            }

        //        }
        //        if (quality == 2)
        //        {
        //            foreach (var item in l2)
        //            {
        //                playList.Append(item, 0, 0);
        //                c++;
        //            }

        //        }
        //        if (quality == 3)
        //        {
        //            foreach (var item in l3)
        //            {
        //                playList.Append(item, 0, 0);
        //                c++;
        //            }

        //        }


        //        // re = await WebClientClass.GetResults_Phone(new Uri(url));

        //        SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
        //        config.DownloadRetryOnFail = true;
        //        config.HttpCookie = string.Empty;
        //        config.UniqueId = string.Empty;
        //        config.HttpReferer = "http://www.bilibili.com/";
        //        config.HttpUserAgent = string.Empty;

        //        playList.NetworkConfigs = config;

        //        // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
        //        return playList;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //}





        //private static async Task<string> GetSoHuPlayInfo(string mid, int quality)
        //{
        //    try
        //    {
        //        string[] str = mid.Split('|');
        //        //http://bangumi.bilibili.com/player/web_api/playurl?cid=10506396&module=movie&player=1&quality=4&ts=1475587467&sign=12b256ad5510d558d07ddf5c4430cd56
        //        // string url = string.Format("http://bangumi.bilibili.com/player/web_api/playurl?cid={0}&module=movie&player=1&quality=4&ts={1}&appkey={2}", mid,ApiHelper.GetTimeSpen,ApiHelper._appkey_DONTNOT);

        //        string url = string.Format("http://api.tv.sohu.com/v4/video/info/{0}.json?api_key=1820c56b9d16bbe3381766192e134811&uid=ad99774cfadfe5ecf12457ec5085359a&poid=1&plat=12&sver=3.7.0&partner=419&sysver=10.0.10586.318&ts={1}&verify=43026f88247fcbe0c56411624bd1531e&passport=&aid={2}&program_id=", str[1], ApiHelper.GetTimeSpan_2, str[0]);

        //        string results = await WebClientClass.GetResults(new Uri(url));
        //        SohuModel model = JsonConvert.DeserializeObject<SohuModel>(results);

        //        if (model.status == 200)
        //        {

        //            switch (quality)
        //            {
        //                case 1:
        //                    return model.data.url_nor + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
        //                case 2:
        //                    return model.data.url_super + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
        //                case 3:
        //                    return model.data.url_original + "&uid=1608111818273358&SOHUSVP=aaxZQgiYTy4uioObZPfLJCVK3BxYwluKsrZ-cpoyfEk&pt=1&prod=h5&pg=1&eye=0&cv=1.0.0&qd=68000&src=11050001&ca=4&cateCode=101&_c=1&appid=tv&oth=&cd=";
        //                default:
        //                    return "";
        //            }
        //        }
        //        else
        //        {
        //            return "";
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return "";

        //    }
        //}

        //public static async Task<string> GetBiliUrl(PlayerModel model, int quality)
        //{
        //    try
        //    {
        //        model.index = Convert.ToInt32(model.No) - 1;
        //        string re = await WebClientClass.PostResults(new Uri("http://www.shokdown.com/parse.php"), "url=" + Uri.EscapeDataString("http://www.bilibili.com/video/av" + model.Aid + "/index_" + model.index + ".html"));

        //        var mc = Regex.Matches(re, "<br>.*?<br>", RegexOptions.Singleline);
        //        List<string> l1 = new List<string>();
        //        List<string> l2 = new List<string>();
        //        List<string> l3 = new List<string>();
        //        for (int i = 0; i < mc.Count; i++)
        //        {
        //            foreach (Match item in Regex.Matches(mc[i].Groups[0].Value, @"<a href=""(.*?)"" target=""_blank"" >(.*?)</a>", RegexOptions.Singleline))
        //            {
        //                if (i == 0)
        //                {
        //                    l1.Add(item.Groups[1].Value);
        //                }
        //                if (i == 1)
        //                {
        //                    l2.Add(item.Groups[1].Value);
        //                }
        //                if (i == 2)
        //                {
        //                    l3.Add(item.Groups[1].Value);
        //                }
        //            }
        //        }


        //        if (quality == 1)
        //        {
        //            return l1[0];
        //        }
        //        else
        //        {
        //            return l2[0];
        //        }
        //        // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();

        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }


        //    // mediaElement.Source = new Uri(playUrl);
        //}
        //public static async Task<string> GetBiliUrl_Ban(PlayerModel model, int quality)
        //{
        //    try
        //    {

        //        model.index = Convert.ToInt32(model.No) - 1;
        //        //https://aikan-tv.com/tong.php?url=http://www.bilibili.com/video/av7400996/
        //        //
        //        string url = "http://api.bilibili.com/playurl?aid=" + model.Aid + "&page=" + model.index + "&platform=html5&quality=1&vtype=mp4&type=jsonp&token=8b30f5e253d2db6d91cf4afd4fcd3d7d";
        //        // url += "&sign=" + ApiHelper.GetSign(url);

        //        string re = "";
        //        if (SettingHelper.Get_UseCN() || SettingHelper.Get_UseHK() || SettingHelper.Get_UseTW())
        //        {
        //            re = await WebClientClass.GetResults_Proxy(url);
        //        }
        //        else
        //        {
        //            re = await WebClientClass.GetResults_Phone(new Uri(url));
        //        }
        //        JObject obj = JObject.Parse(re);

        //        //H5PlyaerUrlModel m = JsonConvert.DeserializeObject<H5PlyaerUrlModel>(re);
        //        // re = await WebClientClass.GetResults_Phone(new Uri(url));

        //        return obj["durl"]["0"]["url"].ToString();
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //    // mediaElement.Source = new Uri(playUrl);
        //}

        //public static async Task<string> GetBiliUrl_Ban(PlayerModel model, int quality)
        //{
        //    try
        //    {
        //        //var qn = 112;
        //        //switch (quality)
        //        //{
        //        //    case 1:
        //        //        qn = 16;
        //        //        break;
        //        //    case 2:
        //        //        qn = 48;
        //        //        break;
        //        //    case 3:
        //        //        qn = 48;
        //        //        break;
        //        //    default:
        //        //        break;
        //        //}

        //        string url = string.Format("http://bangumi.bilibili.com/player/web_api/playurl?cid={0}&module=bangumi&player=1&otype=json&quality={1}&ts={2}", model.Mid, quality, GetTimeSpan_2);
        //        url += "&sign=" + ApiHelper.GetSign_VIP(url);



        //        string re = await WebClientClass.GetResults(new Uri(url));


        //        FlvPlyaerUrlModel m = JsonConvert.DeserializeObject<FlvPlyaerUrlModel>(re);


        //        // mediaPlay.Source = await playList.SaveAndGetFileUriAsync();
        //        return m.durl[0].url;
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //    // mediaElement.Source = new Uri(playUrl);
        //}







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
    }


    public class RegionModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<RegionModel> data { get; set; }

        public int tid { get; set; }
        public int reid { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
        public string _goto{ get; set; }
        public string param { get; set; }
        public string uri { get; set; }
        public int type { get; set; }
        public List<RegionModel> children { get; set; }
        public int is_bangumi { get; set; }


    }

}
