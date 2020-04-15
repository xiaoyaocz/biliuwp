using BiliBili.UWP.Api;
using BiliBili.UWP.Api.Home;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules.Home.RecommendModels;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules.Home
{
    public class RecommendVM:IModules
    {
        readonly RecommendAPI recommendAPI;
        public RecommendVM()
        {
            recommendAPI = new RecommendAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private ObservableCollection<RecommendBannerItemModel> _banner;
        public ObservableCollection<RecommendBannerItemModel> Banner
        {
            get { return _banner; }
            set { _banner = value; DoPropertyChanged("Banner"); }
        }

        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private IncrementalLoadingCollection<RecommendItemSource, RecommendItemModel> _items;
        public IncrementalLoadingCollection<RecommendItemSource, RecommendItemModel> Items
        {
            get { return _items; }
            set { _items = value; DoPropertyChanged("Items"); }
        }


        public async Task LoadRecommend()
        {
            try
            {
                Loading = true;
                var result = await recommendAPI.Recommend("0").Request();
                if (result.status)
                {
                    var obj = await result.GetData<JObject>();
                    if (obj.code == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<RecommendItemModel>>(obj.data["items"].ToString());
                        var banner = items.FirstOrDefault(x => x.card_goto == "banner");
                        if (banner != null)
                        {
                            if (Banner == null || Banner.Count == 0)
                            {
                                Banner = banner.banner_item;
                            }
                            items.Remove(banner);
                        }
                        for (int i = items.Count - 1; i >= 0; i--)
                        {
                            if (items[i].card_goto.Contains("ad_web"))
                                items.Remove(items[i]);
                        }

                        Items = new IncrementalLoadingCollection<RecommendItemSource, RecommendItemModel>(new RecommendItemSource(items));
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
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
        public async void LoadMore()
        {
            await Items.LoadMoreItemsAsync(20);
        }
        public async void Refresh()
        {
            if (Loading)
            {
                Utils.ShowMessageToast("正在加载中....");
                return;
            }
            await LoadRecommend();
        }

        public async Task Dislike(string idx, RecommendThreePointV2ItemModel threePointV2Item, RecommendThreePointV2ItemReasonsModel itemReasons)
        {
            try
            {
                if (!ApiHelper.IsLogin() && await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
                var recommendItem = Items.FirstOrDefault(x => x.idx == idx);
                var api = recommendAPI.Dislike(_goto: recommendItem.card_goto, id: recommendItem.param, mid: recommendItem.args.up_id, reason_id: itemReasons.id, rid: recommendItem.args.rid, tag_id: recommendItem.args.tid);
                if (threePointV2Item.type == "feedback")
                {
                    recommendAPI.Feedback(_goto: recommendItem.card_goto, id: recommendItem.param, mid: recommendItem.args.up_id, feedback_id: itemReasons.id, rid: recommendItem.args.rid, tag_id: recommendItem.args.tid);
                }
                var result = await api.Request();
                if (result.status)
                {
                    var obj = result.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        Items.Remove(recommendItem);
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError(ex);
                Utils.ShowMessageToast(handel.message);
            }
        }



    }

    public class RecommendItemSource :  IIncrementalSource<RecommendItemModel>
    {
        readonly Api.Home.RecommendAPI recommendAPI;
        public RecommendItemSource(List<RecommendItemModel> items)
        {
            recommendAPI = new Api.Home.RecommendAPI();
            last_idx = items.LastOrDefault().idx ?? "0";
            this.recommends = items;
        }
        string last_idx = "0";
        List<RecommendItemModel> recommends;
        public async Task<List<RecommendItemModel>> GetRecommend()
        {
            try
            {
                
                var result = await recommendAPI.Recommend(last_idx).Request();
                if (result.status)
                {
                    var obj =await result.GetData<JObject>();

                    if (obj.code == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<RecommendItemModel>>(obj.data["items"].ToString());
                      
                        for (int i = items.Count - 1; i >= 0; i--)
                        {
                            if (items[i].card_goto.Contains("ad_web")|| items[i].card_goto.Contains("banner"))
                                items.Remove(items[i]);
                        }
                        last_idx = items.LastOrDefault()?.idx??"0";
                        return items;
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj.message);
                        return new List<RecommendItemModel>();
                    }
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                    return new List<RecommendItemModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("加载首页推荐信息失败", LogType.ERROR, ex);
                Utils.ShowMessageToast("加载首页推荐信息失败");
                return new List<RecommendItemModel>();
            }
            
        }
        public async Task<IEnumerable<RecommendItemModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageIndex==0)
            {
                return recommends;
            }
            return await GetRecommend();
        }
    }


    namespace RecommendModels
    {
        public class RecommendItemModel
        {
            public ObservableCollection<RecommendBannerItemModel> banner_item { get; set; }
            private string _title = "";
            public string title
            {
                get
                {
                    if (string.IsNullOrEmpty(_title) && string.IsNullOrEmpty(uri))
                    {
                        if (ad_info == null)
                        {
                            return "你追的番剧更新啦~";
                        }
                        else
                        {
                            return ad_info.creative_content.title;
                        }
                    }
                    return _title;
                }
                set { _title = value; }
            }

            public string _cover { get; set; }

            public string cover
            {
                get
                {
                    if (string.IsNullOrEmpty(_cover) && ad_info != null)
                    {
                        return ad_info.creative_content.image_url;
                    }
                    else
                    {
                        return _cover;
                    }
                }
                set
                {
                    _cover = value;
                }
            }

            public string uri { get; set; }
            public string param { get; set; }
            public string card_goto { get; set; }

            public string idx { get; set; }

            private List<RecommendThreePointV2ItemModel> _three_point_v2;

            public List<RecommendThreePointV2ItemModel> three_point_v2
            {
                get
                {
                    if (_three_point_v2 != null)
                    {
                        foreach (var item in _three_point_v2)
                        {
                            item.idx = idx;
                        }
                    }

                    return _three_point_v2;
                }
                set { _three_point_v2 = value; }
            }


            public RecommendItemArgsModel args { get; set; }

            public RecommendRcmdReasonStyleModel rcmd_reason_style { get; set; }
            public RecommendDescButtonModel desc_button { get; set; }
            public RecommendADInfoModel ad_info { get; set; }
            public string cover_left_text_1 { get; set; }
            public string cover_left_text_2 { get; set; }
            public int cover_left_icon_1 { get; set; }

            public int cover_left_icon_2 { get; set; }
            public string left_text
            {
                get
                {
                    return $"{iconToText(cover_left_icon_1)}{cover_left_text_1 ?? ""} {iconToText(cover_left_icon_2)}{cover_left_text_2 ?? ""}";
                }
            }
            public string cover_right_text { get; set; }

            public string badge { get; set; }
            public bool showBadge
            {
                get
                {
                    return !string.IsNullOrEmpty(badge);
                }
            }

            public bool showCoverText
            {
                get
                {
                    return !string.IsNullOrEmpty(cover_left_text_1) || !string.IsNullOrEmpty(cover_left_text_2) || !string.IsNullOrEmpty(cover_right_text);
                }
            }
            public bool showRcmd
            {
                get
                {
                    return rcmd_reason_style != null;
                }
            }
            public bool showAD
            {
                get
                {
                    return ad_info != null && ad_info.creative_content != null;
                }
            }
            public string bottomText
            {
                get
                {
                    if (desc_button != null)
                    {
                        return desc_button.text;
                    }
                    return "";
                }
            }

            public string iconToText(int icon)
            {
                switch (icon)
                {
                    case 1:
                    case 6:
                        return "观看:";
                    case 2:
                        return "人气:";
                    case 3:
                        return "弹幕:";
                    case 4:
                        return "追番:";
                    case 7:
                        return "评论:";
                    default:
                        return "";
                }
            }
        }
        public class RecommendBannerItemModel
        {
            public int id { get; set; }
            public string title { get; set; }
            public string image { get; set; }


            public string hash { get; set; }

            public string uri { get; set; }

            public string request_id { get; set; }
            /// <summary>
            /// Server_type
            /// </summary>
            public int server_type { get; set; }
            /// <summary>
            /// Resource_id
            /// </summary>
            public int resource_id { get; set; }
            /// <summary>
            /// Index
            /// </summary>
            public int index { get; set; }
            /// <summary>
            /// Cm_mark
            /// </summary>
            public int cm_mark { get; set; }
        }
        public class RecommendThreePointV2ItemModel
        {

            public string idx { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public string subtitle { get; set; }
            public List<RecommendThreePointV2ItemReasonsModel> reasons { get; set; }

        }
        public class RecommendThreePointV2ItemReasonsModel
        {
            public int id { get; set; }
            public string name { get; set; }

        }
        public class RecommendItemArgsModel
        {
            public string up_id { get; set; }
            public string up_name { get; set; }
            public int rid { get; set; }
            public int tid { get; set; }
            public string tname { get; set; }
            public string rname { get; set; }
            public int aid { get; set; }

        }
        public class RecommendRcmdReasonStyleModel
        {
            public string text { get; set; }
            public string text_color { get; set; }

            public string bg_color { get; set; }

            public string border_color { get; set; }
            public string text_color_night { get; set; }
            public string bg_color_night { get; set; }
            public string border_color_night { get; set; }
            public int bg_style { get; set; }

        }
        public class RecommendDescButtonModel
        {
            public string text { get; set; }
            public string uri { get; set; }
        }
        public class RecommendADInfoModel
        {
            public string creative_id { get; set; }
            public RecommendADInfoCreativeModel creative_content { get; set; }
        }
        public class RecommendADInfoCreativeModel
        {
            public string description { get; set; }
            public string title { get; set; }
            public string image_url { get; set; }
            public string url { get; set; }
            public string click_url { get; set; }
        }
    }
}
