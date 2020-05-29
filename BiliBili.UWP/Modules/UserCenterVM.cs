using BiliBili.UWP.Api;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules.BiliBili.UWP.Modules.UserCenterModels;
using BiliBili.UWP.Modules.BiliBili.UWP.Modules.UserSubmitVideoModels;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace BiliBili.UWP.Modules
{
    public class UserCenterVM : IModules
    {
        readonly Api.User.UserCenterAPI userCenterAPI;
        public string mid { get; set; }
        public bool is_self { get; set; } = false;
        public UserCenterVM(string mid)
        {
            userCenterAPI = new Api.User.UserCenterAPI();
            this.mid = mid;
            this.is_self = mid == ApiHelper.GetUserId();
        }
        
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private string _top_image = "ms-appx:///Assets/Img/toutu.png";
        public string top_image
        {
            get { return _top_image; }
            set { _top_image = value; DoPropertyChanged("top_image"); }
        }

        private UserCenterDetailModel _UserCenterDetail;
        public UserCenterDetailModel UserCenterDetail
        {
            get { return _UserCenterDetail; }
            set { _UserCenterDetail = value; DoPropertyChanged("UserCenterDetail"); }
        }
        private IncrementalLoadingCollection<UserSubmitVideoSource, SubmitVideoItemModel> _submitVideos;

        public IncrementalLoadingCollection<UserSubmitVideoSource, SubmitVideoItemModel> SubmitVideos
        {
            get { return _submitVideos; }
            set { _submitVideos = value; DoPropertyChanged("SubmitVideos"); }
        }



        public async Task GetUserDetail()
        {
            try
            {
                Loading = true;
                
                var api = userCenterAPI.UserCenterDetail(mid);

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetData<UserCenterDetailModel>();
                    if (data.success)
                    {
                        UserCenterDetail = data.data;
                        GetTopImage();
                        SubmitVideos = new IncrementalLoadingCollection<UserSubmitVideoSource, SubmitVideoItemModel>(new UserSubmitVideoSource(mid),30);
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                    
                }
                else
                {
                    Utils.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        private async Task GetTopImage()
        {
            try
            {
                var result=await userCenterAPI.UserProfileWeb(mid).Request();
                if (result.status)
                {
                    var data =await result.GetData<JObject>();
                    if (data.success)
                    {
                        top_image = data.data["top_photo"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                HandelError(ex);
            }
        }

    }

    public class UserSubmitVideoSource : IModules,IIncrementalSource<SubmitVideoItemModel>
    {
        readonly Api.User.UserCenterAPI userCenterAPI;
        readonly string mid;
       
        public UserSubmitVideoSource(string mid)
        {
            userCenterAPI = new Api.User.UserCenterAPI();
            this.mid = mid;

        }
        private async Task<List<SubmitVideoItemModel>> GetVideos(int page,int pageSize)
        {
            try
            {
                var api = userCenterAPI.UserSubmitVideosWeb(mid, page, pageSize);
                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<SubmitVideoItemModel>>(data["data"]["list"]["vlist"].ToString());
                        return items;
                    }
                    else
                    {
                        Utils.ShowMessageToast(data["message"].ToString());
                        return new List<SubmitVideoItemModel>();
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                    return new List<SubmitVideoItemModel>();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("读取用户投稿视频失败");

                LogHelper.WriteLog("读取用户投稿视频失败", LogType.ERROR, ex);
                return new List<SubmitVideoItemModel>();
            }
           
        }
        public async Task<IEnumerable<SubmitVideoItemModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            return await GetVideos(pageIndex+1, pageSize);
        }
    }

    namespace BiliBili.UWP.Modules.UserCenterModels
    {
        public class UserCenterDetailModel:IModules
        {
            private int _relation;
            public int relation
            {
                get { return _relation; }
                set { _relation = value; DoPropertyChanged("relation"); DoPropertyChanged("is_follow"); }
            }

            public bool is_follow
            {
                get
                {
                    return relation == 1;
                }
            }
            public int guest_relation { get; set; }
            public int medal { get; set; }
            public string default_tab { get; set; }
            public bool is_params { get; set; }

            public UserCenterDetailCard card { get; set; }
            public UserCenterDetailImages images { get; set; }
            public UserCenterDetailLive live { get; set; }
            public bool living
            {
                get
                {
                    return live!=null&& live.liveStatus == 1;
                }
            }
        }
        public class UserCenterDetailCard
        {
            public string mid { get; set; }
            public string name { get; set; }
            public bool approve { get; set; }
            public string sex { get; set; }
            public string rank { get; set; }
            public string face { get; set; }
            public string DisplayRank { get; set; }
            public int regtime { get; set; }
            public int spacesta { get; set; }
            public string birthday { get; set; }
            public string place { get; set; }
            public string description { get; set; }
            public int article { get; set; }
            public object attentions { get; set; }
            public int fans { get; set; }
            public int friend { get; set; }
            public int attention { get; set; }
            public string sign { get; set; }
            public UserCenterDetailCardLevelInfo level_info { get; set; }
            public UserCenterDetailCardPendant pendant { get; set; }
            public UserCenterDetailCardNameplate nameplate { get; set; }
            public UserCenterDetailCardOfficialVerify official_verify { get; set; }
            public UserCenterDetailCardVip vip { get; set; }
            public UserCenterDetailPrInfo pr_info { get; set; }
            public bool showPrInfo
            {
                get
                {
                    return pr_info != null && !string.IsNullOrEmpty( pr_info.content) ;
                }
            }

            public bool isVip
            {
                get
                {
                    return vip != null && vip.vipType != 0;
                }
            }

            public int fans_group { get; set; }
            public int silence { get; set; }
            public int end_time { get; set; }
            public string silence_url { get; set; }

            public string pendant_url { get; set; }
            public string pendant_title { get; set; }

            public UserCenterDetailCardRelation relation { get; set; }
        }

        public class UserCenterDetailPrInfo
        {
            public string content { get; set; }
        }
        public class UserCenterDetailCardLevelInfo
        {
            public int current_level { get; set; }
            public int current_min { get; set; }
            public int current_exp { get; set; }
            public string next_exp { get; set; }
            public SolidColorBrush level_color
            {
                get
                {
                    if (current_level <= 3)
                    {
                        return new SolidColorBrush(new Windows.UI.Color() { R = 48, G = 161, B = 255, A = 200 });
                    }
                    else
                    {
                        if (current_level <= 6)
                        {
                            return new SolidColorBrush(new Windows.UI.Color() { R = 255, G = 48, B = 48, A = 200 });
                        }
                        else
                        {
                            return new SolidColorBrush(new Windows.UI.Color() { R = 255, G = 199, B = 45, A = 200 });
                        }
                    }
                }
            }
        }

        public class UserCenterDetailCardPendant
        {
            public int pid { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public int expire { get; set; }
            public string image_enhance { get; set; }
        }

        public class UserCenterDetailCardNameplate
        {
            public int nid { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public string image_small { get; set; }
            public string level { get; set; }
            public string condition { get; set; }
        }

        public class UserCenterDetailCardOfficialVerify
        {
            public int type { get; set; }
            public string desc { get; set; }
            public int role { get; set; }
            public string title { get; set; }
            public string icon
            {
                get
                {
                    switch (type)
                    {
                        case 0:
                            return "ms-appx:///Assets/MiniIcon/ic_authentication_personal.png";
                        case 1:
                            return "ms-appx:///Assets/MiniIcon/ic_authentication_organization.png";
                        default:
                            return "ms-appx:///Assets/MiniIcon/transparent.png";
                    }
                }
            }
        }


        public class UserCenterDetailCardVip
        {
            public int vipType { get; set; }
            public long vipDueDate { get; set; }
            public string dueRemark { get; set; }
            public int accessStatus { get; set; }
            public int vipStatus { get; set; }
            public string vipStatusWarn { get; set; }
            public int themeType { get; set; }
            public string vipText
            {
                get
                {
                    switch (vipType)
                    {
                        case 2:
                            return "年度大会员";
                        case 1:
                            return "大会员";
                        default:
                            return "";
                    }
                   
                }
            }
        }


        public class UserCenterDetailCardRelation
        {
            public int status { get; set; }
        }



        public class UserCenterDetailImages
        {
            public string imgUrl { get; set; }
        }

        public class UserCenterDetailLive
        {
            public int roomStatus { get; set; }
            public int roundStatus { get; set; }
            public int liveStatus { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public string cover { get; set; }
            public int online { get; set; }
            public int roomid { get; set; }
            public int broadcast_type { get; set; }
            public int online_hidden { get; set; }
        }

    }
    namespace BiliBili.UWP.Modules.UserSubmitVideoModels
    {
        public class SubmitVideoItemModel
        {
            public int comment { get; set; }
            public string play { get; set; }

            private string _pic;

            public string pic
            {
                get { return _pic.Replace("//", "http://"); }
                set { _pic = value; }
            }
            public string description { get; set; }
            public string title { get; set; }
            public string author { get; set; }
            public string length { get; set; }
            public string aid { get; set; }
            public long created { get; set; }
            public int video_review { get; set; }
        }
    }
}
