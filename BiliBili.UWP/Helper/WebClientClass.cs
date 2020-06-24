using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Windows.Security.ExchangeActiveSyncProvisioning;


namespace BiliBili.UWP
{
    class WebClientClass
    {
        public static async Task<string> GetResults(Uri url)
        {
            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            using (HttpClient hc = new HttpClient(fiter))
            {
                if (url.AbsoluteUri.Contains("23moe"))
                {
                    var ts = ApiHelper.GetTimeSpan.ToString();
                    EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
                    hc.DefaultRequestHeaders.Add("client", "bilibili-uwp");
                    hc.DefaultRequestHeaders.Add("ts", ts);
                    hc.DefaultRequestHeaders.Add("appsign", Utils.ToMD5("biliUwpXycz0423" + ts + "bilibili-uwp" + SettingHelper.GetVersion() + "0BJSDAHDUAHGAI5D45ADS5" + deviceInfo.Id.ToString()));
                    hc.DefaultRequestHeaders.Add("version", SettingHelper.GetVersion());
                    hc.DefaultRequestHeaders.Add("device-id", deviceInfo.Id.ToString());
                }

                //hc.DefaultRequestHeaders.Add("user-agent", $"Mozilla/5.0 BiliDroid/6.1.0 (bbcallen@gmail.com)");
                //hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                var encodeResults = await hr.Content.ReadAsBufferAsync();
                string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);

                //string result = await response.Content.ReadAsStringAsync();
                return results;
            }
        }
        public static async Task<string> GetResults(Uri url,Dictionary<string,string> header)
        {
            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            using (HttpClient hc = new HttpClient(fiter))
            {
                foreach (var item in header)
                {
                    hc.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                var encodeResults = await hr.Content.ReadAsBufferAsync();
                string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);
                return results;
            }
        }


        public static async Task<string> GetResults_NoHeader(Uri url)
        {
            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            using (HttpClient hc = new HttpClient(fiter))
            {

                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                string results = await hr.Content.ReadAsStringAsync();
                return results;
            }

        }
        public static async Task<string> GetResults_DisableAutoRedirect(Uri url)
        {

            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            fiter.AllowAutoRedirect = false;
            using (HttpClient hc = new HttpClient(fiter))
            {

                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                string results = await hr.Content.ReadAsStringAsync();
                return results;
            }
        }

        public static async Task<string> GetResults_Proxy(string url)
        {
            string area = "cn";
            if (SettingHelper.Get_UseCN())
            {
                area = "cn";
            }
            if (SettingHelper.Get_UseHK())
            {
                area = "hk";
            }
            if (SettingHelper.Get_UseTW())
            {
                area = "tw";
            }

            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);

            using (HttpClient hc = new HttpClient(fiter))
            {
                //url
                Uri uri = new Uri(string.Format("http://52uwp.com/api/BiliBili?area={0}&url={1}", area, Uri.EscapeDataString(url)));
                HttpResponseMessage hr = await hc.GetAsync(uri);
                hr.EnsureSuccessStatusCode();
                string results = await hr.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(results);
                if ((int)obj["code"] == 0)
                {
                    return obj["message"].ToString();
                }
                else
                {
                    throw new NotSupportedException(obj["message"].ToString());
                }
            }
        }


        public static async Task<IBuffer> GetBuffer(Uri url)
        {
            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            //sid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToLower();
            //fiter.CookieManager.SetCookie(new HttpCookie("sid", "bilibili.com", "/") { Value = sid });
            using (HttpClient hc = new HttpClient(fiter))
            {
                HttpResponseMessage hr = await hc.GetAsync(url);

                hr.EnsureSuccessStatusCode();
                IBuffer results = await hr.Content.ReadAsBufferAsync();
                return results;
            }
        }


        static string sid = "";
        public static async Task<string> PostResults(Uri url, string PostContent)
        {
            try
            {
                HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
                fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);

                if (url.AbsoluteUri.Contains("oauth2/login")&& sid!="")
                {
                    fiter.CookieManager.SetCookie(new HttpCookie("sid", "bilibili.com", "/") { Value = sid });
                }
                using (HttpClient hc = new HttpClient(fiter))
                {
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    var response = await hc.PostAsync(url, new HttpStringContent(PostContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                 
                    var encodeResults = await response.Content.ReadAsBufferAsync();
                    string result = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static async Task<string> PostResultsJson(Uri url, string PostContent)
        {
            try
            {
                HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
                fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                using (HttpClient hc = new HttpClient(fiter))
                {
                  
                    var response = await hc.PostAsync(url, new HttpStringContent(PostContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static async Task<string> PostResultsUtf8(Uri url, string PostContent)
        {
            try
            {
                HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
                fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                using (HttpClient hc = new HttpClient(fiter))
                {
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    var response = await hc.PostAsync(url, new HttpStringContent(PostContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();

                    var encodeResults = await response.Content.ReadAsBufferAsync();
                    string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);

                    //string result = await response.Content.ReadAsStringAsync();
                    return results;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static async Task<string> PostResults(Uri url, string PostContent, string Referer)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Referer = new Uri(Referer);
                    var response = await hc.PostAsync(url, new HttpStringContent(PostContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static async Task<string> GetResults_Live(Uri url)
        {

            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            //  fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.InvalidName);
            // fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.UnknownCriticalExtension);
            // myClientHandler.ClientCertificateOptions = System.Net.Http.ClientCertificateOption.Automatic;
            //   myClientHandler.AllowAutoRedirect = true;
            //fiter.ServerCredential.
            using (HttpClient hc = new HttpClient(fiter))
            {

                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                string results = await hr.Content.ReadAsStringAsync();

                //HttpResponseMessage hr = await hc.GetAsync(url);
                //hr.EnsureSuccessStatusCode();
                //var encodeResults = await hr.Content.ReadAsBufferAsync();
                //string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);

                return results;
            }


        }

        public static async Task<string> PostResults(Uri url, string PostContent, string Referer, string Home)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Referer = new Uri(Referer);
                    hc.DefaultRequestHeaders.Host = new Windows.Networking.HostName(Home);
                    var response = await hc.PostAsync(url, new HttpStringContent(PostContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static async Task<string> PostResults(Uri url, StorageFile PostContent, string Referer, string Home)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Referer = new Uri(Referer);
                    hc.DefaultRequestHeaders.Host = new Windows.Networking.HostName(Home);
                    IBuffer buffer = await FileIO.ReadBufferAsync(PostContent);

                    var response = await hc.PostAsync(url, new HttpBufferContent(buffer));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static async Task<string> PostResults(Uri url, StorageFile PostContent)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {

                    IBuffer buffer = await FileIO.ReadBufferAsync(PostContent);

                    var response = await hc.PostAsync(url, new HttpBufferContent(buffer));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }


        public static async Task<string> PostResults(Uri url, IInputStream PostContent, string Referer)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    //hc.DefaultRequestHeaders.Add("Content-Disposition", @"form-data; name=""img_file""");
                    //hc.DefaultRequestHeaders.Add("Content-Type", " application/octet-stream");
                    //hc.DefaultRequestHeaders.Add("Content-Transfer-Encoding", " binary");
                    //hc.DefaultRequestHeaders.Host = new Windows.Networking.HostName(Home);
                    hc.DefaultRequestHeaders.Referer = new Uri(Referer);
                    var response = await hc.PostAsync(url, new HttpStreamContent(PostContent));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }


        public static async Task<string> PostResults(Uri url, Stream PostContent)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    //hc.DefaultRequestHeaders.Add("Content-Disposition", @"form-data; name=""img_file""");
                    //hc.DefaultRequestHeaders.Add("Content-Type", " application/octet-stream");
                    //hc.DefaultRequestHeaders.Add("Content-Type", "multipart/form-data;");
                    //hc.DefaultRequestHeaders.Add("Content-Length", PostContent.Length.ToString());
                    //hc.DefaultRequestHeaders.Host = new Windows.Networking.HostName(Home);
                    HttpMultipartFormDataContent httpMultipartFormDataContent = new HttpMultipartFormDataContent();
                    httpMultipartFormDataContent.Add(new HttpStreamContent(PostContent.AsInputStream()), "data");

                    //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                    //request.Content = httpMultipartFormDataContent;

                    HttpResponseMessage response = await hc.PostAsync(url, httpMultipartFormDataContent);
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }


        public static async Task<string> GetResultsUTF8Encode(Uri url)
        {
            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);

            using (HttpClient hc = new HttpClient(fiter))
            {
                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                var encodeResults = await hr.Content.ReadAsBufferAsync();
                string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);
                return results;
            }

        }

        public static async Task<string> GetResults_Phone(Uri url)
        {
            //HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            //HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            ////hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            //foreach (HttpCookie item in cookieCollection)
            //{
            //    if (item.Name == "buvid3" || item.Name == "fts")
            //    {
            //        hb.CookieManager.DeleteCookie(item);
            //        // _uid = item.Value;
            //    }
            //}

            HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
            fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
            using (HttpClient hc = new HttpClient(fiter))
            {

                // hc.DefaultRequestHeaders.Add("user-agent", "Bilibili Windows.Desktop Client/1.2.0.0 (atelier39@outlook.com)");
                hc.DefaultRequestHeaders.Add("Referer", "http://interface.bilibili.com/");
                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();


                string results = await hr.Content.ReadAsStringAsync();

                //HttpResponseMessage hr = await hc.GetAsync(url);
                //hr.EnsureSuccessStatusCode();
                //var encodeResults = await hr.Content.ReadAsBufferAsync();
                //string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);

                return results;
            }


        }

        public static async Task<IRandomAccessStream> GetImageStream(string url)
        {
            try
            {
                if (url == null || url == "")
                {
                    return null;
                }
                using (HttpClient hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Linux; Android 5.0; SM-N9100 Build/LRX21V) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/37.0.0.0 Mobile Safari/537.36 MicroMessenger/6.0.2.56_r958800.520 NetType/WIFI");
                    hc.DefaultRequestHeaders.Add("Referer", "http://www.dmzj.com/");
                    HttpResponseMessage hr = await hc.GetAsync(new Uri(url));
                    hr.EnsureSuccessStatusCode();
                    IBuffer info = await hr.Content.ReadAsBufferAsync();
                    //BitmapImage bmp = new BitmapImage();
                    InMemoryRandomAccessStream inStream = new InMemoryRandomAccessStream();
                    DataWriter datawriter = new DataWriter(inStream.GetOutputStreamAt(0));
                    datawriter.WriteBuffer(info, 0, info.Length);
                    await datawriter.StoreAsync();
                    //IRandomAccessStream readStream = info.
                    //bmp.SetSource(inStream);
                    return inStream;
                }
            }
            catch (Exception ex)
            {

                return null;
            }


        }

    }


}
