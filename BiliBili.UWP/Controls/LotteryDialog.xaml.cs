using System;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili.UWP.Controls
{
	public sealed partial class LotteryDialog : ContentDialog
	{
		public LotteryDialog(string id)
		{
			this.InitializeComponent();
			web.Source = new Uri($"https://t.bilibili.com/lottery/h5/index/#/result?business_id={id}&business_type=1&isWeb=1");
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
		}
	}
}