using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.Web.Http;
using Windows.UI.Core;
using BiliBili.UWP.Modules;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RankPage : Page
    {
        readonly RankVM rankVM;
        public RankPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            rankVM = new RankVM();
            cbType.SelectionChanged += CbType_SelectionChanged;
            cbDays.SelectionChanged += CbDays_SelectionChanged;
        }

        private async void CbDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDays.SelectedItem == null || pivot.SelectedItem == null)
                return;
            foreach (var item in rankVM.RegionItems)
            {
                item.Items = null;
            }
            await rankVM.LoadRankDetail(pivot.SelectedItem as RankRegionModel);
        }

        private async void CbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbType.SelectedItem == null|| pivot.SelectedItem==null)
                return;
            foreach (var item in rankVM.RegionItems)
            {
                item.Items = null;
            }
            await rankVM.LoadRankDetail(pivot.SelectedItem as RankRegionModel);
        }

        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //if (e.NavigationMode == NavigationMode.New)
            //{
                
            //}
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.Back)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private void btn_back_Click(object sender, RoutedEventArgs e)
        {

            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            
        }
        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == null)
            {
                return;
            }
            var data = pivot.SelectedItem as RankRegionModel;
            if (data.Items == null || data.Items.Count == 0)
            {
                await rankVM.LoadRankDetail(data);
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as RankItemModel;
            MessageCenter.SendNavigateTo( NavigateMode.Info,typeof(VideoViewPage), item.aid);
        }
    }

   
}
