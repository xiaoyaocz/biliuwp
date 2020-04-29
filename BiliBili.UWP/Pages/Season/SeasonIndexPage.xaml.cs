using BiliBili.UWP.Modules.Season;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.Season
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SeasonIndexPage : Page
    {
        private SeasonIndexParameter indexParameter;
        readonly SeasonIndexVM seasonIndexVM;
        public SeasonIndexPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            seasonIndexVM = new SeasonIndexVM();
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter == null)
                {
                    indexParameter = new SeasonIndexParameter();
                }
                else 
                {
                    if (e.Parameter is object[])
                    {
                        indexParameter = (e.Parameter as object[])[0] as SeasonIndexParameter;
                    }
                    else
                    {
                        indexParameter = e.Parameter as SeasonIndexParameter;
                    }
                    
                }

                seasonIndexVM.Parameter = indexParameter;
                await seasonIndexVM.LoadConditions();
                if (seasonIndexVM.Conditions != null)
                {
                    await seasonIndexVM.LoadResult();
                }
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.Back)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private void ListResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SeasonIndexResultItemModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), item.season_id);
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combox = sender as ComboBox;
            if (combox.SelectedItem == null || seasonIndexVM.ConditionsLoading || seasonIndexVM.Loading)
            {
                return;
            }
            seasonIndexVM.Page = 1;
            await seasonIndexVM.LoadResult();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
