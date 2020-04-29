using BiliBili.UWP.Controls;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Provider;
using System.Threading.Tasks;
using BiliBili.UWP.Modules;
using Windows.UI.Popups;
using BiliBili.UWP.Pages.User;


// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BanInfoPage : Page
    {
        Download download;

        public BanInfoPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            download = new Download();
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = txt_Name.Text;
            request.Data.Properties.Description = txt_desc.Text + "\r\n——分享自哔哩哔哩 UWP";
            request.Data.SetWebLink(new Uri("http://m.bilibili.com/bangumi/play/ss" + _banId));
        }

        //bool locks = false;





        string _banId = "";
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //if (e.NavigationMode== NavigationMode.New || Back)
            //{
            //mediaElement.Source = null;
            base.OnNavigatedTo(e);
            btn_OrderByUp.Visibility = Visibility.Visible;
            btn_OrderByDown.Visibility = Visibility.Collapsed;


            _banId = (e.Parameter as object[])[0].ToString();
            GetInfo();
            try
            {
                if (SecondaryTile.Exists(_banId))
                {
                    btn_unPin.Visibility = Visibility.Visible;
                    btn_Pin.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btn_unPin.Visibility = Visibility.Collapsed;
                    btn_Pin.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
            }

            pivot.SelectedIndex = 0;
            sv.ChangeView(null, 0, null);
            //}
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            comment.ClearComment();
            base.OnNavigatingFrom(e);
        }
        private async void GetInfo()
        {
            try
            {

                pr_Load.Visibility = Visibility.Visible;
                string uri = string.Format("https://api.bilibili.com/pgc/view/app/season?access_key={0}&appkey={1}&build=5341000&platform=android&season_id={2}&ts={3}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _banId, ApiHelper.GetTimeSpan);
                uri += "&sign=" + ApiHelper.GetSign(uri);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(uri));

                BangumiDataModel model = JsonConvert.DeserializeObject<BangumiDataModel>(results);

                // string results = await WebClientClass.GetResults(new Uri(uri));


                if (model.code == 0 || model.code == -404)
                {
                    if (model.code == -404)
                    {
                        string eresults = await WebClientClass.GetResultsUTF8Encode(new Uri("https://bangumi.bilibili.com/view/web_api/season?season_id=" + _banId));
                        eresults = eresults.Replace("ep_id", "episode_id");
                        eresults = eresults.Replace("cid", "danmaku");
                        eresults = eresults.Replace("aid", "av_id");
                        model = JsonConvert.DeserializeObject<BangumiDataModel>(eresults);

                        if (model.code != 0)
                        {
                            Utils.ShowMessageToast(model.message, 3000);
                            return;
                        }
                    }
                    else
                    {
                        if (model.result.rights.area_limit == 1 || (results.Contains("僅") && results.Contains("地區") && (model.result.episodes == null || model.result.episodes.Count == 0)))
                        {
                            //results = await WebClientClass.GetResultsUTF8Encode(new Uri("http://bangumi.bilibili.com/jsonp/seasoninfo/"+ _banId + ".ver?callback=seasonListCallback&jsonp=jsonp&_="));

                            //results = results.Replace("seasonListCallback(", "");
                            //results = results.Remove(results.Length-2,2);
                            string eresults = await WebClientClass.GetResultsUTF8Encode(new Uri("https://bangumi.bilibili.com/view/web_api/season?season_id=" + _banId));
                            eresults = eresults.Replace("ep_id", "episode_id");
                            eresults = eresults.Replace("cid", "danmaku");
                            eresults = eresults.Replace("aid", "av_id");
                            eresults = eresults.Replace("index_title", "long_title");
                            eresults = eresults.Replace("index", "title");

                            List<episodesModel> ep = JsonConvert.DeserializeObject<List<episodesModel>>(JObject.Parse(eresults)["result"]["episodes"].ToString());
                            model.result.episodes = ep;
                        }
                    }

                    if (model.result.new_ep == null && model.result.newest_ep != null)
                    {
                        model.result.new_ep = model.result.newest_ep;
                    }

                    int i = 0;
                    //model.result.pv_episodes = model.result.episodes.Where(x => x.section_type != 0).ToList();
                    //model.result.pv_episodes.ForEach(x => {
                    //    x.orderindex = i; i++;
                    //    if (x.index==null)
                    //    {
                    //        x.index = x.title;
                    //    }
                    //    if (x.index_title==null)
                    //    {
                    //        x.index_title = x.long_title;
                    //    }
                    //});

                    if (model.result.episodes != null)
                    {
                        model.result.episodes.ForEach(x =>
                        {
                            x.season_type = (model.result.season_type == 0) ? model.result.type : model.result.season_type;
                            x.orderindex = i;
                            i++;
                            if (x.index == null)
                            {
                                x.index = x.title;
                            }
                            if (x.index_title == null)
                            {
                                x.index_title = x.long_title;
                            }
                            if ((x.episode_id == null || x.episode_id == "") && x.id != 0)
                            {
                                x.episode_id = x.id.ToString();
                            }
                        });
                        model.result.episodes = model.result.episodes.OrderByDescending(x => x.orderindex).ToList();
                        //设置下载清晰度
                        if (model.result.episodes.Count != 0)
                        {
                            var data = await PlayurlHelper.GetAnimeQualities(new PlayerModel() {
                               Aid= model.result.episodes[0].av_id.ToString(), 
                               Mid= model.result.episodes[0].danmaku.ToString(), 
                               season_type= model.result.episodes[0].season_type
                            });
                            cb_Qu.ItemsSource = data.OrderByDescending(x=>x.qn).ToList();
                            cb_Qu.SelectedIndex = 0;
                        }

                    }



                    model.result.detail_media = await GetDetail(model.result.media_id.ToString());
                    model.result.evaluate = model.result.detail_media.evaluate;

                    this.DataContext = model.result;


                    gv_Play.ItemsSource = model.result.episodes;

                    // pvSection.ItemsSource= model.result.pv_episodes;
                    if (model.result.section != null && model.result.section.Count != 0)
                    {
                        pvSection.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pvSection.Visibility = Visibility.Collapsed;
                    }


                    last_view.Text = "";
                    if (model.result.user_status != null && model.result.user_status.follow != 0)
                    {
                        btn_Like.Visibility = Visibility.Collapsed;
                        btn_CancelLike.Visibility = Visibility.Visible;
                        if (model.result.user_status.progress != null && model.result.user_status.progress.last_ep_index != "")
                        {
                            var record = SqlHelper.GetVideoWatchRecord("ep"+model.result.user_status.progress.last_ep_id);
                            if (record == null)
                            {
                                SqlHelper.AddVideoWatchRecord(new ViewPostHelperClass()
                                {
                                    epId = "ep" + model.result.user_status.progress.last_ep_id,
                                    Post = model.result.user_status.progress.last_time,
                                    viewTime = DateTime.Now
                                });
                            }
                            else
                            {
                                record.Post = model.result.user_status.progress.last_time;
                                SqlHelper.UpdateVideoWatchRecord(record);
                            }
                            last_view.Tag = model.result.user_status.progress.last_ep_id;
                            last_view.Text = "(上次看到" + model.result.user_status.progress.last_ep_index + "话,点击继续播放)";
                        }

                    }
                    else
                    {
                        btn_Like.Visibility = Visibility.Visible;
                        btn_CancelLike.Visibility = Visibility.Collapsed;
                    }



                    if (model.result.rank != null)
                    {
                        BangumiInfoModel rank = JsonConvert.DeserializeObject<BangumiInfoModel>(model.result.rank.ToString());
                        grid_Cb.DataContext = rank;
                        grid_Cb.Visibility = Visibility.Visible;
                        txt_NotCb.Visibility = Visibility.Collapsed;
                        GetRankInfo();
                    }
                    else
                    {
                        txt_NotCb.Visibility = Visibility.Visible;
                        grid_Cb.Visibility = Visibility.Collapsed;
                    }

                    if (model.result.seasons != null && model.result.seasons.Count != 0)
                    {
                        Grid_About.Visibility = Visibility.Visible;

                        WrapPanel_About.Children.Clear();
                        if (model.result.seasons.Count == 1)
                        {
                            Grid_About.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            foreach (BangumiseasonsModel item in model.result.seasons)
                            {

                                HyperlinkButton btn = new HyperlinkButton();
                                btn.DataContext = item;
                                btn.Margin = new Thickness(0, 0, 10, 0);
                                btn.Content = item.season_title;
                                btn.Foreground = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;
                                if (item.season_id.ToString() == _banId)
                                {
                                    btn.IsEnabled = false;
                                }
                                btn.Click += Btn_Click; ;
                                WrapPanel_About.Children.Add(btn);

                            }
                        }

                        //Grid_About
                    }
                    else
                    {
                        Grid_About.Visibility = Visibility.Collapsed;
                    }


                    if (model.result.styles != null)
                    {
                        WrapPanel_tag.Children.Clear();
                        foreach (var item in model.result.styles)
                        {
                            HyperlinkButton btn = new HyperlinkButton();
                            btn.DataContext = item;
                            btn.Margin = new Thickness(0, 0, 10, 0);
                            btn.Content = item.name;
                            btn.Foreground = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;
                            btn.Click += Btn_Click1; ;
                            WrapPanel_tag.Children.Add(btn);
                        }
                    }

                    if (gv_Play.Items.Count != 0)
                    {
                        gvEpisodes.Visibility = Visibility.Visible;
                        cb_H.SelectedIndex = gv_Play.Items.Count - 1;
                        gv_Play.SelectedIndex = 0;
                    }
                    else
                    {
                        gvEpisodes.Visibility = Visibility.Collapsed;
                        Utils.ShowMessageToast("尚未开播或不支持你所在地区", 3000);
                    }

                    if (model.result.payment != null && model.result.payment.dialog != null)
                    {
                        dialog.Visibility = Visibility.Visible;
                        //付费显示弹窗
                        //MessageDialog dialog = new MessageDialog(model.result.dialog.desc, model.result.dialog.title);
                        //if (model.result.dialog.btn_left != null)
                        //{
                        //    dialog.Commands.Add(new UICommand(model.result.dialog.btn_left.title, (e) =>
                        //    {
                        //        if (model.result.dialog.btn_left.type == "pay" || model.result.dialog.btn_left.type == "vip")
                        //        {
                        //            Utils.ShowMessageToast("UWP端不支持付费功能，请到网页或手机端购买后观看");
                        //        }
                        //    }));
                        //}
                        //if (model.result.dialog.btn_right != null)
                        //{
                        //    dialog.Commands.Add(new UICommand(model.result.dialog.btn_right.title, (e) =>
                        //    {
                        //        if (model.result.dialog.btn_right.type == "pay" || model.result.dialog.btn_right.type == "vip")
                        //        {
                        //            Utils.ShowMessageToast("UWP端不支持付费功能，请到网页或手机端购买后观看");
                        //        }
                        //    }));
                        //}
                        //dialog.Commands.Add(new UICommand("取消"));
                        //await dialog.ShowAsync();
                    }
                    else
                    {
                        dialog.Visibility = Visibility.Collapsed;
                    }

                }
                else
                {
                    Utils.ShowMessageToast(model.message, 3000);
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    Utils.ShowMessageToast("检查你的网络连接！", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("发生错误\r\n" + ex.Message, 3000);
                }
            }
            finally
            {
                Page_SizeChanged(null, null);
                btn_HideAll_Click(null, null);

                pr_Load.Visibility = Visibility.Collapsed;
            }
        }


        private async Task<BangumiDetailModel> GetDetail(string mediaId)
        {
            try
            {
                string url = string.Format("https://bangumi.bilibili.com/media/api/detail?appkey={0}&build=5250000&media_id={1}&platform=android&ts={2}", ApiHelper.AndroidKey.Appkey, mediaId, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResults(new Uri(url));
                BangumiDetailModel bangumiDetailModel = JsonConvert.DeserializeObject<BangumiDetailModel>(results);
                if (bangumiDetailModel.code == 0)
                {
                    return bangumiDetailModel.result;
                }
                else
                {
                    Utils.ShowMessageToast(bangumiDetailModel.message);
                    return new BangumiDetailModel();
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("无法加载内容");
                return new BangumiDetailModel();
            }
        }


        private void Btn_Click1(object sender, RoutedEventArgs e)
        {
            //TO DO TAG
        }
        //bool Back = false;
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            //TO DO Seaseon
            //Back = true;
            this.Frame.Navigate(typeof(BanInfoPage), new object[] { ((sender as HyperlinkButton).DataContext as BangumiseasonsModel).season_id });
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as TextBlock).MaxLines == 3)
            {
                (sender as TextBlock).MaxLines = 0;
            }
            else
            {
                (sender as TextBlock).MaxLines = 3;
            }
        }
        private void Share_Click(object sender, RoutedEventArgs e)
        {


            Utils.SetClipboard("http://m.bilibili.com/bangumi/play/ss" + _banId);

            Utils.ShowMessageToast("已将内容复制到剪切板", 3000);
        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
        private async void GetRankInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;

                string url = string.Empty;
                if (cb_Cb.SelectedIndex == 0)
                {
                    url = string.Format("http://bangumi.bilibili.com/sponsor/rank/get_sponsor_week_list?access_key={0}&appkey={1}&build=418000&mobi_app=android&page=1&pagesize=25&platform=android&season_id={2}&ts={3}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _banId, ApiHelper.GetTimeSpan);
                }
                else
                {
                    url = string.Format("http://bangumi.bilibili.com/sponsor/rank/get_sponsor_total?access_key={0}&appkey={1}&build=418000&mobi_app=android&page=1&pagesize=25&platform=android&season_id={2}&ts={3}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _banId, ApiHelper.GetTimeSpan);
                }
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                CBRankModel model = JsonConvert.DeserializeObject<CBRankModel>(results);
                if (model.code == 0)
                {
                    CBRankModel resultModel = JsonConvert.DeserializeObject<CBRankModel>(model.result.ToString());
                    List<CBRankModel> ls = JsonConvert.DeserializeObject<List<CBRankModel>>(resultModel.list.ToString());
                    list_Rank.ItemsSource = ls;
                }
                else
                {
                    Utils.ShowMessageToast("读取承包失败," + model.message, 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取承包失败", 3000);
                //throw;
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
        private void btn_CB_Click(object sender, RoutedEventArgs e)
        {
            if (!UserManage.IsLogin())
            {
                Utils.ShowMessageToast("请先登录！", 3000);
                return;
            }
            Flyout.ShowAttachedFlyout((HyperlinkButton)sender);
            rb_5.IsChecked = true;
        }

        private void rb_5_Checked(object sender, RoutedEventArgs e)
        {
            txt_Money.Text = "5";
        }

        private void rb_10_Checked(object sender, RoutedEventArgs e)
        {
            txt_Money.Text = "10";
        }

        private void rb_50_Checked(object sender, RoutedEventArgs e)
        {
            txt_Money.Text = "50";
        }

        private void rb_450_Checked(object sender, RoutedEventArgs e)
        {
            txt_Money.Text = "450";
        }

        private void rb_ZDY_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btn_OKCB_Click(object sender, RoutedEventArgs e)
        {
            int b = 0;
            if (int.TryParse(txt_Money.Text.ToString(), out b))
            {
                fy.Hide();
                createOrder(b);
            }
            else
            {
                Utils.ShowMessageToast("输入金额错误,必须为整数", 2000);
            }
        }

        private async void createOrder(int money)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                Utils.ShowMessageToast("开始创建订单", 2000);
                string tokenString = await WebClientClass.GetResults(new Uri("http://bangumi.bilibili.com/web_api/get_token"));
                TokenModel tokenMess = JsonConvert.DeserializeObject<TokenModel>(tokenString);
                if (tokenMess.code == 0)
                {
                    TokenModel token = JsonConvert.DeserializeObject<TokenModel>(tokenMess.result.ToString());
                    string results = await WebClientClass.PostResults(new Uri("http://bangumi.bilibili.com/sponsor/payweb/create_order"), string.Format("pay_method=0&season_id={0}&amount={1}&token={2}", _banId, money, token.token));
                    OrderModel orderMess = JsonConvert.DeserializeObject<OrderModel>(results);
                    if (orderMess.code == 0)
                    {
                        OrderModel orderModel = JsonConvert.DeserializeObject<OrderModel>(orderMess.result.ToString());
                        this.Frame.Navigate(typeof(WebPage), new object[] { orderModel.cashier_url });
                    }
                    else
                    {

                        Utils.ShowMessageToast("订单创建失败," + orderMess.data, 3000);

                    }
                }
                else
                {
                    Utils.ShowMessageToast("读取Token失败," + tokenMess.message, 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("订单创建失败,请稍后重试", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }

        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    cb_H.Visibility = Visibility.Collapsed;
                    cb_Cb.Visibility = Visibility.Collapsed;
                    com_bar.Visibility = Visibility.Visible;
                    break;
                //case 1:
                //    cb_H.Visibility = Visibility.Collapsed;
                //    cb_Cb.Visibility = Visibility.Collapsed;
                //    com_bar.Visibility = Visibility.Collapsed;
                //    break;
                case 1:
                    cb_H.Visibility = Visibility.Visible;
                    cb_Cb.Visibility = Visibility.Collapsed;
                    com_bar.Visibility = Visibility.Collapsed;
                    //Down_ComBar.Visibility = Visibility.Collapsed;
                    if (comment.CommentCount == 0)
                    {
                        await Task.Delay(200);
                        if (cb_H.SelectedItem != null)
                        {
                            comment.LoadComment(new LoadCommentInfo()
                            {
                                commentMode = CommentMode.Video,
                                conmmentSortMode = ConmmentSortMode.All,
                                oid = (cb_H.SelectedItem as episodesModel).av_id
                            });
                        }
                    }

                    break;
                case 2:
                    cb_H.Visibility = Visibility.Collapsed;
                    cb_Cb.Visibility = Visibility.Visible;
                    com_bar.Visibility = Visibility.Collapsed;
                    //Down_ComBar.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void cb_H_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_H.SelectedItem != null && pr_Load.Visibility == Visibility.Collapsed)
            {
                comment.LoadComment(new LoadCommentInfo()
                {
                    commentMode = CommentMode.Video,
                    conmmentSortMode = ConmmentSortMode.All,
                    oid = (cb_H.SelectedItem as episodesModel).av_id
                });

            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void cb_Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Cb.Items.Count != 0 && _banId.Length != 0)
            {
                GetRankInfo();
            }
        }

        private async void btn_GoBrowser_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://m.bilibili.com/bangumi/play/ss" + _banId + "/"));
        }

        private void gv_Play_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void comment_OpenUser(string id)
        {
            this.Frame.Navigate(typeof(UserCenterPage), id );
        }

        private void Video_Refresh_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = 0;
            GetInfo();
        }

        private async void btn_Like_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                Utils.ShowMessageToast("请先登录！", 3000);
                return;
            }
            try
            {
                int stype = (this.DataContext as BangumiDataModel).season_type;
                var url = string.Format("https://bangumi.bilibili.com/follow/api/season/follow?access_key={0}&appkey={1}&build=5250000&mobi_app=android&platform=android&season_id={2}&season_type={3}&ts={4}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _banId, stype, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));

                JObject json = JObject.Parse(results);
                if ((int)json["code"] == 0)
                {
                    btn_CancelLike.Visibility = Visibility.Visible;
                    btn_Like.Visibility = Visibility.Collapsed;
                    if (stype == 1)
                    {
                        Utils.ShowMessageToast("已添加到我的追番");
                    }
                    else
                    {
                        Utils.ShowMessageToast("已添加到我的-影视收藏");
                    }
                }
                else
                {
                    Utils.ShowMessageToast("订阅失败!", 3000);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("订阅发生错误", 3000);
            }
        }

        private async void btn_CancelLike_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                Utils.ShowMessageToast("请先登录！", 3000);
                return;
            }
            try
            {
                int stype = (this.DataContext as BangumiDataModel).season_type;
                var url = string.Format("https://bangumi.bilibili.com/follow/api/season/unfollow?access_key={0}&appkey={1}&build=5250000&mobi_app=android&platform=android&season_id={2}&season_type={3}&ts={4}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _banId, stype, ApiHelper.GetTimeSpan);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                JObject json = JObject.Parse(results);
                if ((int)json["code"] == 0)
                {
                    btn_CancelLike.Visibility = Visibility.Collapsed;
                    btn_Like.Visibility = Visibility.Visible;
                    Utils.ShowMessageToast("取消订阅成功!", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("取消订阅失败!", 3000);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("取消订阅发生错误", 3000);
            }
        }

        private void btn_ALL_Click(object sender, RoutedEventArgs e)
        {
            if (gv_Play.SelectedItems.Count == gv_Play.Items.Count)
            {
                gv_Play.SelectedItems.Clear();
            }
            else
            {
                gv_Play.SelectAll();
            }
        }

        private async void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (gv_Play.SelectedItems.Count == 0)
            {
                return;
            }
            var info = gv_Play.SelectedItem as episodesModel;

            int i = 1;
            pr_Load.Visibility = Visibility.Visible;
            foreach (episodesModel item in gv_Play.SelectedItems)
            {
                if (item.IsDowned == Visibility.Collapsed)
                {
                    var downloadUrl = await download.GetSeasonDownloadUrl(item.av_id.ToString(), item.danmaku.ToString(), _banId.ToInt32(), item.season_type, cb_Qu.SelectedItem as QualityModel, ApiHelper.access_key, ApiHelper.GetUserId());
                    if (!downloadUrl.success)
                    {
                        await new MessageDialog($"{item.index} {item.index_title}下载失败:{downloadUrl.message}").ShowAsync();
                        continue;
                    }

                    await DownloadHelper2.CreateDownload(new DownloadTaskModel()
                    {
                        downloadMode = DownloadMode.Anime,
                        sid = _banId,
                        cid = item.danmaku.ToString(),
                        epIndex = item.orderindex,
                        epid = item.episode_id,
                        epTitle = item.index + " " + item.index_title,
                        thumb = (this.DataContext as BangumiDataModel).cover,
                        quality = cb_Qu.SelectedIndex,
                        title = (this.DataContext as BangumiDataModel).title
                    }, downloadUrl.data);



                    //var vitem = new PlayerModel() { Aid = _banId, Mid = item.danmaku.ToString(), Mode = PlayMode.Video, No = item.index, VideoTitle = item.index_title, Title = (this.DataContext as BangumiInfoModel).title ,episode_id=item.episode_id};

                    //DownloadModel m = new DownloadModel();
                    //m.folderinfo = new FolderListModel()
                    //{
                    //    id = _banId,
                    //    desc = txt_desc.Text,
                    //    title = txt_Name.Text,
                    //    isbangumi = true,
                    //    thumb = (this.DataContext as BangumiInfoModel).cover

                    //};
                    //m.videoinfo = new VideoListModel()
                    //{
                    //    id = _banId,
                    //    mid = vitem.Mid,
                    //    part = i,
                    //    partTitle = vitem.No + " " + vitem.VideoTitle,
                    //    videoUrl = await ApiHelper.GetBiliUrl_Ban(vitem, cb_Qu.SelectedIndex + 1),
                    //    title = txt_Name.Text
                    //};

                    //DownloadHelper.StartDownload(m);
                }

                i++;

            }

            Utils.ShowMessageToast("任务已加入下载", 3000);
            gv_Play.SelectionMode = ListViewSelectionMode.None;
            gv_Play.IsItemClickEnabled = true;
            Down_ComBar.Visibility = Visibility.Collapsed;
            com_bar.Visibility = Visibility.Visible;
            pr_Load.Visibility = Visibility.Collapsed;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            gv_Play.SelectionMode = ListViewSelectionMode.None;
            gv_Play.IsItemClickEnabled = true;
            Down_ComBar.Visibility = Visibility.Collapsed;
            com_bar.Visibility = Visibility.Visible;
        }

        private void btn_Download_Click(object sender, RoutedEventArgs e)
        {
            if (gv_Play.Items != null)
            {
                //if (gv_Play.Items.Count == 1)
                //{
                //    var item = gv_Play.Items[0] as episodesModel;

                //    if (item.IsDowned == Visibility.Collapsed)
                //    {


                //        DownloadHelper2.CreateDownload(new DownloadTaskModel()
                //        {
                //            downloadMode = DownloadMode.Anime,
                //            sid = _banId,
                //            cid = item.danmaku.ToString(),
                //            epIndex = item.orderindex,
                //            epid = item.episode_id,
                //            epTitle = item.index + " " + item.index_title,
                //            thumb = (this.DataContext as BangumiDataModel).cover,
                //            quality = cb_Qu.SelectedIndex + 1,
                //            title = (this.DataContext as BangumiDataModel).title
                //        });




                //        //var vitem = new PlayerModel() { Aid = _banId, Mid = item.danmaku.ToString(), Mode = PlayMode.Video, No = item.index, VideoTitle = item.index_title, Title = (this.DataContext as BangumiInfoModel).title };

                //        //DownloadModel m = new DownloadModel();
                //        //m.folderinfo = new FolderListModel()
                //        //{
                //        //    id = _banId,
                //        //    desc = txt_desc.Text,
                //        //    title = txt_Name.Text,
                //        //    isbangumi = true,
                //        //    thumb = (this.DataContext as BangumiInfoModel).cover

                //        //};
                //        //m.videoinfo = new VideoListModel()
                //        //{
                //        //    id = _banId,
                //        //    mid = vitem.Mid,
                //        //    part = 1,
                //        //    partTitle = vitem.No + " " + vitem.VideoTitle,
                //        //    videoUrl = await ApiHelper.GetBiliUrl_Ban(vitem, cb_Qu.SelectedIndex + 1),
                //        //    title = txt_Name.Text
                //        //};

                //        //DownloadHelper.StartDownload(m);
                //        Utils.ShowMessageToast("任务已加入下载", 3000);
                //    }
                //    else
                //    {
                //        Utils.ShowMessageToast("此视频已下载", 3000);
                //    }

                //}
                //else
                //{
                gv_Play.SelectionMode = ListViewSelectionMode.Multiple;
                gv_Play.IsItemClickEnabled = false;
                Utils.ShowMessageToast("请选中要下载的分P视频，点击确定", 3000);
                Down_ComBar.Visibility = Visibility.Visible;
                com_bar.Visibility = Visibility.Collapsed;
                //}


                //players.LoadPlayer(ls, gv_Play.SelectedIndex);
            }


        }

        private void btn_HideAll_Click(object sender, RoutedEventArgs e)
        {
            btn_HideAll.Visibility = Visibility.Collapsed;
            btn_ShowAll.Visibility = Visibility.Visible;
            if (_rows > 3)
            {
                gv_Play.Height = 54 * 3;
            }
            else
            {
                gv_Play.Height = double.NaN;
            }
        }

        private void btn_ShowAll_Click(object sender, RoutedEventArgs e)
        {
            btn_HideAll.Visibility = Visibility.Visible;
            btn_ShowAll.Visibility = Visibility.Collapsed;
            gv_Play.Height = double.NaN;
        }

        private void btn_OrderByUp_Click(object sender, RoutedEventArgs e)
        {
            btn_OrderByUp.Visibility = Visibility.Collapsed;
            btn_OrderByDown.Visibility = Visibility.Visible;
            var ls = gv_Play.ItemsSource as List<episodesModel>;
            gv_Play.ItemsSource = ls.OrderBy(x => x.orderindex).ToList();

        }

        private void btn_OrderByDown_Click(object sender, RoutedEventArgs e)
        {
            btn_OrderByUp.Visibility = Visibility.Visible;
            btn_OrderByDown.Visibility = Visibility.Collapsed;
            var ls = gv_Play.ItemsSource as List<episodesModel>;
            gv_Play.ItemsSource = ls.OrderByDescending(x => x.orderindex).ToList();
        }
        int _rows = 0;
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {




        }


        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Width != 0)
            {
                int i = 2;
                if (availableSize.Width > 500)
                {
                    i = 3;
                    if (gv_Play.Items.Count % 3 != 0)
                    {
                        _rows = (gv_Play.Items.Count / 3) + 1;
                    }
                    else
                    {
                        _rows = gv_Play.Items.Count / 3;
                    }

                    bor_Width.Width = availableSize.Width / i - 39;
                }
                else
                {
                    if (gv_Play.Items.Count % 2 != 0)
                    {
                        _rows = (gv_Play.Items.Count / 2) + 1;
                    }
                    else
                    {
                        _rows = gv_Play.Items.Count / 2;
                    }
                    bor_Width.Width = availableSize.Width / i - 42;
                }
            }

            return base.MeasureOverride(availableSize);
        }


        private void gv_Play_ItemClick(object sender, ItemClickEventArgs e)
        {
            var info = e.ClickedItem as episodesModel;
            openPlayer(info);
        }

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var x = new ContentDialog();
            StackPanel st = new StackPanel();
            st.Children.Add(new Image()
            {
                Source = new BitmapImage(new Uri((this.DataContext as BangumiDataModel).cover))
            });

            x.Content = st;
            x.PrimaryButtonText = "关闭";
            x.IsPrimaryButtonEnabled = true;
            await x.ShowAsync();
        }

        private void PostHistory(string _aid, string _title)
        {
            try
            {
                if (SqlHelper.GetComicIsOnHistory(_aid))
                {
                    SqlHelper.UpdateComicHistory(new HistoryClass()
                    {
                        _aid = _aid,
                        image = "",
                        title = txt_Name.Text + " - " + _title,
                        up = "",
                        lookTime = DateTime.Now
                    });


                }
                else
                {
                    SqlHelper.AddCommicHistory(new HistoryClass()
                    {
                        _aid = _aid,
                        image = "",
                        title = txt_Name.Text + " - " + _title,
                        up = "",
                        lookTime = DateTime.Now
                    });
                }
            }
            catch (Exception)
            {
            }
        }

        private void btn_DownManage_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Download2Page));
        }


        private async void btn_Pin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string appbarTileId = _banId;
                var str = (this.DataContext as BangumiDataModel).cover;
                StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                StorageFile file = await localFolder.CreateFileAsync(_banId + ".jpg", CreationCollisionOption.OpenIfExists);
                if (file != null)
                {
                    //img_Image
                    IBuffer bu = await WebClientClass.GetBuffer(new Uri((str)));
                    CachedFileManager.DeferUpdates(file);
                    await FileIO.WriteBufferAsync(file, bu);
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        Uri logo = new Uri("ms-appdata:///local/" + _banId + ".jpg");
                        string tileActivationArguments = "bangumi," + _banId;
                        string displayName = (this.DataContext as BangumiDataModel).title;

                        TileSize newTileDesiredSize = TileSize.Square150x150;

                        SecondaryTile secondaryTile = new SecondaryTile(appbarTileId,
                                                                        displayName,
                                                                        tileActivationArguments,
                                                                        logo,
                                                                        newTileDesiredSize);


                        secondaryTile.VisualElements.Square44x44Logo = logo;
                        secondaryTile.VisualElements.Wide310x150Logo = logo;
                        secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                        secondaryTile.VisualElements.ShowNameOnWide310x150Logo = true;


                        await secondaryTile.RequestCreateAsync();
                        btn_Pin.Visibility = Visibility.Collapsed;
                        btn_unPin.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Utils.ShowMessageToast("创建失败", 3000);
                    }
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("创建失败", 3000);
            }





            //await secondaryTile.UpdateAsync();



        }

        private async void btn_unPin_Click(object sender, RoutedEventArgs e)
        {

            SecondaryTile secondaryTile = new SecondaryTile(_banId);
            await secondaryTile.RequestDeleteAsync();
            btn_Pin.Visibility = Visibility.Visible;
            btn_unPin.Visibility = Visibility.Collapsed;
        }

        private void btn_ShowComment_Click(object sender, RoutedEventArgs e)
        {
            comment.ShowCommentBox();
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (comment.HasMore)
                {
                    comment.LoadMore();
                }
            }
        }

        private void btn_AddToView_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("右键或长按集数添加到稍后再看");
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            menu.ShowAt((FrameworkElement)sender);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            menu.ShowAt((FrameworkElement)sender);
        }

        private async void menuAdd_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as episodesModel;
            ToView toView = new ToView();
            var data = await toView.AddToView(item.av_id);
            if (data.success)
            {
                Utils.ShowMessageToast("添加成功");
            }
            else
            {
                Utils.ShowMessageToast("添加失败 ");
            }
        }

        private void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("UWP端不支持付费功能，请到网页或手机端购买后观看");
        }

        private void Txt_desc_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            txt_desc.IsTextSelectionEnabled = !txt_desc.IsTextSelectionEnabled;
        }

        private void gv_Pv_ItemClick(object sender, ItemClickEventArgs e)
        {
            var info = e.ClickedItem as episodesModel;
            List<PlayerModel> ls = new List<PlayerModel>();
            // int i = 1;
            var items = (sender as GridView).ItemsSource as List<episodesModel>;

            foreach (episodesModel item in items)
            {
                ls.Add(new PlayerModel()
                {
                    season_type = item.season_type,
                    banId = _banId,
                    banInfo = item,
                    Aid = item.av_id,
                    Mid = item.danmaku.ToString(),
                    Mode = PlayMode.Video,
                    No = item.orderindex.ToString(),
                    VideoTitle = item.title + " " + item.long_title,
                    Title = txt_Name.Text,
                    episode_id = item.episode_id,
                    index = item.orderindex
                });
            }


            ls = ls.OrderBy(x => x.index).ToList();

            int index = ls.IndexOf(ls.Find(x => x.Mid == info.danmaku.ToString()));

            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, index });
            PostHistory(ls[index].Aid, ls[index].VideoTitle);
        }

        private void openLastWatch_Click(object sender, RoutedEventArgs e)
        {
            var items = gv_Play.ItemsSource as List<episodesModel>;
            var last_ep = last_view.Tag.ToString();
            var info = items.FirstOrDefault(x => x.episode_id == last_ep);
            if (info==null)
            {
                Utils.ShowMessageToast("尝试打开不存在的剧集");
                return;
            }
            openPlayer(info);
        }
        private void openPlayer(episodesModel info)
        {
            List<PlayerModel> ls = new List<PlayerModel>();
            var items = gv_Play.ItemsSource as List<episodesModel>;
            
            foreach (episodesModel item in items)
            {
                if (item.IsDowned == Visibility.Visible)
                {
                    ls.Add(new PlayerModel()
                    {
                        season_type = item.season_type,
                        banId = _banId,
                        Aid = item.av_id,
                        Mid = item.danmaku.ToString(),
                        Mode = PlayMode.Local,
                        No = item.orderindex.ToString(),
                        VideoTitle = item.title + " " + item.long_title,
                        Title = txt_Name.Text,
                        episode_id = item.episode_id,
                        Path = DownloadHelper2.downloadeds[item.danmaku.ToString()],
                        banInfo = item
                    });
                }
                else
                {
                    ls.Add(new PlayerModel()
                    {
                        season_type = item.season_type,
                        banId = _banId,
                        banInfo = item,
                        Aid = item.av_id,
                        Mid = item.danmaku.ToString(),
                        Mode = PlayMode.Bangumi,
                        No = item.orderindex.ToString(),
                        VideoTitle = item.title + " " + item.long_title,
                        Title = txt_Name.Text,
                        episode_id = item.episode_id,
                        index = item.orderindex
                    });
                }
                //  i++;
            }


            ls = ls.OrderBy(x => x.index).ToList();

            int index = ls.IndexOf(ls.Find(x => x.Mid == info.danmaku.ToString()));

            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, index });
            PostHistory(ls[index].Aid, ls[index].VideoTitle);
        }
        //private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //}
    }



    public class BangumiDetailModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public BangumiDetailModel result { get; set; }
        public string actor { get; set; }
        public string alias { get; set; }
        public string cover { get; set; }
        public string evaluate { get; set; }

        public int media_id { get; set; }
        public string origin_name { get; set; }
        public string staff { get; set; }

        public List<BangumiDetailModel> area { get; set; }
        public string area_str
        {
            get
            {
                if (area == null || area.Count == 0)
                {
                    return "未知地区";
                }
                else
                {
                    return area[0].name;
                }
            }
        }

        public List<BangumiDetailModel> style { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int type_id { get; set; }
        public string type_name { get; set; }

        public BangumiDetailModel publish { get; set; }
        public int is_finish { get; set; }
        public int is_multi { get; set; }
        public int is_started { get; set; }
        public string pub_date { get; set; }
        public string pub_date_show { get; set; }

    }


    public class BangumiDataModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public BangumiDataModel result { get; set; }



        public object rank { get; set; }
        public string cover { get; set; }
        public string evaluate { get; set; }
        public string link { get; set; }
        public int media_id { get; set; }
        public int mode { get; set; }
        public int season_status { get; set; }
        public int season_type { get; set; }
        public int type { get; set; } = 1;
        public string season_title { get; set; }
        public string share_url { get; set; }
        public string square_cover { get; set; }
        public string title { get; set; }

        public List<episodesModel> episodes { get; set; }

        public List<episodesModel> pv_episodes { get; set; }
        public BangumipublishModel publish { get; set; }
        public Banguminewest_epModel new_ep { get; set; }
        public Banguminewest_epModel newest_ep { get; set; }
        public BangumiratingModel rating { get; set; }
        public BangumirightsModel rights { get; set; }
        public List<BangumiseasonsModel> seasons { get; set; }
        public BangumistatModel stat { get; set; }
        public Bangumiuser_statusModel user_status { get; set; }

        public string detail { get; set; }

        public BangumiDetailModel detail_media { get; set; }
        public BangumiPaymentModel payment { get; set; }

        public List<BangumiStyleModel> styles { get; set; }

        public List<BangumiSectionModel> section { get; set; }

    }

    public class BangumiStyleModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class BangumiPaymentModel
    {
        public BangumiDialogModel dialog { get; set; }
    }

    public class BangumiSectionModel
    {
        public int id { get; set; }
        public int episode_id { get; set; }
        public string title { get; set; }
        public List<episodesModel> episodes { get; set; }
    }

    public class BangumiDialogModel
    {
        public string desc { get; set; }
        public string title { get; set; }
        public btn_rightModel btn_right { get; set; }
        public btn_rightModel btn_left { get; set; }
    }
    public class btn_rightModel
    {
        public string type { get; set; }
        public string title { get; set; }
    }

    public class Bangumiuser_statusModel
    {
        public int follow { get; set; }
        public int is_vip { get; set; }
        public int pay { get; set; }
        public int pay_pack_paid { get; set; }
        public int sponsor { get; set; }
        public Bangumiuser_statusModel progress { get; set; }
        public int last_ep_id { get; set; }
        public string last_ep_index { get; set; }
        public int last_time { get; set; }
    }
    public class BangumistatModel
    {
        public int danmakus { get; set; }
        public int favorites { get; set; }
        public int views { get; set; }

        public string PlayCount
        {
            get
            {
                if (views > 10000)
                {
                    return ((double)views / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return views.ToString();
                }
            }
        }

        public string favoritesCount
        {
            get
            {
                if (favorites > 10000)
                {
                    return ((double)favorites / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return favorites.ToString();
                }
            }
        }

    }
    public class BangumiseasonsModel
    {
        public string season_title { get; set; }
        public int is_new { get; set; }
        public int season_id { get; set; }
        public string title { get; set; }
    }
    public class BangumipublishModel
    {
        public int is_finish { get; set; }
        public string pub_time { get; set; }
        public string pub_time_show { get; set; }
        public int weekday { get; set; }
    }

    public class Banguminewest_epModel
    {
        public string desc { get; set; }
        public int id { get; set; }
        public string index { get; set; }
        public int is_new { get; set; }
    }
    public class BangumiratingModel
    {
        public int count { get; set; }
        public double score { get; set; }
    }

    public class BangumirightsModel
    {
        public int allow_bp { get; set; }
        public int allow_download { get; set; }
        public int allow_review { get; set; }
        public int area_limit { get; set; }
        public int ban_area_show { get; set; }
        public string copyright { get; set; }
        public int is_preview { get; set; }
    }

}
