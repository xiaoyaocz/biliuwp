using BiliBili.UWP.Controls;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Pages;
using BiliBili.UWP.Pages.FindMore;
using BiliBili.UWP.Pages.Live;
using BiliBili.UWP.Pages.Music;
using BiliBili.UWP.Pages.User;
using BiliBili.UWP.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BiliBili.UWP
{
    public enum NavigateMode
    {
        Main,
        Info,
        Home,
        Play,
        Bg
    }
    public delegate void MessageHandel(object par, params object[] par1);
    public delegate void NavigateHandel(Type page, params object[] par);
    public delegate void LoginedHandel();
    public delegate void ChangeBgHandel();
    public delegate void ShowOrHideBarHandel(bool show);
    public static class MessageCenter
    {
        public static event EventHandler<object> HasMessaged;
        public static event MessageHandel ChanageThemeEvent;
        public static event LoginedHandel Logined;
        public static event ChangeBgHandel ChangeBg;

        public static event EventHandler HideAdEvent;
        public static void SendHideAd()
        {
            HideAdEvent?.Invoke(null,null);
        }

        public static void SendMessage(object par)
        {
            if (HasMessaged!=null)
            {
                HasMessaged(null, par);
            }
        }

        public static void SendLogined()
        {
            Logined();
        }
        public static void SendChangedBg()
        {
            ChangeBg();
        }

        public static void SendChanageThemeEvent(object par, params object[] par1)
        {
            ChanageThemeEvent(par, par1);
        }


        public static event NavigateHandel InfoNavigateToEvent;
        public static event NavigateHandel PlayNavigateToEvent;
        public static event NavigateHandel MianNavigateToEvent;
        public static event NavigateHandel HomeNavigateToEvent;
        public static event NavigateHandel BgNavigateToEvent;
        public async static void SendNavigateTo(NavigateMode mode, Type page, params object[] par)
        {
            
            switch (mode)
            {
                case NavigateMode.Main:
                    MianNavigateToEvent(page, par);
                    break;
                case NavigateMode.Info:
                    if (page.FullName.Contains("WebPage") &&await HandelUrl(par[0].ToString()))
                    {
                        return;
                    }
                    InfoNavigateToEvent(page, par);
                    break;
                case NavigateMode.Play:
                    if (!page.FullName.Contains("MusicMiniPlayerPage"))
                    {
                        MusicHelper.Pause();
                    }
                    //&&SettingHelper.IsPc()
                    if (page==typeof(LiveRoomPage))
                    {
                        PlayNavigateToEvent(typeof(LiveRoomPC), par);
                        return;
                    }

                    PlayNavigateToEvent(page, par);
                    break;
                case NavigateMode.Home:
                    HomeNavigateToEvent(page, par);
                    break;
                case NavigateMode.Bg:
                    BgNavigateToEvent(page, par);
                    break;
                default:
                    break;
            }

        }


     

        /// <summary>
        ///统一处理Url
        /// </summary>
        /// <param name="par"></param>
        public async static Task<bool> HandelUrl(string url)
        {
            /*
             * 视频
             * https://www.bilibili.com/video/av3905642
             * https://m.bilibili.com/video/av3905642.html
             * https://www.bilibili.com/playlist/video/pl688?aid=19827477
             * bilibili://video/19239064
             * bilibili://?av=4284663
             * https://m.bilibili.com/playlist/pl733016988?avid=68818070
             */

            var video = Utils.RegexMatch(url.Replace("aid", "av").Replace("/","").Replace("=",""), @"av(\d+)");
            if (video!="")
            {
                InfoNavigateToEvent(typeof(VideoViewPage), video);
                return true;
            }
            video = Utils.RegexMatch(url, @"bilibili://video/(\d+)");
            if (video != "")
            {
                InfoNavigateToEvent(typeof(VideoViewPage), video);
                return true;
            }
            video = Utils.RegexMatch(url, @"avid=(\d+)");
            if (video != "")
            {
                InfoNavigateToEvent(typeof(VideoViewPage), video);
                return true;
            }

            /*
             * 视频BV号
             * https://www.bilibili.com/video/BV1EE411w75R
             */
            var video_bv = Utils.RegexMatch(url, @"[Bb][Vv]([a-zA-Z0-9]{5,})");
            if (video_bv != "")
            {
                InfoNavigateToEvent(typeof(VideoViewPage), video_bv);
                return true;
            }


            /* 
             * 番剧/影视
             * https://bangumi.bilibili.com/anime/21680
             * https://www.bilibili.com/bangumi/play/ss21715
             * https://www.bilibili.com/bangumi/play/ep150706
             * https://m.bilibili.com/bangumi/play/ep150706
             * http://m.bilibili.com/bangumi/play/ss21715
             * bilibili://bangumi/season/21715
             * https://bangumi.bilibili.com/movie/12364
             */

            var bangumi = Utils.RegexMatch(url.Replace("movie","ss").Replace("anime", "ss").Replace("season", "ss").Replace("/",""), @"ss(\d+)");
            if (bangumi != "")
            {
                InfoNavigateToEvent(typeof(BanInfoPage), bangumi);
                return true;
            }
            bangumi = Utils.RegexMatch(url, @"ep(\d+)");
            if (bangumi != "")
            {
                InfoNavigateToEvent(typeof(BanInfoPage),await Utils.BangumiEpidToSid(bangumi));
                return true;
            }


            /*
             * 点评
             * https://www.bilibili.com/bangumi/media/md11592/
             * https://bangumi.bilibili.com/review/media/11592
             * bilibili://pgc/review/11592
             */

            var review = Utils.RegexMatch(url.Replace("media", "md").Replace("review", "md").Replace("/", ""), @"md(\d+)");
            if (review != "")
            {
                //InfoNavigateToEvent(typeof(BanInfoPage), review);
                await new Windows.UI.Popups.MessageDialog("请求打开点评"+ review).ShowAsync();
                return true;
            }



            /*
            * 直播
            * http://live.bilibili.com/live/5619438.html
            * http://live.bilibili.com/h5/5619438
            * http://live.bilibili.com/5619438
            * bilibili://live/5619438
            */

            var live = Utils.RegexMatch(url.Replace("h5", "live").Replace("live.bilibili.com", "live").Replace("/", ""), @"live(\d+)");
            if (live != "")
            {
                if (!SettingHelper.IsPc())
                {
                    PlayNavigateToEvent(typeof(LiveRoomPage), live);
                }
                else
                {
                    PlayNavigateToEvent(typeof(LiveRoomPC), live);
                }
                return true;
            }

            /*
             * 小视频
             * http://vc.bilibili.com/mobile/detail?vc=1399466&bilifrom=1
             * http://vc.bilibili.com/video/1357956
             * bilibili://clip/1399466
             */

            //var clip = Utils.RegexMatch(url.Replace("vc=", "clip").Replace("vc.bilibili.com/video", "clip").Replace("/", ""), @"clip(\d+)");
            //if (clip != "")
            //{
            //    MiniVideoDialog miniVideoDialog = new MiniVideoDialog();
            //    miniVideoDialog.ShowAsync(clip);
            //    return true;
            //}


            /*
            * 专栏
            * http://www.bilibili.com/read/cv242568
            * https://www.bilibili.com/read/mobile/242568
            * bilibili://article/242568
            */

            var article = Utils.RegexMatch(url.Replace("read/mobile/", "article").Replace("read/cv", "article").Replace("/", ""), @"article(\d+)");
            if (article != "")
            {
                InfoNavigateToEvent(typeof(ArticleContentPage), "https://www.bilibili.com/read/app/" + article);
                return true;
            }


            /*
             * 音频
             * https://m.bilibili.com/audio/au247991
             * bilibili://music/detail/247991
             */

            var music = Utils.RegexMatch(url.Replace("music/detail/", "au").Replace("/", ""), @"au(\d+)");
            if (music != "")
            {
                InfoNavigateToEvent(typeof(MusicInfoPage), music);
                return true;
            }
            /*
             * 歌单
             * https://m.bilibili.com/audio/am78723
             * bilibili://music/menu/detail/78723
             */

            var musicmenu = Utils.RegexMatch(url.Replace("menu/detail/", "am").Replace("/", ""), @"am(\d+)");
            if (musicmenu != "")
            {
                InfoNavigateToEvent(typeof(MusicMenuPage), musicmenu);
                return true;
            }


            /*
             * 相簿及动态
             * http://h.bilibili.com/ywh/h5/2403422
             * http://h.bilibili.com/2403422
             * bilibili://album/2403422
             * https://t.bilibili.com/84935538081511530
             * bilibili://following/detail/314560419758546547
             */
            var album = Utils.RegexMatch(url.Replace("bilibili://following/detail/", "album").Replace("h.bilibili.com/ywh/h5/", "album").Replace("h.bilibili.com", "album").Replace("t.bilibili.com", "album").Replace("/", ""), @"album(\d+)");
            if (album != "")
            {
                InfoNavigateToEvent(typeof(DynamicInfoPage),  album);
                return true;
            }


            /*
            * 用户中心
            * http://space.bilibili.com/7251681
            * https://m.bilibili.com/space/7251681
            * https://space.bilibili.com/1360010
            * bilibili://author/2622476
            */
            var user = Utils.RegexMatch(url.Replace("space.bilibili.com", "space").Replace("author", "space").Replace("/", ""), @"space(\d+)");
            if (user != "")
            {
                InfoNavigateToEvent(typeof(UserCenterPage), user);
                return true;
            }
            /*
            * 话题/频道
            * https://www.bilibili.com/tag/7868838/feed
            * bilibili://tag/0/?name=bilibili%e5%a5%bd%e4%b9%a1%e9%9f%b3
            */
            var topic = Utils.RegexMatch(url, @"tag/(.*?)/feed");
            if (topic != "")
            {
                InfoNavigateToEvent(typeof(DynamicTopicPage), new object[] { "", topic });
                return true;
            }
            var topic1 = Utils.RegexMatch(url+"/", @"tag/.*?/\?name=(.*?)/");
            if (topic1 != "")
            {
                var data = Uri.UnescapeDataString(topic1);
                InfoNavigateToEvent(typeof(DynamicTopicPage), new object[] { data, "" });
                return true;
            }


            /*
             * 播单
             * https://www.bilibili.com/playlist/detail/pl792
             */


            /*
             * 投稿
             * bilibili://uper/user_center/add_archive/
             */
            var add_archive = url.Contains("/add_archive");
            if (add_archive)
            {
                //InfoNavigateToEvent(typeof(DynamicTopicPage), new object[] { "", topic });
                InfoNavigateToEvent(typeof(WebPage), new object[] { "https://member.bilibili.com/v2#/upload/video/frame" });
                return true;
            }

            /*
             * 我的追番
             * bilibili://main/favorite?tab=bangumi&fav_sub_tab=watching&from=21
             */
            if (url.Contains("favorite?tab=bangumi"))
            {
                InfoNavigateToEvent(typeof(FollowSeasonPage), SeasonType.bangumi);
                return true;
            }

            /*
             * 赛事
             * bilibili://pegasus/channel/v2/9222?tab=5709
             */
            if (url.Contains("bilibili://pegasus/channel/v2/9222"))
            {
                InfoNavigateToEvent(typeof(WebPage), new object[] { "https://www.bilibili.com/v/game/match" });
                return true;
            }

            return false;

        }


        public static event ShowOrHideBarHandel ShowOrHideBarEvent;
        public static void ShowOrHideBar(bool show)
        {
            ShowOrHideBarEvent(show);
        }


        public async static void OpenNewWindow(Type page, params object[] par)
        {

            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
               
                Frame frame = new Frame();
                frame.Navigate(page, par);
                Window.Current.Content = frame;
          
                Window.Current.Activate();
               
                newViewId = ApplicationView.GetForCurrentView().Id;
                ChangeTheme(frame);
                ChangeTitbarColor(ApplicationView.GetForCurrentView());

                var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width, bounds.Height);
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
               
                ApplicationView.GetForCurrentView().Consolidated += (sender, args) =>
                {
                    frame.Navigate(typeof(BlankPage));
                    CoreWindow.GetForCurrentThread().Close();
                };
            });

            //ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay)
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        private static void ChangeTheme(Frame f)
        {
            string ThemeName = SettingHelper.Get_Theme();
            if (ThemeName == "Dark")
            {
                f.RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                ResourceDictionary newDictionary = new ResourceDictionary();
                newDictionary.Source = new Uri($"ms-appx:///Theme/{ThemeName}Theme.xaml", UriKind.RelativeOrAbsolute);
                Application.Current.Resources.ThemeDictionaries["Light"] = newDictionary;
                f.RequestedTheme = ElementTheme.Dark;
                f.RequestedTheme = ElementTheme.Light;
            }
        }
        private static void ChangeTitbarColor(ApplicationView v)
        {
            var titleBar = v.TitleBar;
            titleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["Bili-ForeColor"]).Color;
            titleBar.ForegroundColor = Color.FromArgb(255, 254, 254, 254);//Colors.White纯白用不了。。。
            titleBar.ButtonHoverBackgroundColor = ((SolidColorBrush)Application.Current.Resources["Bili-ForeColor-Dark"]).Color;
            titleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["Bili-ForeColor"]).Color;
            titleBar.ButtonForegroundColor = Color.FromArgb(255, 254, 254, 254);
            titleBar.InactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["Bili-ForeColor"]).Color;
            titleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["Bili-ForeColor"]).Color;
        }

        public static event EventHandler<string> NetworkError;
        public static void SendNetworkError(string msg)
        {
            NetworkError?.Invoke(null, msg);
        }
        public static event EventHandler<Exception> ShowError;
        public static void SendShowError(Exception ex)
        {
            ShowError?.Invoke(null, ex);
        }
    }
}
