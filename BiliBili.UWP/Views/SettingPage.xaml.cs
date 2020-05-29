using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (e.NavigationMode == NavigationMode.New)
            {
                GetSetting();
            }
        }
        bool get_ing = true;
        bool loadsetting = true;
        private async void GetSetting()
        {
            //if (!SettingHelper.IsPc())
            //{
            //    CustomBg.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    CustomBg.Visibility = Visibility.Visible;

            //}
            try
            {
                loadsetting = true;
                pr_Load.Visibility = Visibility.Visible;
                CustomBg.Visibility = Visibility.Visible;
                sw_HideStatus.IsOn = SettingHelper.Get_HideStatus();


                sw_NewWidnows.IsOn = SettingHelper.Get_NewWindow();
                sw_UseDASH.IsOn = SettingHelper.Get_UseDASH();

                //if (!await SystemHelper.CheckCodec())
                //{

                //}
                //else
                //{
                //    btnOpenInstallHEVC.Visibility = Visibility.Collapsed;
                //}
                sw_DownFLV.IsOn= SettingHelper.Get_DownFLV();

                btnOpenInstallHEVC.Visibility = Visibility.Visible;
                sw_DASHUseHEVC.IsOn = SettingHelper.Get_DASHUseHEVC();
                sw_PriorityBiliPlus.IsOn = SettingHelper.Get_PriorityBiliPlus();

                sw_ColunmHome.IsOn=SettingHelper.Get_ColunmHome();

                sw_LoadSe.IsOn = SettingHelper.Get_LoadSplash();
                sw_CloseAD.IsOn = SettingHelper.Get_HideAD();
                sw_ForceAudio.IsOn = SettingHelper.Get_ForceAudio();
                sw_ForceVideo.IsOn = SettingHelper.Get_ForceVideo();
                sw_DanmuBorder.IsOn = SettingHelper.Get_DMBorder();
                sw_Use4GDown.IsOn = SettingHelper.Get_Use4GDown();
                sw_RefreshButton.IsOn = SettingHelper.Get_RefreshButton();

                sw_Play4G.IsOn = SettingHelper.Get_Use4GPlay();
                sw_BackgroundPlay.IsOn = SettingHelper.Get_BackPlay();

                sw_SkipToHistory.IsOn = SettingHelper.Get_SkipToHistory();

                sw_HideCursor.IsOn = SettingHelper.Get_HideCursor();

                sw_MouseBack.IsOn = SettingHelper.Get_MouseBack();
                sw_MergeDanmu.IsOn = SettingHelper.Get_MergeDanmu();

                sw_NotSubtitle.IsOn = SettingHelper.Get_DanmuNotSubtitle();
                sw_BoldDanmu.IsOn = SettingHelper.Get_BoldDanmu();
                sw_StatusDanmu.IsOn = SettingHelper.Get_DMStatus();

                sw_DTCT.IsOn = SettingHelper.Get_DTCT();
                sw_DT.IsOn = SettingHelper.Get_DT();
                sw_FJ.IsOn = SettingHelper.Get_FJ();
                sw_CustomPath.IsOn = SettingHelper.Get_CustomDownPath();
                sw_FFmpeg.IsOn = SettingHelper.Get_FFmpeg();

                sw_NewFeed.IsOn = SettingHelper.Get_NewFeed();

                tw_MNGA.IsOn = SettingHelper.Get_UseHK();
                tw_MNTW.IsOn = SettingHelper.Get_UseTW();
                tw_MNDL.IsOn = SettingHelper.Get_UseCN();
                tw_PlayerMode.IsOn = SettingHelper.Get_PlayerMode();
                tw_VipMode.IsOn = SettingHelper.Get_UseVIP();
                tw_OtherSiteMode.IsOn = SettingHelper.Get_UseOtherSite();

                sw_QZHP.IsOn = SettingHelper.Get_QZHP();
                sw_AutoFull.SelectedIndex = SettingHelper.Get_AutoFullIndex();

                slider_DanmuSize.Value = SettingHelper.Get_NewDMSize();
                slider_Num.Value = SettingHelper.Get_DMNumber();
                slider_DanmuTran.Value = SettingHelper.Get_NewDMTran();
                slider_DanmuSpeed.Value = SettingHelper.Get_DMSpeed();

                List<string> fonts = SystemHelper.GetSystemFontFamilies();
                cb_Font.ItemsSource = fonts;
                if (SettingHelper.Get_DanmuFont() != "")
                {
                    cb_Font.SelectedIndex = fonts.IndexOf(SettingHelper.Get_DanmuFont());
                }
                else
                {
                    cb_Font.SelectedIndex = fonts.IndexOf(cb_Font.FontFamily.Source);
                }

                var c = SettingHelper.Get_BiliplusCookie();
                if (c != "")
                {
                    txtBPState.Text = "(已授权)";
                }
                else
                {
                    txtBPState.Text = "";
                }

                cb_PlayQuality.SelectedIndex = SettingHelper.Get_PlayQualit() - 1;
                cb_DownQuality.SelectedIndex = SettingHelper.Get_DownQualit() - 1;
                cb_VideoType.SelectedIndex = SettingHelper.Get_VideoType();
                cb_DanmuStyle.SelectedIndex = SettingHelper.Get_DMStyle();

                cb_DownMode.SelectedIndex = SettingHelper.Get_DownMode();

                sw_Playback.SelectedIndex = SettingHelper.Get_Playback();


                sw_CustomBg.IsOn = SettingHelper.Get_CustomBG();

                txt_BGPath.Text = SettingHelper.Get_BGPath();
                txt_CustomDownPath.Text = SettingHelper.Get_DownPath();


                cb_BGStretch.SelectedIndex = SettingHelper.Get_BGStretch();
                cb_Ver.SelectedIndex = SettingHelper.Get_BGVer();
                cbHor.SelectedIndex = SettingHelper.Get__BGHor();
                cb_BGOpacity.SelectedIndex = SettingHelper.Get_BGOpacity() - 1;
                cb_FrostedGlass.SelectedIndex = SettingHelper.Get_FrostedGlass();
                cb_ClaerLiveComment.SelectedIndex = SettingHelper.Get_ClearLiveComment();
                sw_H5.IsOn = SettingHelper.Get_UseH5();

                txt_BGMaxHeight.Text = SettingHelper.Get_BGMaxHeight().ToString();
                txt_BGMaxWidth.Text = SettingHelper.Get_BGMaxWidth().ToString();

                cb_BanPlayer.SelectedIndex = SettingHelper.Get_BanPlayer();
                if (sw_CustomBg.IsOn)
                {
                    grid_CustomBg.Visibility = Visibility.Visible;
                    grid_BgHeigth.Visibility = Visibility.Visible;
                    grid_BgWdith.Visibility = Visibility.Visible;
                    grid_Hor.Visibility = Visibility.Visible;
                    grid_Stretch.Visibility = Visibility.Visible;
                    grid_Ver.Visibility = Visibility.Visible;
                    grid_Opacity.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_CustomBg.Visibility = Visibility.Collapsed;
                    grid_BgHeigth.Visibility = Visibility.Collapsed;
                    grid_BgWdith.Visibility = Visibility.Collapsed;
                    grid_Hor.Visibility = Visibility.Collapsed;
                    grid_Stretch.Visibility = Visibility.Collapsed;
                    grid_Ver.Visibility = Visibility.Collapsed;
                    grid_Opacity.Visibility = Visibility.Collapsed;
                }


                txt_version.Text = "Ver " + SettingHelper.GetVersion();




                get_ing = true;
                switch (SettingHelper.Get_Theme())
                {
                    case "Red":
                        cb_Theme.SelectedIndex = 2;
                        break;
                    case "Blue":
                        cb_Theme.SelectedIndex = 5;
                        break;
                    case "Green":
                        cb_Theme.SelectedIndex = 4;
                        break;
                    case "Pink":
                        cb_Theme.SelectedIndex = 0;
                        break;
                    case "Purple":
                        cb_Theme.SelectedIndex = 6;
                        break;
                    case "Yellow":
                        cb_Theme.SelectedIndex = 3;
                        break;
                    case "EMT":
                        cb_Theme.SelectedIndex = 7;
                        break;
                    case "Dark":
                        cb_Theme.SelectedIndex = 1;
                        break;
                    default:
                        break;
                }
                get_ing = false;

                cb_Rigth.SelectedIndex = SettingHelper.Get_Rigth();


                if (SettingHelper.Get_BGPath().Length != 0)
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(SettingHelper.Get_BGPath());
                    if (file == null)
                    {
                        txt_BGPath.Text = "没有背景啊，右侧自定义-->";
                    }
                    else

                    {
                        txt_BGPath.Text = file.DisplayName;
                    }

                }

                txt_Ver.Text = AppHelper.verStr.Replace("/", "");
                pr_Load.Visibility = Visibility.Collapsed;




                loadsetting = false;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("设置读取失败");
            }


        }

        private async void cb_Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Theme.SelectedIndex != -1 && !get_ing)
            {
                switch (cb_Theme.SelectedIndex)
                {
                    case 0:
                        SettingHelper.Set_Theme("Pink");
                        break;
                    case 1:
                        SettingHelper.Set_Theme("Dark");
                        break;
                    case 2:
                        SettingHelper.Set_Theme("Red");
                        break;
                    case 3:
                        SettingHelper.Set_Theme("Yellow");
                        break;
                    case 4:
                        SettingHelper.Set_Theme("Green");
                        break;
                    case 5:
                        SettingHelper.Set_Theme("Blue");
                        break;
                    case 6:
                        SettingHelper.Set_Theme("Purple");
                        break;
                    case 7:
                        SettingHelper.Set_Theme("EMT");
                        break;
                    default:
                        break;
                }
                MessageCenter.SendChanageThemeEvent(null);
                MessageDialog messageDialog = new MessageDialog("重启应用一下，效果更好，是否立即重启应用?", "是否重启应用");
                messageDialog.Commands.Add(new UICommand("确定", async (x) =>
                {
                    var result = await CoreApplication.RequestRestartAsync(string.Empty);
                    if (result == AppRestartFailureReason.NotInForeground || result == AppRestartFailureReason.Other)
                    {
                        Utils.ShowMessageToast("请收到重启应用");
                    }
                }));
                messageDialog.Commands.Add(new UICommand("取消", (x) => { }));
                await messageDialog.ShowAsync();
            }
            //MessageCenter.SendChanageThemeEvent(null);
            //await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void cb_Rigth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_Rigth(cb_Rigth.SelectedIndex);

        }

        private void sw_HideStatus_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_HideStatus(sw_HideStatus.IsOn);
        }

        private void sw_LoadSe_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_LoadSplash(sw_LoadSe.IsOn);
        }

        private void sw_CloseAD_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_HideAD(sw_CloseAD.IsOn);
        }

        private void cb_PlayQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_PlayQualit(cb_PlayQuality.SelectedIndex + 1);
        }

        private void cb_VideoType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_VideoType(cb_VideoType.SelectedIndex);
        }

        private void sw_ForceVideo_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_ForceVideo(sw_ForceVideo.IsOn);
        }

        private void sw_ForceAudio_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_ForceAudio(sw_ForceAudio.IsOn);
        }

        private void sw_Playback_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_Playback(sw_Playback.SelectedIndex);
        }

        private void sw_DanmuBorder_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_DMBorder(sw_DanmuBorder.IsOn);
        }

        private void slider_DanmuSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!loadsetting)
            {
                SettingHelper.Set_NewDMSize(slider_DanmuSize.Value);
            }
        }

        private void cb_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loadsetting)
            {
                return;
            }
            SettingHelper.Set_DanmuFont(cb_Font.SelectedItem.ToString());
        }

        private void slider_DanmuSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

            SettingHelper.Set_DMSpeed(slider_DanmuSpeed.Value);
        }

        private void slider_DanmuTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!loadsetting)
            {
                SettingHelper.Set_NewDMTran(slider_DanmuTran.Value);
            }

        }

        private void slider_Num_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.Set_DMNumber(Convert.ToInt32(slider_Num.Value));
        }

        private void cb_DownQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_DownQualit(cb_DownQuality.SelectedIndex + 1);
        }

        private void cb_DownMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_DownMode(cb_DownMode.SelectedIndex);
        }

        private void sw_Use4GDown_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_Use4GDown(sw_Use4GDown.IsOn);
            DownloadHelper2.UpdateDowningStatus();
        }

        private void sw_DTCT_Toggled(object sender, RoutedEventArgs e)
        {
            if (!sw_DTCT.IsOn)
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
            }
            SettingHelper.Set_DTCT(sw_DTCT.IsOn);

        }

        private void sw_DT_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_DT(sw_DT.IsOn);
        }

        private void sw_FJ_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_FJ(sw_FJ.IsOn);
        }

        private void tw_MNGA_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_UseHK(tw_MNGA.IsOn);
            if (tw_MNGA.IsOn)
            {
                tw_MNTW.IsOn = false;
                tw_MNDL.IsOn = false;
            }
        }

        private void tw_MNTW_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_UseTW(tw_MNTW.IsOn);
            if (tw_MNTW.IsOn)
            {
                tw_MNGA.IsOn = false;
                tw_MNDL.IsOn = false;
            }
        }

        private void tw_MNDL_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_UseCN(tw_MNDL.IsOn);
            if (tw_MNDL.IsOn)
            {
                tw_MNGA.IsOn = false;
                tw_MNTW.IsOn = false;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText((sender as HyperlinkButton).Tag.ToString());
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            Utils.ShowMessageToast("已将内容复制到剪切板", 3000);
        }

        private async void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            var x = new ContentDialog();
            StackPanel st = new StackPanel();
            st.Children.Add(new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/zfb.jpg"))
            });
            st.Children.Add(new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true,
                Text = "\r\n如果觉得应用不错，给我点打赏我会很感谢的!\r\n支付宝：2500655055@qq.com,**程\r\n"
            });
            x.Content = st;
            x.PrimaryButtonText = "知道了";
            x.IsPrimaryButtonEnabled = true;
            await x.ShowAsync();
        }

        private void btn_GoManage_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DMHideManagePage));
        }

        private void sw_RefreshButton_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_RefreshButton(sw_RefreshButton.IsOn);
        }

        private void sw_CustomBg_Toggled(object sender, RoutedEventArgs e)
        {
            if (sw_CustomBg.IsOn)
            {
                grid_CustomBg.Visibility = Visibility.Visible;
                grid_BgHeigth.Visibility = Visibility.Visible;
                grid_BgWdith.Visibility = Visibility.Visible;
                grid_Hor.Visibility = Visibility.Visible;
                grid_Stretch.Visibility = Visibility.Visible;
                grid_Ver.Visibility = Visibility.Visible;
                grid_Opacity.Visibility = Visibility.Visible;
            }
            else
            {
                grid_CustomBg.Visibility = Visibility.Collapsed;
                grid_BgHeigth.Visibility = Visibility.Collapsed;
                grid_BgWdith.Visibility = Visibility.Collapsed;
                grid_Hor.Visibility = Visibility.Collapsed;
                grid_Stretch.Visibility = Visibility.Collapsed;
                grid_Ver.Visibility = Visibility.Collapsed;
                grid_Opacity.Visibility = Visibility.Collapsed;
            }
            SettingHelper.Set_CustomBG(sw_CustomBg.IsOn);
            MessageCenter.SendChangedBg();
        }

        private async void btn_ChanageBg_Click(object sender, RoutedEventArgs e)
        {

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.CommitButtonText = "选中此文件";
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".bmp");
            // 弹出文件选择窗口
            StorageFile file = await openPicker.PickSingleFileAsync(); // 用户在“文件选择窗口”中完成操作后，会返回对应的 StorageFile 对象
            if (file != null)
            {
                StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile ex = null;
                try
                {
                    ex = await localFolder.GetFileAsync(file.Name);
                }
                catch (Exception)
                {
                }
                if (ex == null)
                {
                    var cp = await file.CopyAsync(localFolder);
                    SettingHelper.Set_BGPath(cp.Path);
                }
                else
                {
                    SettingHelper.Set_BGPath(ex.Path);
                }

                txt_BGPath.Text = file.Name;


                MessageCenter.SendChangedBg();



            }

        }

        private void cb_BGStretch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_BGStretch(cb_BGStretch.SelectedIndex);
            MessageCenter.SendChangedBg();
        }

        private void cbHor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_BGHor(cbHor.SelectedIndex);
            MessageCenter.SendChangedBg();
        }

        private void cb_Ver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            SettingHelper.Set_BGVer(cb_Ver.SelectedIndex);
            MessageCenter.SendChangedBg();
        }

        private void cb_BGOpacity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_BGOpacity(cb_BGOpacity.SelectedIndex + 1);
            MessageCenter.SendChangedBg();
        }

        private void txt_BGMaxWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            int w = 0;
            if (!int.TryParse(txt_BGMaxWidth.Text, out w) || w < 0)
            {
                txt_BGMaxWidth.Text = "0";
                Utils.ShowMessageToast("请输入正整数！", 2000);
                return;
            }

            SettingHelper.Set_BGMaxWidth(w);
            MessageCenter.SendChangedBg();

        }

        private void txt_BGMaxHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            int h = 0;
            if (!int.TryParse(txt_BGMaxHeight.Text, out h) || h < 0)
            {
                txt_BGMaxHeight.Text = "0";
                Utils.ShowMessageToast("请输入正整数！", 2000);
                return;
            }

            SettingHelper.Set_BGMaxHeight(h);

            MessageCenter.SendChangedBg();


        }

        private  void sw_CustomPath_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_CustomDownPath(sw_CustomPath.IsOn);
        }

        private async void btn_CustomDoanPath_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker fp = new FolderPicker();
            fp.FileTypeFilter.Add(".mp4");
            var f = await fp.PickSingleFolderAsync();
            if (f == null)
            {
                return;
            }
            string mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(f, f.Path);
            SettingHelper.Set_DownPath(f.Path);
            txt_CustomDownPath.Text = f.Path;
            //读取文件夹
            // string mruFirstToken = StorageApplicationPermissions.MostRecentlyUsedList.Entries.First(x=>x.Metadata==f.Path).Token;
            //StorageFolder retrievedFile = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruFirstToken);

            //Utils.ShowMessageToast(mruToken, 3000);

        }

        private void cb_FrostedGlass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_FrostedGlass(cb_FrostedGlass.SelectedIndex);
            MessageCenter.SendChangedBg();
        }

        private void tw_PlayerMode_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_PlayerMode(tw_PlayerMode.IsOn);
        }

        private void sw_FFmpeg_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_FFmpeg(sw_FFmpeg.IsOn);
        }

        private void sw_H5_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_UseH5(sw_H5.IsOn);
        }

        private void cb_ClaerLiveComment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_ClearLiveComment(cb_ClaerLiveComment.SelectedIndex);
        }

        private void sw_BackgroundPlay_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_BackPlay(sw_BackgroundPlay.IsOn);
        }

        private void sw_Play4G_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_Use4GPlay(sw_Play4G.IsOn);
        }

        private void sw_QZHP_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_QZHP(sw_QZHP.IsOn);
        }

        private void sw_AutoFull_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_AutoFull(sw_AutoFull.SelectedIndex);
        }

        private void sw_HideCursor_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_HideCursor(sw_HideCursor.IsOn);
        }

        private void sw_NewFeed_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_NewFeed(sw_NewFeed.IsOn);
        }

        private void sw_NewWidnows_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_NewWindow(sw_NewWidnows.IsOn);
        }

        private void cb_BanPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingHelper.Set_BanPlayer(cb_BanPlayer.SelectedIndex);
        }

        private async void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            await WebView.ClearTemporaryWebDataAsync();
            await new MessageDialog("清除完成,为了能正常使用，请重新登录").ShowAsync();
        }

        private void sw_toMp4_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_ToMp4(sw_toMp4.IsOn);
        }

        private void btn_Help_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "https://www.showdoc.cc/biliuwp");
        }

        private void sw_MouseBack_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_MouseBack(sw_MouseBack.IsOn);
        }

        private void sw_MergeDanmu_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_MergeDanmu(sw_MergeDanmu.IsOn);
        }

        private void cb_DanmuStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cb_DanmuStyle.SelectedIndex)
            {
                case 0:
                    danmustyle_border.Visibility = Visibility.Visible;
                    danmustyle_noborder.Visibility = Visibility.Collapsed;
                    danmustyle_shadow.Visibility = Visibility.Collapsed;
                    danmustyle_borderV2.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    danmustyle_border.Visibility = Visibility.Collapsed;
                    danmustyle_noborder.Visibility = Visibility.Visible;
                    danmustyle_shadow.Visibility = Visibility.Collapsed;
                    danmustyle_borderV2.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    danmustyle_border.Visibility = Visibility.Collapsed;
                    danmustyle_noborder.Visibility = Visibility.Collapsed;
                    danmustyle_shadow.Visibility = Visibility.Visible;
                    danmustyle_borderV2.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    danmustyle_border.Visibility = Visibility.Collapsed;
                    danmustyle_noborder.Visibility = Visibility.Collapsed;
                    danmustyle_shadow.Visibility = Visibility.Collapsed;
                    danmustyle_borderV2.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            SettingHelper.Set_DMStyle(cb_DanmuStyle.SelectedIndex);
        }

        private void sw_NotSubtitle_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_DanmuNotSubtitle(sw_NotSubtitle.IsOn);
        }

        private void btn_Feedback_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "https://www.wjx.top/jq/23344273.aspx");
        }

        private void sw_StatusDanmu_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_DMStatus(sw_StatusDanmu.IsOn);
        }

        private void sw_BoldDanmu_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_BoldDanmu(sw_BoldDanmu.IsOn);
        }

        private void tw_VipMode_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_UseVIP(tw_VipMode.IsOn);
        }

        private void tw_OtherSiteMode_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_UseOtherSite(tw_OtherSiteMode.IsOn);
        }

        private async void BrnAuthBiliPlus_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行此操作");
                return;
            }
            var re = await Account.AuthBiliPlus();
            if (re != "")
            {
                txtBPState.Text = "(已授权)";
            }
            else
            {
                Utils.ShowMessageToast("授权失败了");
            }
        }

        private void Sw_UseDASH_Toggled(object sender, RoutedEventArgs e)
        {
            
            if (sw_UseDASH.IsOn && SystemHelper.GetSystemBuild() < 17763)
            {
                Utils.ShowMessageToast("系统版本1809以上才可以开启");
                sw_UseDASH.IsOn = false;
                return;
            }
            SettingHelper.Set_UseDASH(sw_UseDASH.IsOn);
        }

        private void Sw_DASHUseHEVC_Toggled(object sender, RoutedEventArgs e)
        {

            //if (sw_DASHUseHEVC.IsOn&& !await SystemHelper.CheckCodec())
            //{
            //    sw_DASHUseHEVC.IsOn = false;
            //    Utils.ShowMessageToast("请先安装HEVC扩展");
            //}
            //else
            //{

            //}
            SettingHelper.Set_DASHUseHEVC(sw_DASHUseHEVC.IsOn);
        }

        private void Sw_PriorityBiliPlus_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_PriorityBiliPlus(sw_PriorityBiliPlus.IsOn);
        }

        private void sw_ColunmHome_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_ColunmHome(sw_ColunmHome.IsOn);
        }

        private void sw_SkipToHistory_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_SkipToHistory(sw_SkipToHistory.IsOn);
        }

        private void sw_DownFLV_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set_DownFLV(sw_DownFLV.IsOn);
        }
    }
}
