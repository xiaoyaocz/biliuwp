﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Api
{
	public static class ApiUtils
	{
		public static ApiKeyInfo AndroidKey = new ApiKeyInfo("1d8b6e7d45233436", "560c52ccd288fed045859ed18bffd973");
		public static ApiKeyInfo AndroidTVKey = new ApiKeyInfo("4409e2ce8ffd12b8", "59b43e04ad6965f34319062b478f83dd");
		public static ApiKeyInfo AndroidVideoKey = new ApiKeyInfo("iVGUTjsxvpLeuDCf", "aHRmhWMLkdeMuILqORnYZocwMBpMEOdt");
		public static string baseUrl = "http://biliapi.iliili.cn";
		public static ApiKeyInfo WebVideoKey = new ApiKeyInfo("84956560bc028eb7", "94aba54af9065f71de72f5508f1cd42e");
		private const string _mobi_app = "android";
		private const string _platform = "android";
		private const string build = "5520400";

		/// <summary>
		/// 默认一些请求头
		/// </summary>
		/// <returns></returns>
		public static IDictionary<string, string> GetDefaultHeaders()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("user-agent", "Mozilla/5.0 BiliDroid/5.44.2 (bbcallen@gmail.com)");
			headers.Add("Referer", "https://www.bilibili.com/");
			return headers;
		}

		public static string GetSign(string url, ApiKeyInfo apiKeyInfo)
		{
			string result;
			string str = url.Substring(url.IndexOf("?", 4) + 1);
			List<string> list = str.Split('&').ToList();
			list.Sort();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string str1 in list)
			{
				stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
				stringBuilder.Append(str1);
			}
			stringBuilder.Append(apiKeyInfo.Secret);
			result = Utils.ToMD5(stringBuilder.ToString()).ToLower();
			return "&sign=" + result;
		}

		public static string GetSign(IDictionary<string, string> pars, ApiKeyInfo apiKeyInfo)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var item in pars.OrderBy(x => x.Key))
			{
				sb.Append(item.Key);
				sb.Append("=");
				sb.Append(item.Value);
				sb.Append("&");
			}
			var results = sb.ToString().TrimEnd('&');
			results = results + apiKeyInfo.Secret;
			return "&sign=" + Utils.ToMD5(results).ToLower();
		}

		/// <summary>
		/// 一些必要的参数
		/// </summary>
		/// <param name="needAccesskey">是否需要accesskey</param>
		/// <returns></returns>
		public static string MustParameter(ApiKeyInfo apikey, bool needAccesskey = false)
		{
			var url = "";
			var access_key = SettingHelper.Get_Access_key();
			if (needAccesskey && access_key != "")
			{
				url = $"access_key={access_key}&";
			}
			return url + $"appkey={apikey.Appkey}&build={build}&mobi_app={_mobi_app}&platform={_platform}&ts={Utils.GetTimestampS()}";
		}

		/// <summary>
		/// 发送请求，扩展方法
		/// </summary>
		/// <param name="api"></param>
		/// <returns></returns>
		public async static Task<HttpResults> Request(this ApiModel api)
		{
			if (api.method == HttpMethod.GET)
			{
				return await ApiRequest.Get(api.url, api.headers);
			}
			else
			{
				return await ApiRequest.Post(api.url, api.body, api.headers);
			}
		}
	}

	public class ApiModel
	{
		/// <summary>
		/// API地址
		/// </summary>
		public string baseUrl { get; set; }

		/// <summary>
		/// 发送内容体，用于POST方法
		/// </summary>
		public string body { get; set; }

		/// <summary>
		/// 请求cookie
		/// </summary>
		public IDictionary<string, string> cookies { get; set; }

		/// <summary>
		/// 请求头
		/// </summary>
		public IDictionary<string, string> headers { get; set; }

		/// <summary>
		/// 请求方法
		/// </summary>
		public HttpMethod method { get; set; }

		/// <summary>
		/// Url参数
		/// </summary>
		public string parameter { get; set; }

		/// <summary>
		/// 请求地址
		/// </summary>
		public string url
		{
			get
			{
				return baseUrl + "?" + parameter;
			}
		}
	}
}