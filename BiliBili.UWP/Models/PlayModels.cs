using System.Collections.Generic;

namespace BiliBili.UWP.Models
{
	public class BiliVideoUriModel
	{
		public List<string> backup_url { get; set; }
		public object durl { get; set; }
		public string format { get; set; }//视频类型

		//视频信息

		public string url { get; set; }//视频地址

		//视频备份地址
	}
}