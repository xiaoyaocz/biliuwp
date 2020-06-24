using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Modules.LiveModels;
using BiliBili.UWP.Pages.User;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using NSDanmaku.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static BiliBili.UWP.Helper.BiliLiveDanmu;
using static BiliBili.UWP.Modules.LiveRoom;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.Live
{

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveRoomPC : Page
    {
        public LiveRoomPC()
        {
            this.InitializeComponent();
            this.Loaded += delegate { this.Focus(FocusState.Programmatic); };
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            _systemMediaTransportControls.ButtonPressed += _systemMediaTransportControls_ButtonPressed;
            danmu.danmakuMode = NSDanmaku.Model.DanmakuMode.Live;
            MessageCenter.Logined += MessageCenter_Logined;
            account = new Account();
            liveCenter = new LiveCenter();
            danmu_list = new ObservableCollection<DanmuMsgModel>();
            list_chat.ItemsSource = danmu_list;
        }

        private void MessageCenter_Logined()
        {
            //加载我的信息
            LoadMyInfo();
            //加载我的礼物
            LoadMyGifts();
        }

        private async void _systemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        media.Play();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        media.Pause();
                    });
                    break;
                default:
                    break;
            }
        }
        SystemMediaTransportControls _systemMediaTransportControls;
        BiliLiveDanmu _biliLiveDanmu;
        LiveRoom liveRoom;
        LiveCenter liveCenter;
        //账户
        Account account;
        //加载播放地址中
        bool loadPlayurling = true;
        //打开弹幕
        bool openDanmu = true;
        //加载设置中
        bool loadingSetting = true;
        //宝箱Timer
        DispatcherTimer freeSilverTimer;
        //宝箱可领取时间
        DateTime freeSilverTime;
        //自动领取瓜子
        bool autoGetAward = false;
        //房间号
        int roomId = 0;
        //UPID
        int uId = 0;
        //直播播放地址列表
        List<Durl> durls;
        //房间状态
        LiveStatus liveStatus;
        //轮播播放地址
        List<Durl> playUrls;
        private DisplayRequest dispRequest = null;//保持屏幕常亮

        ObservableCollection<DanmuMsgModel> danmu_list;

        /// <summary>
        /// 是否处于画中画
        /// </summary>
        bool isMini = false;
        bool winfull = false;
        bool isFull = false;
        bool receiveGiftMsg = true;
        bool receiveWelcomeMsg = true;
        bool receiveSysMsg = true;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Frame.Visibility = Visibility.Visible;
            base.OnNavigatedTo(e);
            //if (e.NavigationMode == NavigationMode.New)
            //{
            roomId = Convert.ToInt32((e.Parameter as object[])[0]);
            _biliLiveDanmu = new BiliLiveDanmu();
            _biliLiveDanmu.HasDanmu += _biliLiveDanmu_HasDanmu;
            liveRoom = new LiveRoom();
            await LoadRoomInfo();
            //}
            if (dispRequest == null)
            {
                // 用户观看视频，需要保持屏幕的点亮状态
                dispRequest = new DisplayRequest();
                dispRequest.RequestActive(); // 激活显示请求
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back||e.SourcePageType==typeof(BlankPage))
            {
                try
                {
                    Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().ExitFullScreenMode();
                    if (dispRequest != null)
                    {
                        dispRequest = null;
                    }
                    if (freeSilverTimer != null && freeSilverTimer.IsEnabled)
                    {
                        freeSilverTimer.Stop();
                    }
                    media.Stop();
                    list_chat.ItemsSource = null;
                    _biliLiveDanmu.HasDanmu-= _biliLiveDanmu_HasDanmu;
                    _biliLiveDanmu.Dispose();
                    _biliLiveDanmu = null;
                }
                catch (Exception)
                {
                }
               
            }
            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// 加载房间信息
        /// </summary>
        private async Task LoadRoomInfo()
        {
            pr_Loading.Visibility = Visibility.Visible;
            LoadSetting();
            var roomInfo = await liveRoom.GetRoomInfo(roomId);
            if (roomInfo.success)
            {
                roomId = roomInfo.data.room_id;
                uId = roomInfo.data.uid;
                liveStatus = (LiveStatus)roomInfo.data.live_status;

                this.DataContext = roomInfo.data;
                //加载我的信息
                LoadMyInfo();
                //加载礼物
                LoadGifts(roomInfo.data.area_id, roomInfo.data.parent_area_id);
                //加载我的礼物
                LoadMyGifts();

                //读取最近10条弹幕
                LoadLastMsg();

                //开始接收弹幕
                _biliLiveDanmu.Start(roomId, Convert.ToInt64(ApiHelper.GetUserId()));

                //判断状态
                grid_isStop.Visibility = Visibility.Collapsed;
                cb_quality.Visibility = Visibility.Collapsed;
                cb_line.Visibility = Visibility.Collapsed;
                if (liveStatus == LiveStatus.Live)
                {
                    cb_quality.Visibility = Visibility.Visible;
                    cb_line.Visibility = Visibility.Visible;
                    //加载直播地址
                    LoadPlayUrl();
                }
                else if (liveStatus == LiveStatus.Stop)
                {

                    grid_isStop.Visibility = Visibility.Visible;
                }
                else if (liveStatus == LiveStatus.Round)
                {
                    //加载轮播
                    LoadRoundUrl();
                }
                //宝箱
                HandelFreeSilver();

                //设置排行榜
                cb_rank_cate.ItemsSource = (await liveRoom.GetRankActivity(roomInfo.data.area_id, roomInfo.data.parent_area_id, roomId, roomInfo.data.uid)).data;
                cb_rank_cate.SelectedIndex = 0;
            }
            else
            {
                Utils.ShowMessageToast("读取房间信息失败");
            }

            pr_Loading.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 读取礼物列表
        /// </summary>
        private async void LoadGifts(int area_id, int parent_area_id)
        {
            var gifts = await liveRoom.GetRoomGifts(roomId, area_id, parent_area_id);
            if (gifts.success)
            {
                gridview_Gifts.ItemsSource = gifts.data;
            }
            else
            {
                Utils.ShowMessageToast(gifts.message);
            }
        }
        /// <summary>
        /// 读取我的包裹
        /// </summary>
        private async void LoadMyGifts()
        {
            if (ApiHelper.IsLogin())
            {
                var mygifts = await liveRoom.GetMyGifts();
                if (mygifts.success)
                {
                    gridview_myGifts.ItemsSource = mygifts.data;
                }
                else
                {
                    Utils.ShowMessageToast(mygifts.message);
                }

            }
        }
        /// <summary>
        /// 读取我的信息
        /// </summary>
        private async void LoadMyInfo()
        {
            if (ApiHelper.IsLogin())
            {
                var data = await liveCenter.GetUserInfo();
                if (data.success)
                {
                    txt_gold.Text = data.data.gold.ToW();
                    txt_silver.Text = data.data.silver.ToW();
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }
            }
            else
            {
                txt_gold.Text = "0";
                txt_silver.Text = "0";
            }
        }
        /// <summary>
        /// 读取直播播放地址
        /// </summary>
        /// <param name="quality">清晰度</param>
        private async void LoadPlayUrl(int quality = 0)
        {
            //加载播放地址
            loadPlayurling = true;
            var playUrl = await liveRoom.GetRoomPlayurl(roomId, quality);
            if (playUrl.success)
            {
                durls = playUrl.data.durl;
                if (!media.Options.ContainsKey("http-referrer"))
                {
                    media.Options.Add("http-user-agent", "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)");
                    media.Options.Add("http-referrer", "https://live.bilibili.com/" + roomId);
                }
                media.Source = durls[0].url;
                cb_quality.ItemsSource = playUrl.data.quality_description;
                cb_line.ItemsSource = playUrl.data.durl;
                cb_line.SelectedIndex = 0;
                cb_quality.SelectedIndex = playUrl.data.quality_description.FindIndex(x => x.qn == playUrl.data.current_qn);
            }
            else
            {
                MessageDialog md = new MessageDialog("加载直播失败了，请重试");
                await md.ShowAsync();
            }
            loadPlayurling = false;
        }
        private async void ChangeQuality(int quality)
        {
            //加载播放地址
            loadPlayurling = true;
            var playUrl = await liveRoom.GetRoomPlayurl(roomId, quality);
            if (playUrl.success)
            {
                durls = playUrl.data.durl;
                if (!media.Options.ContainsKey("http-referrer"))
                {
                    media.Options.Add("http-user-agent", "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)");
                    media.Options.Add("http-referrer", "https://live.bilibili.com/" + roomId);
                }
                media.Source = durls[0].url;
                cb_line.ItemsSource = playUrl.data.durl;
                cb_line.SelectedIndex = 0;
                //cb_quality.SelectedIndex = (cb_quality.ItemsSource as List<quality_description_item>).FindIndex(x => x.qn == quality); ;
            }
            else
            {
                Utils.ShowMessageToast("清晰度切换失败");
            }
            loadPlayurling = false;
        }

        /// <summary>
        /// 读取轮播地址
        /// </summary>
        private async void LoadRoundUrl()
        {
            try
            {
                var m = await liveRoom.GetRoundPlayurl(roomId);
                if (m.success)
                {
                    txt_Title.Text = "[轮播]" + m.data.title;
                    playUrls = m.data.data.durl;
                    media.Options.Clear();
                    media.Options.Add("http-user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                    media.Options.Add("http-referrer", "https://www.bilibili.com/blackboard/html5player.html?crossDomain=true");
                    media.Source = null;
                    media.Source = playUrls[0].url;
                }
                else
                {
                    Utils.ShowMessageToast(m.message);
                }
            }
            catch (Exception ex)
            {

            }

        }
        /// <summary>
        /// 读取排行榜
        /// </summary>
        private async void LoadRank()
        {
            pr_RankLoad.Visibility = Visibility.Visible;
            var m = cb_rank_cate.SelectedItem as RankActivityModel;
            if (m.desc == "七日榜")
            {
                var data = await liveRoom.GetGiftTop(roomId);
                if (data.success)
                {
                    list_Rank.ItemsSource = data.data;
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }

            }
            else if (m.desc == "粉丝榜")
            {
                var data = await liveRoom.GetMedalRankList(roomId);
                if (data.success)
                {
                    list_Rank.ItemsSource = data.data;
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }
            }
            else
            {
                var data = await liveRoom.GetOpRank(roomId, m.type);
                if (data.success)
                {
                    list_Rank.ItemsSource = data.data;
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }
            }
            pr_RankLoad.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 读取舰队
        /// </summary>
        private async void LoadGuardRank()
        {
            noGuardRank.Visibility = Visibility.Collapsed;
            var data = await liveRoom.GetGuardRank(uId);
            if (data.success)
            {
                list_Guard.ItemsSource = data.data;
                if (data.data == null || data.data.Count == 0)
                {
                    noGuardRank.Visibility = Visibility.Visible;
                }
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }
        /// <summary>
        /// 读取设置
        /// </summary>
        private void LoadSetting()
        {
            loadingSetting = true;

            openDanmu = SettingHelper.Get_LDanmuStatus();
            danmu.Visibility = openDanmu ? Visibility.Visible: Visibility.Collapsed;
            btn_OpenDanmu.Visibility = !openDanmu ? Visibility.Visible : Visibility.Collapsed;
            btn_CloseDanmu.Visibility = openDanmu ? Visibility.Visible : Visibility.Collapsed;

            //弹幕大小
            danmu.sizeZoom = SettingHelper.Get_NewLDMSize();
            slider_DanmuSize.Value = SettingHelper.Get_NewLDMSize();
            //弹幕透明度
            danmu.Opacity = SettingHelper.Get_LDMTran();
            slider_DanmuTran.Value = SettingHelper.Get_LDMTran();
            //弹幕速度
            danmu.speed = Convert.ToInt32(SettingHelper.Get_NewLDMSpeed());

            danmu.bold = SettingHelper.Get_BoldDanmu();
            sw_BoldDanmu.IsOn = SettingHelper.Get_BoldDanmu();

            slider_DanmuSpeed.Value = SettingHelper.Get_NewLDMSpeed();

            //弹幕边框
            danmu.borderStyle = (DanmakuBorderStyle)SettingHelper.Get_DMStyle();
            //音量
            media.Volume = SettingHelper.Get_LVolume();
            slider_volume.Value = SettingHelper.Get_LVolume();
            //自动领取瓜子
            autoGetAward = SettingHelper.Get_LAutoGetAward();
            sw_AutoGetAward.IsOn = SettingHelper.Get_LAutoGetAward();

            //硬件加速
            media.HardwareAcceleration = SettingHelper.Get_Forcelive();
            sw_HardwareAcceleration.IsOn = SettingHelper.Get_Forcelive();

            //自动清理弹幕
            slider_Clear.Value = SettingHelper.Get_LClear();

            //弹幕加载延迟
            _biliLiveDanmu.delay = SettingHelper.Get_LDelay();
            slider_DanmuDelay.Value = SettingHelper.Get_LDelay();

            sw_receiveGiftMsg.IsOn = SettingHelper.Get_LReceiveGiftMsg();
            receiveGiftMsg = SettingHelper.Get_LReceiveGiftMsg();

            sw_receiveWelcomeMsg.IsOn = SettingHelper.Get_LReceiveWelcomeMsg();
            receiveWelcomeMsg = SettingHelper.Get_LReceiveWelcomeMsg();

            sw_receiveSysMsg.IsOn = SettingHelper.Get_LReceiveSysMsg();
            receiveSysMsg = SettingHelper.Get_LReceiveSysMsg();


            if (!SettingHelper.IsPc())
            {
                btn_winfull.Visibility = Visibility.Collapsed;
                btn_exitwinfull.Visibility = Visibility.Collapsed;
                btn_Mini.Visibility = Visibility.Collapsed;
                btn_ExitMini.Visibility = Visibility.Collapsed;

            }

            loadingSetting = false;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (isMini || winfull)
            {
                return base.MeasureOverride(availableSize);
            }
            if (isFull)
            {
                wide.Width = new GridLength(0.3, GridUnitType.Star);
                height.Height = GridLength.Auto;
                Grid.SetRow(info, 0);
                Grid.SetColumnSpan(medias, 1);
                Grid.SetColumnSpan(info, 1);
                wide.Width = new GridLength(0, GridUnitType.Pixel);
                info.Visibility = Visibility.Collapsed;
                return base.MeasureOverride(availableSize);

            }
            if (availableSize.Width < 600)
            {
                wide.Width = GridLength.Auto;

                height.Height = new GridLength(0.6, GridUnitType.Star);
                Grid.SetRow(info, 1);
                Grid.SetColumnSpan(medias, 2);
                Grid.SetColumnSpan(info, 2);

                danmu.SetSpeed(5);
                danmu.sizeZoom = 0.5;
            }
            else
            {
                wide.Width = new GridLength(0.3, GridUnitType.Star);
                height.Height = GridLength.Auto;
                Grid.SetRow(info, 0);
                Grid.SetColumnSpan(medias, 1);
                Grid.SetColumnSpan(info, 1);

                danmu.speed = SettingHelper.Get_DMSpeed().ToInt32();
                danmu.sizeZoom = SettingHelper.Get_NewDMSize();
            }

            return base.MeasureOverride(availableSize);
        }
        /// <summary>
        /// 处理宝箱
        /// </summary>
        private async void HandelFreeSilver()
        {
            //加载宝箱
            var content = await liveRoom.GetFreeSilverCurrentTask();
            if (content.success)
            {
                if (freeSilverTimer == null)
                {
                    freeSilverTimer = new DispatcherTimer();
                    freeSilverTimer.Interval = TimeSpan.FromSeconds(1);
                    freeSilverTimer.Tick += FreeSilverTimer_Tick;
                }
                freeSilverTimer.Start();
                freeSilverTime = content.data;
            }
            else
            {
                if (freeSilverTimer != null)
                {
                    freeSilverTimer.Stop();
                }
                btn_CanGetAward.Visibility = Visibility.Collapsed;
                btn_UnGetAward.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 宝箱领取倒计时事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FreeSilverTimer_Tick(object sender, object e)
        {
            if (DateTime.Now >= freeSilverTime)
            {
                btn_CanGetAward.Visibility = Visibility.Visible;
                btn_UnGetAward.Visibility = Visibility.Collapsed;
                freeSilverTimer.Stop();
                if (autoGetAward)
                {
                    btn_CanGetAward_Click(sender, null);
                }
            }
            else
            {
                txt_surplus.Text = (freeSilverTime - DateTime.Now).ToString(@"mm\:ss");
                btn_CanGetAward.Visibility = Visibility.Collapsed;
                btn_UnGetAward.Visibility = Visibility.Visible;
            }

        }
        /// <summary>
        /// 读取最近10条弹幕
        /// </summary>
        private void LoadLastMsg()
        {
            //var last = await liveRoom.GetLastLiveMsg(roomId);
            //if (last.success)
            //{
            //    foreach (var m in last.data)
            //    {
            //        if (m.medalColor != null && m.medalColor != "")
            //        {
            //            m.ul_color = new SolidColorBrush(Utils.ToColor(m.ulColor));
            //        }
            //        else
            //        {
            //            m.ul_color = new SolidColorBrush(Colors.Gray);
            //        }
            //        if (m.medalColor != null && m.medalColor != "")
            //        {
            //            m.medal_color = new SolidColorBrush(Utils.ToColor(m.medalColor));
            //        }

            //        list.Items.Add(m);
            //    }
            //}

        }
        /// <summary>
        /// 弹幕事件
        /// </summary>
        /// <param name="value"></param>
        private async void _biliLiveDanmu_HasDanmu(BiliLiveDanmu.LiveDanmuModel value)
        {
            try
            {
                switch (value.type)
                {
                    case BiliLiveDanmu.LiveDanmuTypes.Viewer:
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            txt_online.Text = value.viewer.ToW();
                        });
                        break;
                    case BiliLiveDanmu.LiveDanmuTypes.Danmu:
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {

                            var m = value.value as DanmuMsgModel;
                            m.uname_color = new SolidColorBrush(Colors.Gray);
                            if (openDanmu)
                            {
                                danmu.AddLiveDanmu(m.text, false, Colors.White);
                            }
                            if (m.medalColor != null && m.medalColor != "")
                            {
                                m.ul_color = new SolidColorBrush(Utils.ToColor(m.ulColor));
                            }
                            else
                            {
                                m.ul_color = new SolidColorBrush(Colors.Gray);
                            }
                            if (m.medalColor != null && m.medalColor != "")
                            {
                                m.medal_color = new SolidColorBrush(Utils.ToColor(m.medalColor));
                            }
                            m.content_color = (SettingHelper.Get_Theme() == "Dark") ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                            danmu_list.Add(m);

                        });
                        break;
                    case BiliLiveDanmu.LiveDanmuTypes.Gift:
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (!receiveGiftMsg)
                            {
                                return;
                            }
                            var info = value.value as GiftMsgModel;
                            if (openDanmu)
                            {
                                if (info.giftName == "FFF")
                                {
                                    danmu.AddRollImageDanmu(new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Img/fff.png")));
                                }
                                if (info.giftName == "233")
                                {
                                    danmu.AddRollImageDanmu(new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Img/233.png")));
                                }
                                if (info.giftName == "666")
                                {
                                    danmu.AddRollImageDanmu(new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Img/666.png")));
                                }
                            }

                            danmu_list.Add(new DanmuMsgModel()
                            {
                                hasUL = Visibility.Collapsed,
                                username = info.uname + " 赠送了",
                                uname_color = new SolidColorBrush(Colors.Gray),
                                content_color = new SolidColorBrush(Colors.HotPink),
                                text = info.giftName + "x" + info.num
                            });

                        });
                        break;
                    case BiliLiveDanmu.LiveDanmuTypes.Welcome:
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (!receiveWelcomeMsg)
                            {
                                return;
                            }
                            var info = value.value as WelcomeMsgModel;

                            danmu_list.Add(new DanmuMsgModel()
                            {
                                isVip = ((info.svip) ? Visibility.Collapsed : Visibility.Visible),
                                isBigVip = ((info.svip) ? Visibility.Visible : Visibility.Collapsed),
                                hasUL = Visibility.Collapsed,
                                username = info.uname,
                                content_color = (SettingHelper.Get_Theme() == "Dark") ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black),
                                uname_color = new SolidColorBrush(Colors.HotPink),
                                text = " 进入直播间"
                            });

                        });


                        break;
                    case BiliLiveDanmu.LiveDanmuTypes.SystemMsg:
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (!receiveSysMsg)
                            {
                                return;
                            }
                            danmu_list.Add(new DanmuMsgModel()
                            {
                                hasUL = Visibility.Collapsed,
                                username = "",
                                content_color = new SolidColorBrush(Colors.HotPink),
                                text = value.value.ToString().Replace("?", "")
                            });
                        });

                        break;
                    default:
                        break;
                }

                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (danmu_list.Count > slider_Clear.Value)
                    {
                        danmu_list.Clear();
                        GC.Collect();
                    }
                });

            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 铺满屏幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_winfull_Click(object sender, RoutedEventArgs e)
        {
            winfull = true;
            btn_winfull.Visibility = Visibility.Collapsed;
            btn_exitwinfull.Visibility = Visibility.Visible;
            wide.Width = new GridLength(0, GridUnitType.Pixel);
            info.Visibility = Visibility.Collapsed;


        }
        /// <summary>
        /// 取消铺满屏幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_exitwinfull_Click(object sender, RoutedEventArgs e)
        {
            winfull = false;
            btn_winfull.Visibility = Visibility.Visible;
            btn_exitwinfull.Visibility = Visibility.Collapsed;

            wide.Width = new GridLength(0.3, GridUnitType.Star);
            info.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 开启画中画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Mini_Click(object sender, RoutedEventArgs e)
        {
            if (Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                isMini = true;
                await Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                btn_winfull_Click(sender, e);
                danmu.ClearAll();
                danmu.SetSpeed(5);
                danmu.sizeZoom = 0.5;
                btn_Mini.Visibility = Visibility.Collapsed;
                btn_ExitMini.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// 关闭画中画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_ExitMini_Click(object sender, RoutedEventArgs e)
        {
            isMini = false;
            await Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            btn_exitwinfull_Click(sender, e);
            danmu.ClearAll();
            danmu.speed = SettingHelper.Get_DMSpeed().ToInt32();
            danmu.sizeZoom = SettingHelper.Get_NewDMSize();
            btn_Mini.Visibility = Visibility.Visible;
            btn_ExitMini.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 退出播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        /// <summary>
        /// 显示/隐藏控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (controls.Visibility == Visibility.Visible)
            {
                controls.Visibility = Visibility.Collapsed;
            }
            else
            {
                controls.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// 双击全屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (btn_full.Visibility == Visibility.Visible)
            {
                btn_full_Click(sender, e);
            }
            else
            {
                btn_exitfull_Click(sender, e);
            }
        }
        /// <summary>
        /// 全屏事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_full_Click(object sender, RoutedEventArgs e)
        {
            isFull = true;
            btn_full.Visibility = Visibility.Collapsed;
            btn_exitfull.Visibility = Visibility.Visible;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            DisplayInformation.AutoRotationPreferences = (DisplayOrientations)5;

        }
        /// <summary>
        /// 退出全屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_exitfull_Click(object sender, RoutedEventArgs e)
        {
            isFull = false;
            btn_full.Visibility = Visibility.Visible;
            btn_exitfull.Visibility = Visibility.Collapsed;
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().ExitFullScreenMode();
            wide.Width = new GridLength(0.3, GridUnitType.Star);
            info.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 线路切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_line_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loadPlayurling)
            {
                return;
            }
            media.Source = durls[cb_line.SelectedIndex].url;
        }
        /// <summary>
        /// 清晰度切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_quality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loadPlayurling)
            {
                return;
            }
            ChangeQuality((cb_quality.SelectedItem as quality_description_item).qn);
            //LoadPlayUrl((cb_quality.SelectedItem as quality_description_item).qn);
        }
        /// <summary>
        /// 点击UP信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ShowUserData_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = 1;
        }
        /// <summary>
        /// 关闭弹幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CloseDanmu_Click(object sender, RoutedEventArgs e)
        {
            btn_OpenDanmu.Visibility = Visibility.Visible;
            btn_CloseDanmu.Visibility = Visibility.Collapsed;
            openDanmu = false;
            danmu.ClearAll();
            danmu.Visibility = Visibility.Collapsed;
            SettingHelper.Set_LDanmuStatus(false);
        }
        /// <summary>
        /// 开启弹幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OpenDanmu_Click(object sender, RoutedEventArgs e)
        {
            btn_OpenDanmu.Visibility = Visibility.Collapsed;
            btn_CloseDanmu.Visibility = Visibility.Visible;
            openDanmu = true;
            danmu.Visibility = Visibility.Visible;
            SettingHelper.Set_LDanmuStatus(true);
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            btn_Pause.Visibility = Visibility.Collapsed;
            btn_Play.Visibility = Visibility.Visible;
            media.Pause();
        }
        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            btn_Pause.Visibility = Visibility.Visible;
            btn_Play.Visibility = Visibility.Collapsed;
            media.Play();
        }
        /// <summary>
        /// 静音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Mute_Click(object sender, RoutedEventArgs e)
        {
            btn_Mute.Visibility = Visibility.Collapsed;
            btn_Volume.Visibility = Visibility.Visible;
            media.IsMuted = true;
            slider_volume.IsEnabled = false;
        }
        /// <summary>
        /// 取消静音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Volume_Click(object sender, RoutedEventArgs e)
        {
            btn_Mute.Visibility = Visibility.Visible;
            btn_Volume.Visibility = Visibility.Collapsed;
            media.IsMuted = false;
            slider_volume.IsEnabled = true;
        }
        /// <summary>
        /// 音量调节
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            media.Volume = slider_volume.Value.ToInt32();
            SettingHelper.Set_LVolume(slider_volume.Value.ToInt32());
        }
        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!ApiHelper.IsLogin())
            {
                if (!await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
            }
            if (txt_message.Text.Length == 0)
            {
                Utils.ShowMessageToast("请输入内容");
                return;
            }
            var data = await liveRoom.SendDanmu(txt_message.Text, roomId);
            if (data.success)
            {
                if (openDanmu)
                {
                    danmu.AddLiveDanmu(txt_message.Text, true, Colors.White);
                }
                txt_message.Text = "";
                Utils.ShowMessageToast("发送弹幕成功");
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }
        /// <summary>
        /// 发送热词
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void list_hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                if (!await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
            }
            var data = await liveRoom.SendDanmu(e.ClickedItem.ToString(), roomId);
            if (data.success)
            {
                if (openDanmu)
                {
                    danmu.AddLiveDanmu(e.ClickedItem.ToString(), true, Colors.White);
                }
                flyout_hotword.Hide();
                Utils.ShowMessageToast("发送弹幕成功");
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }

        }
        /// <summary>
        /// 播放状态变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void media_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (media.CurrentState)
                {
                    case MediaElementState.Closed:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (_systemMediaTransportControls != null)
                            {
                                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                            }
                        });
                        break;
                    case MediaElementState.Opening:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            mediaLoading.Visibility = Visibility.Visible;
                        });

                        break;
                    case MediaElementState.Buffering:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            mediaLoading.Visibility = Visibility.Visible;
                        });
                        break;
                    case MediaElementState.Playing:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            btn_Pause.Visibility = Visibility.Visible;
                            btn_Play.Visibility = Visibility.Collapsed;
                            mediaLoading.Visibility = Visibility.Collapsed;
                            if (_systemMediaTransportControls != null)
                            {
                                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                            }
                        });

                        break;
                    case MediaElementState.Paused:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            btn_Pause.Visibility = Visibility.Collapsed;
                            btn_Play.Visibility = Visibility.Visible;
                            if (_systemMediaTransportControls != null)
                            {
                                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                            }
                        });
                        break;
                    case MediaElementState.Stopped:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            btn_Play.Visibility = Visibility.Visible;
                            btn_Pause.Visibility = Visibility.Collapsed;
                            if (_systemMediaTransportControls != null)
                            {
                                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                            }
                        });
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 播放发生错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void media_MediaFailed(object sender, VLC.MediaFailedRoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                new MessageDialog("无法加载直播").ShowAsync();
            });

        }
        private void btn_UnGetAward_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("还没到时间呢");
        }
        /// <summary>
        /// 领取瓜子
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_CanGetAward_Click(object sender, RoutedEventArgs e)
        {
            var content = await liveRoom.GetFreeSilverAward();
            if (content.success)
            {
                txt_silver.Text = content.data.ToW();
                Utils.ShowMessageToast(content.message);
                HandelFreeSilver();
            }
            else
            {
                Utils.ShowMessageToast(content.message);
            }
        }
        /// <summary>
        /// 设置自动清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_Clear_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            SettingHelper.Set_LClear(slider_Clear.Value.ToInt32());
        }
        /// <summary>
        /// 设置弹幕延迟
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_DanmuDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            _biliLiveDanmu.delay = slider_DanmuDelay.Value.ToInt32();
            SettingHelper.Set_LDelay(slider_DanmuDelay.Value.ToInt32());


        }
        /// <summary>
        /// 设置自动领取瓜子
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sw_AutoGetAward_Toggled(object sender, RoutedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            autoGetAward = sw_AutoGetAward.IsOn;
            SettingHelper.Set_LAutoGetAward(sw_AutoGetAward.IsOn);
        }
        /// <summary>
        /// 设置硬件加速
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sw_HardwareAcceleration_Toggled(object sender, RoutedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            media.HardwareAcceleration = sw_HardwareAcceleration.IsOn;
            SettingHelper.Set_Forcelive(sw_HardwareAcceleration.IsOn);
        }
        /// <summary>
        /// 设置弹幕大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_DanmuSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }

            SettingHelper.Set_LDMSize(slider_DanmuSize.Value);
            danmu.sizeZoom = slider_DanmuSize.Value;
        }
        /// <summary>
        /// 设置弹幕速度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_DanmuSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            SettingHelper.Set_LDMSpeed(slider_DanmuSpeed.Value);
            danmu.speed = slider_DanmuSpeed.Value.ToInt32();

        }
        /// <summary>
        /// 设置弹幕透明度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_DanmuTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            SettingHelper.Set_LDMTran(slider_DanmuTran.Value);
            danmu.Opacity = slider_DanmuTran.Value;

        }
        /// <summary>
        /// 设置接收礼物信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sw_receiveGiftMsg_Toggled(object sender, RoutedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            receiveGiftMsg = sw_receiveGiftMsg.IsOn;
            SettingHelper.Set_LReceiveGiftMsg(sw_receiveGiftMsg.IsOn);
        }
        /// <summary>
        /// 设置接收欢迎信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sw_receiveWelcomeMsg_Toggled(object sender, RoutedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            receiveWelcomeMsg = sw_receiveWelcomeMsg.IsOn;
            SettingHelper.Set_LReceiveWelcomeMsg(sw_receiveWelcomeMsg.IsOn);
        }
        /// <summary>
        /// 设置接收系统信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sw_receiveSysMsg_Toggled(object sender, RoutedEventArgs e)
        {
            if (loadingSetting)
            {
                return;
            }
            receiveSysMsg = sw_receiveSysMsg.IsOn;
            SettingHelper.Set_LReceiveSysMsg(sw_receiveSysMsg.IsOn);
        }
        /// <summary>
        /// 打开设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            settingDialog.Visibility = Visibility.Visible;
            await settingDialog.ShowAsync();
        }
        /// <summary>
        /// 刷新整个页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
            _biliLiveDanmu.Dispose();
            _biliLiveDanmu = new BiliLiveDanmu();
            _biliLiveDanmu.HasDanmu += _biliLiveDanmu_HasDanmu;
            await LoadRoomInfo();
        }
        /// <summary>
        /// 分享
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard("https://live.bilibili.com/" + roomId);
            Utils.ShowMessageToast("已将直播间地址复制到剪切板");
        }
        /// <summary>
        /// 取消关注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_UnFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                if (!await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
            }
            var content = await account.UnFollow((this.DataContext as LiveRoomInfoModel).uid.ToString());
            if (content.success)
            {
                btn_UnFollow.Visibility = Visibility.Collapsed;
                btn_Follow.Visibility = Visibility.Visible;
                Utils.ShowMessageToast("已经取消关注");
            }
            else
            {
                Utils.ShowMessageToast(content.message);
            }
        }
        /// <summary>
        /// 关注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Follow_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                if (!await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
            }
            var content = await account.Follow((this.DataContext as LiveRoomInfoModel).uid.ToString());
            if (content.success)
            {
                btn_UnFollow.Visibility = Visibility.Visible;
                btn_Follow.Visibility = Visibility.Collapsed;
                Utils.ShowMessageToast("关注成功");
            }
            else
            {
                Utils.ShowMessageToast(content.message);
            }
        }
        /// <summary>
        /// 打开个人空间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_User_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(UserCenterPage), (this.DataContext as LiveRoomInfoModel).uid);
        }
        /// <summary>
        /// 播放完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (playUrls==null)
            {
                Utils.ShowMessageToast("直播已结束，感谢收看");
                grid_isStop.Visibility = Visibility.Visible;
                return;
            }
            var now = playUrls.FindIndex(x => x.url == media.Source);
            if (now == playUrls.Count - 1)
            {
                media.Stop();
                media.Source = null;
                media.MediaSource = null;
                LoadRoundUrl();
            }
            else
            {
                media.Stop();
                media.Source = null;
                media.MediaSource = null;
                media.Source = playUrls[now + 1].url;
            }


        }
        /// <summary>
        /// 打开调试信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                if (Debug_Data.Visibility == Visibility.Collapsed)
                {
                    uint w, h = 0;
                    media.MediaPlayer.size(0, out w, out h);
                    Debug_Data.Visibility = Visibility.Visible;
                    txt_VideoData.Text = $"地址:{media.Source}\r\n硬件加速:{media.HardwareAcceleration}\r\n分辨率:{ w}*{ h}\r\n当前清晰度:{(cb_quality.SelectedItem as quality_description_item).desc}({(cb_quality.SelectedItem as quality_description_item).qn})";
                }
                else
                {
                    Debug_Data.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("读取直播媒体详细信息失败", LogType.ERROR,ex);
                Debug_Data.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 排行榜切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_rank_cate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_rank_cate.SelectedItem == null)
            {
                return;
            }
            LoadRank();
        }
        /// <summary>
        /// 选项卡切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(200);
            if (pivot.SelectedIndex == 2)
            {
                LoadRank();
            }
            if (pivot.SelectedIndex == 3)
            {
                LoadGuardRank();
            }
        }
        /// <summary>
        /// 赠送数量变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_BuyGiftNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            int num = 0;
            if (!int.TryParse(txt_BuyGiftNum.Text, out num))
            {
                txt_BuyGiftNum.Text = "1";
            }
        }
        /// <summary>
        /// 取消赠送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_cnacelSend_Buy_Click(object sender, RoutedEventArgs e)
        {
            cd_BuyGiftNum.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 点击赠送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_SendOk_Buy_Click(object sender, RoutedEventArgs e)
        {
            int onum = 0;
            if (!int.TryParse(txt_BuyGiftNum.Text, out onum))
            {
                Utils.ShowMessageToast("错误的数量", 3000);
                return;
            }
            if ((sender as Button).DataContext is LiveMyGiftsModel)
            {
                var data = (sender as Button).DataContext as LiveMyGiftsModel;
                var results = await liveRoom.SendMyGift(ApiHelper.GetUserId(), uId.ToString(), data.gift_id, onum, data.bag_id, roomId);
                if (results.success)
                {
                    Utils.ShowMessageToast("赠送成功");
                    cd_BuyGiftNum.Visibility = Visibility.Collapsed;
                    LoadMyGifts();
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }

            }
            else
            {
                string type = "silver";
                if (rb_Gold.IsChecked.Value)
                {
                    type = "gold";
                }
                var data = (sender as Button).DataContext as AllGiftsModel;
                var results = await liveRoom.SendGift(ApiHelper.GetUserId(), uId.ToString(), data.id, onum, roomId, type, data.price);
                if (results.success)
                {
                    Utils.ShowMessageToast("赠送成功");
                    cd_BuyGiftNum.Visibility = Visibility.Collapsed;
                    LoadMyInfo();
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }



        }
        /// <summary>
        /// 赠送我的背包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridview_myGifts_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveMyGiftsModel;
            cd_BuyGiftNum.DataContext = data;
            txt_BuyGiftNum.Text = "1";
            needType.Visibility = Visibility.Collapsed;
            cd_BuyGiftNum.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 赠送礼物
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void gridview_Gifts_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!ApiHelper.IsLogin())
            {
                if (!await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
            }
            var data = e.ClickedItem as AllGiftsModel;
            rb_Gold.Visibility = Visibility.Visible;
            rb_Slider.Visibility = Visibility.Collapsed;
            rb_Gold.IsChecked = true;
            txt_BuyGiftNum.Text = "1";
            if (data.coin_type == "silver")
            {
                rb_Slider.IsChecked = true;
                rb_Slider.Visibility = Visibility.Visible;
            }
            cd_BuyGiftNum.DataContext = data;
            needType.Visibility = Visibility.Visible;
            cd_BuyGiftNum.Visibility = Visibility.Visible;
        }

        private void sw_BoldDanmu_Toggled(object sender, RoutedEventArgs e)
        {
            danmu.bold = sw_BoldDanmu.IsOn;
            SettingHelper.Set_BoldDanmu(sw_BoldDanmu.IsOn);
        }
    }
}
