using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api
{
    public class RankAPI
    {
        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="rid">分区ID</param>
        /// <param name="type">1=全站，2原创</param>
        /// <param name="day">1,3,7,30</param>
        /// <returns></returns>
        public ApiModel Rank(int rid, int type, int day = 1)
        {
            ApiModel api = new ApiModel()
            {
                method = HttpMethod.GET,
                baseUrl = $"https://api.bilibili.com/x/web-interface/ranking",
                parameter = $"rid={rid}&day={day}&type={type}&arc_type=0"
            };
            return api;
        }
    }
}
