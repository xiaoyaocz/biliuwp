using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.FindMore
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ArticleContentPage : Page
    {
        public ArticleContentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
      
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter!=null)
            {
                var url= (e.Parameter as object[])[0].ToString();
                url = url.Replace("www.bilibili.com/read/cv", "www.bilibili.com/read/mobile/");
                if (!url.Contains("bilibili.com"))
                {
                    url = "https://www.bilibili.com/read/mobile/" + url;
                }

                web.Navigate(new Uri(url));
            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private async void web_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            txt_Header.Text = web.DocumentTitle;
            pr_Load.Visibility = Visibility.Collapsed;
            try
            {
                if (web.Source.AbsoluteUri.Contains("read/app")||web.Source.AbsoluteUri.Contains("read/mobile"))
                {
                    var forecolor = (txt_color.Foreground as SolidColorBrush).Color;

                    //var color = (grid_bg.Background as SolidColorBrush).Color;
                    //string bgcolor = "#" + color.R.ToString("X") + color.G.ToString("X") + color.B.ToString("X");
                    //string focolor = "#" + forecolor.R.ToString("X") + forecolor.G.ToString("X") + forecolor.B.ToString("X");

                    string js = "document.getElementsByClassName('h5-download-bar')[0].style.display='none';document.getElementsByClassName('bili-nav-bar')[0].style.display='none';document.getElementsByClassName('top-holder')[0].style.display='none';";

                    //js += "$('.page-container').css('background-color','" + bgcolor + "');";
                    //js += "$('.article-holder').css('color','" + focolor + "');";
                    //js += "$('.title').css('color','" + focolor + "');";
                    //js += "$('.title').css('color','" + focolor + "');";
                    //js += "$('.comment-text').css('color','" + focolor + "')";
                    await web.InvokeScriptAsync("eval", new string[] { js });
                }
                
            }
            catch (Exception)
            {

                //throw;
            }

        }

        private async void web_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri.AbsoluteUri.Contains("read/cv"))
            {
                args.Cancel = true;
                return;
            }
            if (!args.Uri.AbsoluteUri.Contains("read/"))
            {
                var content=await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
                if (content)
                {
                    args.Cancel = true;
                }
                return;
            }

            txt_Header.Text = "专栏";
            pr_Load.Visibility = Visibility.Visible;
        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            var url= web.Source.AbsoluteUri;
            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText(url);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); 
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            Utils.ShowMessageToast("已将地址复制到剪切板", 3000);

        }

        private  async void web_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {

            var re = await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
            args.Handled = true;
            if (!re)
            {
                
                var md = new MessageDialog("是否调用外部浏览器打开此链接？");
                md.Commands.Add(new UICommand("确定", new UICommandInvokedHandler(async (e) => { await Windows.System.Launcher.LaunchUriAsync(args.Uri); })));
                md.Commands.Add(new UICommand("取消", new UICommandInvokedHandler((e) => { })));
                await md.ShowAsync();

            }

            //string ban = Regex.Match(args.Uri.AbsoluteUri, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            //if (ban.Length != 0)
            //{
            //    args.Handled = true;
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban.Replace("/", ""));
            //    // this.Frame.Navigate(typeof(BanInfoPage), ban.Replace("/", ""));
            //    return;
            //}
            //string ban2 = Regex.Match(args.Uri.AbsoluteUri, @"^http://www.bilibili.com/bangumi/i/(.*?)$").Groups[1].Value;
            //if (ban2.Length != 0)
            //{
            //    args.Handled = true;
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban2.Replace("/", ""));
            //    //this.Frame.Navigate(typeof(BanInfoPage), ban2.Replace("/", ""));
            //    return;
            //}
            //string ban3 = Regex.Match(args.Uri.AbsoluteUri, @"^bilibili://?av=(.*?)$").Groups[1].Value;
            //if (ban3.Length != 0)
            //{
            //    args.Handled = true;
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), ban3.Replace("/", ""));
            //    //this.Frame.Navigate(typeof(VideoViewPage), ban3.Replace("/", ""));
            //    return;
            //}

            //string ban4 = Regex.Match(args.Uri.AbsoluteUri, @"^bilibili.com/anime/(.*?)$").Groups[1].Value;
            //if (ban4.Length != 0)
            //{
            //    //args.Handled = true;
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban4.Replace("/", ""));
            //    // this.Frame.Navigate(typeof(BanInfoPage), ban2.Replace("/", ""));
            //    return;
            //}
            //string live = Regex.Match(args.Uri.AbsoluteUri, @"^bilibili://live/(.*?)$").Groups[1].Value;
            //if (live.Length != 0)
            //{
            //    args.Handled = true;
            //    MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), live);

            //    return;
            //}


            ////乱写一通的正则
            ////正则真的真的真的不会啊- -
            //if (Regex.IsMatch(args.Uri.AbsoluteUri, "/video/av(.*)?[/|+](.*)?"))
            //{
            //    args.Handled = true;

            //    string a = Regex.Match(args.Uri.AbsoluteUri, "/video/av(.*)?[/|+](.*)?").Groups[1].Value;
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), a);
            //    //this.Frame.Navigate(typeof(VideoViewPage), a);
            //}
            //else
            //{
            //    if (Regex.IsMatch(args.Uri.AbsoluteUri + "+", "/video/av(.*)[/|+]"))
            //    {
            //        args.Handled = true;
            //        string a = Regex.Match(args.Uri.AbsoluteUri + "+", "/video/av(.*)[/|+]").Groups[1].Value;
            //        //this.Frame.Navigate(typeof(VideoViewPage), a);
            //        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), a);
            //    }
            //    else
            //    {
            //        if (Regex.IsMatch(args.Uri.AbsoluteUri, @"live.bilibili.com/(.*?)"))
            //        {
            //            args.Handled = true;
            //            string a = Regex.Match(args.Uri.AbsoluteUri + "a", "live.bilibili.com/(.*?)a").Groups[1].Value;
            //            // livePlayVideo(a);
            //            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), a.Replace("h5/", ""));
            //        }
            //        else
            //        {
            //            args.Handled = true;

            //            var md = new MessageDialog("是否调用外部浏览器打开此链接？");
            //            md.Commands.Add(new UICommand("确定", new UICommandInvokedHandler(async (e) => { await Windows.System.Launcher.LaunchUriAsync(args.Uri); })));
            //            md.Commands.Add(new UICommand("取消", new UICommandInvokedHandler((e) => { })));
            //            await md.ShowAsync();

            //            //Utils.ShowMessageToast("已禁止跳转：" + args.Uri.AbsoluteUri + "\r\n请点击右上角使用浏览器打开", 3000);
            //            //text.Text = "已禁止跳转：" + args.Uri.AbsoluteUri;
            //        }
            //    }
            //}
        }

        private async void web_UnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            if (args.Uri.AbsoluteUri.Contains("bilibili://")&& !args.Uri.AbsoluteUri.Contains("article"))
            {
                args.Handled = true;
                var re = await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
                if (!re)
                {
                    Utils.ShowMessageToast("不支持打开的链接"+ args.Uri.AbsoluteUri);
                }
            }
            args.Handled = true;

        }


    }
}
