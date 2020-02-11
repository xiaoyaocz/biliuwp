using BiliBili.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Modules
{
    public class LiveArea : IModules
    {
        public async Task<ReturnModel<List<AreaList>>> GetAreaList()
        {
            try
            {
                string url = ApiHelper.GetSignWithUrl($"https://api.live.bilibili.com/room/v1/Area/getList?actionKey=appkey&appkey={ ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&&mobi_app=android&need_entrance=1&parent_id=0&platform=android&ts={ApiHelper.GetTimeSpan}", ApiHelper.AndroidKey);

                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {

                    var m = JsonConvert.DeserializeObject<List<AreaList>>(model.json["data"].ToString());
                    var rec = await GetRecAreaList();
                    if (rec.success)
                    {
                        m.Insert(0, new AreaList()
                        {
                            id = 0,
                            name = "推荐",
                            list = rec.data
                        });
                    }
                    return new ReturnModel<List<AreaList>>()
                    {
                        success = true,
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<List<AreaList>>()
                    {
                        success = false,
                        message = model.message.ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<List<AreaList>>(ex);
            }
        }
        public async Task<ReturnModel<List<AreaListItem>>> GetRecAreaList()
        {
            try
            {
                string url = ApiHelper.GetSignWithUrl($"https://api.live.bilibili.com/room/v1/Area/getRecList?actionKey=appkey&appkey={ ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}", ApiHelper.AndroidKey);

                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    var m = JsonConvert.DeserializeObject<List<AreaListItem>>(model.json["data"].ToString());
                    return new ReturnModel<List<AreaListItem>>()
                    {
                        success = true,
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<List<AreaListItem>>()
                    {
                        success = false,
                        message = model.message.ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<List<AreaListItem>>(ex);
            }
        }

        public async Task<ReturnModel<AreaRoomList>> GetRoomList(int area_id,int parent_area_id, int page,string sort_type= "online")
        {
            try
            {
                string url = ApiHelper.GetSignWithUrl($"https://api.live.bilibili.com/room/v3/Area/getRoomList?actionKey=appkey&appkey={ ApiHelper.AndroidKey.Appkey}&area_id={area_id}&build={ApiHelper.build}&cate_id=0&mobi_app=android&page={page}&page_size=30&parent_area_id={parent_area_id}&platform=android&qn=0&tag_version=1&sort_type={sort_type}&ts={ApiHelper.GetTimeSpan}", ApiHelper.AndroidKey);

                var results = await WebClientClass.GetResults(new Uri(url));
                var model = results.ToDynamicJObject();
                if (model.code == 0)
                {
                    var m = JsonConvert.DeserializeObject<AreaRoomList>(model.json["data"].ToString());
                    return new ReturnModel<AreaRoomList>()
                    {
                        success = true,
                        data = m
                    };
                }
                else
                {
                    return new ReturnModel<AreaRoomList>()
                    {
                        success = false,
                        message = model.message.ToString()
                    };
                }

            }
            catch (Exception ex)
            {

                return HandelError<AreaRoomList>(ex);
            }
        }


    }

    public class AreaList
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<AreaListItem> list { get; set; }
    }
    public class AreaListItem
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string parent_name { get; set; }
        public string name { get; set; }
        public string pic { get; set; }
        public int hot_status { get; set; }
        public int area_type { get; set; }
    }

    public class AreaRoomList
    {
        public int count { get; set; }
        public ObservableCollection<RoomListItem> list { get; set; }
        public List<AreaRoomListBannerItem> banner { get; set; }
        public List<new_tags> new_tags{ get; set; }
    }
    public class AreaRoomListBannerItem
    {
        public int id { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public string link { get; set; }
    }
    public class new_tags
    {
        public int id { get; set; }
        public string name { get; set; }
        public string sort_type { get; set; }
        public string sort { get; set; }
    }

}
