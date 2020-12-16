using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using BiliBili.UWP.Models;
using System.Text.RegularExpressions;
using Windows.Media.Playback;
using Windows.Media;
using Windows.Storage.Streams;
using Windows.System.Display;
using BiliBili.UWP.Controls;
using Windows.Graphics.Display;
using Newtonsoft.Json.Linq;
using Windows.UI.Popups;
using Windows.System;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.ApplicationModel.DataTransfer;
using BiliBili.UWP.Helper;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.StartScreen;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Pages.FindMore;
using BiliBili.UWP.Pages.User;
using BiliBili.UWP.Api.User;
using BiliBili.UWP.Api;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoViewPage : Page
    {
        readonly FollowAPI followAPI;
        Download download;
        public VideoViewPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            download = new Download();
            followAPI = new FollowAPI();
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var detail = this.DataContext as VideoInfoModels;
            DataRequest request = args.Request;
            request.Data.Properties.Title = detail.title;
            request.Data.Properties.Description = txt_desc.Text;
            request.Data.SetWebLink(new Uri(detail.short_link));
        }

        bool isMovie = false;
        bool isSeason = false;
        string _aid;
        string _bvid;
        bool isBVID = false;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            error.Visibility = Visibility.Collapsed;
            var _id = (e.Parameter as object[])[0].ToString();
            if (int.TryParse(_id, out var aid))
            {
                txt_Header.Text = $"AV{_id}";
                _aid = _id;
                isBVID = false;
            }
            else
            {
                if (_id.Substring(0, 2).ToUpper() != "BV")
                {
                    _id = "BV" + _id;
                }
                txt_Header.Text = $"{_id}";
                _bvid = _id;
                isBVID = true;
            }

            await GetFavBox();
            await LoadVideo();


        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            pivot.SelectedIndex = 0;
            comment.ClearComment();
            this.DataContext = null;
        }



        private async Task LoadVideo()
        {
            try
            {
                pivot.SelectedIndex = 0;
                comment.ClearComment();
                isMovie = false;
                tag.Children.Clear();
                pr_Load.Visibility = Visibility.Visible;
                string uri = $"https://app.bilibili.com/x/v2/view?access_key={ ApiHelper.access_key }&{(isBVID ? $"bvid={_bvid}" : $"aid={_aid}") }&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&plat=0&platform=android&ts={ApiHelper.GetTimeSpan}";
                uri += "&sign=" + ApiHelper.GetSign(uri);
                string results = await WebClientClass.GetResults(new Uri(uri));

                VideoInfoModels m = JsonConvert.DeserializeObject<VideoInfoModels>(results);

                if (m.code == -404 || m.code == -403)
                {
                    string re2 = await WebClientClass.GetResults(new Uri("https://www.biliplus.com/api/view?id=" + _aid + "&access_key=" + ApiHelper.access_key));

                    JObject obj = JObject.Parse(re2);
                    if (obj["code"] == null)
                    {
                        m.code = 0;
                        m.data = JsonConvert.DeserializeObject<VideoInfoModels>(obj["v2_app_api"].ToString());
                    }
                }


                if (m.code == 0)
                {
                    txt_Header.Text = $"AV{m.data.aid}/{m.data.bvid}";
                    _aid = m.data.aid;
                    if (m.data.redirect_url != null && m.data.redirect_url != "")
                    {
                        this.Frame.GoBack();
                        Utils.ShowMessageToast("正在跳转至专题");
                        await MessageCenter.HandelUrl(m.data.redirect_url);
                        return;
                    }
                    this.DataContext = m.data;

                    if (m.data.movie != null)
                    {
                        //isMovie = true;

                        grid_Movie.Visibility = Visibility.Visible;
                        if (m.data.movie.movie_status == 1)
                        {
                            if (m.data.movie.pay_user.status == 0)
                            {

                                movie_pay.Visibility = Visibility.Visible;
                                txt_PayMonery.Text = m.data.movie.payment.price.ToString("0.00");
                            }
                            else
                            {
                                isMovie = true;
                                movie_pay.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            movie_pay.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        grid_Movie.Visibility = Visibility.Collapsed;
                        movie_pay.Visibility = Visibility.Collapsed;
                    }
                    if (m.data.interaction != null)
                    {
                        Utils.ShowMessageToast("这是一个互动视频，你的选项会决定剧情走向哦", 5000);
                    }
                    if (!string.IsNullOrEmpty(m.data.argue_msg))
                    {
                        Argue_msg.Visibility = Visibility.Visible;
                        txtArgue_msg.Text = m.data.argue_msg;
                    }
                    else
                    {
                        Argue_msg.Visibility = Visibility.Collapsed;
                    }
                    //m.data.pages
                    gv_Play.SelectedIndex = 0;
                    if (m.data.req_user != null && m.data.req_user.attention != 1)
                    {
                        btn_AttUp.Visibility = Visibility.Visible;
                        btn_CancelAttUp.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btn_AttUp.Visibility = Visibility.Collapsed;
                        btn_CancelAttUp.Visibility = Visibility.Visible;
                    }
                    if (m.data.season != null)
                    {
                        isSeason = true;
                        grid_season.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        isSeason = false;
                        grid_season.Visibility = Visibility.Collapsed;
                    }
                    if (m.data.tag != null)
                    {
                        grid_tag.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_tag.Visibility = Visibility.Collapsed;
                    }

                    if (m.data.elec != null)
                    {
                        grid_elec.Visibility = Visibility.Visible;
                        txt_NotCb.Visibility = Visibility.Collapsed;
                        grid_elec.DataContext = m.data.elec;
                    }
                    else
                    {
                        grid_elec.Visibility = Visibility.Collapsed;
                        txt_NotCb.Visibility = Visibility.Visible;
                    }

                    if (m.data.audio != null)
                    {
                        grid_audio.DataContext = m.data.audio;
                        grid_audio.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_audio.Visibility = Visibility.Collapsed;
                    }

                    list_About.ItemsSource = null;
                    if (m.data.relates != null)
                    {
                        list_About.ItemsSource = m.data.relates;
                    }

                    if (m.data.tag != null)
                    {
                        foreach (var item in m.data.tag)
                        {
                            HyperlinkButton hy = new HyperlinkButton();
                            hy.Content = item.tag_name;
                            hy.Margin = new Thickness(0, 0, 10, 0);
                            hy.Foreground = App.Current.Resources["Bili-ForeColor"] as SolidColorBrush;
                            hy.Click += Hy_Click; ;
                            tag.Children.Add(hy);
                        }
                    }
                    if (m.data.staff != null && m.data.staff.Count != 0)
                    {
                        staff.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        staff.Visibility = Visibility.Collapsed;
                    }
                    if (m.data.pages != null && m.data.pages.Count != 0)
                    {
                        var qualitys = await PlayurlHelper.GetVideoQualities(new PlayerModel()
                        {
                            Aid = _aid,
                            Mid = m.data.pages[0].cid.ToString()
                        });
                        cb_Qu.ItemsSource = qualitys.OrderByDescending(x => x.qn).ToList();
                        if (qualitys.Count != 0)
                        {
                            cb_Qu.SelectedIndex = 0;
                        }
                    }
                    last_view.Text = "";
                    if (m.data.history != null)
                    {
                        var record = SqlHelper.GetVideoWatchRecord(m.data.history.cid.ToString());

                        if (record == null)
                        {
                            SqlHelper.AddVideoWatchRecord(new ViewPostHelperClass()
                            {
                                epId = m.data.history.cid.ToString(),
                                Post = m.data.history.progress,
                                viewTime = DateTime.Now
                            });
                        }
                        else
                        {
                            record.Post = m.data.history.progress;
                            SqlHelper.UpdateVideoWatchRecord(record);
                        }
                        if (m.data.pages != null && m.data.pages.Count > 1)
                        {
                            var episode = m.data.pages.FirstOrDefault(x => x.cid == m.data.history.cid);
                            if (episode != null)
                            {
                                last_view.Text = $"上次看到P{episode.page},点击继续播放";
                                last_view.Tag = m.data.history.cid;
                            }
                        }
                    }
                    if (SecondaryTile.Exists(_aid))
                    {
                        btn_unPin.Visibility = Visibility.Visible;
                        btn_Pin.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btn_unPin.Visibility = Visibility.Collapsed;
                        btn_Pin.Visibility = Visibility.Visible;
                    }

                    comment.LoadComment(new LoadCommentInfo()
                    {
                        commentMode = CommentMode.Video,
                        conmmentSortMode = ConmmentSortMode.All,
                        oid = _aid
                    });
                }
                else
                {
                    if (m.code == -403)
                    {
                        error.Visibility = Visibility.Visible;
                        txt_error.Text = "您的权限不足或者不支持你所在地区";
                        return;

                    }
                    if (m.code == -404)
                    {
                        error.Visibility = Visibility.Visible;
                        txt_error.Text = "视频不存在或已被删除";
                        return;
                    }

                    Utils.ShowMessageToast(m.message, 3000);
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867 || ex.HResult == -2147012889)
                {
                    Utils.ShowMessageToast("无法连接服务器，请检查你的网络连接", 3000);
                }
                else
                {
                    LogHelper.WriteLog($"加载视频失败{_aid}", LogType.FATAL, ex);
                    Utils.ShowMessageToast("加载视频失败", 3000);
                }

            }
            finally
            {
                btn_HideAll_Click(null, null);
                pr_Load.Visibility = Visibility.Collapsed;


            }
        }

        private void Hy_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(DynamicTopicPage), new object[] { (sender as HyperlinkButton).Content.ToString() });
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void txt_desc_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (txt_desc.MaxLines == 3)
            {
                txt_desc.MaxLines = 0;
            }
            else
            {
                txt_desc.MaxLines = 3;
            }
        }

        private void gv_Play_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (gv_Play.SelectedItem!=null)
            //{
            //    var info = gv_Play.SelectedItem as pagesModel;

            //    List<PlayerModel> ls = new List<PlayerModel>();
            //    int i=0;
            //    foreach (pagesModel item in gv_Play.Items)
            //    {

            //        ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid, ImageSrc = (this.DataContext as VideoInfoModels).pic,Mode= PlayMode.Video,No= i.ToString(),VideoTitle=item.View, Title= (this.DataContext as VideoInfoModels).title });
            //        i++;
            //    }

            //  //  players.LoadPlayer(ls, gv_Play.SelectedIndex);
            //}
        }


        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //player.Height = player.ActualWidth * (9 / 16);
        }

        private void btn_Season_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BanInfoPage), new object[] { (this.DataContext as VideoInfoModels).season.season_id });
        }

        private void comment_OpenUser(string id)
        {
            this.Frame.Navigate(typeof(UserCenterPage), id);
        }

        private void btn_UP_Click(object sender, RoutedEventArgs e)
        {
            VideoInfoModels info = (sender as HyperlinkButton).DataContext as VideoInfoModels;
            this.Frame.Navigate(typeof(UserCenterPage), info.owner.mid);
        }

        private async void btn_AttUp_Click(object sender, RoutedEventArgs e)
        {

            if (ApiHelper.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/relation/modify");

                    string content = string.Format(
                        "access_key={0}&act=1&appkey={1}&build=45000&fid={2}&mobi_app=android&platform=android&re_src=90&ts={3}",
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, (Video_UP.DataContext as VideoInfoModels).owner.mid, ApiHelper.GetTimeSpan_2
                        );
                    content += "&sign=" + ApiHelper.GetSign(content);
                    string result = await WebClientClass.PostResults(ReUri,
                        content
                     );
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        Utils.ShowMessageToast("关注成功", 3000);
                        btn_AttUp.Visibility = Visibility.Collapsed;
                        btn_CancelAttUp.Visibility = Visibility.Visible;


                    }
                    else
                    {
                        Utils.ShowMessageToast("关注失败\r\n" + result, 3000);

                    }

                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message, "关注时发生错误").ShowAsync();
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);
            }
        }

        private async void btn_CancelAttUp_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/relation/modify");

                    string content = string.Format(
                        "access_key={0}&act=2&appkey={1}&build=45000&fid={2}&mobi_app=android&platform=android&re_src=90&ts={3}",
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, (Video_UP.DataContext as VideoInfoModels).owner.mid, ApiHelper.GetTimeSpan_2
                        );
                    content += "&sign=" + ApiHelper.GetSign(content);
                    string result = await WebClientClass.PostResults(ReUri,
                        content
                     );
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        Utils.ShowMessageToast("已取消关注", 3000);
                        btn_AttUp.Visibility = Visibility.Visible;
                        btn_CancelAttUp.Visibility = Visibility.Collapsed;


                    }
                    else
                    {
                        Utils.ShowMessageToast("取消关注失败\r\n" + result, 3000);

                    }

                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message, "取消关注时发生错误").ShowAsync();
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);
            }
        }

        private void btn_TB_1_Click(object sender, RoutedEventArgs e)
        {
            TouBi(1);
        }

        private void btn_TB_2_Click(object sender, RoutedEventArgs e)
        {
            TouBi(2);
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            grid_Tb.Hide();
        }

        public async void TouBi(int num)
        {

            if (ApiHelper.IsLogin())
            {
                try
                {
                    WebClientClass wc = new WebClientClass();
                    Uri ReUri = new Uri("https://app.bilibili.com/x/v2/view/coin/add");
                    string QuStr = string.Format("access_key={0}&aid={1}&appkey={2}&build=540000&from=7&mid={3}&platform=android&&multiply={4}&ts={5}", ApiHelper.access_key, _aid, ApiHelper.AndroidKey.Appkey, ApiHelper.GetUserId(), num, ApiHelper.GetTimeSpan);
                    QuStr += "&sign=" + ApiHelper.GetSign(QuStr);
                    string result = await WebClientClass.PostResults(ReUri, QuStr);
                    JObject jObject = JObject.Parse(result);
                    if (Convert.ToInt32(jObject["code"].ToString()) == 0)
                    {
                        Utils.ShowMessageToast("投币成功！", 3000);
                    }
                    else
                    {
                        Utils.ShowMessageToast(jObject["message"].ToString(), 3000);
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("投币时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录!", 3000);
            }
        }

        private void Video_Refresh_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = 0;
            LoadVideo();
        }

        private async void btn_GoBrowser_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.bilibili.com/video/av" + _aid));

        }

        private async void btn_SaveImage_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker save = new FileSavePicker();
            save.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            save.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
            save.SuggestedFileName = "bili_img_" + _aid;
            StorageFile file = await save.PickSaveFileAsync();
            if (file != null)
            {
                //img_Image
                IBuffer bu = await WebClientClass.GetBuffer(new Uri((this.DataContext as VideoInfoModels).pic));
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, bu);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                Utils.ShowMessageToast("保存成功", 3000);
            }
        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            var detail = this.DataContext as VideoInfoModels;
            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText($"{detail.title} {detail.short_link}");
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            Utils.ShowMessageToast("已将内容复制到剪切板", 3000);
        }

        private void btn_ShareData_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void btn_Favbox_Click(object sender, RoutedEventArgs e)
        {
            if (ApiHelper.IsLogin())
            {
                flyout_Favbox.ShowAt(btn_Favbox);
            }
            else
            {
                Utils.ShowMessageToast("请先登录", 3000);
            }
        }

        private async Task GetFavBox()
        {

            if (ApiHelper.IsLogin())
            {
                try
                {
                    var result = await followAPI.MyCreatedFavorite(_aid).Request();
                    if (result.status)
                    {
                        var data = await result.GetJson<ApiDataModel<JObject>>();
                        if (data.success)
                        {
                            Video_ListView_Favbox.ItemsSource = await data.data["list"].ToString().DeserializeJson<List<FavboxModel>>();
                        }
                        else
                        {
                            FavBox_Header.Text = data.message;
                        }
                    }
                    else
                    {
                        FavBox_Header.Text = result.message;
                    }

                }
                catch (Exception ex)
                {
                    FavBox_Header.Text = "获取失败！" + ex.Message;
                }
            }
            else
            {
                FavBox_Header.Text = "请先登录！";
                Video_ListView_Favbox.IsEnabled = false;
            }
        }
        private async void Video_ListView_Favbox_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (ApiHelper.IsLogin())
            {
                try
                {

                    //((FavboxModel)e.ClickedItem).fid


                    //Uri ReUri = new Uri("http://api.bilibili.com/x/v2/fav/video/add");

                    //string content = string.Format(
                    //    "access_key={0}&aid={2}&appkey={1}&build=520001&fid={3}&mobi_app=android&platform=android&re_src=90&ts={4}",
                    //    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _aid, ((FavboxModel)e.ClickedItem).fid, ApiHelper.GetTimeSpan_2
                    //    );
                    //content += "&sign=" + ApiHelper.GetSign(content);
                    //string result = await WebClientClass.PostResults(ReUri,
                    //    content
                    // );
                    var results = await followAPI.AddFavorite(new List<string>() { ((FavboxModel)e.ClickedItem).id }, _aid).Request();
                    if (results.status)
                    {
                        var data = await results.GetJson<ApiDataModel<JObject>>();
                        if (data.success)
                        {
                            Utils.ShowMessageToast("收藏成功！", 2000);
                            GetFavBox();
                        }
                        else
                        {
                            Utils.ShowMessageToast(data.message);
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(results.message, 2000);

                    }
                   
                }
                catch (Exception ex)
                {
                    Utils.ShowMessageToast("收藏失败！" + ex.Message, 2000);
                }
            }
            else
            {
                Utils.ShowMessageToast("请先登录！", 2000);
            }
        }



        private void list_About_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoViewPage), new object[] { (e.ClickedItem as relatesModel).aid });
        }

        private void btn_Download_Click(object sender, RoutedEventArgs e)
        {

            if ((this.DataContext as VideoInfoModels).interaction != null)
            {
                Utils.ShowMessageToast("互动视频不支持下载");
                return;
            }
            if (cb_Qu.Items.Count == 0)
            {
                Utils.ShowMessageToast("视频无法下载");
                return;
            }
            if (gv_Play.Items.Count != 0)
            {
                gv_Play.SelectionMode = ListViewSelectionMode.Multiple;
                gv_Play.IsItemClickEnabled = false;
                Utils.ShowMessageToast("请选中要下载的分P视频，点击确定", 3000);
                Down_ComBar.Visibility = Visibility.Visible;
                com_bar.Visibility = Visibility.Collapsed;
            }




        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            gv_Play.SelectionMode = ListViewSelectionMode.None;
            gv_Play.IsItemClickEnabled = true;
            Down_ComBar.Visibility = Visibility.Collapsed;
            com_bar.Visibility = Visibility.Visible;

        }

        private async void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            if ((this.DataContext as VideoInfoModels).interaction != null)
            {
                Utils.ShowMessageToast("互动视频不支持下载");
                return;
            }
            if (gv_Play.SelectedItems.Count == 0)
            {
                return;
            }
            var info = gv_Play.SelectedItem as pagesModel;

            int i = 1;
            pr_Load.Visibility = Visibility.Visible;
            foreach (pagesModel item in gv_Play.SelectedItems)
            {
                if (item.IsDowned == Visibility.Collapsed)
                {
                    var m = item;
                    var downloadUrl = await download.GetVideoDownloadUrl(_aid, item.cid.ToString(), cb_Qu.SelectedItem as QualityModel, ApiHelper.access_key, ApiHelper.GetUserId());
                    if (!downloadUrl.success)
                    {
                        await new MessageDialog($"{m.page} {m.part}读取下载地址失败,已跳过").ShowAsync();
                        continue;
                    }
                    await DownloadHelper2.CreateDownload(new DownloadTaskModel()
                    {
                        downloadMode = DownloadMode.Video,
                        avid = _aid,
                        cid = m.cid.ToString(),
                        epIndex = Convert.ToInt32(m.page),
                        epTitle = m.page + " " + m.part,
                        thumb = (this.DataContext as VideoInfoModels).pic,
                        quality = cb_Qu.SelectedIndex + 1,
                        title = (this.DataContext as VideoInfoModels).title,
                        is_dash = downloadUrl.data[0].Format == "dash"
                    }, downloadUrl.data);
                }
                i++;

            }

            Utils.ShowMessageToast("已经将任务添加到下载列表", 3000);
            gv_Play.SelectionMode = ListViewSelectionMode.None;
            gv_Play.IsItemClickEnabled = true;
            Down_ComBar.Visibility = Visibility.Collapsed;
            com_bar.Visibility = Visibility.Visible;
            pr_Load.Visibility = Visibility.Collapsed;

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

        private void btn_HideAll_Click(object sender, RoutedEventArgs e)
        {
            btn_HideAll.Visibility = Visibility.Collapsed;
            btn_ShowAll.Visibility = Visibility.Visible;
            if (gv_Play.Items.Count > 3)
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

        private void gv_Play_ItemClick(object sender, ItemClickEventArgs e)
        {
            var info = e.ClickedItem as pagesModel;
            OpenPlayer(info);
            //List<PlayerModel> ls = new List<PlayerModel>();
            //int i = 1;
            //foreach (pagesModel item in gv_Play.Items)
            //{
            //    var data = (this.DataContext as VideoInfoModels);
            //    if (item.IsDowned == Visibility.Collapsed)
            //    {
            //        if (isMovie)
            //        {
            //            ls.Add(new PlayerModel()
            //            {
            //                Aid = _aid,
            //                Mid = item.cid.ToString(),
            //                ImageSrc = data.pic,
            //                Mode = PlayMode.Movie,
            //                No = "",
            //                VideoTitle = item.View,
            //                Title = data.title
            //            });
            //        }
            //        else
            //        {
            //            switch (item.from)
            //            {
            //                case "sohu":
            //                    ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), rich_vid = item.vid, ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Sohu, No = i.ToString(), VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title });
            //                    break;
            //                default:
            //                    if (isSeason)
            //                    {
            //                        ls.Add(new PlayerModel()
            //                        {
            //                            Aid = _aid,
            //                            Mid = item.cid.ToString(),
            //                            ImageSrc = data.pic,
            //                            Mode = PlayMode.Bangumi,
            //                            No = i.ToString(),
            //                            VideoTitle = item.View,
            //                            Title = data.title,
            //                            episode_id = data.season.newest_ep_id
            //                        });
            //                    }
            //                    else
            //                    {
            //                        ls.Add(new PlayerModel()
            //                        {
            //                            Aid = _aid,
            //                            Mid = item.cid.ToString(),
            //                            isInteraction = data.interaction != null,
            //                            graph_version = data.interaction?.graph_version,
            //                            ImageSrc = data.pic,
            //                            Mode = PlayMode.Video,
            //                            No = i.ToString(),
            //                            VideoTitle = item.View,
            //                            Title = data.title
            //                        });
            //                    }
            //                    break;
            //            }

            //        }
            //    }
            //    else
            //    {

            //        ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Local, No = "", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title, Path = DownloadHelper2.downloadeds[item.cid.ToString()] });
            //    }
            //}

            //MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, (gv_Play.ItemsSource as List<pagesModel>).IndexOf(info) });
            //PostHistory();

        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 0)
            {
                com_bar.Visibility = Visibility.Visible;
            }
            else
            {
                com_bar.Visibility = Visibility.Collapsed;
            }
        }

        private async void img_pic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var x = new ContentDialog();
            StackPanel st = new StackPanel();
            st.Children.Add(new Image()
            {
                Source = new BitmapImage(new Uri((this.DataContext as VideoInfoModels).pic))
            });

            x.Content = st;
            x.PrimaryButtonText = "关闭";
            x.IsPrimaryButtonEnabled = true;
            x.SecondaryButtonText = "保存";
            x.IsSecondaryButtonEnabled = true;
            x.SecondaryButtonClick += X_SecondaryButtonClick;
            await x.ShowAsync();
        }

        private void X_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            btn_SaveImage_Click(sender, null);
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            btn_Play_Click(sender, null);
        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            if (gv_Play.Items.Count != 0)
            {
                var info = gv_Play.Items[0] as pagesModel;

                List<PlayerModel> ls = new List<PlayerModel>();
                //int i = 0;
                foreach (pagesModel item in gv_Play.Items)
                {
                    if (item.IsDowned == Visibility.Collapsed)
                    {
                        // ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid, ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Video, No = "", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title });

                        if (isMovie)
                        {
                            ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Movie, No = "", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title });
                        }
                        else
                        {
                            switch (item.from)
                            {
                                case "sohu":
                                    ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), rich_vid = item.vid, ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Sohu, No = "1", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title });
                                    break;
                                default:
                                    if (isSeason)
                                    {
                                        ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Bangumi, No = "1", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title, episode_id = (this.DataContext as VideoInfoModels).season.newest_ep_id });
                                    }
                                    else
                                    {
                                        ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Video, No = "1", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title });
                                    }

                                    break;
                            }

                        }
                    }
                    else
                    {
                        ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Local, No = "", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title, Path = DownloadHelper2.downloadeds[item.cid.ToString()] });
                    }
                }

                MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, (gv_Play.ItemsSource as List<pagesModel>).IndexOf(info) });
                PostHistory();
            }
        }

        private void btn_movie_activity_Click(object sender, RoutedEventArgs e)
        {

        }



        private void PostHistory()
        {
            try
            {
                if (SqlHelper.GetComicIsOnHistory(_aid))
                {
                    SqlHelper.UpdateComicHistory(new HistoryClass()
                    {
                        _aid = _aid,
                        image = (this.DataContext as VideoInfoModels).pic,
                        title = txt_title.Text,
                        up = (Video_UP.DataContext as VideoInfoModels).owner.name,
                        lookTime = DateTime.Now
                    });


                }
                else
                {
                    SqlHelper.AddCommicHistory(new HistoryClass()
                    {
                        _aid = _aid,
                        image = (this.DataContext as VideoInfoModels).pic,
                        title = txt_title.Text,
                        up = (Video_UP.DataContext as VideoInfoModels).owner.name,
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
                //SettingHelper.PinTile(_aid, _aid, txt_title.Text, (this.DataContext as VideoInfoModels).pic);
                string appbarTileId = _aid;
                var str = (this.DataContext as VideoInfoModels).pic;
                StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                StorageFile file = await localFolder.CreateFileAsync(_aid + ".jpg", CreationCollisionOption.OpenIfExists);
                if (file != null)
                {
                    //img_Image
                    IBuffer bu = await WebClientClass.GetBuffer(new Uri((str)));
                    CachedFileManager.DeferUpdates(file);
                    await FileIO.WriteBufferAsync(file, bu);
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        Uri logo = new Uri("ms-appdata:///local/" + _aid + ".jpg");
                        string tileActivationArguments = _aid;
                        string displayName = (this.DataContext as VideoInfoModels).title;

                        TileSize newTileDesiredSize = TileSize.Wide310x150;

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

        }

        private async void btn_unPin_Click(object sender, RoutedEventArgs e)
        {
            SecondaryTile secondaryTile = new SecondaryTile(_aid);
            await secondaryTile.RequestDeleteAsync();
            btn_Pin.Visibility = Visibility.Visible;
            btn_unPin.Visibility = Visibility.Collapsed;
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

        private void btn_ShowComment_Click(object sender, RoutedEventArgs e)
        {
            comment.ShowCommentBox();
        }

        private void grid_audio_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as audioModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), item.song_id);
        }

        private async void btn_AddToView_Click(object sender, RoutedEventArgs e)
        {
            ToView toView = new ToView();
            var data = await toView.AddToView(_aid);
            if (data.success)
            {
                Utils.ShowMessageToast("已添加");
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Width != 0)
            {
                int i = 2;
                if (availableSize.Width > 500)
                {
                    i = 3;

                    bor_Width.Width = availableSize.Width / i - 39;
                }
                else
                {
                    bor_Width.Width = availableSize.Width / i - 42;
                }
            }

            return base.MeasureOverride(availableSize);
        }

        private void Btn_verify_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as staffModel;

            if (data.official_verify.type == -1)
            {
                return;
            }
            if (data.official_verify.desc != "")
            {
                Utils.ShowMessageToast(data.official_verify.desc);
            }
            else if (data.official_verify.type == 0)
            {
                Utils.ShowMessageToast("个人认证");
            }
            else if (data.official_verify.type == 1)
            {
                Utils.ShowMessageToast("企业认证");
            }
        }

        private void Gv_Staff_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as staffModel;
            this.Frame.Navigate(typeof(UserCenterPage), data.mid);
        }

        private void Txt_desc_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            txt_desc.IsTextSelectionEnabled = !txt_desc.IsTextSelectionEnabled;
        }

        private async void Btn_Triple_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行操作");
                return;
            }
            try
            {
                var body = $"access_key={ApiHelper.access_key}&aid={_aid}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&platform=android&ts={ApiHelper.GetTimeSpan}";
                body += "&sign=" + ApiHelper.GetSign(body);
                var results = await WebClientClass.PostResults(new Uri("https://app.bilibili.com/x/v2/view/like/triple"), body);
                var obj = JObject.Parse(results);
                if (obj["code"].ToInt32() == 0)
                {
                    Utils.ShowMessageToast("三连完成");
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("三连失败了啊");
            }


        }

        private async void Btn_Like_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行操作");
                return;
            }
            try
            {
                var body = $"access_key={ApiHelper.access_key}&aid={_aid}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&platform=android&dislike=0&like=0&ts={ApiHelper.GetTimeSpan}";
                body += "&sign=" + ApiHelper.GetSign(body);
                var results = await WebClientClass.PostResults(new Uri("https://app.bilibili.com/x/v2/view/like"), body);
                var obj = JObject.Parse(results);
                if (obj["code"].ToInt32() == 0)
                {
                    Utils.ShowMessageToast("点赞完成");
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("点赞失败了啊");
            }
        }


        private void Btn_Like_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Btn_Triple_Click(sender, null);
        }

        private void openLastWatch_Click(object sender, RoutedEventArgs e)
        {
            var info = (gv_Play.ItemsSource as List<pagesModel>).FirstOrDefault(x => x.cid == (long)last_view.Tag);
            OpenPlayer(info);
        }
        private void OpenPlayer(pagesModel info)
        {
            List<PlayerModel> ls = new List<PlayerModel>();
            int i = 1;
            foreach (pagesModel item in gv_Play.Items)
            {
                var data = (this.DataContext as VideoInfoModels);
                if (item.IsDowned == Visibility.Collapsed)
                {
                    if (isMovie)
                    {
                        ls.Add(new PlayerModel()
                        {
                            Aid = _aid,
                            Mid = item.cid.ToString(),
                            ImageSrc = data.pic,
                            Mode = PlayMode.Movie,
                            No = "",
                            VideoTitle = item.View,
                            Title = data.title
                        });
                    }
                    else
                    {
                        switch (item.from)
                        {
                            case "sohu":
                                ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), rich_vid = item.vid, ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Sohu, No = i.ToString(), VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title });
                                break;
                            default:
                                if (isSeason)
                                {
                                    ls.Add(new PlayerModel()
                                    {
                                        Aid = _aid,
                                        Mid = item.cid.ToString(),
                                        ImageSrc = data.pic,
                                        Mode = PlayMode.Bangumi,
                                        No = i.ToString(),
                                        VideoTitle = item.View,
                                        Title = data.title,
                                        episode_id = data.season.newest_ep_id
                                    });
                                }
                                else
                                {
                                    ls.Add(new PlayerModel()
                                    {
                                        Aid = _aid,
                                        Mid = item.cid.ToString(),
                                        isInteraction = data.interaction != null,
                                        graph_version = data.interaction?.graph_version,
                                        ImageSrc = data.pic,
                                        Mode = PlayMode.Video,
                                        No = i.ToString(),
                                        VideoTitle = item.View,
                                        Title = data.title
                                    });
                                }
                                break;
                        }

                    }
                }
                else
                {

                    ls.Add(new PlayerModel() { Aid = _aid, Mid = item.cid.ToString(), ImageSrc = (this.DataContext as VideoInfoModels).pic, Mode = PlayMode.Local, No = "", VideoTitle = item.View, Title = (this.DataContext as VideoInfoModels).title, Path = DownloadHelper2.downloadeds[item.cid.ToString()] });
                }
            }

            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, (gv_Play.ItemsSource as List<pagesModel>).IndexOf(info) });
            PostHistory();
        }
    }
}
