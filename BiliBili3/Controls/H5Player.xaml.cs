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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BiliBili3.Controls
{
    public sealed partial class H5Player : UserControl
    {
        public event BackHandel BackEvent;
        public H5Player()
        {
            this.InitializeComponent();
            CoreWindow.GetForCurrentThread().KeyDown += PlayerPage_KeyDown;

        }

        private void PlayerPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }
            args.Handled = true;
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Space:
                    btn_Play_Click(sender, null);
                    break;
                case Windows.System.VirtualKey.Left:
                    slider3.Value = slider.Value - 3;
                    TimeSpan ts = TimeSpan.FromSeconds(slider.Value);
                    Utils.ShowMessageToast(ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00"), 3000);
                    break;
                case Windows.System.VirtualKey.Up:
                    // mediaElement.Volume += 0.1;
                    //mediaElement.Balance += 0.1;
                    slider_V.Value += 0.1;
                    Utils.ShowMessageToast("音量:" + slider_V.Value.ToString("P"), 3000);
                    break;
                case Windows.System.VirtualKey.Right:
                    {
                        slider3.Value = slider.Value + 3;
                        TimeSpan ts2 = TimeSpan.FromSeconds(slider.Value);
                        Utils.ShowMessageToast(ts2.Hours.ToString("00") + ":" + ts2.Minutes.ToString("00") + ":" + ts2.Seconds.ToString("00"), 3000);
                    }
                    break;
                case Windows.System.VirtualKey.Down:
                    slider_V.Value -= 0.1;
                    Utils.ShowMessageToast("音量:" + slider_V.Value.ToString("P"), 3000);
                    //mediaElement.Balance -= 0.1;
                    //Utils.ShowMessageToast("音量:" + mediaElement.Volume.ToString("P"), 3000);
                    break;
                case Windows.System.VirtualKey.Escape:
                    if (btn_Full.Visibility == Visibility.Collapsed)
                    {

                        btn_Full.Visibility = Visibility.Visible;
                        btn_ExitFull.Visibility = Visibility.Collapsed;
                        ApplicationView.GetForCurrentView().ExitFullScreenMode();

                        danmu.SetJJ();
                    }
                    break;

                case Windows.System.VirtualKey.F11:
                    if (btn_ExitFull.Visibility == Visibility.Collapsed)
                    {

                        btn_Full.Visibility = Visibility.Collapsed;
                        btn_ExitFull.Visibility = Visibility.Visible;


                        ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                        danmu.SetJJ();
                    }
                    else
                    {

                        btn_Full.Visibility = Visibility.Visible;
                        btn_ExitFull.Visibility = Visibility.Collapsed;
                        ApplicationView.GetForCurrentView().ExitFullScreenMode();
                        danmu.SetJJ();
                    }
                    break;
                default:
                    break;
            }
        }

        private DisplayRequest dispRequest = null;//保持屏幕常亮
        DispatcherTimer timer;
        DispatcherTimer timer_Date;
        List<PlayerModel> playList;
        List<MyDanmaku.DanMuModel> DanMuPool = null;
        PlayerModel playNow;

        //int _index = 0;
        bool playLocal = false;
        bool LoadDanmu = true;
        int LastPost = 0;
        public void LoadPlayer(List<PlayerModel> par, int index)
        {
            LoadOk = false;
            webPlayer.Navigate(new Uri("ms-appx-web:///html/index.html"));
            UpdateSetting();

            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += Timer_Tick;

            }
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
                btn_Full_Click(null, null);
            }

            playList = par;
            playNow = playList[index];

            //  btn_HideInfo.Visibility = Visibility.Collapsed;
            //   btn_ShowInfo.Visibility = Visibility.Collapsed;


            gv_play.ItemsSource = playList;
            gv_play.SelectedIndex = index;


            //DisplayInformation.AutoRotationPreferences = (DisplayOrientations)5;

        }

        public void ClosePLayer()
        {
            try
            {
                if (dispRequest != null)
                {
                    dispRequest = null;
                }
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
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
                gv_play.ItemsSource = null;
                webPlayer.NavigateToString(@"<!DOCTYPE html><html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml""><body>- -!</body></html>");
                danmu.ClearDanmu();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            }
            catch (Exception)
            {
            }
            
        }

        private async void OpenVideo()
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                LoadDanmu = false;
                danmu.ClearDanmu();
                LastPost = 0;
                slider3.Value = 0;
                slider.Value = 0;
                txt_Title.Text = playNow.Title + " - " + playNow.No + " " + playNow.VideoTitle;
                send_danmu.Visibility = Visibility.Visible;
                btn_Open_CloseDanmu.Visibility = Visibility.Visible;
                btn_Select.Visibility = Visibility.Visible;
                cb_Quity.Visibility = Visibility.Visible;
                pr.Text = "正在初始化播放器...";


                if (playNow.Mode == PlayMode.Video)
                {


                    pr.Text = "填充弹幕中...";
                    DanMuPool = await GetDM(playNow.Mid, false, false, "");
                    pr.Text = "加载视频中...";
                    //var url = await ApiHelper.GetVideoUrl_FLV(playNow, cb_Quity.SelectedIndex + 1);
                    while (true)
                    {
                        if (LoadOk)
                        {
                            //await webPlayer.InvokeScriptAsync("Play", new string[] { url });
                            break;
                        }
                        await Task.Delay(200);
                    }

                 
                }


                if (SqlHelper.GetPostIsViewPost(playNow.Mid) && SqlHelper.GettViewPost(playNow.Mid).Post != 0)
                {
                    TimeSpan ts = new TimeSpan(0, 0, SqlHelper.GettViewPost(playNow.Mid).Post);
                    LastPost = SqlHelper.GettViewPost(playNow.Mid).Post;
                    btn_ViewPost.Content = "上次播放到" + ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                    btn_ViewPost.Visibility = Visibility.Visible;
                    _lastpost_in.Begin();
                    await Task.Delay(5000);
                    _lastpost_out.Begin();
                }
                else
                {
                    if (!SqlHelper.GetPostIsViewPost(playNow.Mid))
                    {
                        SqlHelper.AddViewPost(new ViewPostHelperClass() { epId = playNow.Mid, Post = 0, viewTime = DateTime.Now.ToLocalTime() });
                    }
                }

            }
            catch (Exception ex)
            {
                await new MessageDialog("视频暂时无法播放哦").ShowAsync();
            }
            finally
            {

                PostLocalHistory();
                if (playNow.episode_id != null && playNow.episode_id.Length != 0)
                {
                    PostWatch();
                }

                await Task.Delay(3000);
                _Out.Storyboard.Begin();
            }
        }

        #region 手势操作
        private void ss_Volume_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            double Y = e.Delta.Translation.Y;
            if (Y > 0)
            {
                double dd = Y / ss_Volume.ActualHeight;
                double d = dd * slider_V.Maximum;
                slider_V.Value -= d;
            }
            else
            {
                double dd = Math.Abs(Y) / ss_Volume.ActualHeight;
                double d = dd * slider_V.Maximum;
                slider_V.Value += d;
            }
            Utils.ShowMessageToast("音量:" + slider_V.Value.ToString("P"), 3000);
        }

        private void ss_Volume_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
        }
        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            btn_Play_Click(sender, e);
            //progress.Visibility = Visibility.Visible;
            double X = e.Delta.Translation.X;
            if (X > 0)
            {
                double dd = X / this.ActualWidth;
                double d = dd * 90;
                slider3.Value = slider.Value + d;
                // slider.Value += d;
            }
            else
            {
                double dd = Math.Abs(X) / this.ActualWidth;
                double d = dd * 90;
                slider3.Value = slider.Value - d;
                //slider.Value -= d;
            }
            TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(slider.Value));
            TimeSpan ts1 = new TimeSpan(0, 0, Convert.ToInt32(slider.Maximum));
            txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + ts1.Hours.ToString("00") + ":" + ts1.Minutes.ToString("00") + ":" + ts1.Seconds.ToString("00");
            Utils.ShowMessageToast(ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00"), 3000);
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            btn_Play_Click(sender, e);

            double X = e.Cumulative.Translation.X;

        }
        #endregion


        public async Task<List<MyDanmaku.DanMuModel>> GetDM(string cid, bool IsLocal, bool IsOld, string path)
        {
            List<MyDanmaku.DanMuModel> ls = new List<MyDanmaku.DanMuModel>();
            try
            {

                string a = await WebClientClass.GetResults(new Uri("http://comment.bilibili.com/" + cid + ".xml" + "?rnd=" + new Random().Next(1, 9999)));
                XmlDocument xdoc = new XmlDocument();
                a = Regex.Replace(a, @"[\x00-\x08]|[\x0B-\x0C]|[\x0E-\x1F]", "");
                xdoc.LoadXml(a);
                XmlElement el = xdoc.DocumentElement;
                XmlNodeList xml = el.ChildNodes;
                foreach (XmlNode item in xml)
                {
                    if (item.Attributes["p"] != null)
                    {
                        try
                        {
                            string heheda = item.Attributes["p"].Value;
                            string[] haha = heheda.Split(',');
                            ls.Add(new MyDanmaku.DanMuModel
                            {
                                DanTime = decimal.Parse(haha[0]),
                                DanMode = haha[1],
                                DanSize = haha[2],
                                _DanColor = haha[3],
                                DanSendTime = haha[4],
                                DanPool = haha[5],
                                DanID = haha[6],
                                DanRowID = haha[7],
                                DanText = item.InnerText,
                                source = item.OuterXml
                            });
                        }
                        catch (Exception)
                        {
                        }

                    }
                }
                return ls;
            }
            catch (Exception)
            {

                return ls;
            }

        }
        private async void PostLocalHistory()
        {
            try
            {
                string url = string.Format("http://api.bilibili.com/x/history/add?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=5250000&platform=android", ApiHelper.access_key, ApiHelper._appKey);
                url += "&sign=" + ApiHelper.GetSign(url);
                string result = await WebClientClass.PostResults(new Uri(url), "aid=" + playNow.Aid);
            }
            catch (Exception)
            {
            }
        }


        private async void PostWatch()
        {
            try
            {
                string url = string.Format("http://bangumi.bilibili.com/api/report_watch?access_key={0}&appkey={1}&build=5250000&cid={2}&mobi_app=win&platform=android&scale=xhdpi&ts={3}&episode_id={4}", ApiHelper.access_key, ApiHelper._appKey, playNow.Mid, ApiHelper.GetTimeSpan_2, playNow.episode_id);
                url += "&sign=" + ApiHelper.GetSign(url);
                string result = await WebClientClass.GetResults(new Uri(url));
            }
            catch (Exception)
            {
            }
        }


        private async void Timer_Tick(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await webPlayer.InvokeScriptAsync("GetStatus", null);

            });
        }
        int i = 0;
        int DanmuNum = 0;
        MediaElementState state;
        private async void Timer_Date_Tick(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txt_Time.Text = DateTime.Now.ToString("HH:mm");
            });
            try
            {
                if (state == MediaElementState.Playing && LoadDanmu)
                {
                    if (DanMuPool != null)
                    {
                        int now_num = 0;
                        foreach (var item in DanMuPool)
                        {
                            if (!DanDis_Dis(item.DanText))
                            {
                                if (Convert.ToInt32(item.DanTime) == Convert.ToInt32(slider.Value))
                                {
                                    if (now_num >= DanmuNum && DanmuNum != 0)
                                    {
                                        return;
                                    }


                                    if (item.DanMode == "5")
                                    {
                                        danmu.AddTopButtomDanmu(item, true, false);
                                    }
                                    else
                                    {
                                        if (item.DanMode == "4")
                                        {
                                            danmu.AddTopButtomDanmu(item, false, false);
                                        }
                                        else
                                        {
                                            danmu.AddGunDanmu(item, false);
                                        }
                                    }
                                    now_num++;
                                }

                            }

                        }

                        if (i == 3)
                        {
                            danmu.row = 0;
                            i = 0;
                        }
                        else
                        {
                            i++;
                        }
                    }


                }
            }
            catch (Exception)
            {
            }


        }

        #region 弹幕设置
        /// <summary>
        /// 弹幕屏蔽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Dis_Remove_Click(object sender, RoutedEventArgs e)
        {
            foreach (MyDanmaku.DanMuModel item in list_DisDanmu.SelectedItems)
            {
                DanDis_Add(item.DanID, true);
                danmu.RemoveDanmu(item);
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
        string DMZZBDS = "";
        public void UpdateSetting()
        {
            //if (!SettingHelper.IsPc())
            //{
            //    btn_ShowInfo.Visibility = Visibility.Collapsed;
            //    btn_HideInfo.Visibility = Visibility.Collapsed;
            //    // danmu.fontSize = 16;
            //}
            DanDis_Get();
            DMZZBDS = SettingHelper.Get_DMZZ();
            slider_DanmuSize.Value = SettingHelper.Get_DMSize();
            slider_Num.Value = SettingHelper.Get_DMNumber();
            slider_DanmuTran.Value = SettingHelper.Get_DMTran();
            slider_DanmuSpeed.Value = SettingHelper.Get_DMSpeed();

            cb_Font.SelectedIndex = SettingHelper.Get_DMFont();
            sw_DanmuBorder.IsOn = SettingHelper.Get_DMBorder();

            slider_V.Value = SettingHelper.Get_Volume();
            DanmuNum = SettingHelper.Get_DMNumber();

            btn_ViewPost.Visibility = Visibility.Collapsed;
            cb_Quity.SelectedIndex = SettingHelper.Get_PlayQualit() - 1;


            menu_setting_buttom.IsChecked = !SettingHelper.Get_DMVisBottom();
            menu_setting_top.IsChecked = !SettingHelper.Get_DMVisTop();
            menu_setting_gd.IsChecked = !SettingHelper.Get_DMVisRoll();




        }

        private void btn_Full_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.IsPc())
            {

            }
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            btn_Full.Visibility = Visibility.Collapsed;
            btn_ExitFull.Visibility = Visibility.Visible;
            danmu.SetJJ();
        }

        private void btn_ExitFull_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            btn_Full.Visibility = Visibility.Visible;
            btn_ExitFull.Visibility = Visibility.Collapsed;
            danmu.SetJJ();
        }

        private void gv_play_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gv_play.SelectedIndex != -1)
            {
                playNow = gv_play.SelectedItem as PlayerModel;
                // PlayerEvent(gv_play.SelectedIndex);
                OpenVideo();

            }
        }

        private void webPlayer_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var str = e.Value.Split(',');
            if (str[0] == "STARTPLAY")
            {
                state = MediaElementState.Buffering;
                timer.Start();
                slider.Maximum = Convert.ToInt32(str[1]);
                slider3.Maximum = Convert.ToInt32(str[1]);
            }
            else
            {
                danmu.state = state;
                if (str[0] == "seeking")
                {
                    danmu.IsPlaying = false;
                    state = MediaElementState.Buffering;
                    progress.Visibility = Visibility.Visible;
                }
                else
                {
                    progress.Visibility = Visibility.Collapsed;
                }
                if (str[0] == "play")
                {
                    danmu.IsPlaying = true;
                    LoadDanmu = true;
                    state = MediaElementState.Playing;
                    btn_Pause.Visibility = Visibility.Visible;
                    btn_Play.Visibility = Visibility.Collapsed;
                }
                if (str[0] == "pause")
                {
                    danmu.IsPlaying = false;
                    state = MediaElementState.Paused;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    btn_Play.Visibility = Visibility.Visible;
                }
                if (str[0] == "end")
                {
                    state = MediaElementState.Stopped;
                    danmu.IsPlaying = false;
                    danmu.ClearDanmu();
                    LoadDanmu = false;
                    MediaEnded();

                    return;
                }
                if (Convert.ToInt32(str[1]) == slider.Value)
                {
                    if (str[0] != "pause")
                    {
                        progress.Visibility = Visibility.Visible;
                        LoadDanmu = false;
                    }
                }
                else
                {
                    progress.Visibility = Visibility.Collapsed;
                    LoadDanmu = true;
                }
                slider.Value = Convert.ToInt32(str[1]);
                TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(slider.Value));
                TimeSpan ts2 = new TimeSpan(0, 0, Convert.ToInt32(slider.Maximum));
                txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + ts2.Hours.ToString("00") + ":" + ts2.Minutes.ToString("00") + ":" + ts2.Seconds.ToString("00");
                info_txt_height.Text = str[2];
                info_txt_width.Text = str[3];
                if (SqlHelper.GetPostIsViewPost(playNow.Mid))
                {
                    SqlHelper.UpdateViewPost(new ViewPostHelperClass() { epId = playNow.Mid, Post = Convert.ToInt32(slider.Value) });
                }
                //txt.Text = str[2];
            }

        }

        private void MediaEnded()
        {
            try
            {
                if (cb_setting_1.IsChecked.Value)
                {
                    btn_Play_Click(null, null);
                    danmu.ClearDanmu();
                    return;
                }
                if (gv_play.SelectedIndex == gv_play.Items.Count - 1)
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
                else
                {

                    //webPlayer.NavigateToString(@"<!DOCTYPE html><html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml""><body>- -!</body></html>");
                    Utils.ShowMessageToast("开始播放下一集", 3000);
                    slider3.Value = 0;
                    //await Task.Delay(3000);
                    slider.Value = 0;
                    gv_play.SelectedIndex += 1;

                }
            }
            catch (Exception)
            {
            }

        }

        bool LoadOk = false;
        private void webPlayer_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            LoadOk = true;
        }

        private async void slider3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            while (true)
            {
                if (LoadOk)
                {
                    await webPlayer.InvokeScriptAsync("Seek", new string[] { slider3.Value.ToString() });
                    break;
                }
                await Task.Delay(200);
            }
          

        }

        private async void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            await webPlayer.InvokeScriptAsync("PlayPause", null);
        }

        private async void btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            await webPlayer.InvokeScriptAsync("PlayPause", null);
        }

        private void _lastpost_out_Completed(object sender, object e)
        {
            btn_ViewPost.Visibility = Visibility.Collapsed;
        }

        private async void ss_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await webPlayer.InvokeScriptAsync("PlayPause", null);
        }

        private void ss_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (grid.Visibility == Visibility.Collapsed)
            {
                grid.Visibility = Visibility.Visible;
                _In.Storyboard.Begin();

            }
            else
            {
                _Out.Storyboard.Begin();
            }

            _Out.Storyboard.Completed += Storyboard_Completed;
        }

        private void Storyboard_Completed(object sender, object e)
        {
            grid.Visibility = Visibility.Collapsed;
        }

        private void btn_Open_CloseDanmu_Click(object sender, RoutedEventArgs e)
        {
            if (danmu.Visibility == Visibility.Collapsed)
            {
                danmu.Visibility = Visibility.Visible;
                btn_Open_CloseDanmu.Foreground = new SolidColorBrush(Colors.White);
                LoadDanmu = true;

            }
            else
            {
                danmu.Visibility = Visibility.Collapsed;
                btn_Open_CloseDanmu.Foreground = new SolidColorBrush(Colors.Gray);
                LoadDanmu = false;

            }
        }

        private void menuitem_DM_Click(object sender, RoutedEventArgs e)
        {
            _Out.Storyboard.Begin();
            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Visible;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;

        }
        private void btn_VideoInfo_Click(object sender, RoutedEventArgs e)
        {
            _Out.Storyboard.Begin();
            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Visible;
            gv_play.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;
            //string info = string.Format("视频高度：{0}\r\n视频宽度：{1}\r\n视频长度：{2}\r\n缓冲进度:{3}", mediaElement.NaturalVideoHeight, mediaElement.NaturalVideoWidth, mediaElement.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Seconds.ToString("00"), mediaElement.DownloadProgress.ToString("P"));
            //await new MessageDialog(info, "视频信息").ShowAsync();
        }
        private void menuitem_PB_Click(object sender, RoutedEventArgs e)
        {
            _Out.Storyboard.Begin();
            btn_Pause_Click(sender, e);
            sp_View.IsPaneOpen = true;
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Visible;
            list_DisDanmu.Items.Clear();
            foreach (var item in danmu.GetScreenDanmu())
            {
                list_DisDanmu.Items.Add(item);
            }
        }

        private void menuitem_Info_Click(object sender, RoutedEventArgs e)
        {
            _Out.Storyboard.Begin();
            sp_View.IsPaneOpen = true;
            if (playNow != null && DanMuPool != null)
            {
                info_txt_Num.Text = DanMuPool.Count.ToString();
                info_txt_sId.Text = playNow.Aid;
                info_txt_eId.Text = playNow.Mid;
            }
            grid_Setting.Visibility = Visibility.Collapsed;
            gv_play.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Visible;
            grid_PB.Visibility = Visibility.Collapsed;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            danmu.SetJJ();
        }



        #region 设置
        private void sw_DanmuBorder_Toggled(object sender, RoutedEventArgs e)
        {
            danmu.D_Border = sw_DanmuBorder.IsOn;
            SettingHelper.Set_DMBorder(sw_DanmuBorder.IsOn);
        }

        private void slider_DanmuSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.fontSize = slider_DanmuSize.Value;
            danmu.SetJJ();
            SettingHelper.Set_DMSize(slider_DanmuSize.Value);
        }

        private void cb_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cb_Font.SelectedIndex)
            {
                case 0:
                    danmu.fontFamily = "默认";
                    break;
                case 1:
                    danmu.fontFamily = "微软雅黑";
                    break;
                case 2:
                    danmu.fontFamily = "黑体";
                    break;
                case 3:
                    danmu.fontFamily = "楷体";
                    break;
                case 4:
                    danmu.fontFamily = "宋体";
                    break;
                case 5:
                    danmu.fontFamily = "等线";
                    break;
                default:
                    break;
            }
            SettingHelper.Set_DMFont(cb_Font.SelectedIndex);
        }

        private void slider_DanmuSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.Speed = Convert.ToInt32(slider_DanmuSpeed.Value);
            SettingHelper.Set_DMSpeed(slider_DanmuSpeed.Value);
        }

        private void slider_DanmuTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.Tran = slider_DanmuTran.Value / 100;
            SettingHelper.Set_DMTran(slider_DanmuTran.Value);
        }
        private void slider_Num_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            DanmuNum = Convert.ToInt32(slider_Num.Value);
            SettingHelper.Set_DMNumber(Convert.ToInt32(slider_Num.Value));
        }




        private void menu_setting_top_Click(object sender, RoutedEventArgs e)
        {

            danmu.SetDanmuVisibility(false, MyDanmaku.DanmuMode.Top);
            SettingHelper.Set_DMVisTop(false);


        }

        private void menu_setting_buttom_Click(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(false, MyDanmaku.DanmuMode.Buttom);
            SettingHelper.Set_DMVisBottom(false);
        }

        private void menu_setting_gd_Checked(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(false, MyDanmaku.DanmuMode.Roll);
            SettingHelper.Set_DMVisRoll(false);
        }

        private void menu_setting_gd_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(true, MyDanmaku.DanmuMode.Roll);
            SettingHelper.Set_DMVisRoll(true);
        }

        private void menu_setting_top_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(true, MyDanmaku.DanmuMode.Top);
            SettingHelper.Set_DMVisTop(true);
        }

        private void menu_setting_buttom_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(true, MyDanmaku.DanmuMode.Buttom);
            SettingHelper.Set_DMVisBottom(true);
        }


        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {

            DanDis_Add(txt_Dis.Text, false);
            txt_Dis.Text = "";
            var s = danmu.GetScreenDanmu();
            foreach (var item in s)
            {
                if (DanDis_Dis(item.DanText))
                {
                    danmu.RemoveDanmu(item);
                }
            }
        }






        #endregion

        private async void slider_V_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

                while (true)
                {
                    if (LoadOk)
                    {
                        await webPlayer.InvokeScriptAsync("SetVolume", new string[] { slider_V.Value.ToString() });
                        break;
                    }
                    await Task.Delay(200);
                }


        }


        private void Send_btn_Send_Click(object sender, RoutedEventArgs e)
        {

            SenDanmuKa();
        }
        /// <summary>
        /// 发送弹幕
        /// </summary>
        public async void SenDanmuKa()
        {
            if (Send_text_Comment.Text.Length == 0)
            {
                Utils.ShowMessageToast("弹幕内容不能为空!", 2000);
                return;
            }
            if (!ApiHelper.IsLogin())
            {
                Utils.ShowMessageToast("请先登录!", 2000);
                return;
            }
            try
            {
                Uri ReUri = new Uri("http://interface.bilibili.com/dmpost?cid=" + playNow.Mid + "&aid=" + playNow.Aid + "&pid=1");
                int modeInt = 1;
                if (Send_cb_Mode.SelectedIndex == 2)
                {
                    modeInt = 4;
                }
                if (Send_cb_Mode.SelectedIndex == 1)
                {
                    modeInt = 5;
                }
                string Canshu = "message=" + Send_text_Comment.Text + "&pool=0&playTime=" + Convert.ToInt32(slider.Value) + "&cid=" + playNow.Mid + "&date=" + DateTime.Now.ToString() + "&fontsize=25&mode=" + modeInt + "&rnd=" + new Random().Next(100000000, 999999999) + "&color=" + ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag;
                string result = await WebClientClass.PostResults(ReUri, Canshu);
                long code = long.Parse(result);

                if (code < 0)
                {
                    Utils.ShowMessageToast("已发送弹幕！" + result, 3000);
                }
                else
                {
                    Utils.ShowMessageToast("已发送弹幕！", 3000);
                    if (modeInt == 1)
                    {
                        danmu.AddGunDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" }, true);
                    }
                    if (modeInt == 4)
                    {
                        danmu.AddTopButtomDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" }, false, true);
                    }
                    if (modeInt == 5)
                    {
                        danmu.AddTopButtomDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" }, true, true);
                    }
                    Send_text_Comment.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("发送弹幕发生错误！\r\n" + ex.Message, 3000);
            }
        }

        private void Send_text_Comment_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SenDanmuKa();
            }
        }

        private void btn_Select_Click(object sender, RoutedEventArgs e)
        {
            _Out.Storyboard.Begin();
            gv_play.Visibility = Visibility.Visible;
            grid_Setting.Visibility = Visibility.Collapsed;
            grid_DM.Visibility = Visibility.Collapsed;
            grid_Info.Visibility = Visibility.Collapsed;
            grid_PB.Visibility = Visibility.Collapsed;

            sp_View.IsPaneOpen = true;
        }

        private void btn_ViewPost_Click(object sender, RoutedEventArgs e)
        {
            if (LastPost != 0)
            {
                slider3.Value = LastPost;
                btn_ViewPost.Visibility = Visibility.Collapsed;
            }

        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            ClosePLayer();
            BackEvent();
        }

        private void cb_Quity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gv_play.ItemsSource == null)
            {
                return;
            }
            switch (cb_Quity.SelectedIndex)
            {
                case 3:
                    SettingHelper.Set_PlayQualit(4);
                    break;
                case 2:
                    SettingHelper.Set_PlayQualit(3);
                    break;
                case 1:
                    SettingHelper.Set_PlayQualit(2);
                    break;
                case 0:
                    SettingHelper.Set_PlayQualit(1);
                    break;
                default:
                    break;
            }
            OpenVideo();
        }

        private void btn_Dis_Report_Click(object sender, RoutedEventArgs e)
        {
            if (list_DisDanmu.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (MyDanmaku.DanMuModel item in list_DisDanmu.SelectedItems)
            {
                ReportDM(item.DanRowID);
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

        private void webPlayer_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                if (args.Uri.AbsoluteUri == "ms-appx-web:///html/index.html")
                {
                    return;
                }
                var R = args.Uri.AbsolutePath.Replace("/H5Player/", "");
                args.Cancel = true;

                var str = R.Split(',');
                if (str[0] == "STARTPLAY")
                {
                    state = MediaElementState.Buffering;
                    timer.Start();
                    slider.Maximum = Convert.ToInt32(str[1]);
                    slider3.Maximum = Convert.ToInt32(str[1]);
                }
                else
                {
                    danmu.state = state;
                    if (str[0] == "seeking")
                    {
                        danmu.IsPlaying = false;
                        state = MediaElementState.Buffering;
                        progress.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        progress.Visibility = Visibility.Collapsed;
                    }
                    if (str[0] == "play")
                    {
                        danmu.IsPlaying = true;
                        LoadDanmu = true;
                        state = MediaElementState.Playing;
                        btn_Pause.Visibility = Visibility.Visible;
                        btn_Play.Visibility = Visibility.Collapsed;
                    }
                    if (str[0] == "pause")
                    {
                        danmu.IsPlaying = false;
                        state = MediaElementState.Paused;
                        btn_Pause.Visibility = Visibility.Collapsed;
                        btn_Play.Visibility = Visibility.Visible;
                    }
                    if (str[0] == "end")
                    {
                        state = MediaElementState.Stopped;
                        danmu.IsPlaying = false;
                        danmu.ClearDanmu();
                        LoadDanmu = false;
                        MediaEnded();

                        return;
                    }
                    if (Convert.ToInt32(str[1]) == slider.Value)
                    {
                        if (str[0] != "pause")
                        {
                            progress.Visibility = Visibility.Visible;
                            LoadDanmu = false;
                        }
                    }
                    else
                    {
                        progress.Visibility = Visibility.Collapsed;
                        LoadDanmu = true;
                    }
                    slider.Value = Convert.ToInt32(str[1]);
                    TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(slider.Value));
                    TimeSpan ts2 = new TimeSpan(0, 0, Convert.ToInt32(slider.Maximum));
                    txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + ts2.Hours.ToString("00") + ":" + ts2.Minutes.ToString("00") + ":" + ts2.Seconds.ToString("00");
                    info_txt_height.Text = str[2];
                    info_txt_width.Text = str[3];
                    if (SqlHelper.GetPostIsViewPost(playNow.Mid))
                    {
                        SqlHelper.UpdateViewPost(new ViewPostHelperClass() { epId = playNow.Mid, Post = Convert.ToInt32(slider.Value) });
                    }
                    //txt.Text = str[2];
                }

            }
            catch (Exception)
            {
            }
           

        }
    }



}
