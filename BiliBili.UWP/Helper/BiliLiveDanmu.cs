﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BiliBili.UWP.Helper
{
	/*
     使用流程
     1、初始化此类
     2、订阅HasDanmu事件
     3、调用Start方法开始
     4、
         */

	public class BiliLiveDanmu : IDisposable
	{
		public int delay = 100;

		private StreamSocket _clientSocket;

		private int _roomId;

		private bool _StartState = false;

		private DispatcherTimer _timer;

		public BiliLiveDanmu()
		{
		}

		public delegate void HasDanmuHandel(LiveDanmuModel value);

		public event HasDanmuHandel HasDanmu;

		public enum LiveDanmuTypes
		{
			//观众
			Viewer,

			//弹幕
			Danmu,

			//礼物
			Gift,

			//欢迎
			Welcome,

			//系统信息
			SystemMsg
		}

		public void Dispose()
		{
			_StartState = false;
			if (_timer != null)
			{
				_timer.Stop();
				_timer = null;
			}
			if (_clientSocket != null)
			{
				_clientSocket.Dispose();
				_clientSocket = null;
			}
		}

		//开始
		public async void Start(int roomid, long userId)
		{
			try
			{
				_roomId = roomid;

				HostName serverHost = new HostName(await GetDanmuServer());  //设置服务器IP

				_clientSocket = new StreamSocket();
				await _clientSocket.ConnectAsync(serverHost, "788");  //设置服务器端口号
				_StartState = true;
			}
			catch (Exception)
			{
				_StartState = false;
			}

			if (_StartState)
			{
				if (SendJoinChannel(roomid, userId))
				{
					SendHeartbeatAsync();
					_timer = new DispatcherTimer();
					_timer.Interval = new TimeSpan(0, 0, 20);
					_timer.Tick += Timer_Tick;
					_timer.Start();
					await Task.Run(() => { Listen(); });
				}
				else
				{
					throw new NotSupportedException("无法加入频道");
				}
			}
			else
			{
				throw new NotSupportedException("无法连接服务器");
			}
		}

		private async Task<string> GetDanmuServer()
		{
			try
			{
				var chat = "http://live.bilibili.com/api/player?id=cid:" + _roomId;
				string results = await WebClientClass.GetResults(new Uri(chat));
				var url = Regex.Match(results, "<server>(.*?)</server>", RegexOptions.Singleline).Groups[1].Value;
				if (url.Length != 0)
				{
					return url;
				}
				else
				{
					return "livecmt-2.bilibili.com";
				}
			}
			catch (Exception)
			{
				return "livecmt-2.bilibili.com";
			}
		}

		private async void Listen()
		{
			Stream _netStream = _clientSocket.InputStream.AsStreamForRead(1024);
			byte[] stableBuffer = new byte[1024];
			while (true)
			{
				if (!_StartState)
				{
					break;
				}
				try
				{
					_netStream.ReadB(stableBuffer, 0, 4);
					var packetlength = BitConverter.ToInt32(stableBuffer, 0);
					packetlength = IPAddress.NetworkToHostOrder(packetlength);

					if (packetlength < 16)
					{
						throw new NotSupportedException("协议失败: (L:" + packetlength + ")");
					}

					_netStream.ReadB(stableBuffer, 0, 2);//magic
					_netStream.ReadB(stableBuffer, 0, 2);//protocol_version

					_netStream.ReadB(stableBuffer, 0, 4);
					var typeId = BitConverter.ToInt32(stableBuffer, 0);
					typeId = IPAddress.NetworkToHostOrder(typeId);

					_netStream.ReadB(stableBuffer, 0, 4);//magic, params?

					var playloadlength = packetlength - 16;
					if (playloadlength == 0)
					{
						continue;//没有内容了
					}

					typeId = typeId - 1;

					var buffer = new byte[playloadlength];
					_netStream.ReadB(buffer, 0, playloadlength);
					if (typeId == 2)
					{
						var viewer = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0); //观众人数
						if (HasDanmu != null)
						{
							HasDanmu(new LiveDanmuModel() { type = LiveDanmuTypes.Viewer, viewer = Convert.ToInt32(viewer) });
						}
						Debug.WriteLine(viewer);
						continue;
					}
					var json_str = "";
					try
					{
						//临时解决方案，可以优化
						//参考https://github.com/Bililive/BililiveRecorder
						using (MemoryStream outBuffer = new MemoryStream())
						{
							using (System.IO.Compression.DeflateStream compressedzipStream = new System.IO.Compression.DeflateStream(new MemoryStream(buffer, 2, playloadlength - 2), System.IO.Compression.CompressionMode.Decompress))
							{
								byte[] block = new byte[1024];
								while (true)
								{
									int bytesRead = compressedzipStream.Read(block, 0, block.Length);
									if (bytesRead <= 0)
										break;
									else
										outBuffer.Write(block, 0, bytesRead);
								}
								compressedzipStream.Close();
								buffer = outBuffer.ToArray();
							}
						}
						json_str = Regex.Replace(Encoding.UTF8.GetString(buffer, 16, buffer.Length - 16), "}\\0\\0.*?\\0\\0{", "},{");
					}
					catch (Exception)
					{
						json_str = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
					}

					if (json_str.Trim().Length != 0)
					{
						json_str = "[" + json_str + "]";
						Debug.WriteLine(json_str);
						JArray json_array = JArray.Parse(json_str);
						foreach (var obj in json_array)
						{
							if (obj["cmd"].ToString().Contains("DANMU_MSG"))
							{
								var v = new DanmuMsgModel();
								if (obj["info"] != null && obj["info"].ToArray().Length != 0)
								{
									v.text = obj["info"][1].ToString();
									if (obj["info"][2] != null && obj["info"][2].ToArray().Length != 0)
									{
										v.username = obj["info"][2][1].ToString() + ":";

										//v.usernameColor = GetColor(obj["info"][2][0].ToString());
										if (obj["info"][2][3] != null && Convert.ToInt32(obj["info"][2][3].ToString()) == 1)
										{
											v.vip = "老爷";
											v.isVip = Visibility.Visible;
										}
										if (obj["info"][2][4] != null && Convert.ToInt32(obj["info"][2][4].ToString()) == 1)
										{
											v.vip = "年费老爷";
											v.isVip = Visibility.Collapsed;
											v.isBigVip = Visibility.Visible;
										}
										if (obj["info"][2][2] != null && Convert.ToInt32(obj["info"][2][2].ToString()) == 1)
										{
											v.vip = "房管";
											v.isAdmin = Visibility.Visible;
										}
									}
									if (obj["info"][3] != null && obj["info"][3].ToArray().Length != 0)
									{
										v.medal_name = obj["info"][3][1].ToString();
										v.medal_lv = obj["info"][3][0].ToString();
										v.medalColor = obj["info"][3][4].ToString();
										v.hasMedal = Visibility.Visible;
									}
									if (obj["info"][4] != null && obj["info"][4].ToArray().Length != 0)
									{
										v.ul = "UL" + obj["info"][4][0].ToString();
										v.ulColor = obj["info"][4][2].ToString();
									}
									if (obj["info"][5] != null && obj["info"][5].ToArray().Length != 0)
									{
										v.user_title = obj["info"][5][0].ToString();
										v.hasTitle = Visibility.Visible;
									}

									if (HasDanmu != null)
									{
										HasDanmu(new LiveDanmuModel() { type = LiveDanmuTypes.Danmu, value = v });
									}
								}
							}
							//19/10/01,cmd DANMU_MSG变成了DANMU_MSG:4:0:2:2:2:0
							switch (obj["cmd"].ToString())
							{
								//case "DANMU_MSG":
								//    break;
								case "SEND_GIFT":
									var g = new GiftMsgModel();
									if (obj["data"] != null)
									{
										g.uname = obj["data"]["uname"].ToString();
										g.action = obj["data"]["action"].ToString();
										g.giftId = Convert.ToInt32(obj["data"]["giftId"].ToString());
										g.giftName = obj["data"]["giftName"].ToString();
										g.num = obj["data"]["num"].ToString();
										g.uid = obj["data"]["uid"].ToString();
										if (HasDanmu != null)
										{
											HasDanmu(new LiveDanmuModel() { type = LiveDanmuTypes.Gift, value = g });
										}
									}

									break;

								case "WELCOME":
									var w = new WelcomeMsgModel();
									if (obj["data"] != null)
									{
										w.uname = obj["data"]["uname"].ToString();
										w.uid = obj["data"]["uid"].ToString();
										w.svip = obj["data"]["vip"].ToInt32() != 1;
										if (HasDanmu != null)
										{
											HasDanmu(new LiveDanmuModel() { type = LiveDanmuTypes.Welcome, value = w });
										}
									}
									break;

								case "SYS_MSG":
									if (obj["msg"] != null)
									{
										if (HasDanmu != null)
										{
											HasDanmu(new LiveDanmuModel() { type = LiveDanmuTypes.SystemMsg, value = obj["msg"].ToString() });
										}
									}

									break;

								default:

									break;
							}
							await Task.Delay(delay);
						}
					}

					// }
				}
				catch (Exception ex)
				{
					LogHelper.WriteLog("加载直播弹幕失败", LogType.ERROR, ex);
				}

				await Task.Delay(delay);
			}
		}

		///// <summary>
		/////十进制转SolidColorBrush
		///// </summary>
		///// <param name="_color">输入10进制颜色</param>
		///// <returns></returns>
		//public async Task<SolidColorBrush> GetColor(string _color)
		//{
		//    SolidColorBrush solid = new SolidColorBrush(new Color()
		//    {
		//        A = 255,
		//        R = 255,
		//        G = 255,
		//        B = 255
		//    });
		//    await Task.Run(() => {
		//        try
		//        {
		//            _color = Convert.ToInt32(_color).ToString("X2");
		//            if (_color.StartsWith("#"))
		//                _color = _color.Replace("#", string.Empty);
		//            int v = int.Parse(_color, System.Globalization.NumberStyles.HexNumber);
		//             solid = new SolidColorBrush(new Color()
		//            {
		//                A = Convert.ToByte(255),
		//                R = Convert.ToByte((v >> 16) & 255),
		//                G = Convert.ToByte((v >> 8) & 255),
		//                B = Convert.ToByte((v >> 0) & 255)
		//            });
		//            // color = solid;
		//            return solid;
		//        }
		//        catch (Exception)
		//        {
		//            return solid;
		//            // color = solid;

		// } });

		// return solid;

		private void SendHeartbeatAsync()
		{
			SendSocketData(2);
		}

		private bool SendJoinChannel(int channelId, long userId)
		{
			var r = new Random();

			long tmpuid = 0;
			if (userId == 0)
			{
				tmpuid = (long)(1e14 + 2e14 * r.NextDouble());
			}
			else
			{
				tmpuid = userId;
			}
			var packetModel = new { roomid = channelId, uid = tmpuid };
			var playload = JsonConvert.SerializeObject(packetModel);
			SendSocketData(7, playload);
			return true;
		}

		private void SendSocketData(int action, string body = "")
		{
			SendSocketData(0, 16, 1, action, 1, body);
		}

		private async void SendSocketData(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
		{
			try
			{
				var playload = Encoding.UTF8.GetBytes(body);
				if (packetlength == 0)
				{
					packetlength = playload.Length + 16;
				}
				var buffer = new byte[packetlength];
				using (var ms = new MemoryStream(buffer))
				{
					//Array.Reverse(a)
					var b = BitConverter.GetBytes(buffer.Length).ToArray().Reverse().ToArray();

					ms.Write(b, 0, 4);
					b = BitConverter.GetBytes(magic).ToArray().Reverse().ToArray();
					ms.Write(b, 0, 2);
					b = BitConverter.GetBytes(ver).ToArray().Reverse().ToArray();
					ms.Write(b, 0, 2);
					b = BitConverter.GetBytes(action).ToArray().Reverse().ToArray();
					ms.Write(b, 0, 4);
					b = BitConverter.GetBytes(param).ToArray().Reverse().ToArray();
					ms.Write(b, 0, 4);
					if (playload.Length > 0)
					{
						ms.Write(playload, 0, playload.Length);
					}
					DataWriter writer = new DataWriter(_clientSocket.OutputStream);  //实例化writer对象，以StreamSocket的输出流作为writer的方向
																					 // string content = "ABCDEFGH";  //发送一字符串
																					 //byte[] data = Encoding.UTF8.GetBytes(content);  //将字符串转换为字节类型，完全可以不用转换
					writer.WriteBytes(buffer);  //写入字节流，当然可以使用WriteString直接写入字符串
					await writer.StoreAsync();  //异步发送数据
					writer.DetachStream();  //分离
					writer.Dispose();  //结束writer

					// _netStream.WriteAsync(buffer, 0, buffer.Length); _netStream.FlushAsync();
				}
			}
			catch (Exception)
			{
			}
		}

		//}
		private void Timer_Tick(object sender, object e)
		{
			SendHeartbeatAsync();
		}

		public class DanmuMsgModel
		{
			public SolidColorBrush content_color { get; set; }
			public Visibility hasMedal { get; set; } = Visibility.Collapsed;
			public Visibility hasTitle { get; set; } = Visibility.Collapsed;
			public Visibility hasUL { get; set; } = Visibility.Visible;
			public Visibility isAdmin { get; set; } = Visibility.Collapsed;
			public Visibility isBigVip { get; set; } = Visibility.Collapsed;
			public Visibility isVip { get; set; } = Visibility.Collapsed;
			public SolidColorBrush medal_color { get; set; }
			public string medal_lv { get; set; }
			public string medal_name { get; set; }

			//勋章
			public string medalColor { get; set; }

			public string text { get; set; }

			//勋章颜色
			//勋章颜色
			public string titleImg
			{
				get
				{
					return Modules.LiveRoom.titleItems.FirstOrDefault(x => x.identification == user_title)?.web_pic_url;
				}
			}

			public string ul { get; set; }
			public SolidColorBrush ul_color { get; set; }

			//等级
			public string ulColor { get; set; }

			//勋章
			public SolidColorBrush uname_color { get; set; }

			public string user_title { get; set; }
			public string username { get; set; }//昵称
												// public SolidColorBrush usernameColor { get; set; }//昵称颜色

			//等级颜色
			//勋章颜色

			//头衔id（对应的是CSS名）

			public string vip { get; set; }
		}

		public class GiftMsgModel
		{
			public string action { get; set; }
			public int giftId { get; set; }
			public string giftName { get; set; }
			public string num { get; set; }
			public string uid { get; set; }
			public string uname { get; set; }
		}

		public class LiveDanmuModel
		{
			public LiveDanmuTypes type { get; set; }
			public object value { get; set; }
			public int viewer { get; set; }
		}

		public class WelcomeMsgModel
		{
			public string isadmin { get; set; }
			public bool svip { get; set; }
			public string uid { get; set; }
			public string uname { get; set; }
		}
	}
}