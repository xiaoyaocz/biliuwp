using BiliBili.UWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BiliBili.UWP.Helper
{
    static class UserManage
    {
        public static event EventHandler Logined;
        public static string access_key { get; set; }
        public static string username { get; set; }
        public static bool IsLogin()
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
        public static async Task<string> LoginBilibili(string UserName, string Password)
        {
            try
            {
                //https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=JPJclVQpH4jwouRcSnngNnuPEq1S1rizxVJjLTg%2FtdqkKOizeIjS4CeRZsQg4%2F500Oye7IP4gWXhCRfHT6pDrboBNNkYywcrAhbOPtdx35ETcPfbjXNGSxteVDXw9Xq1ng0pcP1burNnAYtNRSayEKC1jiugi1LKyWbXpYE6VaM%3D&type=json&userid=xiaoyaocz&sign=74e4c872ec7b9d83d3a8a714e7e3b4b3
                //发送第一次请求，得到access_key

                string url = "https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=" + WebUtility.UrlEncode(await GetEncryptedPassword(Password)) + "&type=json&userid=" + WebUtility.UrlEncode(UserName);
                url += "&sign=" + ApiHelper.GetSign(url);

                string results = await WebClientClass.GetResults(new Uri(url));
                //Json解析及数据判断
                LoginModel model = new LoginModel();
                model = JsonConvert.DeserializeObject<LoginModel>(results);
                if (model.code == -627)
                {
                    return "登录失败，密码错误！";
                }
                if (model.code == -626)
                {
                    return "登录失败，账号不存在！";
                }
                if (model.code == -625)
                {
                    return "密码错误多次";
                }
                if (model.code == -628)
                {
                    return "未知错误";
                }
                if (model.code == -1)
                {
                    return "登录失败，程序注册失败！请联系作者！";
                }
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                if (model.code == 0)
                {
                    access_key = model.access_key;
                    Windows.Web.Http.HttpResponseMessage hr2 = await hc.GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + model.access_key + "&appkey=422fd9d7289a1dd9&platform=wp"));
                    hr2.EnsureSuccessStatusCode();

                    SettingHelper.Set_Access_key(model.access_key);
                }
                //看看存不存在Cookie
                HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

                List<string> ls = new List<string>();
                foreach (HttpCookie item in cookieCollection)
                {
                  
                    ls.Add(item.Name);
                }
                if (ls.Contains("DedeUserID"))
                {
                   
                    return "登录成功";
                }
                else
                {
                    return "登录失败";
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    return "登录失败，检查你的网络连接！";
                }
                else
                {
                    return "登录发生错误";
                }

            }

        }

        public static async Task<string> LoginSucess(string access_token)
        {

            Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
            access_key = access_token;
            Windows.Web.Http.HttpResponseMessage hr2 = await hc.GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + access_token + "&appkey=422fd9d7289a1dd9&platform=wp"));
            hr2.EnsureSuccessStatusCode();

            SettingHelper.Set_Access_key(access_token);
            //看看存不存在Cookie
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

            List<string> ls = new List<string>();
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name);
            }
            if (ls.Contains("DedeUserID"))
            {
                if (Logined != null)
                {
                    Logined(null, null);
                }
                return "登录成功";
            }
            else
            {
                return "登录失败";
            }

        }

        public static async Task<string> GetEncryptedPassword(string passWord)
        {
            string base64String;
            try
            {
                //https://secure.bilibili.com/login?act=getkey&rnd=4928
                //https://passport.bilibili.com/login?act=getkey&rnd=4928
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);
                //WebClientClass wc = new WebClientClass();
                string stringAsync = await httpClient.GetStringAsync((new Uri("https://passport.bilibili.com/login?act=getkey&rnd=" + new Random().Next(1000, 9999), UriKind.Absolute)));
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
        public static void Logout()
        {
            List<HttpCookie> listCookies = new List<HttpCookie>();
            listCookies.Add(new HttpCookie("sid", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("DedeUserID", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("DedeUserID__ckMd5", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("SESSDATA", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("LIVE_LOGIN_DATA", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("LIVE_LOGIN_DATA__ckMd5", ".bilibili.com", "/"));
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            foreach (HttpCookie cookie in listCookies)
            {
                filter.CookieManager.DeleteCookie(cookie);
            }
            ApiHelper.access_key = string.Empty;
            UserManage.access_key = string.Empty;
            SettingHelper.Set_Access_key(string.Empty);
            SettingHelper.Set_Refresh_Token(string.Empty);
            SettingHelper.Set_Password(string.Empty);
            SettingHelper.Set_UserID(0);
            SettingHelper.Set_LoginExpires(DateTime.Now);
            SettingHelper.Set_BiliplusCookie(string.Empty);
            SettingHelper.Set_UserIsVip(false);
        }


        public static string Uid
        {
            get
            {
                return SettingHelper.Get_UserID().ToString();
            }
        }

      


        //public static async void UpdateFollowList()
        //   {
        //       try
        //       {
        //           //
        //           string url = "http://space.bilibili.com/ajax/member/MyInfo?_=" + ApiHelper.GetTimeSpan_2;
        //           string results = await WebClientClass.GetResults(new Uri(url));

        //           UserInfoModel model = JsonConvert.DeserializeObject<UserInfoModel>(results);


        // ApiHelper.followList = model.data.attentions;
        //       }
        //       catch (Exception)
        //       {
        //          // return null;
        //       }

        //   }
        public static async Task<UserInfoModel> GetMyInfo()
        {

            try
            {
                //
                string url = string.Format("http://app.bilibili.com/x/v2/space?access_key={0}&appkey={1}&platform=wp&ps=10&ts={2}000&vmid={3}&build=5250000&mobi_app=android", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, Uid);
                //string url = string.Format("http://api.bilibili.com/userinfo?access_key={0}&appkey={1}&mid={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, uid);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));

                UserInfoModel model = JsonConvert.DeserializeObject<UserInfoModel>(results);

                return model.data.card;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
