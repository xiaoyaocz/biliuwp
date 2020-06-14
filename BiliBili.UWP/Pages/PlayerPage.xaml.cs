using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Core;
using Newtonsoft.Json.Linq;
using Windows.Media.Editing;
using BiliBili.UWP.Helper;
using NSDanmaku.Helper;
using BiliBili.UWP.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using SYEngine;
using System.Diagnostics;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Api;
using Windows.Media.Streaming.Adaptive;
using Windows.Media.MediaProperties;
using System.Numerics;
using System.Threading;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    public enum PlayMode
    {
        Bangumi,
        Movie,
        VipBangumi,
        Video,
        QQ,
        Sohu,
        Local,
        FormLocal
    }
    enum HeartBeatType
    {
        Start,
        Play,
        End
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayerPage : Page
    {
        MediaPlayer mediaPlayer;
        MediaPlayer mediaPlayer_audio;
        PlayerAPI playerAPI;
        public PlayerPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            mediaPlayer = new MediaPlayer();
            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSession_BufferingProgressChanged;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            mediaElement.SetMediaPlayer(mediaPlayer);
            danmakuParse = new DanmakuParse();
            playerAPI = new PlayerAPI();
            MTC.DanmuLoaded += MTC_DanmuLoaded;
            if (SettingHelper.Get_BackPlay())
            {
                _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
                _systemMediaTransportControls.IsPlayEnabled = true;
                _systemMediaTransportControls.IsPauseEnabled = true;
                _systemMediaTransportControls.ButtonPressed += _systemMediaTransportControls_ButtonPressed;
            }
        }

        private void MediaPlayer_VolumeChanged(MediaPlayer sender, object args)
        {
            if (mediaPlayer_audio != null )
            {
                mediaPlayer_audio.Volume = sender.Volume;
            }
        }

        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            if (mediaPlayer_audio!=null&&Math.Abs( sender.Position.TotalSeconds- mediaPlayer_audio.PlaybackSession.Position.TotalSeconds)>1)
            {
                mediaPlayer_audio.PlaybackSession.Position = sender.Position;
            }
            
        }




        #region MediaPlayer事件
        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
           
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {

                try
                {
                    SetSystemMediaTransportControl();
                    MTC_Video360Changed(this, MTC.Video360);
                    var record = SqlHelper.GetVideoWatchRecord(string.IsNullOrEmpty(playNow.episode_id) ? playNow.Mid : "ep" + playNow.episode_id);
                    if (record != null && record.Post != 0)
                    {
                        if (SettingHelper.Get_SkipToHistory())
                        {
                            mediaElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, record.Post);
                        }
                        else
                        {
                            TimeSpan ts = new TimeSpan(0, 0, record.Post);
                            LastPost = record.Post;
                            btn_ViewPost.Content = "上次播放到" + ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                            btn_ViewPost.Visibility = Visibility.Visible;
                            await Task.Delay(5000);
                            btn_ViewPost.Visibility = Visibility.Collapsed;
                        }
                    }


                }
                catch (Exception)
                {

                }
            });
        }
        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            buffering = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    //case  MediaPlaybackState.Closed:
                    //    if (_systemMediaTransportControls != null)
                    //    {
                    //        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    //    }


                    //    break;
                    case MediaPlaybackState.Opening:

                        progress.Visibility = Visibility.Visible;

                        break;
                    case MediaPlaybackState.Buffering:
                        buffering = true;
                        progress.Visibility = Visibility.Visible;
                        mediaPlayer_audio?.Pause();
                        danmu.PauseDanmaku();
                        break;
                    case MediaPlaybackState.Playing:
                        if (_systemMediaTransportControls != null)
                        {
                            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        }
                        mediaPlayer_audio?.Play();
                        progress.Visibility = Visibility.Collapsed;
                        danmu.ResumeDanmaku();

                        if (timer != null)
                        {
                            timer.Start();
                        }
                        mediaElement.MediaPlayer.PlaybackSession.PlaybackRate = slider_Rate.Value;

                        break;
                    case MediaPlaybackState.Paused:
                        if (_systemMediaTransportControls != null)
                        {
                            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                        }

                        progress.Visibility = Visibility.Collapsed;
                        danmu.PauseDanmaku();
                        if (timer != null)
                        {
                            timer.Stop();
                        }
                        mediaPlayer_audio?.Pause();
                        break;
                    //case MediaPlaybackState.Stopped:
                    //    if (_systemMediaTransportControls != null)
                    //    {
                    //        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    //    }

                    //    progress.Visibility = Visibility.Collapsed;
                    //    danmu.ClearAll();
                    //    if (timer != null)
                    //    {
                    //        timer.Stop();
                    //    }

                    //    break;
                    default:
                        break;
                }
            });

        }
        private async void PlaybackSession_BufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                pr.Text = mediaElement.MediaPlayer.PlaybackSession.BufferingProgress.ToString("P");
            });
        }
        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await new MessageDialog("无法播放此视频 ＞﹏＜ \r\n请尝试更换清晰度或者在播放设置中打开/关闭DASH").ShowAsync();
            });
        }
        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    if (cb_setting_1.IsChecked.Value)
                    {
                        mediaElement.MediaPlayer.Play();
                        danmu.ClearAll();
                        return;
                    }
                    if (gv_play.SelectedIndex == gv_play.Items.Count - 1)
                    {
                        if (playNow.isInteraction)
                        {
                            if (nodeInfo.edges != null)
                            {
                                if (nodeInfo.edges.choices.Count == 1)
                                {
                                    ChangeNode(nodeInfo.edges.choices[0].node_id, nodeInfo.edges.choices[0].cid.ToString());
                                }
                                else
                                {
                                    gridview_node.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                Utils.ShowMessageToast("互动视频已结束，可点击右下角选择节点重新开始", 3000);
                            }

                        }
                        else
                        {
                            if (cb_setting_2.IsChecked.Value)
                            {
                                gv_play.SelectedIndex = 0;
                            }
                            else
                            {
                                Utils.ShowMessageToast("全部看完了", 3000);
                            }
                        }
                    }
                    else
                    {
                        //mediaElement.MediaPlayer.PlaybackSession.();
                        Utils.ShowMessageToast("3秒后播放下一集", 3000);
                        await Task.Delay(3000, m_TaskDelayCancellation.Token); // throws
                        
                        gv_play.SelectedIndex += 1;
                    }
                }
                catch (Exception)
                {
                }
            });

        }

        private CancellationTokenSource m_TaskDelayCancellation = new CancellationTokenSource();

        #endregion
        private void MTC_DanmuLoaded(object sender, NSDanmaku.Controls.Danmaku e)
        {
            if (e != null)
            {
                danmu = e;
            }
        }

        private async void _systemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaElement.MediaPlayer.Play();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaElement.MediaPlayer.Pause();
                    });
                    break;
                default:
                    break;
            }
        }

        private void PlayerPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {

            args.Handled = true;
            if (sp_View.IsPaneOpen)
            {
                return;
            }
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.A:
                    if (MTC.Video360)
                        mediaPlayer.PlaybackSession.SphericalVideoProjection.ViewOrientation *= Quaternion.CreateFromYawPitchRoll(.05f, 0, 0);
                    break;
                case Windows.System.VirtualKey.S:
                    if (MTC.Video360)
                        mediaPlayer.PlaybackSession.SphericalVideoProjection.ViewOrientation *= Quaternion.CreateFromYawPitchRoll(0,0, .05f);
                    break;
                case Windows.System.VirtualKey.W:
                    if (MTC.Video360)
                        mediaPlayer.PlaybackSession.SphericalVideoProjection.ViewOrientation *= Quaternion.CreateFromYawPitchRoll(0,0, -.05f);
                    break;
                case Windows.System.VirtualKey.D:
                    if (MTC.Video360)
                        mediaPlayer.PlaybackSession.SphericalVideoProjection.ViewOrientation *= Quaternion.CreateFromYawPitchRoll(-.05f, 0, 0);
                    break;

                case Windows.System.VirtualKey.Space:
                    if (mediaElement.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                    {
                        mediaElement.MediaPlayer.Pause();
                    }
                    else
                    {
                        mediaElement.MediaPlayer.Play();
                    }
                    break;
                case Windows.System.VirtualKey.Left:
                    TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds - 3));
                    mediaElement.MediaPlayer.PlaybackSession.Position = ts;
                    Utils.ShowMessageToast(mediaElement.MediaPlayer.PlaybackSession.Position.Hours.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.Position.Minutes.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.Position.Seconds.ToString("00"), 3000);
                    break;
                case Windows.System.VirtualKey.Up:
                    {
                        var volume = mediaElement.MediaPlayer.Volume + 0.1;
                        SetVolume(volume);
                        Utils.ShowMessageToast("音量:" + mediaElement.MediaPlayer.Volume.ToString("P"), 3000);
                    }
                    break;
                case Windows.System.VirtualKey.Right:
                    TimeSpan ts2 = new TimeSpan(0, 0, Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds + 3));
                    mediaElement.MediaPlayer.PlaybackSession.Position = ts2;
                    Utils.ShowMessageToast(mediaElement.MediaPlayer.PlaybackSession.Position.Hours.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.Position.Minutes.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.Position.Seconds.ToString("00"), 3000);
                    break;
                case Windows.System.VirtualKey.Down:
                    {
                        var volume = mediaElement.MediaPlayer.Volume-0.1;
                        SetVolume(volume);
                        Utils.ShowMessageToast("音量:" + mediaElement.MediaPlayer.Volume.ToString("P"), 3000);
                    }
                    break;
                case Windows.System.VirtualKey.Escape:
                    if (MTC.IsFullWindow)
                    {
                        ApplicationView.GetForCurrentView().ExitFullScreenMode();
                    }
                    break;

                case Windows.System.VirtualKey.F11:
                case Windows.System.VirtualKey.Enter:
                    if (!MTC.IsFullWindow)
                    {
                        MTC.ToFull();
                        //ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

                    }
                    else
                    {
                        MTC.ExitFull();
                        //ApplicationView.GetForCurrentView().ExitFullScreenMode();

                    }
                    break;
                //跳过OP 90秒
                case Windows.System.VirtualKey.O:
                case Windows.System.VirtualKey.P:
                    {
                        mediaElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds + 90));
                        Utils.ShowMessageToast(mediaElement.MediaPlayer.PlaybackSession.Position.Hours.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.Position.Minutes.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.Position.Seconds.ToString("00"), 3000);
                    }
                    break;
                //打开关闭弹幕
                case Windows.System.VirtualKey.I:
                case Windows.System.VirtualKey.F9:
                    {
                        MTC.OpenOrCloseDanmaku();
                    }
                    break;
                //上一话
                case (Windows.System.VirtualKey)188:
                case Windows.System.VirtualKey.N:
                    if (gv_play.SelectedIndex == 0)
                    {
                        Utils.ShowMessageToast("前面没有了");
                        return;
                    }
                    gv_play.SelectedIndex -= 1;
                    break;
                //下一话
                case (Windows.System.VirtualKey)190:
                case Windows.System.VirtualKey.M:
                    if (gv_play.SelectedIndex == gv_play.Items.Count - 1)
                    {
                        Utils.ShowMessageToast("后面没有了");
                        return;
                    }
                    gv_play.SelectedIndex += 1;
                    break;
                case Windows.System.VirtualKey.F10:
                    CaptureVideo();
                    break;
                default:
                    break;
            }
        }



        SystemMediaTransportControls _systemMediaTransportControls;
        private DisplayRequest dispRequest = null;//保持屏幕常亮
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CoreWindow.GetForCurrentThread().KeyDown += PlayerPage_KeyDown;
            this.Frame.Visibility = Visibility.Visible;
            int flag = 1;
            while (true)
            {
                if (danmu != null)
                {
                    break;
                }
                if (flag >= 100)
                {
                    MessageDialog messageDialog = new MessageDialog("播放组件似乎加载失败了,是否报告开发者？");
                    messageDialog.Commands.Add(new UICommand("确定", (sender) => { LogHelper.WriteLog("无法初始化播放器", LogType.ERROR); }));
                    messageDialog.Commands.Add(new UICommand("取消"));
                    await messageDialog.ShowAsync();
                    flag = 1;
                }
                await Task.Delay(100);
                flag++;
            }

            // CheckNetwork();
            //await Task.sp_View(200);
            if (e.NavigationMode == NavigationMode.New)
            {
                object[] obj = e.Parameter as object[];
                var ls = obj[0] as List<PlayerModel>;
                var index = (int)obj[1];

                LoadPlayer(ls, index);
                sp_View.Focus(FocusState.Pointer);
            }

        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            try
            {
                ClosePlayer();
                //Debug.WriteLine("开始返回");
                CoreWindow.GetForCurrentThread().KeyDown -= PlayerPage_KeyDown;
                this.Frame.Visibility = Visibility.Collapsed;
                mediaElement.MediaPlayer.Pause();

                //mediaElement.Stop();
                base.OnNavigatingFrom(e);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task CheckNetwork()
        {
            if (SystemHelper.GetNetWorkType() == NetworkType.Other && SettingHelper.Get_Use4GPlay())
            {
                var md = new MessageDialog("当前使用的是数据网络，是否继续播放？\r\n可到设置中关闭此提醒", "播放询问");
                md.Commands.Add(new UICommand("确定", new UICommandInvokedHandler((e) => { })));
                md.Commands.Add(new UICommand("取消", new UICommandInvokedHandler((e) => { this.Frame.GoBack(); return; })));
                await md.ShowAsync();
            }
        }


        private NSDanmaku.Controls.Danmaku danmu;
        DanmakuParse danmakuParse;
        DispatcherTimer timer;
        DispatcherTimer timer_Date;
        List<PlayerModel> playList;
        List<NSDanmaku.Model.DanmakuModel> DanMuPool = null;
        PlayerModel playNow;
        InteractionVideo interactionVideo;
        NodeInfo nodeInfo;
        //int _index = 0;
       
        bool LoadDanmu = true;
        int LastPost = 0;
        bool settingFlag = true;
        BrightnessOverride bo;
        double _brightness;
        double Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                //if (bo != null && bo.IsSupported)
                //{
                //    // 0-dark => 1-light
                //    bo.SetBrightnessLevel(1 - value, DisplayBrightnessOverrideOptions.None);
                //}
                //else
                //{
                // 0-light => 1-dark
                MTC.Brightness = value;
                //}
            }
        }

        public async void LoadPlayer(List<PlayerModel> par, int index)
        {


            await Task.Delay(200);
            danmu = MTC.myDanmaku;

            UpdateSetting();
            if (SettingHelper.Get_BackPlay())
            {
                //_mediaPlayer = new MediaPlayer();
                // _systemMediaTransportControls = _mediaPlayer.SystemMediaTransportControls;
                //_mediaPlayer.CommandManager.IsEnabled = true;

                _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
                _systemMediaTransportControls.IsPlayEnabled = true;
                _systemMediaTransportControls.IsPauseEnabled = true;
                _systemMediaTransportControls.ButtonPressed += _systemMediaTransportControls_ButtonPressed;
            }
            //if (timer == null)
            //{
            //    timer = new DispatcherTimer();
            //    timer.Interval = new TimeSpan(0, 0, 1);
            //    timer.Tick += Timer_Tick;
            //    timer.Start();
            //}
            if (timer_Date == null)
            {
                timer_Date = new DispatcherTimer();
                timer_Date.Interval = new TimeSpan(0, 0, 1);
                timer_Date.Tick += Timer_Date_Tick; ;
                timer_Date.Start();
            }
            if (dispRequest == null)
            {
                // 用户观看视频，需要保持屏幕的点亮状态
                dispRequest = new DisplayRequest();
                dispRequest.RequestActive(); // 激活显示请求
            }
            if (SettingHelper.Get_QZHP())
            {
                DisplayInformation.AutoRotationPreferences = (DisplayOrientations)5;
            }

            if (SettingHelper.Get_AutoFull())
            {
                MTC.ToFull();
                //mediaElement.IsFullWindow = true;
            }

            playList = par;
            playNow = playList[index];
            if (playNow.isInteraction)
            {
                interactionVideo = new InteractionVideo(playNow.Aid, playNow.graph_version);
                nodeInfo = await interactionVideo.GetNodes(playNow.node_id);
                gridview_node.ItemsSource = nodeInfo?.edges?.choices;
                gv_story_list.ItemsSource = nodeInfo?.story_list;
                settingStorylist = true;
                gv_story_list.SelectedItem = nodeInfo.story_list.FirstOrDefault(x => x.node_id == nodeInfo.node_id);
                settingStorylist = false;
            }

            //  btn_HideInfo.Visibility = Visibility.Collapsed;
            //   btn_ShowInfo.Visibility = Visibility.Collapsed;
            mediaElement.AutoPlay = true;

            gv_play.ItemsSource = playList;
            gv_play.SelectedIndex = index;


            //DisplayInformation.AutoRotationPreferences = (DisplayOrientations)5;

        }

        public async void ClosePlayer()
        {
            try
            {

                if (dispRequest != null)
                {
                    dispRequest = null;
                }
                await ReportHistory(Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds));

                SettingHelper.Set_Volume(mediaElement.MediaPlayer.Volume);
                SettingHelper.Set_Light(Brightness);
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }
                if (timer_Date != null)
                {
                    timer_Date.Stop();
                    timer_Date = null;
                }
                if (bo != null)
                {
                    if (bo.IsOverrideActive)
                        bo.StopOverride();
                    bo = null;
                }
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                //_mediaPlayer.Source = null;
                //_mediaPlayer = null;

                if (_systemMediaTransportControls != null)
                {
                    _systemMediaTransportControls.DisplayUpdater.ClearAll();
                    _systemMediaTransportControls.IsEnabled = false;
                    _systemMediaTransportControls = null;
                }
                //  mediaElement.Stop();
                //  gv_play.ItemsSource = null;
                // mediaElement.Source = null;

                mediaPlayer = null;
                mediaPlayer_audio = null;
                //danmu.ClearAll();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;

                if (m_TaskDelayCancellation != null)
                    m_TaskDelayCancellation.Cancel();

                GC.Collect();

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 上传播放记录
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private async Task ReportHistory(int progress)
        {
            try
            {
                UpdateLocalHistory(progress);
                var api = playerAPI.SeasonHistoryReport(playNow.Aid, playNow.Mid, progress, playNow.banId, playNow.episode_id, playNow.playMode == PlayMode.Video ? 3 : 4);
                await api.Request();
                Debug.WriteLine(progress);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("上传播放记录失败", LogType.ERROR, ex);
            }
        }

        private void UpdateLocalHistory(int progress)
        {
            if (playNow.isInteraction)
            {
                progress = 0;
            }
            var id = playNow.Mid;
            if (!string.IsNullOrEmpty(playNow.episode_id))
            {
                //加EP是防止EPID与CID重复
                id = "ep" + playNow.episode_id;
            }
            var record = SqlHelper.GetVideoWatchRecord(id);
            if (record != null)
            {
                if (progress != 0)
                {
                    record.Post = progress;
                }
                record.viewTime = DateTime.Now;
                SqlHelper.UpdateVideoWatchRecord(record);
            }
            else
            {
                SqlHelper.AddVideoWatchRecord(new ViewPostHelperClass()
                {
                    epId = id,
                    Post = 0,
                    viewTime = DateTime.Now
                });
            }
        }


        public void UpdateSetting()
        {
            //if (!SettingHelper.IsPc())
            //{
            //    btn_ShowInfo.Visibility = Visibility.Collapsed;
            //    btn_HideInfo.Visibility = Visibility.Collapsed;
            //    // danmu.fontSize = 16;
            //}
            settingFlag = true;

            SYEngine.Core.ForceSoftwareDecode = SettingHelper.Get_ForceVideo();

            DanDis_Get();
            DMZZBDS = SettingHelper.Get_DMZZ();
            slider_DanmuSize.Value = SettingHelper.Get_NewDMSize();
            slider_Num.Value = SettingHelper.Get_DMNumber();
            slider_DanmuTran.Value = SettingHelper.Get_NewDMTran();
            slider_DanmuSpeed.Value = SettingHelper.Get_DMSpeed();
            cb_Style.SelectedIndex = SettingHelper.Get_DMStyle();

            sw_DanmuBorder.IsOn = SettingHelper.Get_DMBorder();
            sw_MergeDanmu.IsOn = SettingHelper.Get_MergeDanmu();
            mergeDanmu = sw_MergeDanmu.IsOn;

            sw_DanmuNotSubtitle.IsOn = SettingHelper.Get_DanmuNotSubtitle();
            //danmu.notHideSubtitle = sw_DanmuNotSubtitle.IsOn;

            sw_BoldDanmu.IsOn = SettingHelper.Get_BoldDanmu();

            sw_UseDASH.IsOn = SettingHelper.Get_UseDASH();
            btnOpenInstallHEVC.Visibility = Visibility.Visible;
            //if (!await SystemHelper.CheckCodec())
            //{

            //}
            //else
            //{
            //    btnOpenInstallHEVC.Visibility = Visibility.Collapsed;
            //}
            sw_DASHUseHEVC.IsOn = SettingHelper.Get_DASHUseHEVC();

            List<string> fonts = SystemHelper.GetSystemFontFamilies();
            cb_Font.ItemsSource = fonts;
            cb_SubtitleFont.ItemsSource = fonts;
            if (SettingHelper.Get_DanmuFont() != "")
            {
                cb_Font.SelectedIndex = fonts.IndexOf(SettingHelper.Get_DanmuFont());
            }
            else
            {
                cb_Font.SelectedIndex = fonts.IndexOf(cb_Font.FontFamily.Source);
            }
            if (SettingHelper.Get_SubtitleFontFamily() != "")
            {
                cb_SubtitleFont.SelectedIndex = fonts.IndexOf(SettingHelper.Get_SubtitleFontFamily());
            }
            else
            {
                cb_Font.SelectedIndex = fonts.IndexOf(cb_Font.FontFamily.Source);
            }

            var subColor = SettingHelper.Get_SubtitleColor();
            foreach (ComboBoxItem item in cb_SubtitleColor.Items)
            {
                if (item.Tag.ToString() == subColor)
                {
                    cb_SubtitleColor.SelectedItem = item;
                    break;
                }
            }
            slider_SubtitleTran.Value = SettingHelper.Get_SubtitleBgTran();
            slider_SubtitleSize.Value = SettingHelper.Get_SubtitleSize();

            //mediaElement.MediaPlayer.Volume = SettingHelper.Get_Volume();
            SetVolume(SettingHelper.Get_Volume());
            bo = BrightnessOverride.GetForCurrentView();
            if (bo.IsSupported)
            {
                bo.StartOverride();
            }
            bo.IsSupportedChanged += Bo_IsSupportedChanged;
            Brightness = SettingHelper.Get_Light();

            DanmuNum = SettingHelper.Get_DMNumber();
            rb_defu.IsChecked = true;
            btn_ViewPost.Visibility = Visibility.Collapsed;

            //danmu.borderStyle = (NSDanmaku.Model.DanmakuBorderStyle)SettingHelper.Get_DMStyle();
            menu_setting_buttom.IsChecked = !SettingHelper.Get_DMVisBottom();
            menu_setting_top.IsChecked = !SettingHelper.Get_DMVisTop();
            menu_setting_gd.IsChecked = !SettingHelper.Get_DMVisRoll();

            var danmuStatus = SettingHelper.Get_DMStatus();
            if (danmuStatus)
            {
                danmu.Visibility = Visibility.Visible;
                LoadDanmu = true;
            }
            else
            {
                danmu.Visibility = Visibility.Collapsed;
                LoadDanmu = false;
            }
            settingFlag = false;
        }

        private void Bo_IsSupportedChanged(BrightnessOverride sender, object args)
        {
            //if (bo.IsSupported)
            //{
            //    MTC.Brightness = 0;
            //    bo.SetBrightnessLevel(1 - Brightness, DisplayBrightnessOverrideOptions.None);
            //}
            //else
            //{
            MTC.Brightness = Brightness;
            //}
        }

        string DMZZBDS = "";
        bool hidePointerFlag = false;
        int DanmuNum = 0;
        bool mergeDanmu = false;
        List<string> sended = new List<string>();
        private void Timer_Date_Tick(object sender, object e)
        {


            if (_PointerHideTime >= 5 && !hidePointerFlag)
            {
                Window.Current.CoreWindow.PointerCursor = null;
            }
            _PointerHideTime++;

            try
            {
                if (mediaElement.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing && LoadDanmu)
                {
                    if (DanMuPool != null)
                    {
                        int now_num = 0;

                        var pool = DanMuPool.Where(x => Convert.ToInt32(x.time) == Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds));
                        foreach (var item in pool)
                        {
                            if (!DanDis_Dis(item.text))
                            {
                                if (now_num >= DanmuNum && DanmuNum != 0)
                                {
                                    return;
                                }
                                try
                                {
                                    if (DMZZBDS.Length != 0 && Regex.IsMatch(item.source, DMZZBDS))
                                    {
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                if (mergeDanmu)
                                {
                                    if (sended.Contains(item.text + item.location))
                                    {
                                        return;
                                    }
                                    sended.Add(item.text + item.location);
                                }

                                switch (item.location)
                                {
                                    case NSDanmaku.Model.DanmakuLocation.Top:
                                        danmu.AddTopDanmu(item, false);
                                        break;
                                    case NSDanmaku.Model.DanmakuLocation.Bottom:
                                        danmu.AddBottomDanmu(item, false);
                                        break;
                                    case NSDanmaku.Model.DanmakuLocation.Position:
                                        danmu.AddPositionDanmu(item);
                                        break;
                                    default:
                                        danmu.AddRollDanmu(item, false);
                                        break;
                                }

                                now_num++;
                            }



                        }


                    }


                }
            }
            catch (Exception ex)
            {

            }

            sended.Clear();
        }



        //private async void Timer_Tick(object sender, object e)
        //{
        //    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        if (SqlHelper.GetPostIsViewPost(playNow.Mid))
        //        {
        //            SqlHelper.UpdateViewPost(new ViewPostHelperClass() { epId = playNow.Mid, Post = Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds) });
        //        }

        //        //sql.UpdateValue(Cid, Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds));
        //    });
        //}

        #region 弹幕设置
        /// <summary>
        /// 弹幕屏蔽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Dis_Remove_Click(object sender, RoutedEventArgs e)
        {
            foreach (NSDanmaku.Model.DanmakuModel item in list_DisDanmu.SelectedItems)
            {
                DanDis_Add(item.sendID, true);
                danmu.Remove(item);
                list_DisDanmu.Items.Remove(item);
            }
        }
        List<string> Guanjianzi = new List<string>();
        List<string> Yonghu = new List<string>();
        private void DanDis_Get()
        {


            string a = SettingHelper.Get_Guanjianzi();
            string b = SettingHelper.Get_Yonghu();
            if (a.Length != 0)
            {

                Guanjianzi = a.Split('|').ToList();
                Yonghu = b.Split('|').ToList();
                Guanjianzi.Remove(string.Empty);
                Yonghu.Remove(string.Empty);
            }


        }
        private bool DanDis_Dis(string text)
        {
            var a = (from sb in Guanjianzi where text.Contains(sb) select sb).ToList();
            var b = (from sb in Yonghu where text.Contains(sb) select sb).ToList();
            if (b.Count != 0 || a.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void DanDis_Add(string text, bool IsYonghu)
        {
            if (IsYonghu)
            {
                SettingHelper.Set_Yonghu(SettingHelper.Get_Yonghu() + "|" + text);
                Yonghu.Add(text);
            }
            else
            {
                SettingHelper.Set_Guanjianzi(SettingHelper.Get_Guanjianzi() + "|" + text);

                Guanjianzi.Add(text);
            }

        }
        #endregion


        //private async void GetPlayUrl(string cid)
        //{
        //    string url = "http://interface.bilibili.com/playurl?_device=uwp&cid=" + cid + "&otype=xml&quality=" + 2 + "&appkey=" + ApiHelper.AndroidKey.Appkey + "&access_key=" + ApiHelper.access_key + "&type=mp4&mid=" + "" + "&_buvid=" + ApiHelper._buvid + "&_hwid=" + ApiHelper._hwid + "&platform=uwp_desktop" + "&ts=" + ApiHelper.GetTimeSpan;
        //    url += "&sign=" + ApiHelper.GetSign(url);
        //    string re = await WebClientClass.GetResults_Phone(new Uri(url));
        //    re = await WebClientClass.GetResults_Phone(new Uri(url));
        //    string playUrl = Regex.Match(re, "<url>(.*?)</url>").Groups[1].Value;
        //    playUrl = playUrl.Replace("<![CDATA[", "");
        //    playUrl = playUrl.Replace("]]>", "");
        //    mediaElement.Source = new Uri(playUrl);
        //}

        bool QuityLoading = false;
        private async void OpenVideo()
        {
            try
            {

                if (gv_play.Items.Count == 0 || gv_play.Items.Count == 1)
                {

                    MTC.ShowPlayListBtn = playNow.isInteraction;


                    MTC.ShowNextButton = false;
                    MTC.ShowPreviousButton = false;
                }
                else
                {
                    MTC.ShowPlayListBtn = true;
                    MTC.ShowPreviousButton = true;
                    MTC.ShowNextButton = true;
                    if (gv_play.SelectedIndex == 0)
                    {
                        MTC.ShowPreviousButton = false;
                    }
                    if (gv_play.SelectedIndex == gv_play.Items.Count - 1)
                    {
                        MTC.ShowNextButton = false;
                    }
                }

                LastPost = 0;
                ClearSubTitle();
                MTC.ClearLog();
                //txt_Title.Text = playNow.Title + " - " + playNow.No + " " + playNow.VideoTitle;
                MTC.VideoTitle = playNow.Title + " - " + playNow.VideoTitle;
                MTC.ShowSendDanmuBtn = true;
                MTC.ShowDanmakuBtn = Visibility.Visible;
                //btn_Open_CloseDanmu.Visibility = Visibility.Visible;
                cb_Quity.Visibility = Visibility.Visible;
                mediaPlayer_audio = null;
                pr.Text = "正在初始化播放器...";
                AddLog("正在初始化播放器...");
                await LoadQualities();
                txt_fvideo.Text = SettingHelper.Get_ForceVideo().ToString();
                AddLog("强制软解视频：" + txt_fvideo.Text);

                //if (playNow.Mode!= PlayMode.Local&&)
                //{

                    cb_Quity.IsEnabled = true;
                    switch (playNow.Mode)
                    {
                        case PlayMode.Bangumi:
                        case PlayMode.Movie:
                        case PlayMode.VipBangumi:
                            pr.Text = "填充弹幕中...";
                            AddLog("开始填充弹幕...");

                            DanMuPool = await danmakuParse.ParseBiliBili(Convert.ToInt64(playNow.Mid));

                            pr.Text = "开始读取视频...";
                            AddLog(string.Format("开始读取视频{0}-{1}-{2}...", "anime", playNow.banId, playNow.Mid));
                            //string url = await ApiHelper.GetBiliUrl_Ban(playNow, cb_Quity.SelectedIndex + 1);
                            //playNow.Path = url;
                            //mediaElement.Source = new Uri(url);

                            var ban = await PlayurlHelper.GetBangumiUrl(playNow, (cb_Quity.SelectedItem as QualityModel).qn);
                            txt_site.Text = ban.from;
                            if (ban.usePlayMode == UsePlayMode.System)
                            {
                                mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(ban.url));
                                //mediaElement.Source = new Uri(ban.url);
                            }
                            else if (ban.usePlayMode == UsePlayMode.Dash)
                            {
                                mediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource((AdaptiveMediaSource)ban.mediaSource);
                                //mediaPlayer.Source = MediaSource.CreateFromIMediaSource(ban.mediaSource);
                                //mediaElement.SetMediaStreamSource(ban.mediaSource);
                            }
                            else
                            {
                                mediaPlayer.Source = MediaSource.CreateFromUri(await ban.playlist.SaveAndGetFileUriAsync());
                                //mediaElement.Source = await ban.playlist.SaveAndGetFileUriAsync();
                            }
                            AddLog("播放器类型:" + ban.usePlayMode.ToString());
                            break;


                        case PlayMode.Video:

                            pr.Text = "填充弹幕中...";
                            AddLog("开始填充弹幕...");
                            DanMuPool = await danmakuParse.ParseBiliBili(Convert.ToInt64(playNow.Mid));
                            pr.Text = "加载视频中...";
                            AddLog(string.Format("开始读取视频{0}-{1}-{2}...", "video", playNow.Aid, playNow.Mid));
                            var ss = await PlayurlHelper.GetVideoUrl(playNow.Aid, playNow.Mid, (cb_Quity.SelectedItem as QualityModel).qn);
                            txt_site.Text = ss.from;

                            if (ss.usePlayMode == UsePlayMode.System)
                            {
                                mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(ss.url));
                                //mediaElement.Source = new Uri(ss.url);
                            }
                            else if (ss.usePlayMode == UsePlayMode.Dash)
                            {
                                mediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource((AdaptiveMediaSource)ss.mediaSource);
                                //mediaElement.SetMediaStreamSource(ss.mediaSource);
                            }
                            else
                            {
                                mediaPlayer.Source = MediaSource.CreateFromUri(await ss.playlist.SaveAndGetFileUriAsync());
                                //mediaElement.Source = await ss.playlist.SaveAndGetFileUriAsync();
                            }

                            break;
                        case PlayMode.QQ:
                            AddLog("不支持播放的源:腾讯");
                            break;
                        case PlayMode.Sohu:

                            pr.Text = "填充弹幕中...";
                            AddLog("开始填充弹幕...");
                            DanMuPool = await danmakuParse.ParseBiliBili(Convert.ToInt64(playNow.Mid));
                            pr.Text = "加载视频中...";
                            AddLog(string.Format("开始读取视频{0}-{1}...", "sohu", playNow.Mid));
                            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(await PlayurlHelper.GetSoHuPlayInfo(playNow.rich_vid, cb_Quity.SelectedIndex + 1)));
                            //mediaElement.Source = new Uri(await PlayurlHelper.GetSoHuPlayInfo(playNow.rich_vid, cb_Quity.SelectedIndex + 1));
                            txt_site.Text = "sohu";
                            break;
                        case PlayMode.Local:

                            pr.Text = "加载视频中...";

                            MTC.ShowShareBtn = Visibility.Collapsed;
                            MTC.ShowCoinsBtn = Visibility.Collapsed;
                            cb_Quity.Visibility = Visibility.Collapsed;
                            await PlayLocal();
                            txt_site.Text = "本地";
                            break;
                        case PlayMode.FormLocal:
                            pr.Text = "加载视频中...";
                            AddLog("读取本地视频...");
                            MTC.ShowSendDanmuBtn = false;

                            MTC.ShowDanmakuBtn = Visibility.Collapsed;
                            cb_Quity.Visibility = Visibility.Collapsed;
                            txt_site.Text = "本地";
                            PlayFromLocal();
                            break;
                        default:
                            break;
                    }

                    AddLog("读取是否包含字幕");
                    MTC.HideLog();
                    var hasSub = await PlayurlHelper.GetHasSubTitle(playNow.Aid, playNow.Mid);
                    LaodSubTitleMenu(hasSub);

                //}
                //else
                //{
                //    AddLog("读取本地视频...");
                //    MTC.ShowDanmakuBtn = Visibility.Collapsed;
                //    cb_Quity.Visibility = Visibility.Collapsed;
                //    MTC.ShowSendDanmuBtn = false;
                //    StorageFile file = await StorageFile.GetFileFromPathAsync(playNow.Mid);
                //    IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);
                //    // var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                //    mediaPlayer.Source = MediaSource.CreateFromStream(readStream, file.ContentType);
                //    //mediaElement.SetSource(readStream, file.ContentType);

                //}

                AddLog("准备开始播放...");
                MTC.HideLog();
                MTC.timer2.Start();



            }
            catch (Exception ex)
            {
                AddLog("视频播放失败了" + ex.HResult);
                await new MessageDialog("无法读取到播放地址 ＞﹏＜ \r\n请尝试登录、更换清晰度、开通大会员或FQ后再试\r\n如果是地区限制番可能无法第一时间观看，请过段时间重试").ShowAsync();
            }
            finally
            {
                await ReportHistory(0);
            }
        }

        private void LaodSubTitleMenu(HasSubtitleModel hasSub)
        {
            if (hasSub.subtitles != null && hasSub.subtitles.Count != 0)
            {
                AddLog($"该视频包含了{hasSub.subtitles.Count}个字幕文件");
                var menu = new MenuFlyout();


                foreach (var item in hasSub.subtitles)
                {
                    ToggleMenuFlyoutItem menuitem = new ToggleMenuFlyoutItem() { Text = item.lan_doc, Tag = item.subtitle_url };
                    menuitem.Click += Menuitem_Click;
                    menu.Items.Add(menuitem);
                }
                ToggleMenuFlyoutItem noneItem = new ToggleMenuFlyoutItem() { Text = "无" };
                noneItem.Click += Menuitem_Click;
                menu.Items.Add(noneItem);
                (menu.Items[0] as ToggleMenuFlyoutItem).IsChecked = true;
                SetSubTitle((menu.Items[0] as ToggleMenuFlyoutItem).Tag.ToString());
                MTC.CCSelectFlyout = menu;
            }
            else
            {
                AddLog("该视频没有字幕文件");
                var menu = new MenuFlyout();
                menu.Items.Add(new ToggleMenuFlyoutItem() { Text = "无", IsChecked = true });
                MTC.CCSelectFlyout = menu;
            }


        }
        /// <summary>
        /// 字幕文件
        /// </summary>
        SubtitleModel subtitles;
        /// <summary>
        /// 字幕Timer
        /// </summary>
        DispatcherTimer subtitleTimer;
        /// <summary>
        /// 选择字幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menuitem_Click(object sender, RoutedEventArgs e)
        {

            foreach (ToggleMenuFlyoutItem item in (MTC.CCSelectFlyout as MenuFlyout).Items)
            {
                item.IsChecked = false;
            }
            var menuitem = (sender as ToggleMenuFlyoutItem);
            if (menuitem.Text == "无")
            {
                ClearSubTitle();
            }
            else
            {
                SetSubTitle(menuitem.Tag.ToString());
            }
            menuitem.IsChecked = true;
        }
        /// <summary>
        /// 设置字幕文件
        /// </summary>
        /// <param name="url"></param>
        private async void SetSubTitle(string url)
        {
            try
            {
                subtitles = await PlayurlHelper.GetSubtitle(url);
                if (subtitles != null)
                {

                    subtitleTimer = new DispatcherTimer();
                    subtitleTimer.Interval = TimeSpan.FromMilliseconds(100);
                    subtitleTimer.Tick += SubtitleTimer_Tick;
                    subtitleTimer.Start();

                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("加载字幕失败了");
            }


        }

        private void SubtitleTimer_Tick(object sender, object e)
        {
            if (mediaElement.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                var time = mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds;
                var first = subtitles.body.FirstOrDefault(x => x.from <= time && x.to >= time);
                if (first != null)
                {
                    if (first.content != MTC.GetSubtitle())
                    {
                        MTC.ShowSubtitle();
                        MTC.SetSubtitle(first.content);
                    }
                }
                else
                {
                    MTC.HideSubtitle();
                }
            }
        }

        private void ClearSubTitle()
        {
            if (subtitles != null)
            {
                if (subtitleTimer != null)
                {
                    subtitleTimer.Stop();
                    subtitleTimer = null;
                }
                MTC.HideSubtitle();
                subtitles = null;
            }
        }


        private async void ChangeQuality()
        {
            switch (playNow.Mode)
            {
                case PlayMode.Bangumi:
                case PlayMode.Movie:
                case PlayMode.VipBangumi:

                    var ban = await PlayurlHelper.GetBangumiUrl(playNow, (cb_Quity.SelectedItem as QualityModel).qn);
                    if (ban != null)
                    {
                        txt_site.Text = ban.from;
                        if (ban.usePlayMode == UsePlayMode.System)
                        {
                            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(ban.url));
                            //mediaElement.Source = new Uri(ban.url);
                        }
                        else if (ban.usePlayMode == UsePlayMode.Dash)
                        {
                            mediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource((AdaptiveMediaSource)ban.mediaSource);
                            //mediaPlayer.Source = MediaSource.CreateFromIMediaSource(ban.mediaSource);
                            //mediaElement.SetMediaStreamSource(ban.mediaSource);
                        }
                        else
                        {
                            mediaPlayer.Source = MediaSource.CreateFromUri(await ban.playlist.SaveAndGetFileUriAsync());
                            //mediaElement.Source = await ban.playlist.SaveAndGetFileUriAsync();
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast("更换清晰度失败，无法读取播放地址");
                    }

                    break;


                case PlayMode.Video:

                    var ss = await PlayurlHelper.GetVideoUrl(playNow.Aid, playNow.Mid, (cb_Quity.SelectedItem as QualityModel).qn);
                    if (ss != null)
                    {
                        txt_site.Text = ss.from;
                        if (ss.usePlayMode == UsePlayMode.System && !string.IsNullOrEmpty(ss.url))
                        {
                            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(ss.url));
                        }
                        else if (ss.usePlayMode == UsePlayMode.Dash && ss.mediaSource != null)
                        {
                            mediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource((AdaptiveMediaSource)ss.mediaSource);
                            //mediaPlayer.Source = MediaSource.CreateFromIMediaSource(ss.mediaSource);
                        }
                        else if (ss.playlist != null)
                        {
                            mediaPlayer.Source = MediaSource.CreateFromUri(await ss.playlist.SaveAndGetFileUriAsync());
                        }
                        else
                        {
                            Utils.ShowMessageToast("更换清晰度失败，无法读取播放地址");
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast("更换清晰度失败，无法读取播放地址");
                    }

                    break;
                case PlayMode.Sohu:
                    mediaElement.MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(await PlayurlHelper.GetSoHuPlayInfo(playNow.rich_vid, cb_Quity.SelectedIndex + 1)));
                    break;
                default:
                    break;
            }
            MTC.HideLog();
        }

        /// <summary>
        /// 读取清晰度
        /// </summary>
        private async Task LoadQualities()
        {
            AddLog("正在获取视频清晰度");
            QuityLoading = true;
            switch (playNow.Mode)
            {
                case PlayMode.Bangumi:
                case PlayMode.Movie:
                case PlayMode.VipBangumi:
                    cb_Quity.ItemsSource = await PlayurlHelper.GetAnimeQualities(playNow);
                    break;
                case PlayMode.Video:
                    cb_Quity.ItemsSource = await PlayurlHelper.GetVideoQualities(playNow);
                    break;
                case PlayMode.QQ:
                    cb_Quity.ItemsSource = new List<QualityModel>() { new QualityModel() { description = "默认", qn = 64 } };
                    AddLog("不支持的播放源:腾讯");
                    break;
                case PlayMode.Sohu:
                    cb_Quity.ItemsSource = PlayurlHelper.GetDefaultQualities();
                    break;
                default:
                    cb_Quity.ItemsSource = new List<QualityModel>() { new QualityModel() { description = "默认", qn = 64 } };
                    break;
            }

            var settingq = SettingHelper.Get_NewQuality();
            var ls = (cb_Quity.ItemsSource as List<QualityModel>).Where(x => x.qn == settingq).ToList();
            if (ls != null && ls.Count != 0)
            {
                cb_Quity.SelectedItem = ls[0];
            }
            else
            {
                cb_Quity.SelectedIndex = cb_Quity.Items.Count - 1;
            }
            QuityLoading = false;

        }

        private void AddLog(string msg)
        {
            MTC.AddLog(msg);
            //txt_log.Text += string.Format("[{0}]{1}\r\n",DateTime.Now.ToString("HH:mm:ss"),msg);
        }


        private async void PlayFromLocal()
        {
            var item = playNow.Parameter as StorageFile;
            //if (item .FileType== ".mp4")
            //{
            IRandomAccessStream readStream = await item.OpenAsync(FileAccessMode.Read);
            // var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            mediaElement.MediaPlayer.Source = MediaSource.CreateFromStream(readStream, item.ContentType);
            //mediaElement.SetSource(readStream, item.ContentType);
            //if (SettingHelper.Get_FFmpeg())
            //{

            //    mediaElement_MediaFailed(null, null);
            //}
            //}

        }
        private async Task PlayLocal()
        {
            AddLog("开始读取本地视频...");
            StorageFolder f = await StorageFolder.GetFolderFromPathAsync(playNow.Path);
            var ls = await f.GetFilesAsync();
            var danmakuFile = ls.FirstOrDefault(x => x.FileType == ".xml");
            if (danmakuFile!=null)
            {
                pr.Text = "填充弹幕中...";
                AddLog("填充弹幕中...");
                DanMuPool = await danmakuParse.ParseBiliBili(danmakuFile);
            }
            var video = ls.FirstOrDefault(x => x.Name == "video.m4s");
            if (video!=null)
            {
               
                var audio = ls.FirstOrDefault(x => x.Name == "audio.m4s");
                if (mediaPlayer_audio ==null)
                {
                    mediaPlayer_audio = new MediaPlayer();
                    mediaPlayer_audio.Volume = mediaPlayer.Volume;
                    mediaPlayer_audio.Source = MediaSource.CreateFromStream(await audio.OpenReadAsync(), audio.ContentType);
                }
                mediaElement.MediaPlayer.Source = MediaSource.CreateFromStream(await video.OpenReadAsync(), video.ContentType);
            
            }
            else
            {
                var paths = ls.Where(x => x.FileType == ".mp4" || x.FileType == ".flv").Select(x => x.Path).ToList();
                if (paths.Count == 1)
                {
                    var file = await StorageFile.GetFileFromPathAsync(paths[0]);
                    mediaElement.MediaPlayer.Source = MediaSource.CreateFromStream(await file.OpenReadAsync(), file.ContentType);
                }
                else
                {
                    var s = await PlayLocalVideo(paths);
                    mediaElement.MediaPlayer.Source = MediaSource.CreateFromMediaStreamSource(s);
                }
            }
            MTC.HideLog();
            //playNow.Path
        }

        private async Task<MediaStreamSource> PlayLocalVideo(List<string> paths)
        {
            var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.LocalFile);

            MediaComposition composition = new MediaComposition();
            foreach (var item in paths)
            {
                playList.Append(item, 0, 0);
                var file = await StorageFile.GetFileFromPathAsync(item);
                var clip = await MediaClip.CreateFromFileAsync(file);
                composition.Clips.Add(clip);
            }
            return composition.GenerateMediaStreamSource();
        }




        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            //if (this.Frame.CanGoBack)
            //{
            //    mediaElement.Stop();
            //    this.Frame.GoBack();
            //}
        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.MediaPlayer.Play();

        }

        private void btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.MediaPlayer.PlaybackSession.CanPause)
            {
                mediaElement.MediaPlayer.Pause();
            }
        }




        #region 手势操作
        double ssValue = 0;
        bool ManipulatingBrightness = false;

        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            //progress.Visibility = Visibility.Visible;
            if (e.Delta.Translation.Y == 0)
            {
                if (MTC.Video360)
                {
                    mediaPlayer.PlaybackSession.SphericalVideoProjection.ViewOrientation *= Quaternion.CreateFromYawPitchRoll(e.Delta.Translation.X>0? -.01f: .01f, 0, 0);
                }
                else
                {
                    HandleSlideProgressDelta(e.Delta.Translation.X);
                }
                
            }
            else
            {
                if (MTC.Video360)
                {
                    mediaPlayer.PlaybackSession.SphericalVideoProjection.ViewOrientation *= Quaternion.CreateFromYawPitchRoll(0, 0, e.Delta.Translation.Y > 0 ? -.01f : .01f);
                }
                else
                {
                    if (ManipulatingBrightness)
                        HandleSlideBrightnessDelta(e.Delta.Translation.Y);
                    else
                        HandleSlideVolumeDelta(e.Delta.Translation.Y);
                }
            }
        }

        private void HandleSlideProgressDelta(double delta)
        {
            if (mediaElement.MediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                return;

            if (delta > 0)
            {
                double dd = delta / this.ActualWidth;
                double d = dd * 90;
                ssValue += d;
                //slider.Value += d;
            }
            else
            {
                double dd = Math.Abs(delta) / this.ActualWidth;
                double d = dd * 90;
                ssValue -= d;
                //slider.Value -= d;
            }
            TimeSpan ts = mediaElement.MediaPlayer.PlaybackSession.Position;
            ts = ts.Add(TimeSpan.FromSeconds(ssValue));

            if (ts < TimeSpan.Zero)
                ts = TimeSpan.Zero;
            else if (ts > mediaElement.MediaPlayer.PlaybackSession.NaturalDuration)
                ts = mediaElement.MediaPlayer.PlaybackSession.NaturalDuration;
            //txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Seconds.ToString("00");

            txt_SSPosition.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            //Utils.ShowMessageToast(ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00"), 3000);
        }

        private void HandleSlideVolumeDelta(double delta)
        {
            if (delta > 0)
            {
                double dd = delta / (this.ActualHeight * 0.8);

                //slider_V.Value -= d;
                var volume = mediaElement.MediaPlayer.Volume-dd;
                SetVolume(volume);

            }
            else
            {
                double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
                var volume = mediaElement.MediaPlayer.Volume+dd;
                SetVolume(volume);
                //slider_V.Value += d;
            }
            txt_SSPosition.Text = "音量:" + mediaElement.MediaPlayer.Volume.ToString("P");
            //Utils.ShowMessageToast("音量:" +  mediaElement.MediaPlayer.Volume.ToString("P"), 3000);
        }

        private void HandleSlideBrightnessDelta(double delta)
        {
            double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
            if (delta > 0)
            {
                Brightness = Math.Min(Brightness + dd, 1);
            }
            else
            {
                Brightness = Math.Max(Brightness - dd, 0);
            }
            txt_SSPosition.Text = "亮度:" + Math.Abs(Brightness - 1).ToString("P");
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;

            if (ssValue != 0)
            {
                mediaElement.MediaPlayer.PlaybackSession.Position = mediaElement.MediaPlayer.PlaybackSession.Position.Add(TimeSpan.FromSeconds(ssValue));
            }
            txt_SSPosition.Visibility = Visibility.Collapsed;
        }

        private void MTC_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
            ssValue = 0;
            txt_SSPosition.Text = "";
            txt_SSPosition.Visibility = Visibility.Visible;

            if (e.Position.X < this.ActualWidth / 2)
                ManipulatingBrightness = true;
            else
                ManipulatingBrightness = false;
        }
        #endregion

        private void gv_play_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gv_play.SelectedIndex != -1)
            {
                playNow = gv_play.SelectedItem as PlayerModel;

                cb_Quity.ItemsSource = null;

                // PlayerEvent(gv_play.SelectedIndex);
                OpenVideo();

            }
        }

        private void btn_Select_Click(object sender, RoutedEventArgs e)
        {

            gv_play.Visibility = Visibility.Visible;
            gv_story_list.Visibility = Visibility.Collapsed;
            grid_Setting.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;
            grid_Subtitle.Visibility = Visibility.Collapsed;
            sp_View.IsPaneOpen = true;
        }



        private void cb_Quity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!QuityLoading && cb_Quity.SelectedItem != null)
            {
                UpdateLocalHistory(Convert.ToInt32(mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds));
                SettingHelper.Set_NewQuality((cb_Quity.SelectedItem as QualityModel).qn);
                mediaElement.MediaPlayer.Source = null;
                ChangeQuality();
            }
            //if (gv_play.ItemsSource == null)
            //{
            //    return;
            //}
            //mediaElement.Stop();
            //SettingHelper.Set_PlayQualit(cb_Quity.SelectedIndex + 1);
            //OpenVideo();
        }
        private void _lastpost_out_Completed(object sender, object e)
        {
            btn_ViewPost.Visibility = Visibility.Collapsed;
        }

        private void btn_ViewPost_Click(object sender, RoutedEventArgs e)
        {
            if (LastPost != 0)
            {
                mediaElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, Convert.ToInt32(LastPost));
                btn_ViewPost.Visibility = Visibility.Collapsed;

            }
        }

        private void btn_VideoInfo_Click(object sender, RoutedEventArgs e)
        {

            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Visible;
            gv_play.Visibility = Visibility.Collapsed;
            gv_story_list.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;
            grid_Subtitle.Visibility = Visibility.Collapsed;
            //string info = string.Format("视频高度：{0}\r\n视频宽度：{1}\r\n视频长度：{2}\r\n缓冲进度:{3}", mediaElement.NaturalVideoHeight, mediaElement.NaturalVideoWidth, mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Seconds.ToString("00"), mediaElement.DownloadProgress.ToString("P"));
            //await new MessageDialog(info, "视频信息").ShowAsync();
        }

        private void cb_setting_defu_Checked(object sender, RoutedEventArgs e)
        {
            if (mediaElement == null)
            {
                return;
            }
            mediaElement.Width = this.ActualWidth;
            mediaElement.Height = this.ActualHeight;
            mediaElement.Stretch = Stretch.Uniform;
        }

        private void cb_setting_43_Checked(object sender, RoutedEventArgs e)
        {
            if (mediaElement == null)
            {
                return;
            }
            mediaElement.Stretch = Stretch.Fill;
            mediaElement.Height = this.ActualHeight;
            mediaElement.Width = this.ActualHeight * 4 / 3;
        }

        private void cb_setting_169_Checked(object sender, RoutedEventArgs e)
        {
            if (mediaElement == null)
            {
                return;
            }
            mediaElement.Stretch = Stretch.Fill;
            mediaElement.Height = this.ActualHeight;
            mediaElement.Width = this.ActualHeight * 16 / 9;


        }

        //private void btn_HideInfo_Click(object sender, RoutedEventArgs e)
        //{
        //    //ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        //    //MaxWIndowsEvent(true);
        //    btn_HideInfo.Visibility = Visibility.Collapsed;
        //    btn_ShowInfo.Visibility = Visibility.Visible;
        //    danmu.SetJJ();
        //}

        //private void btn_ShowInfo_Click(object sender, RoutedEventArgs e)
        //{
        //    //MaxWIndowsEvent(false);
        //    //ApplicationView.GetForCurrentView().ExitFullScreenMode();
        //    btn_HideInfo.Visibility = Visibility.Visible;
        //    btn_ShowInfo.Visibility = Visibility.Collapsed;
        //    danmu.SetJJ();
        //}





        /// <summary>
        /// 设置系统播放控制器
        /// </summary>
        private void SetSystemMediaTransportControl()
        {
            try
            {
                if (_systemMediaTransportControls == null)
                {
                    return;
                }
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Video;
                updater.VideoProperties.Subtitle = playNow.VideoTitle;
                updater.VideoProperties.Title = playNow.Title;

                updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Logo.png"));

                updater.Update();
                var timelineProperties = new SystemMediaTransportControlsTimelineProperties();

                timelineProperties.StartTime = TimeSpan.FromSeconds(0);
                timelineProperties.MinSeekTime = TimeSpan.FromSeconds(0);
                timelineProperties.Position = mediaElement.MediaPlayer.PlaybackSession.Position;
                timelineProperties.MaxSeekTime = mediaElement.MediaPlayer.PlaybackSession.NaturalDuration;
                timelineProperties.EndTime = mediaElement.MediaPlayer.PlaybackSession.NaturalDuration;

                _systemMediaTransportControls.UpdateTimelineProperties(timelineProperties);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("设置系统播放控件失败", LogType.ERROR, ex);
            }
        }

        private void menuitem_DM_Click(object sender, RoutedEventArgs e)
        {

            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            gv_story_list.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Visible;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_Subtitle.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;

        }

        private void menuitem_PB_Click(object sender, RoutedEventArgs e)
        {

            mediaElement.MediaPlayer.Pause();
            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            gv_story_list.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_Subtitle.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Visible;
            list_DisDanmu.Items.Clear();
            foreach (var item in danmu.GetDanmakus())
            {
                list_DisDanmu.Items.Add(item);
            }
        }

        private void menuitem_Info_Click(object sender, RoutedEventArgs e)
        {

            sp_View.IsPaneOpen = true;
            if (playNow != null && DanMuPool != null)
            {
                txt_Num.Text = DanMuPool.Count.ToString();
                txt_sId.Text = playNow.Aid;
                txt_eId.Text = playNow.Mid;
            }
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            gv_story_list.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Visible;
            grid_PB.Visibility = Visibility.Collapsed;
        }

        #region 设置
        private void sw_DanmuBorder_Toggled(object sender, RoutedEventArgs e)
        {
            //danmu.D_Border = sw_DanmuBorder.IsOn;
            SettingHelper.Set_DMBorder(sw_DanmuBorder.IsOn);
        }

        private void slider_DanmuSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (danmu == null)
            {
                return;
            }
            danmu.sizeZoom = slider_DanmuSize.Value;

            SettingHelper.Set_NewDMSize(slider_DanmuSize.Value);
        }

        private void cb_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //switch (cb_Font.SelectedIndex)
            //{
            //    case 0:
            //        danmu.fontFamily = "默认";
            //        break;
            //    case 1:
            //        danmu.fontFamily = "微软雅黑";
            //        break;
            //    case 2:
            //        danmu.fontFamily = "黑体";
            //        break;
            //    case 3:
            //        danmu.fontFamily = "楷体";
            //        break;
            //    case 4:
            //        danmu.fontFamily = "宋体";
            //        break;
            //    case 5:
            //        danmu.fontFamily = "等线";
            //        break;
            //    default:
            //        break;
            //}
            if (cb_Font.SelectedItem == null)
            {
                return;
            }
            SettingHelper.Set_DanmuFont(cb_Font.SelectedItem.ToString());
            danmu.font = cb_Font.SelectedItem.ToString();
        }

        private void slider_DanmuSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (danmu == null)
            {
                return;
            }
            danmu.speed = Convert.ToInt32(slider_DanmuSpeed.Value);
            if (slider_DanmuSpeed.Value == 0 || slider_DanmuSpeed.Value == -1)
            {
                return;
            }
            SettingHelper.Set_DMSpeed(slider_DanmuSpeed.Value);
        }

        private void slider_DanmuTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (danmu == null)
            {
                return;
            }
            if (slider_DanmuTran.Value == 0 || slider_DanmuTran.Value == -1)
            {
                return;
            }
            danmu.Opacity = slider_DanmuTran.Value;
            SettingHelper.Set_NewDMTran(slider_DanmuTran.Value);
        }
        private void slider_Num_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (danmu == null)
            {
                return;
            }
            DanmuNum = Convert.ToInt32(slider_Num.Value);
            SettingHelper.Set_DMNumber(Convert.ToInt32(slider_Num.Value));
        }




        private void menu_setting_top_Click(object sender, RoutedEventArgs e)
        {

            danmu.HideDanmaku(NSDanmaku.Model.DanmakuLocation.Top);
            SettingHelper.Set_DMVisTop(false);


        }

        private void menu_setting_buttom_Click(object sender, RoutedEventArgs e)
        {
            danmu.HideDanmaku(NSDanmaku.Model.DanmakuLocation.Bottom);
            SettingHelper.Set_DMVisBottom(false);
        }

        private void menu_setting_gd_Checked(object sender, RoutedEventArgs e)
        {
            danmu.HideDanmaku(NSDanmaku.Model.DanmakuLocation.Roll);
            SettingHelper.Set_DMVisRoll(false);
        }

        private void menu_setting_gd_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.ShowDanmaku(NSDanmaku.Model.DanmakuLocation.Roll);
            SettingHelper.Set_DMVisRoll(true);
        }

        private void menu_setting_top_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.ShowDanmaku(NSDanmaku.Model.DanmakuLocation.Top);
            SettingHelper.Set_DMVisTop(true);
        }

        private void menu_setting_buttom_Unchecked(object sender, RoutedEventArgs e)
        {
            // danmu.SetDanmuVisibility(true, MyDanmaku.DanmuMode.Buttom);
            danmu.ShowDanmaku(NSDanmaku.Model.DanmakuLocation.Bottom);
            SettingHelper.Set_DMVisBottom(true);
        }


        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {

            DanDis_Add(txt_Dis.Text, false);
            txt_Dis.Text = "";
            var s = danmu.GetDanmakus();
            foreach (var item in s)
            {
                if (DanDis_Dis(item.text))
                {
                    danmu.Remove(item);
                }
            }
        }






        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {

            return base.MeasureOverride(availableSize);
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }





        private void btn_Dis_Report_Click(object sender, RoutedEventArgs e)
        {
            if (list_DisDanmu.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (NSDanmaku.Model.DanmakuModel item in list_DisDanmu.SelectedItems)
            {
                ReportDM(item.rowID);
            }
        }

        private async void ReportDM(string dmid)
        {
            try
            {
                string results = await WebClientClass.PostResults(new Uri("http://interface.bilibili.com/dmreport"), string.Format("reportToAdmin=0&reason=&dm_inid={0}&dmid={1}", playNow.Mid, dmid), "http://www.bilibili.com");
                if (results == "0")
                {
                    Utils.ShowMessageToast("举报成功", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("举报失败", 3000);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("举报错误", 3000);
            }
        }

        private async void menuitem_UpdateDanmu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DanMuPool = await danmakuParse.ParseBiliBili(Convert.ToInt64(playNow.Mid));
                Utils.ShowMessageToast("已经更新弹幕池", 3000);
            }
            catch (Exception)
            {
            }

        }


        private void sw_MergeDanmu_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_MergeDanmu(sw_MergeDanmu.IsOn);
            mergeDanmu = sw_MergeDanmu.IsOn;
        }

        private void MTC_OpenDanmaku(object sender, bool e)
        {
            LoadDanmu = e;
        }

        private void MTC_ExitPlayer(object sender, EventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void MTC_OnMiniWindows(object sender, EventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                danmu.ClearAll();
                danmu.SetSpeed(5);
                danmu.sizeZoom = 0.5;
            }
        }

        private async void MTC_OnExitMiniWindows(object sender, EventArgs e)
        {
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            danmu.ClearAll();
            danmu.speed = SettingHelper.Get_DMSpeed().ToInt32();
            danmu.sizeZoom = SettingHelper.Get_NewDMSize();
        }

        private void MTC_DanmakuSetting(object sender, EventArgs e)
        {
            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            gv_story_list.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Visible;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_Subtitle.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;
        }

        private void MTC_SelectList(object sender, EventArgs e)
        {
            if (playNow.isInteraction)
            {
                gv_story_list.Visibility = Visibility.Visible;
                gv_play.Visibility = Visibility.Collapsed;

            }
            else
            {
                gv_play.Visibility = Visibility.Visible;
                gv_story_list.Visibility = Visibility.Collapsed;
            }

            grid_Setting.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_Subtitle.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;

            sp_View.IsPaneOpen = true;
        }

        private void MTC_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!SettingHelper.IsPc() || SettingHelper.IsTabletMode())
            {
                if (mediaElement.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaElement.MediaPlayer.Pause();
                }
                else
                {
                    mediaElement.MediaPlayer.Play();
                }
            }
            else
            {
                if (MTC.IsFullWindow)
                {
                    MTC.ExitFull();
                    //mediaElement.IsFullWindow = false;
                }
                else
                {
                    MTC.ToFull();
                    //mediaElement.IsFullWindow = true;
                }
            }
        }

        private void MTC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SettingHelper.IsPc() && e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                if (mediaElement.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaElement.MediaPlayer.Pause();
                }
                else
                {
                    mediaElement.MediaPlayer.Play();
                }
            }
        }

        private void MTC_Next(object sender, EventArgs e)
        {
            gv_play.SelectedIndex += 1;
        }

        private void MTC_Previous(object sender, EventArgs e)
        {
            gv_play.SelectedIndex -= 1;
        }

        private async void MTC_SendDanmakued(object sender, EventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录!", 3000);
            }
            CoreWindow.GetForCurrentThread().KeyDown -= PlayerPage_KeyDown;
            hidePointerFlag = true;
            mediaElement.MediaPlayer.Pause();
            SendDanmakuDialog dialog = new SendDanmakuDialog(playNow.Aid, playNow.Mid, mediaElement.MediaPlayer.PlaybackSession.Position.TotalSeconds);
            dialog.DanmakuSended += new EventHandler<SendDanmakuModel>((obj, item) =>
            {

                if (item.location == 1)
                {
                    danmu.AddRollDanmu(new NSDanmaku.Model.DanmakuModel { text = item.text, color = item.color.ToColor(), size = 25 }, true);
                }
                if (item.location == 4)
                {
                    danmu.AddBottomDanmu(new NSDanmaku.Model.DanmakuModel { text = item.text, color = item.color.ToColor(), size = 25 }, true);
                }
                if (item.location == 5)
                {
                    danmu.AddTopDanmu(new NSDanmaku.Model.DanmakuModel { text = item.text, color = item.color.ToColor(), size = 25 }, true);
                }
                mediaElement.MediaPlayer.Play();
            });
            await dialog.ShowAsync();
            CoreWindow.GetForCurrentThread().KeyDown += PlayerPage_KeyDown;
            hidePointerFlag = false;
            mediaElement.MediaPlayer.Play();
        }

        private void MTC_ShareEvent(object sender, EventArgs e)
        {
            Utils.SetClipboard(string.Format("http://www.bilibili.com/video/av{0}", playNow.Aid));
            Utils.ShowMessageToast("已将内容复制到剪切板", 3000);
        }

        private async void MTC_CoinsEvent(object sender, EventArgs e)
        {
            if (SettingHelper.IsPc())
            {
                MessageDialog messageDialog = new MessageDialog("确定要投币吗?", "投币");
                messageDialog.Commands.Add(new UICommand("投币X1", (com) => { TouBi(1); }, "1"));
                messageDialog.Commands.Add(new UICommand("投币X2", (com) => { TouBi(2); }, "2"));
                messageDialog.Commands.Add(new UICommand("取消", (com) => { }, "0"));
                await messageDialog.ShowAsync();
            }
            else
            {
                MenuFlyout menuFlyout = new MenuFlyout();
                var menu1 = new MenuFlyoutItem() { Text = "投币X1" };
                menu1.Click += new RoutedEventHandler((x, y) => { TouBi(1); });
                var menu2 = new MenuFlyoutItem() { Text = "投币X2" };
                menu2.Click += new RoutedEventHandler((x, y) => { TouBi(2); });
                menuFlyout.Items.Add(menu1);
                menuFlyout.Items.Add(menu2);

                menuFlyout.ShowAt(sender as AppBarButton);
            }

        }
        public async void TouBi(int num)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录!", 3000);
            }

            try
            {
                WebClientClass wc = new WebClientClass();
                Uri ReUri = new Uri("https://app.bilibili.com/x/v2/view/coin/add");
                string QuStr = string.Format("access_key={0}&aid={1}&appkey={2}&build=540000&from=7&mid={3}&platform=android&&multiply={4}&ts={5}", ApiHelper.access_key, playNow.Aid, ApiHelper.AndroidKey.Appkey, ApiHelper.GetUserId(), num, ApiHelper.GetTimeSpan);
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

        private void cb_Style_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Style.SelectedIndex == -1 || danmu == null)
            {
                return;
            }
            danmu.borderStyle = (NSDanmaku.Model.DanmakuBorderStyle)cb_Style.SelectedIndex;
            SettingHelper.Set_DMStyle(cb_Style.SelectedIndex);

        }

        private void sw_DanmuNotSubtitle_Toggled(object sender, RoutedEventArgs e)
        {
            if (danmu == null)
            {
                return;
            }
            danmu.notHideSubtitle = sw_DanmuNotSubtitle.IsOn;
            SettingHelper.Set_DanmuNotSubtitle(sw_DanmuNotSubtitle.IsOn);

        }

        private void slider_Rate_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mediaElement == null)
            {
                return;
            }
            mediaElement.MediaPlayer.PlaybackSession.PlaybackRate = slider_Rate.Value;
        }

        private void MTC_FullWindows(object sender, EventArgs e)
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private void MTC_ExitFullWindows(object sender, EventArgs e)
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
        }

        private void menuitem_Capture_Click(object sender, RoutedEventArgs e)
        {
            CaptureVideo();
        }
        private async void CaptureVideo()
        {
            try
            {
                MTC.Visibility = Visibility.Collapsed;
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                StorageFolder applicationFolder = KnownFolders.PicturesLibrary;
                StorageFolder folder = await applicationFolder.CreateFolderAsync("bilibili UWP", CreationCollisionOption.OpenIfExists);
                StorageFile saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                RenderTargetBitmap bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(mediaElement);
                var pixelBuffer = await bitmap.GetPixelsAsync();
                using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                         (uint)bitmap.PixelWidth,
                         (uint)bitmap.PixelHeight,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                }
                Utils.ShowMessageToast("截图已经保存至图片库");
                MTC.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("截图失败");
            }

        }

        private void MTC_Captured(object sender, EventArgs e)
        {
            CaptureVideo();
        }

        private void MTC_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            MTC.HideOrShowMTC();
        }

        private void MTC_FastForward(object sender, double e)
        {
            mediaElement.MediaPlayer.PlaybackSession.Position.Add(TimeSpan.FromSeconds(e));
        }




        private void sw_BoldDanmu_Toggled(object sender, RoutedEventArgs e)
        {
            if (danmu == null)
            {
                return;
            }
            danmu.bold = sw_BoldDanmu.IsOn;
            SettingHelper.Set_BoldDanmu(sw_BoldDanmu.IsOn);
        }

        private async void menuitem_LocalDanmu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
                fileOpenPicker.FileTypeFilter.Add(".xml");
                var file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    var ls = await danmakuParse.ParseBiliBili(file);
                    DanMuPool.AddRange(ls);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载失败");
            }


        }

        private async void menuitem_tantan_Click(object sender, RoutedEventArgs e)
        {
            NSDanmaku.Controls.TantanDialog tantanDialog = new NSDanmaku.Controls.TantanDialog();
            tantanDialog.ReturnDanmakus += TantanDialog_ReturnDanmakus;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            hidePointerFlag = true;
            await tantanDialog.ShowAsync();
            hidePointerFlag = false;
        }

        private void TantanDialog_ReturnDanmakus(object sender, List<NSDanmaku.Model.DanmakuModel> e)
        {
            DanMuPool.AddRange(e);
        }

        private void Gridview_node_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((ItemsWrapGrid)gridview_node.ItemsPanelRoot).ItemWidth = (e.NewSize.Width) / 2;
        }

        private void Gridview_node_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickItem = e.ClickedItem as Choices;
            ChangeNode(clickItem.node_id, clickItem.cid.ToString());
        }
        public async void ChangeNode(int node_id, string cid)
        {
            var data = await interactionVideo.GetNodes(node_id);
            if (data == null)
            {
                Utils.ShowMessageToast("加载分支失败，请重试");
                return;
            }
            nodeInfo = data;
            gridview_node.ItemsSource = nodeInfo.edges?.choices;
            gv_story_list.ItemsSource = nodeInfo.story_list;
            settingStorylist = true;
            gv_story_list.SelectedItem = nodeInfo.story_list.FirstOrDefault(x => x.node_id == data.node_id);
            settingStorylist = false;
            playNow.Mid = cid;
            playNow.node_id = node_id;
            playNow.VideoTitle = data.title;
            gridview_node.Visibility = Visibility.Collapsed;
            DanMuPool = await danmakuParse.ParseBiliBili(Convert.ToInt64(playNow.Mid));
            danmu.ClearAll();
            ChangeQuality();
        }



        bool settingStorylist = false;
        private void Gv_story_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gv_story_list.SelectedItem == null || settingStorylist)
            {
                return;
            }

            var clickItem = gv_story_list.SelectedItem as StoryList;
            ChangeNode(clickItem.node_id, clickItem.cid.ToString());

        }

        private void Sw_DASHUseHEVC_Toggled(object sender, RoutedEventArgs e)
        {
            if (settingFlag)
            {
                return;
            }
            //if (sw_DASHUseHEVC.IsOn && !await SystemHelper.CheckCodec())
            //{
            //    sw_DASHUseHEVC.IsOn = false;
            //    Utils.ShowMessageToast("请先安装HEVC扩展");
            //}
            //else
            //{
            SettingHelper.Set_DASHUseHEVC(sw_DASHUseHEVC.IsOn);
            Utils.ShowMessageToast("更改清晰度或重新加载生效");
            //}

        }

        private void Sw_UseDASH_Toggled(object sender, RoutedEventArgs e)
        {
            if (settingFlag)
            {
                return;
            }
            if (sw_UseDASH.IsOn && SystemHelper.GetSystemBuild() < 17763)
            {
                Utils.ShowMessageToast("系统版本1809以上才可以开启");
                sw_UseDASH.IsOn = false;
                return;
            }
            SettingHelper.Set_UseDASH(sw_UseDASH.IsOn);
            Utils.ShowMessageToast("更改清晰度或重新加载生效");
        }


        private void Sp_View_PaneClosed(SplitView sender, object args)
        {

        }

        private void Cb_SubtitleFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_SubtitleFont.SelectedItem == null)
            {
                return;
            }
            SettingHelper.Set_SubtitleFontFamily(cb_SubtitleFont.SelectedItem.ToString());
            MTC.SubTitleFontFamily = new FontFamily(cb_SubtitleFont.SelectedItem.ToString());
        }

        private void Cb_SubtitleColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_SubtitleColor.SelectedItem == null)
            {
                return;
            }
            MTC.SubTitleColor = new SolidColorBrush(Utils.ToColor2((cb_SubtitleColor.SelectedItem as ComboBoxItem).Tag.ToString()));
            SettingHelper.Set_SubtitleColor((cb_SubtitleColor.SelectedItem as ComboBoxItem).Tag.ToString());
        }

        private void Slider_SubtitleSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MTC == null)
            {
                return;
            }
            MTC.SubTitleFontSize = e.NewValue;
            SettingHelper.Set_SubtitleSize(e.NewValue);
        }

        private void Menuitem_SubtitleSetting_Click(object sender, RoutedEventArgs e)
        {
            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            gv_story_list.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_Subtitle.Visibility = Visibility.Visible;
            grid_PB.Visibility = Visibility.Collapsed;
        }

        private void Slider_SubtitleTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MTC == null)
            {
                return;
            }
            MTC.SubTitleBackground = new SolidColorBrush(Color.FromArgb(Convert.ToByte(e.NewValue * 255), 0, 0, 0));
            SettingHelper.Set_SubtitleBgTran(e.NewValue);
        }

        #region 播放器事件
        private async void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                SetSystemMediaTransportControl();

                var record = SqlHelper.GetVideoWatchRecord(string.IsNullOrEmpty(playNow.episode_id) ? playNow.Mid : "ep" + playNow.episode_id);
                if (record != null && record.Post != 0)
                {
                    if (SettingHelper.Get_SkipToHistory())
                    {
                        mediaElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, record.Post);
                    }
                    else
                    {
                        TimeSpan ts = new TimeSpan(0, 0, record.Post);
                        LastPost = record.Post;
                        btn_ViewPost.Content = "上次播放到" + ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                        btn_ViewPost.Visibility = Visibility.Visible;
                        await Task.Delay(5000);
                        btn_ViewPost.Visibility = Visibility.Collapsed;
                    }
                }


            }
            catch (Exception)
            {

            }

        }
        private async void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cb_setting_1.IsChecked.Value)
                {
                    mediaElement.MediaPlayer.Play();
                    danmu.ClearAll();
                    return;
                }
                if (gv_play.SelectedIndex == gv_play.Items.Count - 1)
                {
                    if (playNow.isInteraction)
                    {
                        if (nodeInfo.edges != null)
                        {
                            if (nodeInfo.edges.choices.Count == 1)
                            {
                                ChangeNode(nodeInfo.edges.choices[0].node_id, nodeInfo.edges.choices[0].cid.ToString());
                            }
                            else
                            {
                                gridview_node.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            Utils.ShowMessageToast("互动视频已结束，可点击右下角选择节点重新开始", 3000);
                        }

                    }
                    else
                    {
                        if (cb_setting_2.IsChecked.Value)
                        {
                            gv_play.SelectedIndex = 0;
                        }
                        else
                        {
                            Utils.ShowMessageToast("全部看完了", 3000);
                        }
                    }
                }
                else
                {
                    //mediaElement.MediaPlayer.PlaybackSession.();
                    Utils.ShowMessageToast("3秒后播放下一集", 3000);
                    await Task.Delay(3000);
                    if(this.Frame.Visibility == Visibility.Visible)
                        gv_play.SelectedIndex += 1;
                }
            }
            catch (Exception)
            {
            }

        }
        private async void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //if (e.ErrorMessage.Contains("SRC_NOT_SUPPORT"))
            //{
            //    await new MessageDialog("暂时无法播放此视频，请稍后再试").ShowAsync();
            //}
            //else
            //{
            //    await new MessageDialog("无法播放此视频" + e.ErrorMessage).ShowAsync();

            //}
            await new MessageDialog("无法播放此视频 ＞﹏＜ \r\n请尝试更换清晰度或者在播放设置中打开/关闭DASH").ShowAsync();
        }

        private void mediaElement_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            pr.Text = mediaElement.MediaPlayer.PlaybackSession.BufferingProgress.ToString("P");
        }
        bool buffering = false;
        private void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            buffering = false;
            switch (mediaElement.MediaPlayer.PlaybackSession.PlaybackState)
            {
                //case  MediaPlaybackState.Closed:
                //    if (_systemMediaTransportControls != null)
                //    {
                //        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                //    }


                //    break;
                case MediaPlaybackState.Opening:

                    progress.Visibility = Visibility.Visible;

                    break;
                case MediaPlaybackState.Buffering:
                    buffering = true;
                    progress.Visibility = Visibility.Visible;
                    danmu.PauseDanmaku();
                    break;
                case MediaPlaybackState.Playing:
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    }

                    progress.Visibility = Visibility.Collapsed;
                    danmu.ResumeDanmaku();

                    if (timer != null)
                    {
                        timer.Start();
                    }
                    mediaElement.MediaPlayer.PlaybackSession.PlaybackRate = slider_Rate.Value;

                    break;
                case MediaPlaybackState.Paused:
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    }

                    progress.Visibility = Visibility.Collapsed;
                    danmu.PauseDanmaku();
                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    break;
                //case MediaPlaybackState.Stopped:
                //    if (_systemMediaTransportControls != null)
                //    {
                //        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                //    }

                //    progress.Visibility = Visibility.Collapsed;
                //    danmu.ClearAll();
                //    if (timer != null)
                //    {
                //        timer.Stop();
                //    }

                //    break;
                default:
                    break;
            }
        }
        private void mediaElement_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            _PointerHideTime = 1;
        }
        private void mediaElement_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            _PointerHideTime = 1;
        }
        int _PointerHideTime = 1;
        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            _PointerHideTime = 1;
        }

        #endregion

        private void MTC_Video360Changed(object sender, bool e)
        {
            if (mediaPlayer.PlaybackSession.SphericalVideoProjection.FrameFormat== SphericalVideoFrameFormat.None)
            {
                mediaPlayer.PlaybackSession.SphericalVideoProjection.FrameFormat = SphericalVideoFrameFormat.Equirectangular;
            }
            mediaPlayer.PlaybackSession.SphericalVideoProjection.IsEnabled = e;
            if (e)
            {
                mediaPlayer.PlaybackSession.SphericalVideoProjection.ProjectionMode = SphericalVideoProjectionMode.Spherical;
                mediaPlayer.PlaybackSession.SphericalVideoProjection.HorizontalFieldOfViewInDegrees = 90;
            }
           
        }

        private void SetVolume(double volume)
        {
            if (volume >= 1)
                volume = 1;
            if (volume <= 0)
                volume = 0;
            mediaElement.MediaPlayer.Volume = volume;
            if (mediaPlayer_audio!=null)
            {
                mediaPlayer_audio.Volume = volume;
            }
        }

    }
}
