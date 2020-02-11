using BiliBili.UWP.Helper;
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
    public sealed partial class ErrorDialog : ContentDialog
    {
        Exception exception;
        public ErrorDialog(Exception ex)
        {
            this.InitializeComponent();
            txt_title.Text =ex.HResult+ex.Message;
            if (ex.StackTrace!=null)
            {
                txt_message.Text = ex.StackTrace;
            }
         
            exception = ex;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var data = new
            {
                version = SettingHelper.GetVersion(),
                time = DateTime.Now.ToString(),
                device = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily + " " + SystemHelper.SystemVersion(),
                network = SystemHelper.GetNetWorkType(),
                message = exception.Message + "\r\n\r\n" + exception.StackTrace
            };
            try
            {
                await WebClientClass.PostResultsJson(new Uri("https://api.iliili.cn/api/BiliError"), JsonConvert.SerializeObject(data));
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("发送失败");
            }
            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
