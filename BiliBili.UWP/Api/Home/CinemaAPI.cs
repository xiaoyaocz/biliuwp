﻿namespace BiliBili.UWP.Api.Home
{
	public class CinemaAPI
	{
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

		public ApiModel CinemaHome()
		{
			ApiModel api = new ApiModel()
			{
				method = HttpMethod.GET,
				baseUrl = $"{ApiUtils.baseUrl}/api/cinema/home"
			};
			return api;
		}
	}
}