using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace BiliBili.UWP.Models
{
    public class BangumiHomeModel
    {
        public int code { get; set; }
        public string message { get; set; }

        public BangumiHomeModel result { get; set; }

        public BangumiHomeModel recommend_cn { get; set; }

        public BangumiHomeModel recommend_jp { get; set; }

        public List<BangumiHomeModel> foot { get; set; }
        public string cover { get; set; }
        public string cursor { get; set; }
        public string desc { get; set; }
        public int id { get; set; }
        public int is_new { get; set; }
        public string link { get; set; }
        public string onDt { get; set; }
        public string title { get; set; }

        public List<BangumiHomeModel> recommend { get; set; }
        public string favourites { get; set; }
        public int is_started { get; set; }
        public int is_finish { get; set; }
        public long? last_time { get; set; }
        public string newest_ep_index { get; set; }
        public int season_id { get; set; }
        public int season_status { get; set; }
        public int watching_count { get; set; }

        public string favouritesCount
        {
            get
            {
                double i = double.Parse(favourites);
                double d = i / 10000;
                if (d >= 1)
                {
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return favourites;
                }
            }
        }
    }

    public class JpHomeModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public JpHomeModel result { get; set; }
        public JpHomeModel ad { get; set; }
        public List<JpHomeModel> head { get; set; }
        public int id { get; set; }
        public string img { get; set; }
        public string link { get; set; }
        public string title { get; set; }

        public List<JpHomeModel> end_recommend { get; set; }
        public JpHomeModel previous { get; set; }
        public List<JpHomeModel> list { get; set; }
        //public string cover { get; set; }

        private string _cover;
        public string cover
        {
            get { return _cover + "@300w.jpg"; }
            set { _cover = value; }
        }

        public string favourites { get; set; }
        public string season_id { get; set; }
        public string favouritesCount
        {
            get
            {
                double i = double.Parse(favourites);
                double d = i / 10000;
                if (d >= 1)
                {
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return favourites;
                }
            }
        }

        public List<JpHomeModel> serializing { get; set; }
        public string newest_ep_index { get; set; }
        public string watching_count { get; set; }
    }

    public class BanTJModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<BanTJModel> result { get; set; }
        public int id { get; set; }
        public int is_new { get; set; }
        public string title { get; set; }
        public string onDt { get; set; }
        // public string cover { get; set; }
        private string _cover;
        public string cover
        {
            get { return _cover + "@500w.jpg"; }
            set { _cover = value; }
        }
        public string desc { get; set; }
        public string link { get; set; }
        public string cursor { get; set; }//以防万一数字太大，用string
    }


    public class FollowSeasonModel
    {

        public int status { get; set; }
        public bool show_badge
        {
            get
            {
                return !string.IsNullOrEmpty(badge);
            }
        }
        public string badge { get; set; }
        public string square_cover { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public int season_id { get; set; }
        public string url { get; set; }
        public string progress_text
        {
            get
            {
                if (progress != null)
                {
                    return progress.index_show;
                }
                else
                {
                    return "尚未观看";
                }
            }
        }

        public FollowSeasonNewEpModel new_ep { get; set; }
        public FollowSeasonProgressModel progress { get; set; }
    }
    public class FollowSeasonNewEpModel
    {

        public string cover { get; set; }
        public int duration { get; set; }
        public int id { get; set; }
        public string index_show { get; set; }
    }
    public class FollowSeasonProgressModel
    {
        public int last_ep_id { get; set; }
        public string index_show { get; set; }
    }



    public class TimeLineModel
    {
        public int code { get; set; }
        public string message { get; set; }

        public List<TimeLineModel> result { get; set; }
        public string badge { get; set; }
        public int area_id { get; set; }
        //public string cover { get; set; }

        private string _cover;
        public string cover
        {
            get { return _cover + "@300w.jpg"; }
            set { _cover = value; }
        }

        public string title { get; set; }
        public string ontime { get; set; }
        public string pub_date { get; set; }
        public long date { get; set; }
        public int season_id { get; set; }
        public string ep_index { get; set; }
        public int season_status { get; set; }
        public Visibility vis
        {
            get
            {
                if (badge != null && badge.Length != 0)
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

    public class AllTimeLineModel
    {
        public AllTimeLineModel today { get; set; }

        public AllTimeLineModel per1 { get; set; }
        public AllTimeLineModel per2 { get; set; }
        public AllTimeLineModel per3 { get; set; }
        public AllTimeLineModel per4 { get; set; }
        public AllTimeLineModel per5 { get; set; }
        public AllTimeLineModel per6 { get; set; }

        public AllTimeLineModel next1 { get; set; }
        public AllTimeLineModel next2 { get; set; }
        public AllTimeLineModel next3 { get; set; }
        public AllTimeLineModel next4 { get; set; }
        public AllTimeLineModel next5 { get; set; }
        public AllTimeLineModel next6 { get; set; }

        public List<TimeLineModel> ls { get; set; }
        public string Week { get; set; }
        public string WeekStr { get; set; }
        public string Date { get; set; }

        public Visibility Vis
        {
            get
            {
                if (ls.Count == 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public string NoStr
        {
            get
            {
                int i = new Random().Next(0, 2);
                switch (i)
                {
                    case 0:
                        return "竟然什么都没有呢";
                    case 1:
                        return "这天没有番剧播出";
                    default:
                        return "这天没有番剧播出";
                }
            }
        }

    }
    public class FilterModel
    {
        public string name { get; set; }
        public string data { get; set; }


    }
    public class BantagModel
    {
        public int code { get; set; }
        public string message { get; set; }

        public BantagModel result { get; set; }

        public List<BantagModel> category { get; set; }
        public string cover { get; set; }
        public string tag_id { get; set; }
        public string tag_name { get; set; }

        public List<string> years { get; set; }

    }

    public class AllBanModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public int count { get; set; }
        public int pages { get; set; }
        public AllBanModel result { get; set; }

        public List<AllBanModel> data { get; set; }
        public string badge { get; set; }
        public string brief { get; set; }
        //@160w_214h.webp
        private string _cover;
        public string cover { get { return _cover + "@160w_214h.jpg"; } set { _cover = value; } }
        public string is_finish { get; set; }
        public int is_started { get; set; }
        public string title { get; set; }
        public int season_id { get; set; }
        public string index_show { get; set; }
        public AllBanOrderModel order { get; set; }
        public int media_id { get; set; }




        public Visibility vis
        {
            get
            {
                if (badge != null && badge.Length != 0)
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

    public class AllBanOrderModel
    {
        public string follow { get; set; }
        public string play { get; set; }
        public string score { get; set; }
    }


}
