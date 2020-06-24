using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
            //webView = new WebView(WebViewExecutionMode.SameThread);
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        BiliBili.JSBridge.biliapp _biliapp = new BiliBili.JSBridge.biliapp();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {

                _biliapp.CloseBrowserEvent += _biliapp_CloseBrowserEvent;
                _biliapp.ValidateLoginEvent += _biliapp_ValidateLoginEvent;
                if (e.Parameter is object[])
                {
                    webView.Navigate(new Uri((e.Parameter as object[])[0].ToString()));
                }
                else
                {
                    webView.Navigate(new Uri(e.Parameter.ToString()));
                }

            }
            
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                webView.NavigateToString("");
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatedFrom(e);
        }
        private void _biliapp_CloseBrowserEvent(object sender, string e)
        {
            this.Frame.GoBack();
        }

        private async void _biliapp_ValidateLoginEvent(object sender, string e)
        {
            try
            {
                JObject jObject = JObject.Parse(e);
                if (jObject["access_token"] != null)
                {
                    Account account = new Account();
                    var m= await account.CheckAgainLogin(jObject["access_token"].ToString(), jObject["refresh_token"].ToString(), jObject["expires_in"].ToInt32(),Convert.ToInt64(jObject["mid"]));
                    if (m.success)
                    {
                        Utils.ShowMessageToast("登录成功");
                    }
                    else
                    {
                        Utils.ShowMessageToast("登录失败");
                    }
                    //await UserManage.LoginSucess(jObject["access_token"].ToString());
                }
                else
                {
                    Utils.ShowMessageToast("登录失败");
                }
                this.Frame.GoBack();
            }
            catch (Exception)
            {
            }
            
        }

      



        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            //if (webView.CanGoBack)
            //{
            //    webView.GoBack();
            //}
            //else
            //{
              //  this.Frame.GoBack();
            //}
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private async void webview_WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri==null)
            {
                return;
            }
            if (await MessageCenter.HandelUrl(args.Uri.AbsoluteUri))
            {
                args.Cancel = true;
            }
            try
            {
                this.webView.AddWebAllowedObject("biliapp", _biliapp);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("初始化JSbridge失败", LogType.ERROR,ex);
            }
          



            //string ban = Regex.Match(args.Uri.AbsoluteUri, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            //if (ban.Length != 0)
            //{
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban.Replace("/", ""));
            //    return;
            //}
            //string ban2 = Regex.Match(args.Uri.AbsoluteUri, @"^http://www.bilibili.com/bangumi/i/(.*?)$").Groups[1].Value;
            //if (ban2.Length != 0)
            //{
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban2.Replace("/", ""));
            //    return;
            //}

            ////bilibili://?av=4284663
            //string ban3 = Regex.Match(args.Uri.AbsoluteUri, @"^bilibili://?av=(.*?)$").Groups[1].Value;
            //if (ban3.Length != 0)
            //{
            //    //args.Handled = true;
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), ban3.Replace("/", ""));
            //    //this.Frame.Navigate(typeof(VideoViewPage), ban3.Replace("/", ""));
            //    return;
            //}
            ////https://bangumi.bilibili.com/anime/6499
            //string ban4 = Regex.Match(args.Uri.AbsoluteUri+"/", @"/anime/(.*?)/",RegexOptions.Singleline).Groups[1].Value;
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
            //    MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), live);

            //    return;
            //}

            //string live2 = Regex.Match(args.Uri.AbsoluteUri, @"^http://live.bilibili.com/(.*?)$").Groups[1].Value;
            //if (live2.Length != 0)
            //{
            //    long roomid = 0;
            //    if (long.TryParse(live2.Replace("/",""),out roomid))
            //    {
            //        MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), live2.Replace("/", "").Replace("h5/",""));
            //        return;
            //    }



            //}

            //string minivideo = Regex.Match(args.Uri.AbsoluteUri+"/", @"vc=(.*?)/").Groups[1].Value;
            //if (minivideo.Length != 0)
            //{

            //    //MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), minivideo);
            //    MessageCenter.ShowMiniVideo(minivideo);
            //    return;
            //}


            ////text .Text= args.Uri.AbsoluteUri;
            //webview_progressBar.Visibility = Visibility.Visible;
            //if (Regex.IsMatch(args.Uri.AbsoluteUri, "/video/av(.*)?[/|+](.*)?"))
            //{

            //    string a = Regex.Match(args.Uri.AbsoluteUri, "/video/av(.*)?[/|+](.*)?").Groups[1].Value;
            //    //this.Frame.Navigate(typeof(VideoViewPage), a);
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), a);
            //}


        }

     

        private void webview_WebView_FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            webview_progressBar.Visibility = Visibility.Collapsed;
            
        }

        private void webview_WebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            webview_progressBar.Visibility = Visibility.Collapsed;

        }


        private async void webview_WebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            var re = await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
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

            //string minivideo = Regex.Match(args.Uri.AbsoluteUri + "/", @"vc=(.*?)/").Groups[1].Value;
            //if (minivideo.Length != 0)
            //{
            //    args.Handled = true;
            //    //MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), minivideo);
            //    MessageCenter.ShowMiniVideo(minivideo);



            //    return;
            //}


            //string video = Regex.Match(args.Uri.AbsoluteUri + "?", @"av(.*?)\?").Groups[1].Value;
            //if (video.Length != 0)
            //{
            //    args.Handled = true;
            //    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), video);
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
            //            md.Commands.Add(new UICommand("取消", new UICommandInvokedHandler((e) => {  })));
            //            await md.ShowAsync();

            //            //Utils.ShowMessageToast("已禁止跳转：" + args.Uri.AbsoluteUri + "\r\n请点击右上角使用浏览器打开", 3000);
            //            //text.Text = "已禁止跳转：" + args.Uri.AbsoluteUri;
            //        }
            //    }
            //}

        }

        private void menu_copy_Click(object sender, RoutedEventArgs e)
        {
            DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText(webView.Source.AbsoluteUri);
            Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Clipboard.Flush();
        }

        private async void menu_open_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(webView.Source);
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            webView.Refresh();
        }

        private async void web_UnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            if (args.Uri.AbsoluteUri.Contains("bilibili://"))
            {
                args.Handled = true;

                var re = await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
                if (!re)
                {
                    Utils.ShowMessageToast("不支持打开的链接" + args.Uri.AbsoluteUri);
                }

            }
        }

        private void btn_WebBack_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
            {
                webView.GoBack();
            }
           
        }

        private void btn_WebRefresh_Click(object sender, RoutedEventArgs e)
        {
            webView.Refresh();
        }

        private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            try
            {
                if (args.Uri != null && args.Uri.AbsoluteUri.Contains("bilibili.com"))
                {
                    //微软商店的审核都是傻逼
                    await webView.InvokeScriptAsync("eval", new string[] {
                        "document.getElementById('internationalHeader').style.display='none';document.getElementsByClassName('international-footer')[0].style.display='none';" ,
                    });
                }

                if (args.Uri!=null&&args.Uri.AbsoluteUri.Contains("23344273.aspx"))
                {
                    string appVer = SettingHelper.GetVersion();
                 
                    string systemVer = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily + " " +SystemHelper.SystemVersion();
                    string js = $"document.getElementById('q2').value='{appVer}';document.getElementById('q3').value='{systemVer}';";
                    await webView.InvokeScriptAsync("eval", new string[] { js });
                }


            }
            catch (Exception)
            {
            }
        }
    }
}
