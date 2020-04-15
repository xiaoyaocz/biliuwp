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
        DispatcherTimer timer;
        StartModel m;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();
            if (m.StartType == StartTypes.None && SettingHelper.Get_LoadSplash())
            {
                // await GetResults();

            }
            else
            {
                // await Task.Delay(2000);
                // this.Frame.Navigate(typeof(MainPage), m);
            }




        }
        int i = 1;
        int maxnum = 3;
        private void Timer_Tick(object sender, object e)
        {
            if (i != maxnum)
            {
                i++;
            }
            else
            {
                this.Frame.Navigate(typeof(MainPage), m);

            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            timer.Stop();
            timer = null;
        }
        private void InitializedFrostedGlass(UIElement glassHost)
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            Compositor compositor = hostVisual.Compositor;

            // Create a glass effect, requires Win2D NuGet package
            var glassEffect = new GaussianBlurEffect
            {
                BlurAmount = 20.0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new ArithmeticCompositeEffect
                {
                    MultiplyAmount = 0,
                    Source1Amount = 0.5f,
                    Source2Amount = 0.5f,
                    Source1 = new CompositionEffectSourceParameter("backdropBrush"),
                    Source2 = new ColorSourceEffect
                    {
                        Color = Color.FromArgb(255, 245, 245, 245)
                    }
                }
            };

            //  Create an instance of the effect and set its source to a CompositionBackdropBrush
            var effectFactory = compositor.CreateEffectFactory(glassEffect);
            var backdropBrush = compositor.CreateBackdropBrush();
            var effectBrush = effectFactory.CreateBrush();

            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

            // Create a Visual to contain the frosted glass effect
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = effectBrush;

            // Add the blur as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);

            // Make sure size of glass host and glass visual always stay in sync
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }

        private async Task GetResults()
        {
            try
            {
                string url = "http://app.bilibili.com/x/splash?plat=0&build=414000&channel=master&width=1080&height=1920";
                bool pc = SettingHelper.IsPc();
                if (pc)
                {

                    img.Stretch = Stretch.Uniform;
                    url = "http://app.bilibili.com/x/splash?plat=0&build=414000&channel=master&width=1920&height=1080";
                }

                string Result = await WebClientClass.GetResults(new Uri(url));
                LoadModel obj = JsonConvert.DeserializeObject<LoadModel>(Result);

                if (obj.code == 0)
                {
                    if (obj.data.Count != 0)
                    {
                        var buff = await WebClientClass.GetBuffer(new Uri(obj.data[0].image));
                        BitmapImage bit = new BitmapImage();
                        await bit.SetSourceAsync(buff.AsStream().AsRandomAccessStream());
                        if (!pc)
                        {
                            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                            {
                                var applicationView = ApplicationView.GetForCurrentView();
                                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

                                // StatusBar.GetForCurrentView().HideAsync();
                                StatusBar statusBar = StatusBar.GetForCurrentView();
                                statusBar.ForegroundColor = Colors.Gray;
                                statusBar.BackgroundColor = Color.FromArgb(255, 55, 63, 76);
                                statusBar.BackgroundOpacity = 0;
                            }
                        }
                        else
                        {
                            img_bg.Source = bit;
                            InitializedFrostedGlass(GlassHost);
                        }
                        img.Source = bit;
                        _url = obj.data[0].param;
                        maxnum = 5;
                        //await Task.Delay(3000);
                        //this.Frame.Navigate(typeof(MainPage), m);
                    }
                    else
                    {
                        // await Task.Delay(2000);

                    }
                }
                else
                {
                    // await Task.Delay(2000);
                    //this.Frame.Navigate(typeof(MainPage), m);
                }


            }
            catch (Exception)
            {
                // await Task.Delay(2000);
                //this.Frame.Navigate(typeof(MainPage), m);
            }
            finally
            {


            }

        }
        string _url;
        private void grid_Load_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //url=

            //var anime = Regex.Match(_url, @"anime/(\d{1,9})").Groups;
            //if (anime.Count==2)
            //{
            //    m.StartType = StartTypes.Bangumi;
            //    m.Par1 = anime[1].Value;
            //    return;
            //}
            //var video = Regex.Match(_url, @"av(\d{1,9})").Groups;
            //if (video.Count == 2)
            //{
            //    m.StartType = StartTypes.Video;
            //    m.Par1 = video[1].Value;
            //    return;
            //}
            m.StartType = StartTypes.Web;
            m.Par1 = _url;

        }

        public class LoadModel
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<LoadModel> data { get; set; }
            public int id { get; set; }
            public int animate { get; set; }
            public string image { get; set; }
            public string param { get; set; }
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
