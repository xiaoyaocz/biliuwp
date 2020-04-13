using BiliBili.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using BiliBili.UWP.Helper;

namespace BiliBili.UWP.Modules
{
    /// <summary>
    /// 稍后再看模块
    /// </summary>
    public class ToView : IModules
    {
        /// <summary>
        /// 取稍后再看列表
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<ObservableCollection<ToViewsModel>>> GetToViewList()
        {
            try
            {
                
                if (!ApiHelper.IsLogin())
                {
                    return new ReturnModel<ObservableCollection<ToViewsModel>>()
                    {
                        message = "请先登录",
                        success = false
                    };
                }
                string url = string.Format("https://api.bilibili.com/x/v2/history/toview/web?access_key={0}&appkey={1}&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);

                var results = await WebClientClass.GetResults(new Uri(url));

                ToViewsModel model = JsonConvert.DeserializeObject<ToViewsModel>(results);
                if (model.code == 0)
                {
                    if (model.data.list==null)
                    {
                        model.data.list = new ObservableCollection<ToViewsModel>();
                    }
                    return new ReturnModel<ObservableCollection<ToViewsModel>>()
                    {
                        success = true,
                        message = "",
                        data = model.data.list,
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<ToViewsModel>>()
                    {
                        success = false,
                        message = model.message
                    };
                }


            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<ToViewsModel>>(ex);

            }





        }

        /// <summary>
        /// 添加稍后再看
        /// </summary>
        /// <param name="aid">AV号</param>
        /// <returns></returns>
        public async Task<ReturnModel> AddToView(string aid)
        {
            try
            {

                string url = "https://api.bilibili.com/x/v2/history/toview/add";
                string par = string.Format("aid={3}&access_key={0}&appkey={1}&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, aid);
                par += "&sign=" + ApiHelper.GetSign(par);

                var str = await WebClientClass.PostResults(new Uri(url), par);
                JObject obj = JObject.Parse(str);
                if (obj["code"].ToInt32() == 0)
                {
                    return new ReturnModel() { success = true, message = "" };
                }
                else
                {
                    return new ReturnModel() { success = false, message = obj["message"].ToString() };
                }

            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }
        /// <summary>
        /// 删除稍后再看
        /// </summary>
        /// <param name="aid">AV号</param>
        /// <returns></returns>
        public async Task<ReturnModel> DeleteToView(string aid)
        {
            try
            {

                string url = "https://api.bilibili.com/x/v2/history/toview/del";
                string par = string.Format("aid={3}&access_key={0}&appkey={1}&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, aid);
                par += "&sign=" + ApiHelper.GetSign(par);

                var str = await WebClientClass.PostResults(new Uri(url), par);
                JObject obj = JObject.Parse(str);
                if (obj["code"].ToInt32() == 0)
                {
                    return new ReturnModel() { success = true, message = "" };
                }
                else
                {
                    return new ReturnModel() { success = false, message = obj["message"].ToString() };
                }

            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }
        /// <summary>
        /// 清除全部
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel> ClearToView()
        {
            try
            {

                string url = "https://api.bilibili.com/x/v2/history/toview/clear";
                string par = string.Format("access_key={0}&appkey={1}&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                par += "&sign=" + ApiHelper.GetSign(par);

                var str = await WebClientClass.PostResults(new Uri(url), par);
                JObject obj = JObject.Parse(str);
                if (obj["code"].ToInt32() == 0)
                {
                    return new ReturnModel() { success = true, message = "" };
                }
                else
                {
                    return new ReturnModel() { success = false, message = obj["message"].ToString() };
                }

            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }
        /// <summary>
        /// 清除已观看
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel> ClearPlayed()
        {
            try
            {

                string url = "https://api.bilibili.com/x/v2/history/toview/del";
                string par = string.Format("access_key={0}&appkey={1}&ts={2}&viewed=true", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                par += "&sign=" + ApiHelper.GetSign(par);

                var str = await WebClientClass.PostResults(new Uri(url), par);
                JObject obj = JObject.Parse(str);
                if (obj["code"].ToInt32() == 0)
                {
                    return new ReturnModel() { success = true, message = "" };
                }
                else
                {
                    return new ReturnModel() { success = false, message = obj["message"].ToString() };
                }

            }
            catch (Exception ex)
            {
                return HandelError(ex);
            }
        }
        public class ToViewsModel
        {
            public int code { get; set; }
            public string message { get; set; }
            public ToViewsModel data { get; set; }
            public ObservableCollection<ToViewsModel> list { get; set; }


            public string aid { get; set; }
            public int videos { get; set; }
            public string tname { get; set; }
            private string _pic;
            public string pic
            {
                get
                {
                    return _pic + "@300w.png";
                }
                set
                {
                    _pic = value;
                }
            }
          
            public string title { get; set; }
            public string desc { get; set; }
            public string dynamic { get; set; }
            public long cid { get; set; }
            public long add_at { get; set; }
            public rightsModel rights { get; set; }
            public ownerModel owner { get; set; }
            public statModel stat { get; set; }
            public List<pagesModel> pages { get; set; }

            public int progress { get; set; }
            public int duration { get; set; }

            public string display
            {
                get
                {
                    var ts = TimeSpan.FromSeconds(duration);
                    return $"{videos}P {ts.ToString("c")}";
                }
            }

            public string state
            {
                get
                {
                    if (progress == -1)
                    {
                        return "已看完";
                    }
                    else
                    {
                        if (progress != 0)
                        {
                            var index = pages.IndexOf(pages.Find(x => x.cid == cid)) + 1;
                            return $"第{index}P {((double)progress / pages[index - 1].duration).ToString("p")}";
                        }
                        else
                        {
                            return "尚未观看";
                        }

                    }
                }
            }

            public ToViewBangumiModel bangumi { get; set; }
        }


        public class ToViewBangumiModel
        {
            public int ep_id { get; set; }
            public string title { get; set; }
            public string long_title { get; set; }
            public string cover { get; set; }
            public ToViewBangumiModel season { get; set; }
            public int season_id { get; set; }

        }

    }

}
