using BiliBili.UWP.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BiliBili.UWP.Models
{
    class VideoInfoModels
    {
        public int code { get; set; }
        public string message { get; set; }
        public VideoInfoModels data { get; set; }
        public string aid { get; set; }
        public string attribute { get; set; }
        public int copyright { get; set; }
        public long? ctime { get; set; }
        public string desc { get; set; }
        public long? duration { get; set; }//长度，秒
        public string pic { get; set; }
        public long? pubdate { get; set; }
        public int state { get; set; }
        public string title { get; set; }
        public int tid { get; set; }
        public string tname { get; set; }
        public string redirect_url { get; set; }
        public string argue_msg { get; set; }
        public ownerModel owner { get; set; }
        public owner_extModel owner_ext { get; set; }
        public List<pagesModel> pages { get; set; }
        public List<relatesModel> relates { get; set; }
        public req_userModel req_user { get; set; }
        public rightsModel rights { get; set; }
        public seasonModel season { get; set; }
        public List<tagModel> tag { get; set; }
        public statModel stat { get; set; }

        public MovieModel movie { get; set; }

        public elecModel elec { get; set; }
        public audioModel audio { get; set; }
        public historyModel history { get; set; }
        public List<staffModel> staff { get; set; }
        /// <summary>
        /// 互动视频
        /// </summary>
        public interactionModel interaction { get; set; }

        public string bvid { get; set; }
        public string short_link { get; set; }
        public string Created_at
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0);
                long lTime = long.Parse(pubdate + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = new TimeSpan(lTime);

                DateTime dt = dtStart.Add(toNow).ToLocalTime();
                //TimeSpan toNow = TimeSpan.FromSeconds(ctime);
                //DateTime dt = dtStart.Add(toNow).ToLocalTime();
                TimeSpan span = DateTime.Now - dt;
                if (span.TotalDays > 7)
                {
                    return dt.ToString();
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
                //if (dt.Date == DateTime.Now.Date)
                //{
                //    TimeSpan ts = DateTime.Now - dt;
                //    return ts.Hours + "小时前";
                //}
                //else
                //{
                //    return dt.ToString();
                //}
            }
        }
    }
    public class interactionModel
    {
        public int graph_version { get; set; }
        public history_nodeModel history_node { get; set; }

    }
    public class historyModel
    {
        public long cid { get; set; }
        public int progress { get; set; }

    }

    public class history_nodeModel
    {
        public int node_id { get; set; }
        public string title { get; set; }
        public long cid { get; set; }
    }
    public class staffModel
    {
        public int attention { get; set; }
        private string _face;
        public string face
        {
            get { return _face + "@100w.jpg"; }
            set { _face = value; }
        }
        public string name { get; set; }
        public string title { get; set; }
        public long mid { get; set; }
        public official_verify official_verify { get; set; }
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
    public class official_verify
    {
        public string desc { get; set; }
        public int type { get; set; }

    }
    public class ActorModel
    {
        public string actor { get; set; }
        public int actor_id { get; set; }
    }
    public class TagModel
    {
        public object result { get; set; }
        public string cover { get; set; }
        public int index { get; set; }
        public int tag_id { get; set; }
        public string tag_name { get; set; }
    }
    public class MovieModel
    {
        public int movie_status { get; set; }//1为收费，2为免费
        public MovieModel pay_user { get; set; }
        public int status { get; set; }//0为未付费
        public MovieModel payment { get; set; }
        public string pay_begin_time { get; set; }
        public decimal price { get; set; }
        public string product_id { get; set; }
        public MovieModel season { get; set; }

        public List<ActorModel> actor { get; set; }
        public string actors
        {
            get
            {
                if (actor.Count != 0)
                {
                    return actor[0].actor;
                }
                else
                {
                    return "";
                }
            }
        }




        public List<TagModel> tags { get; set; }
        public string tag
        {
            get
            {
                string str = "";
                foreach (var item in tags)
                {
                    str += item.tag_name + "、";
                }
                if (str.Length != 0)
                {
                    str = str.Remove(str.Length - 1);
                }
                return str;
            }
        }

        public string area { get; set; }
        public string cover { get; set; }
        public string pub_time { get; set; }
        public string time
        {
            get
            {
                DateTime dt = Convert.ToDateTime(pub_time);
                return dt.ToString("yyyy-MM-dd");
            }
        }

        public string title { get; set; }
        public string total_duration { get; set; }
        public MovieModel activity { get; set; }
        public int activity_id { get; set; }
        public string link { get; set; }
        public string script_src { get; set; }
    }
    public class relatesModel
    {
        public long aid { get; set; }
        public relatesModel owner { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string pic { get; set; }
        public relatesModel stat { get; set; }
        public string danmaku { get; set; }
        public string view { get; set; }
    }


    public class audioModel
    {
        public string cover_url { get; set; }
        public string entrance { get; set; }
        public string title { get; set; }
        public int play_count { get; set; }
        public int reply_count { get; set; }
        public int song_id { get; set; }
    }


    public class ownerModel
    {
        public string face { get; set; }
        public long mid { get; set; }
        public string name { get; set; }
    }

    public class elecModel
    {
        public int count { get; set; }
        public int elec_num { get; set; }
        public int total { get; set; }
        public List<elecModel> list { get; set; }
        public string avatar { get; set; }
        public string pay_mid { get; set; }
        public string rank { get; set; }
        public string uname { get; set; }
    }



    public class owner_extModel
    {

    }
    public class FavboxModel
    {
        public object data { get; set; }

        public int code { get; set; }

        public string fid { get; set; }
        public string mid { get; set; }
        public string name { get; set; }
        public int max_count { get; set; }//总数
        public int cur_count { get; set; }//现存

        public string Count
        {
            get
            {
                return cur_count + "/" + max_count;
            }
        }
    }

    public class pagesModel
    {
        public long cid { get; set; }
        public string from { get; set; }
        public string has_alias { get; set; }
        public string link { get; set; }
        public string page { get; set; }
        public string part { get; set; }
        public int duration { get; set; }
        public string View
        {
            get
            {
                if (part.Length == 0)
                {
                    return page;
                }
                else
                {
                    return page + " " + part;
                }

            }
        }
        public string rich_vid { get; set; }
        public string vid { get; set; }
        public string weblink { get; set; }
        public Visibility IsDowned
        {
            get
            {

                if (DownloadHelper2.downloadeds.ContainsKey(cid.ToString()))
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public SolidColorBrush f
        {
            get
            {
                if (SqlHelper.GetVideoWatchRecord(cid.ToString()) != null)
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
        }


    }
    public class statModel
    {
        public long coin { get; set; }
        public long danmaku { get; set; }
        public long favorite { get; set; }
        // public int his_rank { get; set; }
        // public int now_rank { get; set; }
        public long reply { get; set; }
        public long share { get; set; }
        public long view { get; set; }
        public string View
        {
            get
            {
                if (Convert.ToInt64(view) > 10000)
                {
                    double d = (double)Convert.ToDouble(view) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return view.ToString();
                }
            }
        }
        public string Coin
        {
            get
            {
                if (coin > 10000)
                {
                    double d = (double)Convert.ToDouble(coin) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return coin.ToString();
                }
            }
        }
        public string Danmaku
        {
            get
            {
                if (danmaku > 10000)
                {
                    double d = (double)Convert.ToDouble(danmaku) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return danmaku.ToString();
                }
            }
        }
        public string Favorite
        {
            get
            {
                if (favorite > 10000)
                {
                    double d = (double)Convert.ToDouble(favorite) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return favorite.ToString();
                }
            }
        }
    }

    public class req_userModel
    {
        public int attention { get; set; }
        public int favorite { get; set; }
        public int like { get; set; }
        public int coin { get; set; }
        public int dislike { get; set; }
    }
    public class rightsModel
    {
        public int bp { get; set; }
        public int download { get; set; }
        public int elec { get; set; }
        public int hd5 { get; set; }
        public int movie { get; set; }
        public int pay { get; set; }
    }
    public class seasonModel
    {
        public string season_id { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public int is_finish { get; set; }
        public int episode_status { get; set; }
        public string newest_ep_index { get; set; }
        public string newest_ep_id { get; set; }
        public int weekday { get; set; }
        public int? total_count { get; set; }
        public string BanText
        {
            get
            {
                if (is_finish == 1)
                {
                    return string.Format("已完结,共{0}话", total_count);
                }
                else
                {
                    string we = string.Empty;
                    switch (weekday)
                    {
                        case 1:
                            we = "一";
                            break;
                        case 2:
                            we = "二";
                            break;
                        case 3:
                            we = "三";
                            break;
                        case 4:
                            we = "四";
                            break;
                        case 5:
                            we = "五";
                            break;
                        case 6:
                            we = "六";
                            break;
                        case 7:
                            we = "日";
                            break;
                        default:
                            break;
                    }
                    return string.Format("连载中,每周{0}更新", we);
                }
            }
        }
    }
    public class tagModel
    {
        public string cover { get; set; }
        public string hates { get; set; }
        public string likes { get; set; }
        public string tag_id { get; set; }
        public string tag_name { get; set; }
    }

}
