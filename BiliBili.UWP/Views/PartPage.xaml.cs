using BiliBili.UWP.Pages;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PartPage : Page
    {
        public PartPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.Frame.Name == "bg_Frame")
            {
                g.Background = null;
            }
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (int.Parse((sender as HyperlinkButton).Tag.ToString()))
            {
                case 0:
                    //pivot_Home.SelectedIndex = 0;
                    this.Frame.GoBack();
                    MessageCenter.SendNavigateTo(NavigateMode.Home, typeof(LiveV2Page));
                    break;
                case 1:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.bangumi);
                    //infoFrame.Navigate(typeof(PartPage), );
                    break;
                case 2:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.douga);
                  
                    break;
                case 3:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.music);
                    
                    break;
                case 4:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.dance);
                    //infoFrame.Navigate(typeof(PartPage), Parts.dance);
                    break;
                case 5:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.technology);
                   // infoFrame.Navigate(typeof(PartPage), Parts.technology);
                    break;
                case 6:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.game);
                    //infoFrame.Navigate(typeof(PartPage), Parts.game);
                    break;
                case 7:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.kichiku);
                    //infoFrame.Navigate(typeof(PartPage), Parts.kichiku);
                    break;
                case 8:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.ent);
                    //infoFrame.Navigate(typeof(PartPage), Parts.ent);
                    break;
                case 9:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.movie);
                    //infoFrame.Navigate(typeof(PartPage), Parts.movie);
                    break;
                case 10:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.tv);
                    //infoFrame.Navigate(typeof(PartPage), Parts.tv);
                    break;
                case 11:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.fashion);
                    //infoFrame.Navigate(typeof(PartPage), Parts.fashion);
                    break;
                case 12:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.life);
                    //infoFrame.Navigate(typeof(PartPage), Parts.life);
                    break;
                case 13:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.ad);
                    //infoFrame.Navigate(typeof(PartPage), Parts.ad);
                    break;
                case 167:
                    MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(PartsPage), Parts.cn);
                    //infoFrame.Navigate(typeof(PartPage), Parts.ad);
                    break;
                default:
                    break;
            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
