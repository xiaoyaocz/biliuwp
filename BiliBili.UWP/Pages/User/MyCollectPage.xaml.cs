using BiliBili.UWP.Models;
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
        public MyCollectPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                await Task.Delay(200);
                List<GetUserFovBox> favList = await GetUserFovBox();
                cb_favbox.ItemsSource = favList;

                if (e.Parameter != null&& !(e.Parameter is object[]))
                {
                    
                    var d = from a in favList where a.fav_box == (string)e.Parameter select a;
                    cb_favbox.SelectedIndex = favList.IndexOf(d.ToList()[0]);
                }
                else
                {
                    cb_favbox.SelectedIndex = 0;
                }

            }
        }
        public async Task<List<GetUserFovBox>> GetUserFovBox()
        {

            try
            {
                string results = await WebClientClass.GetResults(new Uri("http://space.bilibili.com/ajax/fav/getBoxList?mid=" + ApiHelper.GetUserId()));
                //一层
                GetUserFovBox model1 = JsonConvert.DeserializeObject<GetUserFovBox>(results);
                if (model1.status)
                {
                    //二层
                    GetUserFovBox model2 = JsonConvert.DeserializeObject<GetUserFovBox>(model1.data.ToString());
                    //三层
                    List<GetUserFovBox> lsModel = JsonConvert.DeserializeObject<List<GetUserFovBox>>(model2.list.ToString());
                    return lsModel;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<GetFavouriteBoxsVideoModel>> GetFavouriteBoxVideo(string fid, int PageNum)
        {
            //啊啊啊啊，没心情啊，下面代码都是乱写的，啊啊啊啊啊 啊啊啊啊啊啊
           
                try
                {

                    string results = await WebClientClass.GetResults(new Uri("http://space.bilibili.com/ajax/fav/getList?mid=" + ApiHelper.GetUserId() + "&pagesize=20&fid=" + fid + "&tid=0&kw=&pid=" + PageNum+ "&order=fav_time"));
                    //一层
                    GetFavouriteBoxsVideoModel model = JsonConvert.DeserializeObject<GetFavouriteBoxsVideoModel>(results);
                    //二层
                    if (model.status)
                    {
                        GetFavouriteBoxsVideoModel model2 = JsonConvert.DeserializeObject<GetFavouriteBoxsVideoModel>(model.data.ToString());
                        //三层
                        List<GetFavouriteBoxsVideoModel> lsModel = JsonConvert.DeserializeObject<List<GetFavouriteBoxsVideoModel>>(model2.vlist.ToString());
                        List<GetFavouriteBoxsVideoModel> RelsModel = new List<GetFavouriteBoxsVideoModel>();
                        foreach (GetFavouriteBoxsVideoModel item in lsModel)
                        {
                            item.pages = model2.pages;
                            RelsModel.Add(item);
                        }
                        return RelsModel;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return null;
                }

        }

        bool loading = true;
        private int pageNum = 1;
        private int MaxPage = 0;
        private string fid = "";
        private async void GetFavouriteBoxVideo()
        {
            pr_Load.Visibility = Visibility.Visible;
            loading = true;
            try
            {
                
                List<GetFavouriteBoxsVideoModel> lsModel = await GetFavouriteBoxVideo(fid, pageNum);
                if (lsModel != null)
                {
                    if (lsModel.Count==0)
                    {
                        Utils.ShowMessageToast("没有更多内容了...", 3000);
                    }
                    else
                    {
                        foreach (GetFavouriteBoxsVideoModel item in lsModel)
                        {
                            MaxPage = item.pages;
                            User_ListView_FavouriteVideo.Items.Add(item);
                        }
                        //为下一页做准备
                        pageNum++;
                    }
                   

                   
                }
                else
                {
                    Utils.ShowMessageToast("信息获取失败",3000);

                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("读取收藏夹信息失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                loading = false;
            }

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
                pageNum = 1;
                MaxPage = 0;
                fid = (cb_favbox.SelectedItem as GetUserFovBox).fav_box;
                top_txt_Header.Text = (cb_favbox.SelectedItem as GetUserFovBox).name;
                User_ListView_FavouriteVideo.Items.Clear();
                GetFavouriteBoxVideo();
            }
        }

        private void User_ListView_FavouriteVideo_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage),(e.ClickedItem as GetFavouriteBoxsVideoModel).aid);
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!loading)
            {
                GetFavouriteBoxVideo();
            }
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!loading)
                {
                    GetFavouriteBoxVideo();
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            int d = Convert.ToInt32(this.ActualWidth / 400);
            if (d>3)
            {
                d = 3;
            }
            bor_Width.Width = this.ActualWidth / d - 22;
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

        private async void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                foreach (GetFavouriteBoxsVideoModel item in User_ListView_FavouriteVideo.SelectedItems)
                {
                    //string results = await WebClientClass.PostResults(new Uri("http://space.bilibili.com/ajax/fav/mdel"), string.Format("fid={0}&aids={1}", fid, item.aid));

                    Uri ReUri = new Uri("http://api.bilibili.com/x/v2/fav/video/del");

                    string content = string.Format(
                        "access_key={0}&aid={2}&appkey={1}&build=520001&fid={3}&mobi_app=android&platform=android&re_src=90&ts={4}",
                        ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, item.aid, fid, ApiHelper.GetTimeSpan_2
                        );
                    content += "&sign=" + ApiHelper.GetSign(content);
                    string result = await WebClientClass.PostResults(ReUri,
                        content
                     );
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        User_ListView_FavouriteVideo.Items.Remove(item);
                    }
                    else{
                        Utils.ShowMessageToast("取消收藏失败", 2000);
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("删除失败\r\n" + ex.Message).ShowAsync();
            }
        }
    }
}
