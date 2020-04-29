using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api.Home
{
    public class CinemaAPI
    {
        public ApiModel CinemaHome()
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"{ApiUtils.baseUrl}/api/cinema/home"
            };
            return api;
        }
        public ApiModel CinemaFallMore(int wid, long cursor = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"{ApiUtils.baseUrl}/api/cinema/falls",
                parameter = $"wid={wid}&cursor={cursor}"
            };
            return api;
        }
    }
}
