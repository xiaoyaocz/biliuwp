using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiliBili.UWP.Modules
{
	public class Choices
	{
		public long cid { get; set; }
		public int is_default { get; set; }
		public int node_id { get; set; }
		public string option { get; set; }
	}

	public class Edges
	{
		/// <summary>
		/// 选项
		/// </summary>
		public List<Choices> choices { get; set; }

		public int show_time { get; set; }
		public int type { get; set; }
		public int version { get; set; }
	}

	/// <summary>
	/// 互动视频
	/// </summary>
	public class InteractionVideo : IModules
	{
		private string aid = "";
		private int graph_version = 467;

		public InteractionVideo(string avid, int? graph_version)
		{
			this.aid = avid;
			if (graph_version.HasValue)
			{
				this.graph_version = graph_version.Value;
			}
		}

		public async Task<NodeInfo> GetNodes(int nodeid = 0)
		{
			try
			{
				string url = $"https://api.bilibili.com/x/stein/nodeinfo?access_key={ ApiHelper.access_key}&aid={aid}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&delay=0&graph_version={graph_version}&mobi_app=android&node_id={nodeid}&platform=android&ts={ApiHelper.GetTimeSpan}";
				url += "&sign=" + ApiHelper.GetSign(url);
				var results = await WebClientClass.GetResults(new Uri(url));
				var obj = JObject.Parse(results);
				if (obj["code"].ToInt32() == 0)
				{
					var nodeInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<NodeInfo>(obj["data"].ToString());
					return nodeInfo;
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}

	public class NodeInfo
	{
		public Edges edges { get; set; }
		public int node_id { get; set; }

		/// <summary>
		/// 解锁的节点
		/// </summary>
		public List<StoryList> story_list { get; set; }

		public string title { get; set; }
	}

	public class StoryList
	{
		public long cid { get; set; }
		public int node_id { get; set; }
		public string title { get; set; }
	}
}