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

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalHistoryPage : Page
    {
        public LocalHistoryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();

            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.New)
            {
                cb_select.SelectedIndex = -1;
                cb_select.SelectedIndex = 0;
            }
       
        }

        private void cb_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_select.SelectedIndex==-1)
            {
                return;
            }
            var ls = SqlHelper.GetHistoryList(cb_select.SelectedIndex);
            list.ItemsSource = ls;
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoViewPage),new object[] { (e.ClickedItem as HistoryClass)._aid});
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            SqlHelper.ClearHistory();
            var ls = SqlHelper.GetHistoryList(cb_select.SelectedIndex);
            list.ItemsSource = ls;
        }
    }
}
