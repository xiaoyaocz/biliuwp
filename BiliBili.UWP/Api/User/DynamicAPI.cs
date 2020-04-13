using BiliBili.UWP.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api.User
{
    public class DynamicAPI
    {
        /// <summary>
        /// 发表图片动态
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="mode">1为关注，2为取消关注</param>
        /// <returns></returns>
        public ApiModel CreateDynamicPhoto(string imgs, string content,string at_uids, string at_control)
        {
           
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.POST,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create_draw",
                parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, true),
                body = $"uid={ApiHelper.GetUserId()}&category=3&pictures={Uri.EscapeDataString(imgs)}&description={Uri.EscapeDataString(content)}&content={Uri.EscapeDataString(content)}&setting=%7B%22copy_forbidden%22%3A0%7D&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&jumpfrom=110&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 发表文本动态
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="mode">1为关注，2为取消关注</param>
        /// <returns></returns>
        public ApiModel CreateDynamicText(string content, string at_uids, string at_control)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.POST,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create",
                parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, true),
                body = $"uid={ApiHelper.GetUserId()}&dynamic_id=0&type=4&content={Uri.EscapeDataString(content)}&setting=%7B%22copy_forbidden%22%3A0%7D&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&jumpfrom=110&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

    }
}
