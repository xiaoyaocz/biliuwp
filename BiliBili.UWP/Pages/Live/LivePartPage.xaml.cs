using BiliBili.UWP.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class LivePartPage : Page
    {
        LiveArea liveArea;
        public LivePartPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            liveArea =new LiveArea();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode== NavigationMode.New && pivot.ItemsSource==null)
            {
                await LoadData();
            }
        }
        private async Task LoadData()
        {
            prLoad.Visibility = Visibility.Visible;
               var data = await liveArea.GetAreaList();
            if (data.success)
            {
                pivot.ItemsSource = data.data;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            prLoad.Visibility = Visibility.Collapsed;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.Back)
            {
                pivot.ItemsSource = null;
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as AreaListItem;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LivePartInfoPage), new object[] {
                   item.parent_id,
                   item.id,
                   item.name
            });
        }
    }
}
