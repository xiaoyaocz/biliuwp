using BiliBili.UWP.Models;
using BiliBili.UWP.Pages;
using BiliBili.UWP.Pages.User;
using Newtonsoft.Json;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili.UWP.Controls
{
    public sealed partial class MiniVideoDialog : ContentDialog
    {
        public MiniVideoDialog()
        {
            this.InitializeComponent();
          
        }
        public  void ShowAsync(string vid)
        {
            this.ShowAsync();
            LoadMiniVideo(vid);
        }
        private async void LoadMiniVideo(string vid)
        {
            try
            {

                pr_load.Visibility = Visibility.Visible;
                string url = string.Format("http://api.vc.bilibili.com/clip/v1/video/detail?access_key={0}&appkey={1}&build=434000&mobi_app=android&need_playurl=1&platform=android&src=master&trace_id=20170204152000022&version=4.34.0.434000&video_id={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, vid);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResults(new Uri(url));
                LiveVideoModel m = JsonConvert.DeserializeObject<LiveVideoModel>(results.Replace("default", "_default"));
                if (m.code == 0)
                {
                    this.DataContext = m.data;
                }
                else
                {
                    Utils.ShowMessageToast(m.message, 3000);
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
                    Utils.ShowMessageToast("无法读取小视频发生错误\r\n" + ex.Message, 3000);
                }
            }
            finally
            {

                pr_load.Visibility = Visibility.Collapsed;

            }


        }



        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText((sender.DataContext as LiveVideoModel).item.share_url);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            Utils.ShowMessageToast("已将内容复制到剪切板", 3000);

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), ((sender as Button).DataContext as LiveVideoModel).user.uid);
        }
    }
}
