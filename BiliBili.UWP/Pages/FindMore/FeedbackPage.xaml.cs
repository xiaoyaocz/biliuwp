using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.FindMore
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class FeedbackPage : Page
	{
		public FeedbackPage()
		{
			this.InitializeComponent();
		}

		private void btn_Back_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.GoBack();
		}
	}
}