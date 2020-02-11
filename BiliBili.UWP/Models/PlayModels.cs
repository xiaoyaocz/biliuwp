using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Models
{
    public class BiliVideoUriModel
    {
        public string format { get; set; }//视频类型

        public object durl { get; set; }//视频信息

        public string url { get; set; }//视频地址

        public List<string> backup_url { get; set; }//视频备份地址
    }
}
