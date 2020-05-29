using BiliBili.UWP.Models;
using BiliBili.UWP.Modules.User;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyCollectPage : Page
    {
        readonly MyFollowVideoVM myFollowVideoVM;
        public MyCollectPage()
        {
            this.InitializeComponent();
            myFollowVideoVM = new MyFollowVideoVM();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New || myFollowVideoVM == null)
            {
                await myFollowVideoVM.LoadFavorite();
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void cb_favbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_favbox.SelectedItem != null)
            {
                //pageNum = 1;
                //MaxPage = 0;
                //fid = (cb_favbox.SelectedItem as GetUserFovBox).fav_box;
                //top_txt_Header.Text = (cb_favbox.SelectedItem as GetUserFovBox).name;
                //User_ListView_FavouriteVideo.Items.Clear();
                //GetFavouriteBoxVideo();

                myFollowVideoVM.Refresh();
            }
        }



        private void sw_Select_Checked(object sender, RoutedEventArgs e)
        {
            User_ListView_FavouriteVideo.IsItemClickEnabled = false;
            User_ListView_FavouriteVideo.SelectionMode = ListViewSelectionMode.Multiple;
        }

        private void sw_Select_Unchecked(object sender, RoutedEventArgs e)
        {
            User_ListView_FavouriteVideo.IsItemClickEnabled = true;
            User_ListView_FavouriteVideo.SelectionMode = ListViewSelectionMode.None;
        }


        private void Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), (e.ClickedItem as FavoriteInfoVideoItemModel).id);
        }

        private async void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            foreach (FavoriteInfoVideoItemModel item in User_ListView_FavouriteVideo.SelectedItems)
            {
               await myFollowVideoVM.RemoveFavoriteVideo(item);
            }
        }

        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var item= (sender as MenuFlyoutItem).DataContext as FavoriteInfoVideoItemModel;
            await myFollowVideoVM.RemoveFavoriteVideo(item);
        }
    }
}
