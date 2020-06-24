using BiliBili.UWP.Controls;
using BiliBili.UWP.Models;
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
using BiliBili.UWP.Helper;

namespace BiliBili.UWP
{
    enum LoginStatus
    {
        Succeed,
        Failure,
        Error
    }
    public class ApiHelper
    {
        //九幽反馈
        public const string JyAppkey = @"afaaf76fbe62a275d4dc309d6151d3c3";
        //public static ApiKeyInfo AndroidKey = new ApiKeyInfo("1d8b6e7d45233436", "560c52ccd288fed045859ed18bffd973");
        public static ApiKeyInfo AndroidKey = new ApiKeyInfo("4409e2ce8ffd12b8", "59b43e04ad6965f34319062b478f83dd");
        public static ApiKeyInfo AndroidVideoKey = new ApiKeyInfo("iVGUTjsxvpLeuDCf", "aHRmhWMLkdeMuILqORnYZocwMBpMEOdt");
        public static ApiKeyInfo WebVideoKey = new ApiKeyInfo("84956560bc028eb7", "94aba54af9065f71de72f5508f1cd42e");
        public static ApiKeyInfo VideoKey = new ApiKeyInfo("", "1c15888dc316e05a15fdd0a02ed6584f");
        public static ApiKeyInfo IosKey = new ApiKeyInfo("4ebafd7c4951b366", "8cb98205e9b2ad3669aad0fce12a4c13");


        public const string build = "5520400";

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


        public static string GetSign(string url, ApiKeyInfo apiKeyInfo= null)
        {
            if (apiKeyInfo==null)
            {
                apiKeyInfo = ApiHelper.AndroidKey;
            }
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
            stringBuilder.Append(apiKeyInfo.Secret);
            result = Utils.ToMD5(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static string GetSignWithUrl(string url, ApiKeyInfo apiKeyInfo)
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
            stringBuilder.Append(apiKeyInfo.Secret);
            result = Utils.ToMD5(stringBuilder.ToString()).ToLower();
            return url+="&sign="+result;
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
      

      
        public static long GetTimeSpan
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalSeconds); }
        }
        public static long GetTimeSpan_2
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalMilliseconds); }
        }

        public static List<RegionModel> regions;
        public static async Task SetRegions()
        {
            try
            {
                string url = string.Format("https://app.bilibili.com/x/v2/region/index?appkey={0}&build={2}&mobi_app=android&platform=android&ts={1}", ApiHelper.AndroidKey.Appkey,GetTimeSpan,ApiHelper.build);
                url += "&sign=" + ApiHelper.GetSign(url);

                string results = await WebClientClass.GetResults(new Uri(url));
                RegionModel model = JsonConvert.DeserializeObject<RegionModel>(results);
                if (model.code==0)
                {
                    model.data.RemoveAll(x =>(x.name == "会员购" || x.name == "游戏中心")|| x.logo==""||x.name=="漫画" || x.name.Contains("赛事") || x.name.Contains("课堂"));
                
                    regions = model.data;

                   // model.data.ForEach(x => x.emojis.ForEach(y => emojis.Add(y)));
                }
            }
            catch (Exception)
            {
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

    }


    public class ApiKeyInfo
    {
        public ApiKeyInfo(string key,string secret)
        {
            Appkey = key;
            Secret = secret;
        }
        public string Appkey { get; set; }
        public string Secret { get; set; }
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
