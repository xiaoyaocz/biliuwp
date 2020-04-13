using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BiliBili.UWP.Models
{
    //这个Model用来保存登录请求的access_key
    public class LoginModel
    {
        public string message { get; set; }
        public LoginModel data { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        private string _access_key;
        public string access_key
        {
            get { return _access_key; }
            set { _access_key = value; }
        }
        public string url { get; set; }
        public string mid { get; set; }
        public int code { get; set; }
        public string expires
        {
            get; set;
        }
    }
    public class MessageModel
    {
        public int code { get; set; }
        public object data { get; set; }
        public string message { get; set; }

        public int reply_me { get; set; }
        public int praise_me { get; set; }
        public int notify_me { get; set; }
        public int at_me { get; set; }
        public int chat_me { get; set; }
    }
    public class MessageReplyModel
    {
        public int code { get; set; }
        public object data { get; set; }
        public string message { get; set; }
        public string id { get; set; }
        public string cursor { get; set; }

        public string title { get; set; }

        public string Title
        {
            get
            {
                //#{【4月】迷家 04【独家正版】}{"http://www.bilibili.com/video/av4439268/"}评论中回复了你
                Match ban = Regex.Match(title, @"#{(.*?)}{""(.*?)""}");
                if (ban.Groups[1].Value.Length == 0)
                {
                    return title;
                }
                else
                {
                    string a = ban.Groups[1].Value + title.Replace(ban.Groups[0].Value, string.Empty);
                    link = ban.Groups[2].Value;
                    return a;
                }

            }
        }
        public string link { get; set; }
        public string content { get; set; }
        public string Content
        {
            get
            {
                string ban = Regex.Match(content, @"^#{(.*?)}{""(.*?)""}$").Groups[1].Value;
                if (ban.Length == 0)
                {
                    return content;
                }
                else
                {
                    if (link == null)
                    {
                        link = Regex.Match(content, @"^#{(.*?)}{""(.*?)""}$").Groups[2].Value;
                    }

                    return ban;
                }
            }

        }
        public string Content_Notiy
        {
            get
            {
                Match ban = Regex.Match(content, @"#{(.*?)}{""(.*?)""}");
                if (ban.Groups[1].Value.Length == 0)
                {
                    return content;
                }
                else
                {
                    string a = content.Replace(ban.Groups[0].Value, ban.Groups[1].Value);
                    link = ban.Groups[2].Value;
                    return a;
                }
            }
        }
        public object publisher { get; set; }
        public string name { get; set; }
        public string face { get; set; }
        public string mid { get; set; }
        public string time_at { get; set; }
    }
    public class MessageChatModel
    {
        public int code { get; set; }
        public object data { get; set; }
        public string message { get; set; }

        public string rid { get; set; }
        public string room_name { get; set; }
        public string avatar_url { get; set; }
        public int msg_count { get; set; }
        public string last_msg { get; set; }
        public long last_time { get; set; }

        public string Last_time
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(last_time + "0000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = new TimeSpan(lTime);
                DateTime dt = dtStart.Add(toNow).ToLocalTime();
                TimeSpan span = DateTime.Now - dt;
                if (span.TotalDays > 7)
                {
                    return dt.ToString("yyyy-MM-dd");
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
    //用户信息
    public class UserInfoModel
    {

        public int code { get; set; }
        public string message { get; set; }
        public UserInfoModel data { get; set; }
        public UserInfoModel card { get; set; }

        public UserInfoModel favourite { get; set; }
        public UserInfoModel season { get; set; }
        public UserInfoModel coin_archive { get; set; }
        public UserInfoModel live { get; set; }
        public List<UserInfoModel> item { get; set; }
        public string fid { get; set; }
        public int cur_count { get; set; }

        public int? relation { get; set; }

        public string param { get; set; }
        public string title { get; set; }

        public object cover { get; set; }

        public int liveStatus { get; set; }
        public string roomid { get; set; }

        public string play { get; set; }
        public string danmaku { get; set; }
        public long ctime { get; set; }
        public string Ctime {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(ctime + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow).ToString("d");
            }
        }


        public string mid { get; set; }//ID
        public string name { get; set; }//昵称
        public string sex { get; set; }//性别
        public string coins { get; set; }//硬币
        public string face { get; set; }//头像
        public bool approve { get; set; }
        public int? rank { get; set; }//用户级别
        //public GetLoginInfoModel data { get; set; }
        public string RankStr
        {
            get
            {
                switch (rank)
                {
                    case 0:
                        return "普通用户";
                    case 5000:
                        return "注册会员";
                    case 10000:
                        return "正式会员";
                    case 20000:
                        return "字幕君";
                    case 25000:
                        return "VIP用户";
                    case 30000:
                        return "职人";
                    case 32000:
                        return "站长大人";
                    default:
                        return "蜜汁等级";
                }
            }
        }

        public string birthday { get; set; }//生日
        public long regtime { get; set; }//注册时间
        public string Regtime
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(regtime + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow).ToString("d");
            }
        }//转换后注册时间

        public string sign { get; set; }//个性签名
        public int fans { get; set; }//粉丝
        public string attention { get; set; }//关注

        public UserInfoModel level_info { get; set; }//等级信息
        public int current_min { get; set; }
        public int current_exp { get; set; }
        public string next_exp { get; set; }

        public List<string> attentions { get; set; }
        //public string current_level_string { get { return "LV" + current_level; } }//等级
        public int current_level { get; set; }
        public string place { get; set; }//地址
        public string toutu { get; set; }

        public UserInfoModel vip { get; set; }
        public int vipType { get; set; }//1大会员
        public int vipStatus { get; set; }//1为
        public string vipDueDate { get; set; }//VIP过期时间
        public string accessStatus { get; set; }//0为正在使用
        public string vipSurplusMsec { get; set; }//VIP剩余毫秒？为毛用毫秒- -

        public UserInfoModel official_verify { get; set; }//认证
        public int type { get; set; }
        public string desc { get; set; }


        public string verify
        {
            get
            {
                if (official_verify != null)
                {
                    switch (official_verify.type)
                    {
                        case 0:
                            return "ms-appx:///Assets/MiniIcon/ic_authentication_personal.png";
                        case 1:
                            return "ms-appx:///Assets/MiniIcon/ic_authentication_organization.png";
                        default:
                            return "ms-appx:///Assets/MiniIcon/transparent.png";
                    }
                }
                else
                {
                    return "";
                }
            }
        }


    }
   
    public class UserInfoCard
    {

    }
  

    public class GetUserAttention
    {
        //Josn：http://space.bilibili.com/ajax/friend/GetAttentionList?mid=XXXX&pagesize=999
        //第一层
        public bool status { get; set; }//状态
        public object data { get; set; }//数据，包含第二层
                                        //第二层
        public object list { get; set; }//结果，包含第三层
                                        //第三层
        public string record_id { get; set; }//记录ID，重要！！！
        public string uname { get; set; }//昵称
        public string face { get; set; }//头像
        public string fid { get; set; }//FID
        public long addtime { get; set; }//记录时间
        public int pages { get; set; }

    }

    public class GetUserSubmit
    {
        //Josn：http://space.bilibili.com/ajax/friend/GetAttentionList?mid=XXXX&pagesize=999
        //第一层
        public bool status { get; set; }//状态
        public object data { get; set; }//数据，包含第二层
                                        //第二层
        public object vlist { get; set; }//结果，包含第三层
        public object list { get; set; }//结果，包含第三层
                                        //第三层
        public string aid { get; set; }//视频ID
        public string title { get; set; }//标题
        public string pic { get; set; }//图片
        public string Pic
        {
            get
            {

                return "http://" + pic;
            }
        }
        public string video_review { get; set; }//弹幕
        public string play { get; set; }//播放
        public long created { get; set; }//上传时间
        public string length { get; set; }//长度
        public string description { get; set; }
        public string uname { get; set; }
        public int count { get; set; }
        public int pages { get; set; }
        public string Created
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(created + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow).ToString();
            }
        }
    }


    public class UserModel
    {
        public bool status { get; set; }
        public string message { get; set; }

        public UserModel data { get; set; }
        public string mid { get; set; }//ID
        public string name { get; set; }//昵称
        public string uname { get; set; }//昵称
        public string sex { get; set; }//性别
        public string coins { get; set; }//硬币
        public string face { get; set; }//头像
        public bool approve { get; set; }
        public int? rank { get; set; }//用户级别
        //public GetLoginInfoModel data { get; set; }
        public string RankStr
        {
            get
            {
                switch (rank)
                {
                    case 0:
                        return "普通用户";
                    case 5000:
                        return "注册会员";
                    case 10000:
                        return "正式会员";
                    case 20000:
                        return "字幕君";
                    case 25000:
                        return "VIP用户";
                    case 30000:
                        return "职人";
                    case 32000:
                        return "站长大人";
                    default:
                        return "蜜汁等级";
                }
            }
        }

        public string birthday { get; set; }//生日
        public DateTime Birthday { get {
                return DateTime.Parse(birthday);
            } }//生日
        public long regtime { get; set; }//注册时间
        public string Regtime
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(regtime + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow).ToString("yyyy-MM-dd");
            }
        }//转换后注册时间

        public string sign { get; set; }//个性签名
        public int fans { get; set; }//粉丝
        public string attention { get; set; }//关注

        public UserModel level_info { get; set; }//等级信息
        public int current_min { get; set; }
        public int current_exp { get; set; }
        public string next_exp { get; set; }

        public List<string> attentions { get; set; }
        //public string current_level_string { get { return "LV" + current_level; } }//等级
        public int current_level { get; set; }
        public string place { get; set; }//地址
        public string toutu { get; set; }
        public string _toutu { get {
                return "http://i0.hdslb.com/" + toutu;
            } }
       

        public UserModel official_verify { get; set; }//认证
        public int type { get; set; }
        public string desc { get; set; }

    }

    public class GetUserFovBox
    {
        //Josn：http://space.bilibili.com/ajax/fav/getBoxList?mid=XXXXX
        //第一层
        public bool status { get; set; }//状态
        public object data { get; set; }//数据，包含第二层
                                        //第二层
        public object list { get; set; }//结果，包含第三层
                                        //第三层
        public string fav_box { get; set; }//收藏夹ID，重要！！！
        public int count { get; set; }//数量
        public string Count
        {
            get
            {
                return count + "个视频";
            }
        }
        public string name { get; set; }//标题
        public long ctime { get; set; }//未转换创建时间
        public int max_count { get; set; }//最大数量

    }
    public class GetUserFollow
    {
        public int code { get; set; }
        public string message { get; set; }
        public GetUserFollow data { get; set; }
        public List<GetUserFollow> list { get; set; }

        public string mid { get; set; }
        public int attribute { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
        public string sign { get; set; }
     
    }
    public class GetFavouriteBoxsVideoModel
    {
        //Josn：http://space.bilibili.com/ajax/fav/getList?mid=用户ID&pagesize=30&fid=收藏夹编号
        //第一层
        public bool status { get; set; }//标题
        public object data { get; set; }//包含第二层
                                        //第二层
        public int pages { get; set; }//页数
        public int count { get; set; }//数量
        public object vlist { get; set; }//包含第三层
                                         //第三层
        public string aid { get; set; }//AID
        public string tname { get; set; }//类型
        public string title { get; set; }//标题
        public string name { get; set; }//作者
        public string pic { get; set; }//封面
        public string fav_create_at { get; set; }

        public GetFavouriteBoxsVideoModel owner { get; set; }

    }
    public class GetHistoryModel
    {
        //必须有登录Cookie
        //Josn：http://api.bilibili.com/x/history?jsonp=jsonp&ps=20&pn=1
        public int code { get; set; }
        public object data { get; set; }
        public string aid { get; set; }
        public string cover { get; set; }

        public GetHistoryModel owner { get; set; }
        public string name { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public long view_at { get; set; }
        public string View_at
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(view_at + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow).ToString("d");
            }
        }//转
        public string tname { get; set; }
    }

}
