using BiliBili.UWP.Helper;
using Newtonsoft.Json;
using System;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili.UWP.Controls
{
	public sealed partial class ErrorDialog : ContentDialog
	{
		private Exception exception;

		public ErrorDialog(Exception ex)
		{
			this.InitializeComponent();
			txt_title.Text = ex.HResult + ex.Message;
			if (ex.StackTrace != null)
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