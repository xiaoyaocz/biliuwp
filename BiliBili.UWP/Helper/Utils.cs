using BiliBili.UWP.Controls;
using BiliBili.UWP.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;
using Newtonsoft.Json;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;

namespace BiliBili.UWP
{
    public static class Utils
    {

        public static ReturnJObject ToDynamicJObject(this string json)
        {
            try
            {
                var obj = JObject.Parse(json);
                ReturnJObject returnJObject = new ReturnJObject()
                {
                    code= obj["code"].ToInt32(),
                    message = (obj["message"]==null)?"": obj["message"].ToString(),
                    msg= (obj["msg"] == null) ? "" : obj["msg"].ToString(),
                    json = obj
                };
                return returnJObject;
            }
            catch (Exception)
            {
                return new ReturnJObject() {
                    code=-999,
                    message="解析JSON失败",
                    msg= "解析JSON失败"
                };
            }
        }

        //public static ObservableCollection<T> ToList<T>(this List<object> ls)
        //{
        //    ObservableCollection<T> list = new ObservableCollection<T>();
        //    foreach (DynamicJObject item in ls)
        //    {
        //        list.Add(JsonConvert.DeserializeObject<T>(item.ToJsonString()));
        //    }
        //    return list;
        //}


        //public static Newtonsoft.Json.Linq.JArray ToJArray(this List<object> list)
        //{
        //    JArray jArray = new JArray();
        //    foreach (var item in list)
        //    {
        //        var str = (item as DynamicJObject).ToJsonString();
        //        jArray.Add(JToken.Parse(str));
        //    }
        //    return jArray;
        //}

        public static void ReadB(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            var read = 0;
            while (read < count)
            {
                var available = stream.Read(buffer, offset, count - read);
                if (available == 0)
                {
                   // throw new ObjectDisposedException(null);
                }
                read += available;
                offset += available;
            }
        }
        public static string RegexMatch(string input, string regular)
        {
            var data = Regex.Match(input, regular);
            if (data.Groups.Count >= 2 && data.Groups[1].Value != "")
            {
                return data.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }


        public static void SetClipboard(string content)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText(content);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
        }

        public async static Task<bool> ShowLoginDialog()
        {
            LoginDialog login = new LoginDialog();
            await login.ShowAsync();
            if (ApiHelper.IsLogin())
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 根据Epid取番剧ID
        /// </summary>
        /// <returns></returns>
        public async static Task<string> BangumiEpidToSid(string url)
        {
            try
            {
                if (!url.Contains("http"))
                {
                    url = "https://www.bilibili.com/bangumi/play/ep" + url;
                }

                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                var data = RegexMatch(re, @"ss(\d+)");
                if (data != "")
                {
                    return data;
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


        public static int ToInt32(this object obj)
        {

            if (int.TryParse(obj.ToString(), out var value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }
        public static Color ToColor(this string obj)
        {
            obj = obj.Replace("#", "");
            obj = Convert.ToInt32(obj).ToString("X2");

            Color color = new Color();
            if (obj.Length == 4)
            {
                obj = "00" + obj;
            }
            if (obj.Length == 6)
            {
                color.R = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = 255;
            }
            if (obj.Length == 8)
            {
                color.R = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return color;
        }
        public static Color ToColor2(this string obj)
        {
            obj = obj.Replace("#", "");

            Color color = new Color();
            if (obj.Length == 4)
            {
                obj = "00" + obj;
            }
            if (obj.Length == 6)
            {
                color.R = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = 255;
            }
            if (obj.Length == 8)
            {
                color.R = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return color;
        }
        public static string DecodeHTML(this string obj)
        {
            obj = System.Net.WebUtility.HtmlDecode(obj);

            return obj;
        }

        public static void ShowMessageToast(string message)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(2));
            ms.Show();
        }

        public static void ShowMessageToast(string message, int time)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromMilliseconds(time));
            ms.Show();
        }


        public static string ToW(this object par)
        {
            try
            {
                var num = Convert.ToDouble(par);
                if (num>=10000)
                {
                    return (num / (double)10000).ToString("0.00")+"万";
                }
                else
                {
                    return num.ToString("0");
                }
            }
            catch (Exception)
            {

                return "0";
            }
         



        }
        /// <summary>
        /// 将时间戳转为时间
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static DateTime TimestampToDatetime(long ts)
        {
            DateTime dtStart = new DateTime(1970, 1, 1, 8, 0, 0);
            long lTime = long.Parse(ts + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        /// <summary>
        /// 生成时间戳/秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimestampS()
        {
            return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalSeconds);
        }
        /// <summary>
        /// 生成时间戳/豪秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimestampMS()
        {
            return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalMilliseconds);
        }
        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToMD5(string input)
        {
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashed = provider.HashData(buffer);
            var result = CryptographicBuffer.EncodeToHexString(hashed);
            return result;
        }

        public static byte[] ToBE(this byte[] b)
        {
            if (BitConverter.IsLittleEndian)
            {
                return b.Reverse().ToArray();
            }
            else
            {
                return b;
            }
        }
        public static string HandelTimestamp(string ts)
        {
            if (ts.Length == 10)
            {
                ts += "0000000";
            }
            DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0);
            long lTime = long.Parse(ts);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dt = dtStart.Add(toNow).ToLocalTime();
            TimeSpan span = DateTime.Now.Date - dt.Date;
            if (span.TotalDays <= 0)
            {
                return "今天" + dt.ToString("HH:mm");
            }
            else if (span.TotalDays >= 1 && span.TotalDays < 2)
            {
                return "昨天" + dt.ToString("HH:mm");
            }
            else
            {
                return dt.ToString("MM-dd");
            }
        }

        public static async Task<T> DeserializeJson<T>(this string results)
        {
            return await Task.Run<T>(() =>
            {
                return JsonConvert.DeserializeObject<T>(results);
            });
        }


    }


    public sealed class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }


        public bool CanExecute(object parameter)
        {
            return true;
        }

      


        public void Execute(object parameter)
        {
            try
            {
                this.MyExecute(parameter);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
            }
        }


        public Action<Object> MyExecute { get; set; }




    }

}
