using BiliBili.UWP.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserCenterPage : Page
    {
        private Modules.UserCenterVM userCenterVM;
        readonly Modules.Account account;
        string mid = "";
        public UserCenterPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            account = new Modules.Account();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New|| e.NavigationMode == NavigationMode.Back)
            {
                if (e.Parameter == null)
                {
                    mid = ApiHelper.GetUserId();
                }
                else if (e.Parameter is object[])
                {
                    mid = (e.Parameter as object[])[0].ToString();
                }
                else
                {
                    mid = e.Parameter.ToString();
                }
                if (userCenterVM == null)
                {
                    userCenterVM = new Modules.UserCenterVM(mid);
                    await userCenterVM.GetUserDetail();
                }
                else if (userCenterVM.mid != mid)
                {
                    userCenterVM.mid = mid;
                    userCenterVM.is_self = mid == ApiHelper.GetUserId();
                    userCenterVM.UserCenterDetail = null;
                    userCenterVM.SubmitVideos.Clear();
                    await userCenterVM.GetUserDetail();
                }
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void btn_verify_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast(userCenterVM.UserCenterDetail.card.official_verify.desc);
        }

        private async void btnAddFollow_Click(object sender, RoutedEventArgs e)
        {
            var result = await account.Follow(mid);
            if (result.success)
            {
                userCenterVM.UserCenterDetail.relation = 1;
            }
            else
            {
                Utils.ShowMessageToast(result.message);
            }
        }

        private async void btnCancelFollow_Click(object sender, RoutedEventArgs e)
        {
            var result = await account.UnFollow(mid);
            if (result.success)
            {
                userCenterVM.UserCenterDetail.relation = -999;
            }
            else
            {
                Utils.ShowMessageToast(result.message);
            }

        }

        private void btnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EditProfilePage));
        }

        private void SubmitVideo_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), (e.ClickedItem as Modules.BiliBili.UWP.Modules.UserSubmitVideoModels.SubmitVideoItemModel).aid);
        }

        private void btnOpenLiveRoom_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(Live.LiveRoomPC), userCenterVM.UserCenterDetail.live.roomid);
        }

        private async void btnRefreshSubmitVideos_Click(object sender, RoutedEventArgs e)
        {
            await userCenterVM.SubmitVideos.RefreshAsync();
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 1)
            {
                if (ls_dynamic.Count() == 0)
                {
                    GetDynamic();
                }
            }
        }

        #region 动态

        //TODO 需要重写

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
                ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, mid, next, ApiHelper.GetTimeSpan_2, ApiHelper.GetUserId());
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
    }
}
