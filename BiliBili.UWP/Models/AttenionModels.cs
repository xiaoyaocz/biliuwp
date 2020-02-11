using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Models
{
    public class AttentionModel
    {
        //必须有登录Cookie
        //Josn：http://api.bilibili.com/x/feed/pull?jsonp=jsonp&ps=20&type=1&pn=1
        //第一层
        public int code { get; set; }//状态，0为正常
        public string message { get; set; }//状态，0为正常
        public object data { get; set; }//数据，包含第二层

        public List<AttentionModel> feeds { get; set; }

        public AttentionModel page { get; set; }
        public int count { get; set; }
        public int num { get; set; }
        public int size { get; set; }


        public string id { get; set; }//视频ID
        public string src_id { get; set; }//作者信息，包含第四层
        public string add_id { get; set; }//视频信息，包含第四层
        public int type { get; set; }
        public string mcid { get; set; }
        public AttentionModel source { get; set; }
        public string mid { get; set; }
        public string uname { get; set; }
        public string sex { get; set; }
        public string avatar { get; set; }
        public string sign { get; set; }
        public AttentionModel new_ep { get; set; }
        public string av_id { get; set; }
        public string index { get; set; }

        public AttentionModel addition { get; set; }
        public string aid { get; set; }
        public string title { get; set; }//标题
        public string typename { get; set; }
        public int typeid { get; set; }

        public string play { get; set; }//弹幕数
        public string video_review { get; set; }//上传时间


        private string _pic;//封面
        public string pic {
            get { return _pic+ "@300w.jpg"; }
            set { _pic = value; }
        }


        public long ctime { get; set; }

        public AttentionModel content { get; set; }


        public string Create
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                //long lTime = long.Parse(ctime + "000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = TimeSpan.FromSeconds(ctime);
                DateTime dt = dtStart.Add(toNow).ToLocalTime();
                TimeSpan span = DateTime.Now - dt;
                if (span.TotalDays > 7)
                {
                    return dt.ToString("MM-dd");
                }
                else
                if (span.TotalDays > 1)
                {
                    return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
                }
                else
                if (span.TotalHours > 1)
                {
                    return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                }
                else
                if (span.TotalMinutes > 1)
                {
                    return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                }
                else
                if (span.TotalSeconds >= 1)
                {
                    return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                }
                else
                {
                    return "1秒前";
                }
            }
        }

    }
    //用户追番
    public class UserBangumiModel
    {
        //Josn：http://space.bilibili.com/ajax/Bangumi/getList?mid=XXX&pagesize=9999
        //第一层
        public bool status { get; set; }//状态
        public object data { get; set; }//数据，包含第二层
                                        //第二层
        public int count { get; set; }//总数量
        public object result { get; set; }//结果，包含第三层
                                          //第三层
        public string season_id { get; set; }//专题ID，重要！！！
        public string title { get; set; }//标题
        public int is_finish { get; set; }//是否完结，0为连载，1为完结
        public string favorites { get; set; }//有多少人关注
        public int newest_ep_index { get; set; }//最新话
        public int total_count { get; set; }//一共多少话
        public string NewOver
        {
            get
            {
                if (is_finish == 0)
                {
                    return "更新至第" + newest_ep_index + "话";
                }
                else
                {
                    return total_count + "话全";
                }
            }
        }
        public string cover { get; set; }//封面
        public string brief { get; set; }//简介
        public int pages { get; set; }
    }
}
