using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace BiliBili.UWP.Models
{
    public class HomeLiveModel
    {
        public int code { get; set; }
        public string message { get; set; }

        public HomeLiveModel data { get; set; }
        public List<HomeLiveModel> banner { get; set; }
        public string title { get; set; }
        public string img { get; set; }
        public string remark { get; set; }
        public string link { get; set; }
        public List<HomeLiveModel> partitions { get; set; }
        public HomeLiveModel partition { get; set; }
        public HomeLiveModel sub_icon { get; set; }
        //public string src { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public List<HomeLiveModel> lives { get; set; }
        public HomeLiveModel owner { get; set; }
        public string face { get; set; }
        public string mid { get; set; }
        public HomeLiveModel cover { get; set; }


        private string _src;
        public string src
        {
            get { return _src + "@300w.jpg"; }
            set { _src = value; }
        }

        //public string title { get; set; }
        public long online { get; set; }
        public string room_id { get; set; }
    }

    public class FeedModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public FeedModel data { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public List<FeedModel> list { get; set; }

        public string uid { get; set; }
        public string uname { get; set; }


        public string cover { get; set; }
        public string title { get; set; }
        public string online { get; set; }

        public string face { get; set; }
        public string roomid { get; set; }
        public string area_name { get; set; }
        public int tstatus { get; set; }

        public int fans_num { get; set; }
        public string FansNum
        {
            get
            {
                if (fans_num > 10000)
                {
                    return ((double)fans_num / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return fans_num.ToString();
                }
            }
        }

        public int live_status { get; set; }
        public int round_status { get; set; }

        public Visibility liveing
        {
            get
            {
                if (live_status == 1)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public Visibility rounding
        {
            get
            {
                if (live_status == 2)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public Visibility stop
        {
            get
            {
                if (live_status == 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
    }

    public class LiveCenterModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public LiveCenterModel data { get; set; }

        public string uname { get; set; }
        public string pic { get; set; }

        public int silver { get; set; }
        public int gold { get; set; }

        public int vip { get; set; }
        public int svip { get; set; }

        public string room_id { get; set; }
        public string user_level { get; set; }
        public int isSign { get; set; }
        public string vip_time { get; set; }
        public string svip_time { get; set; }

        public LiveCenterModel medal { get; set; }
        public string color { get; set; }
        public string medal_name { get; set; }
        public string level { get; set; }


        public string user_level_color { get; set; }

        public bool ShowVip
        {
            get
            {
                if (vip == 1 || svip == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string ExTime
        {
            get
            {
                if (vip == 1)
                {
                    return "老爷到期时间:" + vip_time;
                }
                else
                {
                    if (svip == 1)
                    {
                        return "年费老爷到期时间:" + svip_time;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }


    }
    public class LiveCenterModel1
    {
        public string code { get; set; }
        public string message { get; set; }
        public LiveCenterModel data { get; set; }

        public string uname { get; set; }
        public string pic { get; set; }

        public int silver { get; set; }
        public int gold { get; set; }

        public int vip { get; set; }
        public int svip { get; set; }

        public string room_id { get; set; }
        public string user_level { get; set; }
        public int isSign { get; set; }
        public string vip_time { get; set; }
        public string svip_time { get; set; }

        public LiveCenterModel medal { get; set; }
        public string color { get; set; }
        public string medal_name { get; set; }
        public string level { get; set; }


        public string user_level_color { get; set; }

        public bool ShowVip
        {
            get
            {
                if (vip == 1 || svip == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string ExTime
        {
            get
            {
                if (vip == 1)
                {
                    return "老爷到期时间:" + vip_time;
                }
                else
                {
                    if (svip == 1)
                    {
                        return "年费老爷到期时间:" + svip_time;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }


    }
    public class LiveSearchModel
    {
        public int code { get; set; }
        public string message { get; set; }


        public string type { get; set; }
        public int uid { get; set; }
        public int short_id { get; set; }
        public string tags { get; set; }
        public int online { get; set; }
        public string uname { get; set; }
        public string Uname
        {
            get
            {
                return uname.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
            }
        }


        public string uface { get; set; }
        public string face
        {
            get { return "https://" + uface; }
        }

        public string cover { get; set; }
        public string _cover
        {
            get { return "https://" + cover; }
        }

        public string title { get; set; }
        public string Title
        {
            get
            {
                return title.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
            }
        }

        public string user_cover { get; set; }
        public int roomid { get; set; }



        public int attentions { get; set; }
        public string FansNum
        {
            get
            {
                if (attentions > 10000)
                {
                    return ((double)attentions / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return attentions.ToString();
                }
            }
        }



        public int live_status { get; set; }
        public Visibility liveing
        {
            get
            {
                if (live_status == 1)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }

            }
        }


        public Visibility stop
        {
            get
            {
                if (live_status != 1)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }

            }
        }
    }

    public class RoomListModel
    {
        public int count { get; set; }
        public ObservableCollection<RoomListItem> list { get; set; }

    }
    public class RoomListItem
    {
        public int roomid { get; set; }
        public string cover { get; set; }
        public string face { get; set; }
        public string title { get; set; }
        public string uname { get; set; }
        public string online { get; set; }
        public string area_name { get; set; }
        public int area_id { get; set; }
        public string parent_name { get; set; }
    }


    public class SignModel
    {
        public int code { get; set; }
        public string msg { get; set; }
        public object data { get; set; }

        public string text { get; set; }
        public int status { get; set; }//1已签到，0未签到
        public int taskStatus { get; set; }

        //用于用户
        public string uname { get; set; }
        public string face { get; set; }
        public double silver { get; set; }
        public double gold { get; set; }
        public int vip { get; set; }//0为false,1为true
        public int svip { get; set; }//0为false,1为true
        public int user_level { get; set; }//现在等级
        public int user_next_level { get; set; }//下一等级
    }

    public class LiveVideoModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public LiveVideoModel data { get; set; }
        public List<LiveVideoModel> items { get; set; }
        public string next_offset { get; set; }
        public LiveVideoModel user { get; set; }
        public string uid { get; set; }
        public string head_url { get; set; }
        public string name { get; set; }
        public int is_vip { get; set; }
        public LiveVideoModel item { get; set; }

        public LiveVideoModel cover { get; set; }
        public string _default { get; set; }
        public string share_url { get; set; }
        public int type { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
        public string Tag
        {
            get
            {
                if (tags != null && tags.Count != 0)
                {
                    return "#" + tags[0] + "#";
                }
                else
                {
                    return "";
                }
            }
        }

        public double video_time { get; set; }
        public string Time
        {
            get
            {
                if (type == 0)
                {
                    var ts = TimeSpan.FromSeconds(video_time);
                    return ts.Minutes + ":" + ts.Seconds;
                }
                else
                {
                    return "";
                }
            }
        }
        public string upload_time { get; set; }
        public string jump_url { get; set; }
        public string damaku_num { get; set; }
        public string watched_num { get; set; }
        public string video_playurl { get; set; }
        public Visibility Vis
        {
            get
            {
                if (type == 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
    }

    public class LiveInfoModel
    {
        public int code { get; set; }
        public string message { get; set; }

        public LiveInfoModel data { get; set; }
        public string room_id { get; set; }
        public string title { get; set; }

        public string mid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
        public int background_id { get; set; }//？？不明
        public long online { get; set; }
        public string start { get; set; }
        public string create_at { get; set; }
        public string status { get; set; }//LIVE or 
        public int is_attention { get; set; }//0-false,1-true

        public LiveInfoModel meta { get; set; }
        public List<string> tag { get; set; }
        public string description { get; set; }//Html格式
        public int typeid { get; set; }
        public string cover { get; set; }
        public string check_status { get; set; }//VERIFY

        public string prepare { get; set; }

        public List<LiveInfoModel> roomgifts { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public LiveInfoModel coin_type { get; set; }
        public string gold { get; set; }
        public string silver { get; set; }
        public string img { get; set; }


        public string master_level { get; set; }
        public string master_level_color { get; set; }

        public string Price
        {
            get
            {
                if (silver == null || silver == string.Empty)
                {
                    return "金瓜子:" + price;
                }
                else
                {
                    return "银瓜子:" + price;
                }
            }
        }
    }

}
