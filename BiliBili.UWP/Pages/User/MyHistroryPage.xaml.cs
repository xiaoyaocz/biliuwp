using BiliBili.UWP.Models;
using Newtonsoft.Json;
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
    public sealed partial class MyHistroryPage : Page
    {
        public MyHistroryPage()
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
        private int pageNum_His = 1;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await Task.Delay(200);
            if (e.NavigationMode == NavigationMode.New)
            {
                pageNum_His = 1;
                User_ListView_History.Items.Clear();
                GetHistoryInfo();
            }
        }
        private void User_ListView_History_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), ((GetHistoryModel)e.ClickedItem).aid);
        }
        bool More = true;
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if ((sender as ScrollViewer).VerticalOffset == (sender as ScrollViewer).ScrollableHeight)
            {
                if (More)
                {
                    GetHistoryInfo();
                }
            }
        }

        private async void GetHistoryInfo()
        {
            try
            {
                More = false;
                pro_Load.Visibility = Visibility.Visible;


                List<GetHistoryModel> lsModel = await GetHistory(pageNum_His);
                if (lsModel != null)
                {
                    foreach (GetHistoryModel item in lsModel)
                    {
                        User_ListView_History.Items.Add(item);
                    }
                }
                else
                {
                    Utils.ShowMessageToast("加载完了...", 3000);
                }
                pageNum_His++;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
            finally
            {
                pro_Load.Visibility = Visibility.Collapsed;
                More = true;
            }

        }
        public async Task<List<GetHistoryModel>> GetHistory(int PageNum)
        {

            try
            {
                string url = string.Format("http://api.bilibili.com/x/v2/history?pn={0}&ps=30&jsonp=json", PageNum);

                string results = await WebClientClass.GetResults(new Uri(url));
                //一层
                GetHistoryModel model = JsonConvert.DeserializeObject<GetHistoryModel>(results);
                if (model.data == null)
                {
                    return null;
                }
                else
                {
                    List<GetHistoryModel> lsModel = JsonConvert.DeserializeObject<List<GetHistoryModel>>(model.data.ToString());
                    return lsModel;
                }

            }
            catch (Exception)
            {
                return null;
            }

        }

        private async void btn_ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog("确定要清除全部历史记录吗？");
            messageDialog.Commands.Add(new UICommand("确认", async (value) =>
            {
                try
                {
                    string url = string.Format("http://api.bilibili.com/x/v2/history/clear?_device=android&access_key={0}&appkey={1}&build=421000&mobi_app=android&platform=android", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey);
                    url += "&sign=" + ApiHelper.GetSign(url);
                    string results = await WebClientClass.PostResults(new Uri(url), "");
                    User_ListView_History.Items.Clear();
                    pageNum_His = 1;
                    Utils.ShowMessageToast("清除完成", 3000);
                }
                catch (Exception)
                {
                    Utils.ShowMessageToast("清除失败", 3000);
                }
            }));
            messageDialog.Commands.Add(new UICommand("取消"));
            await messageDialog.ShowAsync();

            

        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (More)
            {
                GetHistoryInfo();
            }
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
