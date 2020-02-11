using BiliBili.UWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EditProfilePage : Page
    {
        public EditProfilePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           // if (e.NavigationMode == NavigationMode.New)
           // {
                GetProfile();
            //}
        }

        private async void GetProfile()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string Uid = ApiHelper.GetUserId();
                string results = await WebClientClass.GetResults(new Uri(ApiHelper.GetSignWithUrl($"https://app.bilibili.com/x/v2/account/myinfo?access_key={ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}",ApiHelper.AndroidKey)));
                UserModel um = JsonConvert.DeserializeObject<UserModel>(results);
                switch (um.data.sex)
                {
                    case "1":
                        rb_N.IsChecked = true;
                        break;
                    case "2":
                        rb_V.IsChecked = true;
                        break;
                    default:
                        rb_B.IsChecked = true;
                        break;
                }
                dt_Date.Date =DateTime.Parse(um.data.birthday);
                this.DataContext = um.data;
            }
            catch
            {
                Utils.ShowMessageToast("读取用户信息失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sex = "保密";
                if (rb_B.IsChecked.Value)
                {
                    sex = "保密";
                }
                if (rb_N.IsChecked.Value)
                {
                    sex = "男";
                }
                if (rb_V.IsChecked.Value)
                {
                    sex = "女";
                }
                Uri ReUri = new Uri("https://api.bilibili.com/x/member/web/update");
                HttpClient hc = new HttpClient();
                hc.DefaultRequestHeaders.Referer = new Uri("https://account.bilibili.com/site/setting");
                string QuStr = string.Format("uname={0}&sign={1}&sex={2}&birthday={3}", txt_UserName.Text, txt_Sign.Text, sex, dt_Date.Date.Year + "-" + dt_Date.Date.Month + "-" + dt_Date.Date.Day);
                var response = await hc.PostAsync(ReUri, new HttpStringContent(QuStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(result);
                if (json["code"].ToInt32()==0)
                {
                    MessageCenter.SendLogined();
                    Utils.ShowMessageToast("成功更新了你的资料", 3000);
                }
                else
                {
                    Utils.ShowMessageToast((string)json["message"], 3000);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast(ex.Message, 3000);
            }

        }

        private async void btn_Random_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                pr_Load.Visibility = Visibility.Visible;
                Uri uri = new Uri("ms-appx:///Assets/avatar/ic_avatar" + new Random().Next(1, 12) + ".jpg");
                RandomAccessStreamReference streamRef = RandomAccessStreamReference.CreateFromUri(uri);
                Windows.Storage.Streams.Buffer buffer = null;
                using (IRandomAccessStreamWithContentType fileStream = await streamRef.OpenReadAsync())
                {
                    buffer = new Windows.Storage.Streams.Buffer((uint)fileStream.Size);
                    await fileStream.ReadAsync(buffer, (uint)fileStream.Size, InputStreamOptions.None);
                }
                
                Stream stream = WindowsRuntimeBufferExtensions.AsStream(buffer);
                string resutls = "";
               // resutls = await WebClientClass.PostResults(new Uri(string.Format("https://account.bilibili.com/pendant/updateFace?type=jpg&size=s")), stream.AsInputStream(), "https://account.bilibili.com/site/updateface.html?type=face");
               // resutls = await WebClientClass.PostResults(new Uri(string.Format("https://account.bilibili.com/pendant/updateFace?type=jpg&size=m")), stream.AsInputStream(), "https://account.bilibili.com/site/updateface.html?type=face");
                resutls = await WebClientClass.PostResults(new Uri(string.Format("https://account.bilibili.com/pendant/updateFace?type=jpg&size=l")), stream.AsInputStream(), "https://account.bilibili.com/site/updateface.html?type=face");
                JObject obj = JObject.Parse(resutls);
                if (obj["state"].ToString() == "OK")
                {
                    MessageCenter.SendLogined();
                    Utils.ShowMessageToast("头像更换成功", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("头像更换失败", 3000);
                }
               
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("头像更换失败\r\n"+ex.Message, 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                GetProfile();
            }
        }


        private async void btn_Select_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                Uri uri = new Uri("ms-appx:///Assets/avatar/ic_avatar" + new Random().Next(1, 12) + ".jpg");
             
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.CommitButtonText = "选中此文件";
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".gif");
                openPicker.FileTypeFilter.Add(".png");

                // 弹出文件选择窗口
                StorageFile file = await openPicker.PickSingleFileAsync(); // 用户在“文件选择窗口”中完成操作后，会返回对应的 
                if (file==null)
                {
                    Utils.ShowMessageToast("没有选择图片", 3000);
                    return;
                }


                Stream stream = await file.OpenStreamForReadAsync();


                string resutls = "";
                // resutls = await WebClientClass.PostResults(new Uri(string.Format("https://account.bilibili.com/pendant/updateFace?type=jpg&size=s")), stream.AsInputStream(), "https://account.bilibili.com/site/updateface.html?type=face");
                // resutls = await WebClientClass.PostResults(new Uri(string.Format("https://account.bilibili.com/pendant/updateFace?type=jpg&size=m")), stream.AsInputStream(), "https://account.bilibili.com/site/updateface.html?type=face");
                resutls = await WebClientClass.PostResults(new Uri(string.Format("https://account.bilibili.com/pendant/updateFace?type=jpg&size=l")), stream.AsInputStream(), "https://account.bilibili.com/site/updateface.html?type=face");
                JObject obj = JObject.Parse(resutls);
                if (obj["state"].ToString() == "OK")
                {
                    MessageCenter.SendLogined();
                    Utils.ShowMessageToast("头像更换成功", 3000);
                }
                else
                {
                    Utils.ShowMessageToast("头像更换失败", 3000);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("头像更换失败\r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                GetProfile();
            }
        }
    }
}
