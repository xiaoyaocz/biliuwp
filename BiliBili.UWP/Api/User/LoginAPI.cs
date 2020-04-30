using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api.User
{
    public class LoginAPI
    {
        /// <summary>
        /// 二维码登录获取二维码及AuthCode
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel QRLoginAuthCode(string local_id)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.POST,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/auth_code",
                body = ApiUtils.MustParameter(ApiUtils.AndroidTVKey, false)+ $"&local_id={local_id}",
            };
            api.body += ApiUtils.GetSign(api.body, ApiUtils.AndroidTVKey);
            return api;
        }

        /// <summary>
        /// 二维码登录轮询
        /// </summary>
        /// <param name="auth_code"></param>
        /// <returns></returns>
        public ApiModel QRLoginPoll(string auth_code, string local_id)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.POST,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/poll",
                body = ApiUtils.MustParameter(ApiUtils.AndroidTVKey, false)+ $"&auth_code={auth_code}&guid={Guid.NewGuid().ToString()}&local_id={local_id}",
            };
            api.body += ApiUtils.GetSign(api.body, ApiUtils.AndroidTVKey);
            return api;
        }


    }
}
