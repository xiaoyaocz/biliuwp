using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http.Filters;
using BiliBili.UWP.Modules.AccountModels;
using Newtonsoft.Json;
using Windows.Web.Http;
using BiliBili.UWP.Api.User;
using BiliBili.UWP.Api;

namespace BiliBili.UWP.Modules
{
    public class Account : IModules
    {
        readonly UserCenterAPI userCenterAPI;
        readonly LoginAPI loginAPI;
        string guid = "";
        public Account()
        {
            userCenterAPI = new UserCenterAPI();
            loginAPI = new LoginAPI();
            guid = Guid.NewGuid().ToString();
        }
        public static MyInfoModel myInfo;

        public async Task<ReturnModel> Follow(string uid)
        {
            try
            {
                var result = await userCenterAPI.Attention(uid, 1).Request();
                if (result.status)
                {
                    var data = await result.GetJson<ApiDataModel<JObject>>();

                    if (data.success)
                    {
                        return new ReturnModel()
                        {
                            success = true,
                            message = ""
                        };
                    }
                    else
                    {
                        return new ReturnModel()
                        {
                            success = false,
                            message = data.message
                        };
                    }
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = result.message
                    };
                }

            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }
        public async Task<ReturnModel> UnFollow(string uid)
        {
            try
            {
                var result = await userCenterAPI.Attention(uid,2).Request();
                if (result.status)
                {
                    var data = await result.GetJson<ApiDataModel<JObject>>();

                    if (data.success)
                    {
                        return new ReturnModel()
                        {
                            success = true,
                            message = ""
                        };
                    }
                    else
                    {
                        return new ReturnModel()
                        {
                            success = false,
                            message = data.message
                        };
                    }
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = result.message
                    };
                }

            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }

        private static async Task<string> EncryptedPassword(string passWord)
        {
            string base64String;
            try
            {
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);
                string url = "https://passport.bilibili.com/api/oauth2/getKey";
                string content = $"appkey={ApiHelper.AndroidKey.Appkey}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                content += "&sign=" + ApiHelper.GetSign(content);
                string stringAsync = await WebClientClass.PostResults(new Uri(url), content);
                JObject jObjects = JObject.Parse(stringAsync);
                string str = jObjects["data"]["hash"].ToString();
                string str1 = jObjects["data"]["key"].ToString();
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
                base64String = passWord;
            }
            return base64String;
        }
        /// <summary>
        /// 登录V3版本，由于Edge未能很好支持Webp格式图片，会出现无法显示拼图验证码问题
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captcha">验证码</param>
        /// <returns></returns>
        public async Task<LoginCallbackModel> LoginV3(string username, string password)
        {
            try
            {
                string url = "https://passport.bilibili.com/api/v3/oauth2/login";
                var pwd = Uri.EscapeDataString(await EncryptedPassword(password));

                string data = $"username={Uri.EscapeDataString(username)}&password={pwd}&gee_type=10&appkey={ApiHelper.AndroidKey.Appkey}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                data += "&sign=" + ApiHelper.GetSign(data);
                var results = await WebClientClass.PostResults(new Uri(url), data);
                var m = JsonConvert.DeserializeObject<AccountLoginModel>(results);
                if (m.code == 0)
                {
                    if (m.data.status == 0)
                    {
                        SettingHelper.Set_Access_key(m.data.token_info.access_token);
                        SettingHelper.Set_Refresh_Token(m.data.token_info.refresh_token);
                        SettingHelper.Set_LoginExpires(DateTime.Now.AddSeconds(m.data.token_info.expires_in));
                        SettingHelper.Set_UserID(m.data.token_info.mid);
                        //foreach (var item in m.data.sso)
                        //{
                        await SSO(m.data.token_info.access_token);
                        //}
                        MessageCenter.SendLogined();
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Success,
                            message = "登录成功"
                        };
                    }
                    if (m.data.status == 1)
                    {
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.NeedValidate,
                            message = "本次登录需要安全验证",
                            url = m.data.url
                        };
                    }

                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Fail,
                        message = m.message
                    };
                }
                else if (m.code == -105)
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.NeedCaptcha,
                        url = m.data.url,
                        message = "登录需要验证码"
                    };
                }
                else
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Fail,
                        message = m.message
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Error,
                    message = "登录出现小问题,请重试"
                };
            }

        }


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captcha">验证码</param>
        /// <returns></returns>
        public async Task<LoginCallbackModel> LoginV2(string username, string password, string captcha = null)
        {
            try
            {
                string url = "https://passport.bilibili.com/api/oauth2/login";
                string data = $"appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&password={Uri.EscapeDataString(await EncryptedPassword(password))}&platform=android&ts={ApiHelper.GetTimeSpan}&username={Uri.EscapeDataString(username)}";
                if (!string.IsNullOrEmpty(captcha))
                {
                    data += "&captcha=" + captcha;
                }
                data += "&sign=" + ApiHelper.GetSign(data);
                var results = await WebClientClass.PostResults(new Uri(url), data);
                var m = JsonConvert.DeserializeObject<AccountLoginModel>(results);
                if (m.code == 0)
                {

                    SettingHelper.Set_Access_key(m.data.access_token);
                    SettingHelper.Set_Refresh_Token(m.data.refresh_token);
                    SettingHelper.Set_LoginExpires(DateTime.Now.AddSeconds(m.data.expires_in));
                    SettingHelper.Set_UserID(m.data.mid);
                    //foreach (var item in m.data.sso)
                    //{
                    await SSO(m.data.access_token);
                    //}
                    MessageCenter.SendLogined();
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Success,
                        message = "登录成功"
                    };
                }
                else if (m.code == -2100)
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.NeedValidate,
                        url = m.url,
                        message = "登录需要验证"
                    };
                }
                else if (m.code == -105)
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.NeedCaptcha,
                        message = "登录需要验证码"
                    };
                }
                else
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Fail,
                        message = m.message
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Error,
                    message = "登录出现小问题,请重试"
                };
            }

        }

        public async Task SetLoginSuccess(string access_token, string mid)
        {
            SettingHelper.Set_Access_key(access_token);
            SettingHelper.Set_Refresh_Token(access_token);
            SettingHelper.Set_LoginExpires(DateTime.Now.AddSeconds(7200));
            SettingHelper.Set_UserID(long.Parse(mid));
            await SSO(access_token);
            MessageCenter.SendLogined();
        }

        /// <summary>
        /// SSO，将accesskey转为cookie
        /// </summary>
        /// <param name="domain">域</param>
        /// <param name="access_key">access token</param>
        /// <returns></returns>
        public async Task SSO(string access_key)
        {
            try
            {
                //var url = $"{domain}?access_key={access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                //url += "&sign=" + ApiHelper.GetSign(url);

                var url = $"https://passport.bilibili.com/api/login/sso?access_key={access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&gourl=https%3A%2F%2Faccount.bilibili.com%2Faccount%2Fhome&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                var content = await WebClientClass.GetResults(new Uri(url));
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="access_key">access token</param>
        /// <param name="refresh_token">access token</param>
        /// <returns></returns>
        public async Task<ReturnModel> RefreshToken(string access_key, string refresh_token)
        {
            try
            {
                var url = "https://passport.bilibili.com/api/oauth2/refreshToken";
                var data = $"access_token={access_key}&refresh_token={refresh_token}&appkey={ApiHelper.AndroidKey.Appkey}&ts={ApiHelper.GetTimeSpan}";
                data += "&sign=" + ApiHelper.GetSign(data);
                var content = await WebClientClass.PostResults(new Uri(url), data);
                var obj = JObject.Parse(content);
                if (obj["code"].ToInt32() == 0)
                {
                    var m = JsonConvert.DeserializeObject<Token_info>(obj["data"].ToString());
                    SettingHelper.Set_Access_key(m.access_token);
                    SettingHelper.Set_Refresh_Token(m.refresh_token);
                    SettingHelper.Set_LoginExpires(DateTime.Now.AddSeconds(m.expires_in));
                    SettingHelper.Set_UserID(m.mid);
                    List<string> sso = new List<string>() {
                        "https://passport.bilibili.com/api/v2/sso",
                        "https://passport.biligame.com/api/v2/sso",
                        "https://passport.im9.com/api/v2/sso"
                    };

                    //foreach (var item in sso)
                    //{

                    //}
                    await SSO(m.access_token);
                    MessageCenter.SendLogined();
                    return new ReturnModel()
                    {
                        success = true,
                        message = "刷新成功"
                    };
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = "刷新Token失败,请重新登录"
                    };
                }
            }
            catch (Exception)
            {

                return new ReturnModel()
                {
                    success = false,
                    message = "刷新Token失败，请重新登录"
                };
            }
        }

        /// <summary>
        /// 检查登录状态
        /// </summary>
        /// <param name="access_key"></param>
        /// <returns></returns>
        public async Task<ReturnModel> CheckLoginState(string access_key)
        {
            try
            {
                var url = $"https://passport.bilibili.com/api/oauth2/info?access_token={access_key}&appkey={ApiHelper.AndroidKey.Appkey}&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var content = await WebClientClass.GetResults(new Uri(url));
                var obj = JObject.Parse(content);
                if (obj["code"].ToInt32() == 0)
                {
                    return new ReturnModel()
                    {
                        success = true,
                        message = "检查状态成功"
                    };
                }
                else
                {
                    return new ReturnModel()
                    {
                        success = false,
                        message = "检查状态失败"
                    };
                }
            }
            catch (Exception)
            {
                return new ReturnModel()
                {
                    success = true,
                    message = "检查状态失败"
                };
            }
        }

        /// <summary>
        /// 安全验证后保存状态
        /// </summary>
        /// <param name="access_key"></param>
        /// <param name="refresh_token"></param>
        /// <param name="expires"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<ReturnModel> CheckAgainLogin(string access_key, string refresh_token, int expires, long userid)
        {
            try
            {
                SettingHelper.Set_Access_key(access_key);
                SettingHelper.Set_Refresh_Token(refresh_token);
                SettingHelper.Set_LoginExpires(DateTime.Now.AddSeconds(expires));
                SettingHelper.Set_UserID(userid);
                List<string> sso = new List<string>() {
                        "https://passport.bilibili.com/api/v2/sso",
                        "https://passport.biligame.com/api/v2/sso",
                        "https://passport.im9.com/api/v2/sso"
                    };

                //foreach (var item in sso)
                //{
                await SSO(access_key);
                //}
                MessageCenter.SendLogined();
                return new ReturnModel()
                {
                    success = true,
                    message = "登录成功"
                };

            }
            catch (Exception ex)
            {

                return new ReturnModel()
                {
                    success = false,
                    message = "登录失败"
                };
            }
        }
        /// <summary>
        /// 读取我的信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<MyInfoModel>> GetMyInfo()
        {
            try
            {
                var url = $"https://app.bilibili.com/x/v2/account/myinfo?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);
                var str = await WebClientClass.GetResults(new Uri(url));
                var m = str.ToDynamicJObject();
                if (m.code == 0)
                {
                    var data = JsonConvert.DeserializeObject<MyInfoModel>(m.json["data"].ToString());
                    myInfo = data;

                    return new ReturnModel<MyInfoModel>()
                    {
                        success = true,
                        data = data
                    };
                }
                else
                {
                    return new ReturnModel<MyInfoModel>()
                    {
                        success = false,
                        message = m.message
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<MyInfoModel>(ex);
            }
        }

        /// <summary>
        /// 授权Biliplus
        /// </summary>
        /// <returns></returns>
        public static async Task<string> AuthBiliPlus()
        {
            try
            {
                if (!ApiHelper.IsLogin())
                {
                    return "";
                }
                var url = new Uri($"https://www.biliplus.com/login?act=savekey&mid={SettingHelper.Get_UserID()}&access_key={ApiHelper.access_key}&expire=");
                using (HttpClient httpClient = new HttpClient())
                {
                    var rq = await httpClient.GetAsync(url);
                    var setCookie = rq.Headers["set-cookie"];
                    StringBuilder stringBuilder = new StringBuilder();
                    var matches = Regex.Matches(setCookie, "(.*?)=(.*?); ", RegexOptions.Singleline);
                    foreach (Match match in matches)
                    {
                        var key = match.Groups[1].Value.Replace("HttpOnly, ", "");
                        var value = match.Groups[2].Value;
                        if (key != "expires" && key != "Max-Age" && key != "path" && key != "domain")
                        {
                            stringBuilder.Append(match.Groups[0].Value.Replace("HttpOnly, ", ""));
                        }
                    }
                    SettingHelper.Set_BiliplusCookie(stringBuilder.ToString());
                    return stringBuilder.ToString();
                }
            }
            catch (Exception)
            {

                return "";
            }

        }
        /// <summary>
        /// 获取二维码登录信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<QRAuthInfo>> GetQRAuthInfo()
        {
            try
            {
                var result =await loginAPI.QRLoginAuthCode(guid).Request();
                if (result.status)
                {
                    var data =await result.GetData<QRAuthInfo>();
                    if (data.success)
                    {
                        return new ReturnModel<QRAuthInfo>()
                        {
                            success=true,
                            data= data.data
                        };
                    }
                    else
                    {
                        return new ReturnModel<QRAuthInfo>()
                        {
                            success = false,
                            message = data.message
                        };

                    }
                }
                else
                {
                    return new ReturnModel<QRAuthInfo>()
                    {
                        success = false,
                        message = result.message
                    };
                }
            }
            catch (Exception ex)
            {
                return HandelError<QRAuthInfo>(ex);
            }
        }
        /// <summary>
        /// 轮询二维码扫描信息
        /// </summary>
        /// <returns></returns>
        public async Task<LoginCallbackModel> PollQRAuthInfo(string auth_code)
        {
            try
            {
                var result = await loginAPI.QRLoginPoll(auth_code,guid).Request();
                if (result.status)
                {
                    var data = await result.GetData<Token_info>();
                    if (data.success)
                    {
                        SettingHelper.Set_Access_key(data.data.access_token);
                        SettingHelper.Set_Refresh_Token(data.data.refresh_token);
                        SettingHelper.Set_LoginExpires(DateTime.Now.AddSeconds(data.data.expires_in));
                        SettingHelper.Set_UserID(data.data.mid);
                        await SSO(data.data.access_token);
                        MessageCenter.SendLogined();
                        return new LoginCallbackModel() { 
                            status= LoginStatus.Success,
                            message= ""
                        };
                    }
                    else
                    {
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Fail,
                            message = data.message
                        };

                    }
                }
                else
                {
                    return new LoginCallbackModel() { 
                        status= LoginStatus.Fail,
                        message= result.message
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginCallbackModel() { 
                    status= LoginStatus.Fail,
                    message=ex.Message
                };
            }
        }
    }
    public enum LoginStatus
    {
        /// <summary>
        /// 登录成功
        /// </summary>
        Success,
        /// <summary>
        /// 登录失败
        /// </summary>
        Fail,
        /// <summary>
        /// 登录错误
        /// </summary>
        Error,
        /// <summary>
        /// 登录需要验证码
        /// </summary>
        NeedCaptcha,
        /// <summary>
        /// 需要安全认证
        /// </summary>
        NeedValidate
    }
    namespace AccountModels
    {
        public class Token_info
        {
            /// <summary>
            /// Mid
            /// </summary>
            public long mid { get; set; }
            /// <summary>
            /// ac4dd9f599aeccd54e25f01ef1b222cc
            /// </summary>
            public string access_token { get; set; }
            /// <summary>
            /// 9f6632f1d5e0e2cd2373b488546d71da
            /// </summary>
            public string refresh_token { get; set; }
            /// <summary>
            /// Expires_in
            /// </summary>
            public int expires_in { get; set; }
        }

        public class Cookies
        {
            /// <summary>
            /// bili_jct
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 94d8d5b4fa1223a32f236ccc2012ba17
            /// </summary>
            public string value { get; set; }
            /// <summary>
            /// Http_only
            /// </summary>
            public int http_only { get; set; }
            /// <summary>
            /// Expires
            /// </summary>
            public int expires { get; set; }
        }

        public class Cookie_info
        {
            /// <summary>
            /// Cookies
            /// </summary>
            public List<Cookies> cookies { get; set; }
            /// <summary>
            /// Domains
            /// </summary>
            public List<string> domains { get; set; }
        }

        public class LoginDataModel
        {
            /// <summary>
            /// Status
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// Token_info
            /// </summary>
            public Token_info token_info { get; set; }
            /// <summary>
            /// Cookie_info
            /// </summary>
            public Cookie_info cookie_info { get; set; }
            /// <summary>
            /// Sso
            /// </summary>
            public List<string> sso { get; set; }

            public string url { get; set; }

            /// <summary>
            /// Mid
            /// </summary>
            public long mid { get; set; }
            /// <summary>
            /// ac4dd9f599aeccd54e25f01ef1b222cc
            /// </summary>
            public string access_token { get; set; }
            /// <summary>
            /// 9f6632f1d5e0e2cd2373b488546d71da
            /// </summary>
            public string refresh_token { get; set; }
            /// <summary>
            /// Expires_in
            /// </summary>
            public int expires_in { get; set; }

        }

        public class AccountLoginModel
        {
            /// <summary>
            /// Ts
            /// </summary>
            public int ts { get; set; }
            /// <summary>
            /// Code
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// Data
            /// </summary>
            public LoginDataModel data { get; set; }

            public string url { get; set; }
            public string message { get; set; }
        }


        public class LoginCallbackModel
        {
            public LoginStatus status { get; set; }
            public string message { get; set; }
            public string url { get; set; }
        }



        public class Vip
        {
            /// <summary>
            /// Type
            /// </summary>
            public int type { get; set; }
            /// <summary>
            /// Status
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// Due_date
            /// </summary>
            public string due_date { get; set; }
        }

        public class Official
        {
            /// <summary>
            /// Role
            /// </summary>
            public int role { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string desc { get; set; }
        }

        public class MyInfoModel
        {
            /// <summary>
            /// Mid
            /// </summary>
            public int mid { get; set; }
            /// <summary>
            /// xiaoyaocz
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 死宅，半个程序猿.....
            /// </summary>
            public string sign { get; set; }
            /// <summary>
            /// Coins
            /// </summary>
            public string coins { get; set; }
            /// <summary>
            /// 1997-09-21
            /// </summary>
            public DateTime birthday { get; set; }
            /// <summary>
            /// http://i1.hdslb.com/bfs/face/3e323499026ad0019be48dcd76f8e03199bd606c.jpg
            /// </summary>
            private string _face;

            public string face
            {
                get
                {
                    return _face + "@100w.jpg";
                }
                set { _face = value; }
            }

            /// <summary>
            /// Sex
            /// </summary>
            public int sex { get; set; }
            /// <summary>
            /// Level
            /// </summary>
            public int level { get; set; }
            /// <summary>
            /// Rank
            /// </summary>
            public int rank { get; set; }
            /// <summary>
            /// Silence
            /// </summary>
            public int silence { get; set; }
            /// <summary>
            /// Vip
            /// </summary>
            public Vip vip { get; set; }
            /// <summary>
            /// Email_status
            /// </summary>
            public int email_status { get; set; }
            /// <summary>
            /// Tel_status
            /// </summary>
            public int tel_status { get; set; }
            /// <summary>
            /// Official
            /// </summary>
            public Official official { get; set; }


            public string Sex
            {
                get
                {
                    switch (sex)
                    {
                        case 0:
                            return "保密";
                        case 1:
                            return "男";
                        case 2:
                            return "女";
                        default:
                            return "保密";
                    }
                }
            }
        }


        public class QRAuthInfo
        {
            public string url { get; set; }
            public string auth_code { get; set; }
        }
      
    }
}
