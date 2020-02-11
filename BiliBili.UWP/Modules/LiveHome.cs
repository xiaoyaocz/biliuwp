using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Windows.Input;
using BiliBili.UWP.Pages;

namespace BiliBili.UWP.Modules
{
    public class LiveHome : LiveCommand, INotifyPropertyChanged
    {
 
        public LiveHome()
        {
           
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _Loading = true;

        public bool Loading
        {   
            get { return _Loading; }
            set { _Loading = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Loading")); }
        }


        private live_area_entrance_v2 _areas;
        /// <summary>
        /// 分区信息
        /// </summary>
        public live_area_entrance_v2 Areas
        {
            get { return _areas; }
            set { _areas = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Areas")); }
        }

        private live_banner _banner;
        /// <summary>
        /// Banner
        /// </summary>
        public live_banner Banner
        {
            get { return _banner; }
            set { _banner = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Banner")); }
        }

        private live_hour_rank _hour_rank;
        /// <summary>
        /// Banner
        /// </summary>
        public live_hour_rank HourRank
        {
            get { return _hour_rank; }
            set { _hour_rank = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HourRank")); }
        }

        private List<room_list> _room_list;
        /// <summary>
        /// Banner
        /// </summary>
        public List<room_list> RoomList
        {
            get { return _room_list; }
            set { _room_list = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoomList")); }
        }


        public async Task LoadHome()
        {
            try
            {
                Loading = true;
                string url =ApiHelper.GetSignWithUrl( $"https://api.live.bilibili.com/xlive/app-interface/v2/index/getAllList?actionKey=appkey&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&device=android&mobi_app=android&platform=android&qn=0&rec_page=1&relation_page=1&scale=xxhdpi&ts={ApiHelper.GetTimeSpan}",ApiHelper.AndroidKey);
                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    live_home m = JsonConvert.DeserializeObject<live_home>(model.json["data"].ToString());
                    Areas = m.area_entrance_v2[0];
                    Banner = m.banner[0];
                    HourRank = m.hour_rank[0];
                    RoomList = m.room_list;
                }
                else
                {
                    Utils.ShowMessageToast(model.message);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast(ex.Message);
            }
            finally
            {
                Loading = false;
            }
        }

    }
    public class live_home
    {
        public List<live_banner> banner { get; set; }
        public List<live_area_entrance_v2> area_entrance_v2 { get; set; }
        public List<live_area_entrance_v2> area_entrance { get; set; }
        public List<live_hour_rank> hour_rank { get; set; }
        public List<room_list> room_list { get; set; }
    }
    public class live_module_info
    {
        public int id { get; set; }
        public int type { get; set; }
        public int sort { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        public int count { get; set; }
    }
    public class live_extra_info
    {
        public string sub_title { get; set; }
    }

    public class live_area_entrance_v2
    {
        public live_module_info module_info { get; set; }
        public ObservableCollection<live_area_entrance_v2_item> list { get; set; }
    }
    public class live_area_entrance_v2_item
    {
        public int id { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        public int area_v2_id { get; set; }
        public int area_v2_parent_id { get; set; }
        public int tag_type { get; set; }
    }

    public class live_banner
    {
        
        public live_module_info module_info { get; set; }
        public ObservableCollection<live_banner_item> list { get; set; }
    }
    public class live_banner_item : LiveCommand
    {
        public int id { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        public string content { get; set; }
    }
    public class live_hour_rank
    {
        public live_module_info module_info { get; set; }
        public live_extra_info extra_info { get; set; }
        public ObservableCollection<live_hour_rank_item> list { get; set; }
    }
    public class live_hour_rank_item:LiveCommand
    {
        public int uid { get; set; }
        public int roomid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
        public int area_v2_parent_id { get; set; }
        public int area_v2_id { get; set; }
        public string area_v2_name { get; set; }
        public string area_v2_parent_name { get; set; }
    }

    public class room_list:LiveCommand
    {
        public live_module_info module_info { get; set; }
        public ObservableCollection<room_list_item> list { get; set; }
    }
    public class room_list_item
    {
        public int area_v2_parent_id { get; set; }
        public int area_v2_id { get; set; }
        public string area_v2_name { get; set; }
        public string area_v2_parent_name { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public int roomid { get; set; }
        public string uname { get; set; }
        public long online { get; set; }
        public string online_str {
            get
            {
                if (online >= 10000)
                {
                    return (online / (double)10000).ToString("0.00") + "万";
                }
                else {
                    return online.ToString();
                }
            }
        }
    }
}
