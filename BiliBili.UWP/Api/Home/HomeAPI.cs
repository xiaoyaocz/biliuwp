using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api.Home
{
    public class HomeAPI
    {
        public ApiModel Tab()
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://app.bilibili.com/x/resource/show/tab",
                parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, false)
            };
            api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel TabDetail(int tab_id)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://app.bilibili.com/x/feed/index/tab",
                parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, false)+ $"&id={tab_id}"
            };
            api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

    }
}
