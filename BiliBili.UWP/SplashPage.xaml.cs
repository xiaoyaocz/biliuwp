using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.ViewManagement;
using BiliBili.UWP.Helper;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Microsoft.Graphics.Canvas.Effects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;
using BiliBili.UWP.Modules;
using Microsoft.Toolkit.Uwp.Helpers;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SplashPage : Page
    {
        public SplashPage()
        {
            this.InitializeComponent();
            var bg = new Color() { R = 233, G = 233, B = 233 };
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                // StatusBar.GetForCurrentView().HideAsync();
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.Black;
                statusBar.BackgroundColor = bg;
                statusBar.BackgroundOpacity = 100;
            }

            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = bg;
            titleBar.ForegroundColor = Colors.Black;//Colors.White纯白用不了。。。
            titleBar.ButtonHoverBackgroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = bg;
            titleBar.ButtonForegroundColor = Color.FromArgb(255, 254, 254, 254);
            titleBar.InactiveBackgroundColor = bg;
            titleBar.ButtonInactiveBackgroundColor = bg;
        }
        
        StartModel m;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            #region
            try
            {
                //注册后台任务
                RegisterBackgroundTask();
                //读取已下载的文件
                DownloadHelper2.LoadDowned();
                //加载分区
                ApiHelper.SetRegions();
                //加载直播头衔
                LiveRoom.GetTitleItems();
                //ApiHelper.SetEmojis();
            }
            catch (Exception)
            {
            }
            #endregion

            m = e.Parameter as StartModel;
            await Task.Delay(1000);
            this.Frame.Navigate(typeof(MainPage), m);
           

        }

        #region 后台任务注册

        private void RegisterBackgroundTask()
        {
            var task = BackgroundTaskHelper.Register(typeof(BiliBili.Background.BackgroundTask), new TimeTrigger(15, true),true,true,null);
            task.Progress += TaskOnProgress;
            task.Completed += TaskOnCompleted;
        }

        private void TaskOnProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            Debug.WriteLine($"Background {sender.Name} TaskOnProgress.");
        }

        private void TaskOnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine($"Background {sender.Name} TaskOnCompleted.");
        }

        #endregion


    }
}
