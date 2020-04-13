using BiliBili.UWP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BiliBili.UWP.Helper;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using BiliBili.UWP.Views;
using BiliBili.UWP.Pages.FindMore;
using Windows.UI;
using BiliBili.UWP.Controls;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserInfoPage : Page
    {
        public UserInfoPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }
        string Uid = string.Empty;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New||string.IsNullOrEmpty(Uid))
            {
                Uid = "";
                pivot.SelectedIndex = 0;
                getPage = 1;
                page = 1;
                MaxPage = 0;
                //list_AUser.Items.Clear();
                list_ASubit.Items.Clear();
                list_ACoin.ItemsSource = null;
                ls_dynamic.ClearData();
                //一堆IF嵌套，我TM自己都看不懂了 - -！
                if (e.Parameter == null || (e.Parameter as object[]).Length == 0)
                {
                    //读取自己的个人信息
                    btn_EditProfile.Visibility = Visibility.Visible;
                    btn_Chat.Visibility = Visibility.Collapsed;
                    btn_AddFollow.Visibility = Visibility.Collapsed;
                    btn_CancelFollow.Visibility = Visibility.Collapsed;
                    favbox.Visibility = Visibility.Visible;
                    Uid = ApiHelper.GetUserId();

                }
                else
                {
                    if ((e.Parameter as object[]).Length != 1)
                    {
                        btn_EditProfile.Visibility = Visibility.Visible;
                        favbox.Visibility = Visibility.Visible;
                        btn_Chat.Visibility = Visibility.Collapsed;
                        btn_AddFollow.Visibility = Visibility.Collapsed;
                        btn_CancelFollow.Visibility = Visibility.Collapsed;
                        Uid = ApiHelper.GetUserId();
                        pivot.SelectedIndex = (int)(e.Parameter as object[])[1];
                    }
                    else
                    {
                        //读取其他人信息

                        btn_EditProfile.Visibility = Visibility.Collapsed;
                        favbox.Visibility = Visibility.Collapsed;
                        btn_Chat.Visibility = Visibility.Visible;
                        btn_AddFollow.Visibility = Visibility.Visible;
                        btn_CancelFollow.Visibility = Visibility.Visible;
                        Uid = Convert.ToString((e.Parameter as object[])[0]);

                    }
                }

                GetUserInfo();
            }
        }
        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.Back)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        public async Task<List<GetUserFovBox>> GetUserFovBox()
        {

            try
            {
                string results = await WebClientClass.GetResults(new Uri("http://space.bilibili.com/ajax/fav/getBoxList?mid=" + ApiHelper.GetUserId()));
                //一层
                GetUserFovBox model1 = JsonConvert.DeserializeObject<GetUserFovBox>(results);
                if (model1.status)
                {
                    //二层
                    GetUserFovBox model2 = JsonConvert.DeserializeObject<GetUserFovBox>(model1.data.ToString());
                    //三层
                    List<GetUserFovBox> lsModel = JsonConvert.DeserializeObject<List<GetUserFovBox>>(model2.list.ToString());
                    return lsModel;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async void GetUserInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                try
                {
                    string results = await WebClientClass.PostResults(new Uri("http://space.bilibili.com/ajax/member/GetInfo"), "mid=" + Uid + "&_=" + ApiHelper.GetTimeSpan_2, "http://space.bilibili.com/" + Uid + "/");
                    JObject jObject = JObject.Parse(results);
                    img_bg.ImageSource = new BitmapImage(new Uri("https://i0.hdslb.com/" + jObject["data"]["toutu"]));
                }
                catch (Exception)
                {
                    img_bg.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Img/toutu.png"));
                }

                string url = string.Format("https://app.bilibili.com/x/v2/space?access_key={0}&appkey={1}&build=5250000&from=712&mobi_app=android&platform=android&ps=10&ts={2}&vmid={3}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan, Uid);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results1 = await WebClientClass.GetResults(new Uri(url));
                UserInfoModel m = JsonConvert.DeserializeObject<UserInfoModel>(results1);
                if (m.code == 0)
                {
                    if (m.data.relation == 1 && ApiHelper.IsLogin())
                    {
                        btn_AddFollow.Visibility = Visibility.Collapsed;
                        btn_CancelFollow.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btn_AddFollow.Visibility = Visibility.Visible;
                        btn_CancelFollow.Visibility = Visibility.Collapsed;
                    }

                    if (Uid == ApiHelper.GetUserId())
                    {
                        btn_AddFollow.Visibility = Visibility.Collapsed;
                        btn_CancelFollow.Visibility = Visibility.Collapsed;
                    }

                    if (m.data.season != null && m.data.season.item != null)
                    {
                        DT_0.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        DT_0.Visibility = Visibility.Visible;
                    }
                    if (m.data.live != null && m.data.live.liveStatus == 1)
                    {
                        live.Visibility = Visibility.Visible;
                        liveTitle.Text = m.data.live.title;
                    }
                    else
                    {
                        live.Visibility = Visibility.Collapsed;
                    }
                    if (m.data.card.vip.vipType != 0)
                    {
                        if (m.data.card.vip.vipType == 2)
                        {
                            viptype.Text = "年度大会员";
                        }
                        else
                        {
                            viptype.Text = "大会员";
                        }
                        bor_Vip.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bor_Vip.Visibility = Visibility.Collapsed;
                    }

                    if (m.data.card.level_info.current_level <= 3)
                    {
                        bor_Level.Background = new SolidColorBrush(new Windows.UI.Color() { R = 48, G = 161, B = 255, A = 200 });
                    }
                    else
                    {
                        if (m.data.card.level_info.current_level <= 6)
                        {
                            bor_Level.Background = new SolidColorBrush(new Windows.UI.Color() { R = 255, G = 48, B = 48, A = 200 });
                        }
                        else
                        {
                            bor_Level.Background = new SolidColorBrush(new Windows.UI.Color() { R = 255, G = 199, B = 45, A = 200 });
                        }
                    }

                    switch (m.data.card.sex)
                    {
                        case "男":
                            ic_female.Visibility = Visibility.Collapsed;
                            ic_gay.Visibility = Visibility.Collapsed;
                            ic_male.Visibility = Visibility.Visible;
                            break;
                        case "女":
                            ic_female.Visibility = Visibility.Visible;
                            ic_gay.Visibility = Visibility.Collapsed;
                            ic_male.Visibility = Visibility.Collapsed;
                            break;
                        default:
                            ic_female.Visibility = Visibility.Collapsed;
                            ic_gay.Visibility = Visibility.Visible;
                            ic_male.Visibility = Visibility.Collapsed;
                            break;
                    }
                    if (m.data.coin_archive != null && m.data.coin_archive.item != null)
                    {
                        list_ACoin.ItemsSource = m.data.coin_archive.item;

                    }
                    info.DataContext = m.data.card;
                    grid_UserInfo.DataContext = m.data.card;
                    data.DataContext = m.data;
                }

                user_GridView_FovBox.ItemsSource = await GetUserFovBox();
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("读取用户信息失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        int page = 1;
        int MaxPage = 99;
        bool IsLoading = true;
        //public async void GetUserAttention(int pageNum)
        //{
        //    try
        //    {
        //        pr_Load.Visibility = Visibility.Visible;
        //        IsLoading = true;


        //        string results = await WebClientClass.GetResults(new Uri("https://api.bilibili.com/x/relation/followings?vmid=" + Uid + "&ps=20&pn=" + pageNum + "&order=desc&jsonp=json"));
        //        //一层
        //        GetUserFollow model1 = JsonConvert.DeserializeObject<GetUserFollow>(results);
        //        if (model1.code == 0)
        //        {
        //            //二层
        //            // GetUserAttention model2 = JsonConvert.DeserializeObject<GetUserAttention>(model1.data.ToString());
        //            // MaxPage = model2.pages;
        //            //三层
        //            //  List<GetUserAttention> lsModel = JsonConvert.DeserializeObject<List<GetUserAttention>>(model2.list.ToString());
        //            foreach (GetUserFollow item in model1.data.list)
        //            {
        //                list_AUser.Items.Add(item);
        //            }
        //            page++;
        //        }
        //        else
        //        {
        //            Utils.ShowMessageToast(model1.message, 2000);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (list_AUser.Items.Count == 0)
        //        {
        //            Utils.ShowMessageToast("没有关注任何人", 2000);
        //        }
        //        else
        //        {
        //            Utils.ShowMessageToast("读取关注失败！", 2000);
        //        }
        //        //await new MessageDialog("读取关注失败！\r\n" + ex.Message).ShowAsync();
        //    }
        //    finally
        //    {
        //        pr_Load.Visibility = Visibility.Collapsed;
        //        IsLoading = false;
        //    }
        //}



        private void user_GridView_FovBox_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(MyCollectPage), (e.ClickedItem as GetUserFovBox).fav_box);
        }

        private void user_GridView_Bangumi_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as UserInfoModel).param);
            //this.Frame.Navigate(typeof(MyCollectPage), );
        }

        private void btn_AttBangumi_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(FollowSeasonPage), Modules.SeasonType.bangumi);
        }

        private void list_ASubit_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), (e.ClickedItem as UserInfoModel).param);
        }

        //private void btn_load_More_Atton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!IsLoading)
        //    {
        //        GetUserAttention(page);
        //    }
        //}

        //private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        //{
        //    if (sv.VerticalOffset == sv.ScrollableHeight)
        //    {
        //        if (!IsLoading)
        //        {
        //            GetUserAttention(page);
        //        }
        //    }
        //}

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int d = Convert.ToInt32(this.ActualWidth / 400);
            if (d > 3)
            {
                d = 3;
            }
            bor_Width.Width = this.ActualWidth / d - 22;
        }

        private void list_AUser_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(UserInfoPage), new object[] { ((GetUserFollow)e.ClickedItem).mid });
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 1:

                    if (ls_dynamic.Count() == 0)
                    {
                        GetDynamic();
                    }
                    break;
                case 2:

                    if (list_ASubit.Items.Count == 0)
                    {
                        getPage = 1;

                        await GetSubInfo(Uid);
                    }
                    break;
                //case 3:
                //    if (list_AUser.Items.Count == 0)
                //    {
                //        GetUserAttention(page);
                //    }
                //    break;
                default:
                    break;
            }
        }

        private async void btn_More_Video_Click(object sender, RoutedEventArgs e)
        {
            if (page <= MaxPage && !IsLoading)
            {
                subLoading = true;
                await GetSubInfo(Uid);
                subLoading = false;
            }
            else
            {
                Utils.ShowMessageToast("没有更多了...", 2000);
            }
        }

        private int getPage = 1;
        private async Task GetSubInfo(string uid)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                btn_More_Video.Visibility = Visibility.Collapsed;
                string results = await WebClientClass.GetResults(new Uri("http://space.bilibili.com/ajax/member/getSubmitVideos?mid=" + uid + "&pagesize=30" + "&page=" + getPage));
                //一层
                GetUserSubmit model1 = JsonConvert.DeserializeObject<GetUserSubmit>(results);
                //二层
                GetUserSubmit model2 = JsonConvert.DeserializeObject<GetUserSubmit>(model1.data.ToString());
                //三层
                List<GetUserSubmit> lsModel = JsonConvert.DeserializeObject<List<GetUserSubmit>>(model2.vlist.ToString());
                if (lsModel.Count != 0)
                {
                    foreach (GetUserSubmit item in lsModel)
                    {
                        list_ASubit.Items.Add(item);
                    }
                    getPage++;
                }
                else
                {
                    Utils.ShowMessageToast("加载完了", 3000);
                }



            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载投稿失败", 3000);
            }
            finally
            {
                btn_More_Video.Visibility = Visibility.Visible;
                pr_Load.Visibility = Visibility.Collapsed;
                if (list_ASubit.Items.Count == 0)
                {
                    Utils.ShowMessageToast("没有投稿", 3000);
                    btn_More_Video.Visibility = Visibility.Collapsed;
                }

            }
        }

        bool subLoading = false;
        private async void sv1_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv1.VerticalOffset == sv1.ScrollableHeight)
            {
                if (!subLoading)
                {
                    subLoading = true;
                    await GetSubInfo(Uid);
                    subLoading = false;
                }
            }

        }

        private void list_ASubit_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), (e.ClickedItem as GetUserSubmit).aid);
        }

        private async void btn_AddFollow_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/relation/modify");

                    string content = string.Format(
                        "access_key={0}&act=1&appkey={1}&build=45000&fid={2}&mobi_app=android&platform=android&re_src=90&ts={3}",
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, Uid, ApiHelper.GetTimeSpan_2
                        );
                    content += "&sign=" + ApiHelper.GetSign(content);
                    string result = await WebClientClass.PostResults(ReUri,
                        content
                     );
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        Utils.ShowMessageToast("关注成功", 3000);
                        btn_AddFollow.Visibility = Visibility.Collapsed;
                        btn_CancelFollow.Visibility = Visibility.Visible;

                        MessageCenter.SendLogined();
                    }
                    else
                    {
                        Utils.ShowMessageToast("关注失败\r\n" + json["message"].ToString(), 3000);

                    }

                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("关注时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);
            }
        }

        private async void btn_CancelFollow_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/relation/modify");

                    string content = string.Format(
                        "access_key={0}&act=2&appkey={1}&build=45000&fid={2}&mobi_app=android&platform=android&re_src=90&ts={3}",
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, Uid, ApiHelper.GetTimeSpan_2
                        );
                    content += "&sign=" + ApiHelper.GetSign(content);
                    string result = await WebClientClass.PostResults(ReUri,
                        content
                     );
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        Utils.ShowMessageToast("已取消关注", 3000);
                        btn_AddFollow.Visibility = Visibility.Visible;
                        btn_CancelFollow.Visibility = Visibility.Collapsed;


                        MessageCenter.SendLogined();
                    }
                    else
                    {
                        Utils.ShowMessageToast("取消关注失败\r\n" + json["message"].ToString(), 3000);

                    }

                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("关注时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);
            }
        }

        private void btn_Chat_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(ChatPage), new object[] { Uid, ChatType.New });
        }

        private void btn_EditProfile_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EditProfilePage));
        }



        #region 动态


     
        bool _loadDynamic = false;
        private async void GetDynamic()
        {
            try
            {

                pr_Load.Visibility = Visibility.Visible;
                _loadDynamic = true;
                var next = "0";
                if (ls_dynamic.Count() != 0)
                {
                    next = ls_dynamic.GetLastDynamicId();
                }

                string url = string.Format("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?_device=android&access_key={0}&appkey={1}&build=5250000&host_uid={2}&mobi_app=android&offset_dynamic_id={3}&platform=android&qn=32&src=bilih5&ts={4}&visitor_uid={5}",
                ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, Uid, next, ApiHelper.GetTimeSpan_2, ApiHelper.GetUserId());
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                results = results.Replace("default", "_default");
                DynamicModel dynamicModel = JsonConvert.DeserializeObject<DynamicModel>(results);
                if (dynamicModel.code == 0)
                {
                    if (dynamicModel.data.cards == null)
                    {
                        Utils.ShowMessageToast("没有更多动态了");
                        return;
                    }
                    ObservableCollection<DynamicCardsModel> cards = new ObservableCollection<DynamicCardsModel>();
                    foreach (var item in dynamicModel.data.cards)
                    {
                        if (item.desc.type != 32)
                        {
                            cards.Add(item);
                        }


                    }
                    ls_dynamic.LoadData(cards);
                }
                else
                {
                    Utils.ShowMessageToast(dynamicModel.msg);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("读取综合动态失败");
            }
            finally
            {
                _loadDynamic = false;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }


        private void ls_dynamic_LoadMore(object sender, string e)
        {
            if (_loadDynamic)
            {
                return;
            }
            GetDynamic();
        }

        private void ls_dynamic_Refresh(object sender, EventArgs e)
        {

        }

        #endregion

        private void btn_User_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_verify_Click(object sender, RoutedEventArgs e)
        {
            var data= (sender as HyperlinkButton).DataContext as UserInfoModel;

            if (data.official_verify.type==-1)
            {
                return;
            }
            if (data.official_verify.desc!="")
            {
                Utils.ShowMessageToast(data.official_verify.desc);
            }
            else if (data.official_verify.type==0)
            {
                Utils.ShowMessageToast("个人认证");
            }
            else if (data.official_verify.type == 1)
            {
                Utils.ShowMessageToast("企业认证");
            }


        }

        private void live_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(Live.LiveRoomPC), (data.DataContext as UserInfoModel).live.roomid);
        }
    }
}
