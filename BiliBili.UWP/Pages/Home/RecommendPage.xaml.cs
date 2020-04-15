using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.Home;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RecommendPage : Page
    {
        readonly RecommendVM recommendVM;
        readonly ToView toView;
        public RecommendPage()
        {
            this.InitializeComponent();
            this.Loaded += RecommendPage_Loaded;
            recommendVM = new RecommendVM();
            toView = new ToView();
        }

        private async void RecommendPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!recommendVM.Loading&& recommendVM.Items==null)
            {
                await recommendVM.LoadRecommend();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void ListDislike_ItemClick(object sender, ItemClickEventArgs e)
        {
            var reasons = e.ClickedItem as Modules.Home.RecommendModels.RecommendThreePointV2ItemReasonsModel;
            var threePoint = (sender as GridView).DataContext as Modules.Home.RecommendModels.RecommendThreePointV2ItemModel;
            await recommendVM.Dislike(threePoint.idx, threePoint, reasons);
        }

        private async void ls_feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as Modules.Home.RecommendModels.RecommendItemModel;
            if (data.uri == null)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(FollowSeasonPage), Modules.SeasonType.bangumi);
                return;
            }
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            if (Uri.TryCreate(data.uri, UriKind.Absolute, out var uri) && (uri.Scheme == "https" || uri.Scheme == "http)"))
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
            }
            else
            {
                await new ContentDialog()
                {
                    Title = "暂不支持跳转的类型",
                    Content = new TextBox()
                    {
                        Text = data.uri,
                        AcceptsReturn = true,
                        Height = 120
                    },
                    IsPrimaryButtonEnabled = true,
                    PrimaryButtonText = "知道了"
                }.ShowAsync();
            }

        }

        private async void btn_banner_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as Modules.Home.RecommendModels.RecommendBannerItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private async void ListMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Modules.Home.RecommendModels.RecommendThreePointV2ItemModel;
         
            if (item.type == "watch_later")
            {
                if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }

                var data = (sender as ListView).DataContext as Modules.Home.RecommendModels.RecommendItemModel;

                var result = await toView.AddToView(data.param);
                if (result.success)
                {
                    Utils.ShowMessageToast("添加成功");
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                }

            
                return;
            }

        }
    }
}
