﻿using BiliBili.UWP.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BiliBili.UWP.Api
{
	public enum HttpMethod
	{
		GET,
		POST
	}

	public static class ApiRequest
	{
		/// <summary>
		/// 发送一个GET请求
		/// </summary>
		/// <param name="url"></param>
		/// <param name="headers"></param>
		/// <param name="cookie"></param>
		/// <returns></returns>
		public async static Task<HttpResults> Get(string url, IDictionary<string, string> headers = null)
		{
			try
			{
				HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
				fiter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
				using (var client = new HttpClient(fiter))
				{
					if (headers != null)
					{
						foreach (var item in headers)
						{
							client.DefaultRequestHeaders.Add(item.Key, item.Value);
						}
					}

					var response = await client.GetAsync(new Uri(url));
					if (!response.IsSuccessStatusCode)
					{
						return new HttpResults()
						{
							code = (int)response.StatusCode,
							status = false,
							message = StatusCodeToMessage((int)response.StatusCode)
						};
					}
					response.EnsureSuccessStatusCode();
					HttpResults httpResults = new HttpResults()
					{
						code = (int)response.StatusCode,
						status = response.StatusCode == HttpStatusCode.Ok,
						results = await response.Content.ReadAsStringAsync(),
						message = StatusCodeToMessage((int)response.StatusCode)
					};
					return httpResults;
				}
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("GET请求失败" + url, LogType.ERROR, ex);
				return new HttpResults()
				{
					code = ex.HResult,
					status = false,
					message = "网络请求出现错误(GET)"
				};
			}
		}

		/// <summary>
		/// 发送一个GET请求
		/// </summary>
		/// <param name="url"></param>
		/// <param name="headers"></param>
		/// <param name="cookie"></param>
		/// <returns></returns>
		public async static Task<Stream> GetStream(string url, IDictionary<string, string> headers = null)
		{
			try
			{
				using (var client = new HttpClient())
				{
					if (headers != null)
					{
						foreach (var item in headers)
						{
							client.DefaultRequestHeaders.Add(item.Key, item.Value);
						}
					}

					var response = await client.GetAsync(new Uri(url));
					response.EnsureSuccessStatusCode();
					return (await response.Content.ReadAsInputStreamAsync()).AsStreamForRead();
				}
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("GET请求Stream失败" + url, LogType.ERROR, ex);
				return null;
			}
		}

		/// <summary>
		/// 发送一个GET请求
		/// </summary>
		/// <param name="url"></param>
		/// <param name="headers"></param>
		/// <param name="cookie"></param>
		/// <returns></returns>
		public async static Task<String> GetString(string url, IDictionary<string, string> headers = null, IDictionary<string, string> cookie = null)
		{
			try
			{
				using (var client = new HttpClient())
				{
					if (headers != null)
					{
						foreach (var item in headers)
						{
							client.DefaultRequestHeaders.Add(item.Key, item.Value);
						}
					}

					var response = await client.GetAsync(new Uri(url));
					response.EnsureSuccessStatusCode();
					return await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("GET请求String失败" + url, LogType.ERROR, ex);

				return null;
			}
		}

		/// <summary>
		/// 发送一个POST请求
		/// </summary>
		/// <param name="url"></param>
		/// <param name="body"></param>
		/// <param name="headers"></param>
		/// <param name="cookie"></param>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public async static Task<HttpResults> Post(string url, string body, IDictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
		{
			try
			{
				using (var client = new HttpClient())
				{
					if (headers != null)
					{
						foreach (var item in headers)
						{
							client.DefaultRequestHeaders.Add(item.Key, item.Value);
						}
					}
					var response = await client.PostAsync(new Uri(url), new HttpStringContent(body, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
					if (!response.IsSuccessStatusCode)
					{
						return new HttpResults()
						{
							code = (int)response.StatusCode,
							status = false,
							message = StatusCodeToMessage((int)response.StatusCode)
						};
					}
					string result = await response.Content.ReadAsStringAsync();
					HttpResults httpResults = new HttpResults()
					{
						code = (int)response.StatusCode,
						status = response.StatusCode == HttpStatusCode.Ok,
						results = result,
						message = StatusCodeToMessage((int)response.StatusCode)
					};
					return httpResults;
				}
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("POST请求失败" + url, LogType.ERROR, ex);
				return new HttpResults()
				{
					code = ex.HResult,
					status = false,
					message = "网络请求出现错误(POST)"
				};
			}
		}

		private static string StatusCodeToMessage(int code)
		{
			switch (code)
			{
				case 0:
				case 200:
					return "请求成功";

				case 504:
					return "请求超时了";

				case 301:
				case 302:
				case 303:
				case 305:
				case 306:
				case 400:
				case 401:
				case 402:
				case 403:
				case 404:
				case 500:
				case 501:
				case 502:
				case 503:
				case 505:
					return "网络请求失败，代码:" + code;

				case -2147012867:
				case -2147012889:
					return "请检查的网络连接";

				default:
					return "未知错误";
			}
		}
	}

	public class ApiDataModel<T>
	{
		private string _message;
		public int code { get; set; }
		public T data { get; set; }

		public string message
		{
			get
			{
				if (string.IsNullOrEmpty(_message))
				{
					return msg;
				}
				else
				{
					return _message;
				}
			}
			set { _message = value; }
		}

		public string msg { get; set; } = "";

		public bool success
		{
			get
			{
				return code == 0;
			}
		}
	}

	public class ApiResultModel<T>
	{
		private string _message;
		public int code { get; set; }

		public string message
		{
			get
			{
				if (string.IsNullOrEmpty(_message))
				{
					return msg;
				}
				else
				{
					return _message;
				}
			}
			set { _message = value; }
		}

		public string msg { get; set; } = "";

		public T result { get; set; }

		public bool success
		{
			get
			{
				return code == 0;
			}
		}
	}

	public class HttpResults
	{
		public int code { get; set; }
		public string message { get; set; }
		public string results { get; set; }
		public bool status { get; set; }

		public async Task<ApiDataModel<T>> GetData<T>()
		{
			try
			{
				return await GetJson<ApiDataModel<T>>();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public JObject GetJObject()
		{
			try
			{
				var obj = JObject.Parse(results);
				return obj;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public async Task<T> GetJson<T>()
		{
			return await Task.Run<T>(() =>
			{
				return JsonConvert.DeserializeObject<T>(results ?? "");
			});
		}

		public async Task<ApiResultModel<T>> GetResult<T>()
		{
			try
			{
				return await GetJson<ApiResultModel<T>>();
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}