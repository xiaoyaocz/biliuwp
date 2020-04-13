using BiliBili.UWP.Helper;
using BiliBili.UWP.Pages;
using BiliBili.UWP.Pages.FindMore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BiliBili.UWP
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();


            this.Suspending += OnSuspending;
            App.Current.UnhandledException += App_UnhandledException;

            this.EnteredBackground += App_EnteredBackground;
            this.LeavingBackground += App_LeavingBackground;
        }
        private void RegisterExceptionHandlingSynchronizationContext()
        {
            ExceptionHandlingSynchronizationContext
                .Register()
                .UnhandledException += SynchronizationContext_UnhandledException;
        }
        private void SynchronizationContext_UnhandledException(object sender, AysncUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                LogHelper.WriteLog("程序运行出现错误", LogType.ERROR,e.Exception);
                Utils.ShowMessageToast("程序出现一个错误，已记录");
                MessageCenter.SendShowError(e.Exception);
            }
            catch (Exception)
            {
            }

            //await new MessageDialog("出现了一个错误:\r\n" + e.Exception.Message + "\r\n" + e.Exception.StackTrace).ShowAsync();
        }
        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                LogHelper.WriteLog("程序运行出现错误", LogType.ERROR,e.Exception);
                Utils.ShowMessageToast("程序出现一个错误，已记录");
                MessageCenter.SendShowError(e.Exception);
            }
            catch (Exception)
            {
            }
            //await new MessageDialog("出现了一个错误:\r\n" + e.Exception.Message+"\r\n"+e.Exception.StackTrace).ShowAsync();

        }

        private void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            SYEngine.Core.Initialize();

            RegisterExceptionHandlingSynchronizationContext();
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;





            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {

                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态


                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {

                ApiHelper.access_key = SettingHelper.Get_Access_key();
                UserManage.access_key = SettingHelper.Get_Access_key();
                var par = new StartModel() { StartType = StartTypes.None };
                if (e.Arguments.Length != 0)
                {
                    var d = e.Arguments.Split(',');
                    if (d.Length > 1)
                    {
                        if (d[0] == "bangumi")
                        {
                            par.StartType = StartTypes.Bangumi;
                            par.Par1 = d[1];
                        }
                        if (d[0] == "live")
                        {
                            par.StartType = StartTypes.Live;
                            par.Par1 = d[1];
                        }
                    }
                    else
                    {
                        par.StartType = StartTypes.Video;
                        par.Par1 = e.Arguments;
                    }
                }

                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(SplashPage), par);
                }
                else
                {
                    if (par.StartType == StartTypes.Video)
                    {
                        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), par.Par1);
                    }
                    if (par.StartType == StartTypes.Bangumi)
                    {
                        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), par.Par1);
                    }
                    if (par.StartType == StartTypes.Live)
                    {
                        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveRoomPage), par.Par1);
                    }

                }

                // 确保当前窗口处于活动状态


            }
            Window.Current.Activate();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {

            RegisterExceptionHandlingSynchronizationContext();

            //bilibili://bangumi/season/21715
            //bilibili://live/5619438
            //bilibili://video/19239064
            //bilibili://clip/1399466
            //bilibili://pgc/review/11592?url_from_h5=1 点评
            //bilibili://article/242568
            //bilibili://music/detail/247991?name=2018%E5%B8%9D%E7%8E%96%E6%BC%94%E5%A5%8F%E8%AE%A1%E5%88%92%EF%BC%9A%E4%BA%A4%E5%93%8D%E7%BB%84%E6%9B%B2%E3%80%8C%E5%90%9B%E3%81%AE%E5%90%8D%E3%81%AF%E3%80%82%E3%80%8D&cover_url=http:%2F%2Fi0.hdslb.com%2Fbfs%2Fmusic%2F9892d28aa5858a571e4a832d314e2136211ad7f7.jpg&from=outer_h5
            //bilibili://album/2403422
            //bilibili://author/2622476
            // bilibili://music/menu/detail/78723
            if (args.Kind == ActivationKind.Protocol)
            {

                Frame rootFrame = Window.Current.Content as Frame;

                StartModel par = new StartModel() { StartType = StartTypes.HandelUri };

                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                par.Par1 = eventArgs.Uri.AbsoluteUri;
                //string video = Regex.Match(eventArgs.Uri.AbsoluteUri, @"video/(\d+)").Groups[1].Value;
                //string video2 = Regex.Match(eventArgs.Uri.AbsoluteUri.Replace("/","").Replace("=",""), @"av(\d+)").Groups[1].Value;
                //string live = Regex.Match(eventArgs.Uri.AbsoluteUri, @"live/(\d+)").Groups[1].Value;
                //string minivideo = Regex.Match(eventArgs.Uri.AbsoluteUri, @"clip/(\d+)").Groups[1].Value;
                //string bangumi = Regex.Match(eventArgs.Uri.AbsoluteUri, @"season/(\d+)").Groups[1].Value;
                //string music = Regex.Match(eventArgs.Uri.AbsoluteUri, @"music/detail/(\d+)").Groups[1].Value;
                //string album = Regex.Match(eventArgs.Uri.AbsoluteUri, @"album/(\d+)").Groups[1].Value;
                //string article = Regex.Match(eventArgs.Uri.AbsoluteUri, @"article/(\d+)").Groups[1].Value;
                //string author = Regex.Match(eventArgs.Uri.AbsoluteUri, @"author/(\d+)").Groups[1].Value;

                if (rootFrame != null)
                {
                    if (!await MessageCenter.HandelUrl(eventArgs.Uri.AbsoluteUri))
                    {
                        ContentDialog contentDialog = new ContentDialog()
                        {
                            PrimaryButtonText = "确定",
                            Title = "不支持跳转的地址"
                        };
                        TextBlock textBlock = new TextBlock()
                        {
                            Text = eventArgs.Uri.AbsoluteUri,
                            IsTextSelectionEnabled = true
                        };
                        contentDialog.Content = textBlock;
                        contentDialog.ShowAsync();
                    }
                }
                else
                {
                    SYEngine.Core.Initialize();
                    // 创建要充当导航上下文的框架，并导航到第一页
                    rootFrame = new Frame();
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    // 将框架放在当前窗口中
                    Window.Current.Content = rootFrame;

                    rootFrame.Navigate(typeof(SplashPage), par);
                    Window.Current.Activate();


                }

                //if (live.Length != 0)
                //{
                //    par.StartType = StartTypes.Live;
                //    par.Par1 = live;
                //    if (rootFrame!=null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), live);
                //        return;
                //    }

                //}
                //if (video.Length != 0)
                //{
                //    par.StartType = StartTypes.Video;
                //    par.Par1 = video;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), video);
                //        return;
                //    }

                //}
                //if (video2.Length != 0)
                //{
                //    par.StartType = StartTypes.Video;
                //    par.Par1 = video2;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), video2);
                //        return;
                //    }

                //}
                //if (minivideo.Length != 0)
                //{
                //    par.StartType = StartTypes.MiniVideo;
                //    par.Par1 = minivideo;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.ShowMiniVideo(minivideo);
                //        return;
                //    }

                //}
                //if (bangumi.Length != 0)
                //{
                //    par.StartType = StartTypes.Bangumi;
                //    par.Par1 = bangumi;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), bangumi);
                //        return;
                //    }

                //}
                //if (album.Length != 0)
                //{
                //    par.StartType = StartTypes.Album;
                //    par.Par1 = album;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicInfoPage), album);
                //        return;
                //    }

                //}
                //if (music.Length != 0)
                //{
                //    par.StartType = StartTypes.Music;
                //    par.Par1 = music;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(MusicInfoPage), music);
                //        return;
                //    }

                //}
                //if (musicmenu.Length != 0)
                //{
                //    par.StartType = StartTypes.Music;
                //    par.Par1 = music;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(MusicInfoPage), music);
                //        return;
                //    }

                //}

                //if (article.Length != 0)
                //{
                //    par.StartType = StartTypes.Article;
                //    par.Par1 = "https://www.bilibili.com/read/app/" + article;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), "https://www.bilibili.com/read/app/" + article);
                //        return;
                //    }

                //}
                //if (author.Length != 0)
                //{
                //    par.StartType = StartTypes.User;
                //    par.Par1 = author;
                //    if (rootFrame != null)
                //    {
                //        MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserInfoPage), author);
                //        return;
                //    }

                //}





            }
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {

            RegisterExceptionHandlingSynchronizationContext();
            StartModel par = new StartModel() { StartType = StartTypes.File, Par3 = args.Files };
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                SYEngine.Core.Initialize();
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(SplashPage), par);
                Window.Current.Activate();
                return;
            }

            rootFrame.Navigate(typeof(SplashPage), par);
            Window.Current.Activate();
        }


    }




}
