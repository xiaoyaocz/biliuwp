﻿namespace BiliBili.UWP.Api
{
	public class VideoAPI
	{
		/// <summary>
		/// 关注
		/// </summary>
		/// <param name="mid">用户ID</param>
		/// <param name="mode">1为关注，2为取消关注</param>
		/// <returns></returns>
		public ApiModel Attention(string mid, string mode)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.POST,
				baseUrl = $"https://api.bilibili.com/x/relation/modify",
				body = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&act={mode}&fid={mid}&re_src=32"
			};
			api.body += ApiUtils.GetSign(api.body, ApiHelper.AndroidKey);
			return api;
		}

		public ApiModel Coin(string aid, int num)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.POST,
				baseUrl = $"https://app.biliapi.net/x/v2/view/coin/add",
				body = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&multiply={num}&platform=android&select_like=0"
			};
			api.body += ApiUtils.GetSign(api.body, ApiHelper.AndroidKey);
			return api;
		}

		public ApiModel Detail(string id, bool isbvid)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.GET,
				baseUrl = $"https://app.bilibili.com/x/v2/view",
				parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&{(isbvid ? "bvid=" : "aid=")}{id}&plat=0"
			};
			api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
			return api;
		}

		/// <summary>
		///点赞
		/// </summary>
		/// <param name="dislike"> 当前dislike状态</param>
		/// <param name="like">当前like状态</param>
		/// <returns></returns>
		public ApiModel Dislike(string aid, int dislike, int like)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.POST,
				baseUrl = $"https://app.biliapi.net/x/v2/view/dislike",
				body = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&dislike={dislike}&from=7&like={like}"
			};
			api.body += ApiUtils.GetSign(api.body, ApiHelper.AndroidKey);
			return api;
		}

		/// <summary>
		///点赞
		/// </summary>
		/// <param name="dislike"> 当前dislike状态</param>
		/// <param name="like">当前like状态</param>
		/// <returns></returns>
		public ApiModel Like(string aid, int dislike, int like)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.POST,
				baseUrl = $"https://app.bilibili.com/x/v2/view/like",
				body = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&dislike={dislike}&from=7&like={like}"
			};
			api.body += ApiUtils.GetSign(api.body, ApiHelper.AndroidKey);
			return api;
		}

		/// <summary>
		///一键三连
		/// </summary>
		/// <returns></returns>
		public ApiModel Triple(string aid)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.POST,
				baseUrl = $"https://app.bilibili.com/x/v2/view/like/triple",
				body = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}"
			};
			api.body += ApiUtils.GetSign(api.body, ApiHelper.AndroidKey);
			return api;
		}
	}
}