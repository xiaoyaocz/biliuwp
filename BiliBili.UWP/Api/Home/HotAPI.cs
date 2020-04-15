using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api.Home
{
    public class HotAPI
    {
        public ApiModel Popular(string idx = "0", string last_param = "")
        {
            ApiModel api = new ApiModel()
            {
                method =  HttpMethod.GET,
                baseUrl = $"https://app.bilibili.com/x/v2/show/popular/index",
                parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&idx={idx}&last_param={last_param}"
            };
            api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
