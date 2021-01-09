﻿namespace BiliBili.UWP.Api.Home
{
	public class RecommendAPI
	{
		public ApiModel Dislike(string _goto, string id, string mid, int reason_id, int rid, int tag_id)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.GET,
				baseUrl = $"https://app.biliapi.net/x/feed/dislike",
				parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&goto={_goto}&id={id}&mid={mid}&reason_id={reason_id}&rid={rid}&tag_id={tag_id}"
			};
			api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
			return api;
		}

		public ApiModel Feedback(string _goto, string id, string mid, int feedback_id, int rid, int tag_id)
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.GET,
				baseUrl = $"https://app.biliapi.net/x/feed/dislike",
				parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&goto={_goto}&id={id}&mid={mid}&feedback_id={feedback_id}&rid={rid}&tag_id={tag_id}"
			};
			api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
			return api;
		}

		public ApiModel Recommend(string idx = "0")
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.GET,
				baseUrl = $"https://app.bilibili.com/x/v2/feed/index",
				parameter = ApiUtils.MustParameter(ApiHelper.AndroidKey, true) + $"&flush=0&idx={idx}&login_event=2&network=wifi&open_event=&pull={(idx == "0").ToString().ToLower()}&qn=32&style=2"
			};
			api.parameter += ApiUtils.GetSign(api.parameter, ApiHelper.AndroidKey);
			return api;
		}
	}
}