using BiliBili.UWP.Controls;
using BiliBili.UWP.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownloadPage : Page
    {
        public DownloadPage()
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

        List<DownloadModel> downlingModel = new List<DownloadModel>();
        List<string> HandleList = new List<string>();
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //if (e.NavigationMode== NavigationMode.New||)
            //{
              
                list_Downing.Items.Clear();
                downlingModel.Clear();
                var d = await DownloadHelper.GetDownList();
             
                if (d.Count>0)
                {
                    txt_NoDown.Visibility = Visibility.Collapsed;
                    List<Task> tasks = new List<Task>();
                    foreach (var model in d)
                    {
                        list_Downing.Items.Add(model);
                        //bool test = HandleList.Contains(model.downOp.Guid.ToString());
                        if (!HandleList.Contains(model.handel.downOp.Guid.ToString()))
                        {
                            // 监视指定的后台下载任务
                            HandleList.Add(model.handel.downOp.Guid.ToString());
                            tasks.Add(HandleDownloadAsync(model));
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                else
                {
                    txt_NoDown.Visibility = Visibility.Visible;
                }
                GetDownOk();
           // }
            
        }
        private async void GetDownOk()
        {
            try
            {
                load_Downed.Visibility = Visibility.Visible;
                list_Downed_New.ItemsSource = await DownloadHelper.GetDownEedList();
                if (list_Downed_New.Items.Count == 0)
                {
                    txt_NoDowned.Visibility = Visibility.Visible;
                }
                else
                {
                    txt_NoDowned.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("无法读取下载完成列表\r\n" + ex.Message).ShowAsync();
            }
            finally
            {
                load_Downed.Visibility = Visibility.Collapsed;
            }
          
            //list_Downed.Items.Clear();
            //var x = await DownloadHelper.GetDownedList();

            //foreach (var item in x)
            //{
            //    try
            //    {
            //        if (item.folderinfo != null && item.folderinfo.thumb != null && item.folderinfo.thumb.Length != 0)
            //        {
            //            BitmapImage img = new BitmapImage();
            //            StorageFile file = await StorageFile.GetFileFromPathAsync(item.folderinfo.thumb);
            //            img.SetSource(await file.OpenReadAsync());
            //            item.img = img;
            //        }
            //    }
            //    catch (Exception)
            //    {
            //    }
            //    list_Downed.Items.Add(item);
            //}
          
        }

        /// 监视指定的后台下载任务
        /// </summary>
        /// <param name="download">后台下载任务</param>
        private async Task HandleDownloadAsync(DownloadModel model)
        {
            try
            {
                DownloadProgress(model.handel.downOp);
                //进度监控
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                await model.handel.downOp.AttachAsync().AsTask(model.handel.cts.Token, progressCallback);

                model.videoinfo.downstatus = true;
                var PartFolder = await StorageFolder.GetFolderFromPathAsync(model.videoinfo.folderPath);
                StorageFile sefile = await PartFolder.CreateFileAsync(model.videoinfo.mid + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(sefile, JsonConvert.SerializeObject(model.videoinfo));
                ////保存任务信息
                ////  StorageFolder folder = ApplicationData.Current.LocalFolder;
                //StorageFolder DowFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("Bili-Down", CreationCollisionOption.OpenIfExists);
                //StorageFile file = await DowFolder.GetFileAsync(model.Guid + ".bili");
                ////用Url编码是因为不支持读取中文名
                ////含会出现：在多字节的目标代码页中，没有此 Unicode 字符可以映射到的字符。错误
                //string path = WebUtility.UrlDecode(await FileIO.ReadTextAsync(file));
                //StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                //StorageFile files = await folder.CreateFileAsync(model.Guid + ".json", CreationCollisionOption.OpenIfExists); //await StorageFile.GetFileFromPathAsync(path+@"\" + model.Guid + ".json");
                //string json = await FileIO.ReadTextAsync(files);
                //DownloadManage.DownModel info = JsonConvert.DeserializeObject<DownloadManage.DownModel>(json);
                //info.downloaded = true;
                //string jsonInfo = JsonConvert.SerializeObject(info);
                //StorageFile fileWrite = await folder.CreateFileAsync(info.Guid + ".json", CreationCollisionOption.ReplaceExisting);
                //await FileIO.WriteTextAsync(fileWrite, jsonInfo);
                ////移除正在监控
                SendToast("《" + model.videoinfo.title + " " + model.videoinfo.partTitle+"》下载完成");
                HandleList.Remove(model.videoinfo.downGUID);
                list_Downing.Items.Remove(model);
                //GetDownOk_New();
                 GetDownOk();
                // list_Downing.Items.Remove(model);
            }
            catch (TaskCanceledException)
            {
                //取消通知
                SendToast("取消任务《"+model.videoinfo.title+" "+model.videoinfo.partTitle+"》");
                list_Downing.Items.Remove(model);
                GetDownOk();
            }
            catch (Exception ex)
            {
                WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                return;
            }
            
        }
      
        private void SendToast(string content)
        {
            if (DownloadHelper.contents.Contains(content))
            {
                return;
            }
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(content));
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
            DownloadHelper.contents.Add(content);

        }


        //进度发生变化时,通知更新UI
        private void DownloadProgress(DownloadOperation op)
        {
            try
            {
                DownloadModel test = null;
                foreach (DownloadModel item in list_Downing.Items)
                {
                    if (item.handel.downOp.Guid == op.Guid)
                    {
                        test = item;
                    }
                }
                if (list_Downing.Items.Contains(test))
                {
                    ((DownloadModel)list_Downing.Items[list_Downing.Items.IndexOf(test)]).handel.Size = op.Progress.BytesReceived.ToString();
                    ((DownloadModel)list_Downing.Items[list_Downing.Items.IndexOf(test)]).handel.Status = op.Progress.Status.ToString();
                    ((DownloadModel)list_Downing.Items[list_Downing.Items.IndexOf(test)]).handel.Progress = ((double)op.Progress.BytesReceived / op.Progress.TotalBytesToReceive) * 100;
                }
            }
            catch (Exception)
            {
                return;
            }

        }



        private void btn_Pause_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                ((sender as AppBarButton).DataContext as DownloadModel).handel.downOp.Pause();
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("操作失败", 2000);
            }

        }

        private void btn_Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ss = ((sender as AppBarButton).DataContext as DownloadModel);
                if (ss.handel.downOp.Progress.Status == BackgroundTransferStatus.PausedByApplication)
                {
                    ss.handel.downOp.Resume();
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("操作失败", 2000);
            }

        }

        private async void btn_Canacel_Click(object sender, RoutedEventArgs e)
        {
            var ss = ((sender as AppBarButton).DataContext as DownloadModel);
            ss.handel.cts.Cancel(false);
            ss.handel.cts.Dispose();
            await DownloadHelper.DeleteFile(ss);
            //try
            //{

            //    StorageFolder DowFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("ZF-Down", CreationCollisionOption.OpenIfExists);
            //    StorageFile file = await DowFolder.GetFileAsync(ss.Guid + ".bili");
            //    //用Url编码是因为不支持读取中文名
            //    //含会出现：在多字节的目标代码页中，没有此 Unicode 字符可以映射到的字符。错误
            //    string path = WebUtility.UrlDecode(await FileIO.ReadTextAsync(file));
            //    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            //    await folder.DeleteAsync(StorageDeleteOption.Default);
            //    await file.DeleteAsync(StorageDeleteOption.Default);
            //}
            //catch (Exception)
            //{
            //    Utils.ShowMessageToast("操作失败", 2000);
            //    //throw;
            //}
        }

        private async void list_Downed_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data=  e.ClickedItem as DownloadedModel;
            vlist = data.videoinfo;
            cd.DataContext = data;
            await cd.ShowAsync();
        }


        //private void btn_Select_Checked(object sender, RoutedEventArgs e)
        //{
        //    list_Downed.IsItemClickEnabled = false;
        //    list_Downed.SelectionMode = ListViewSelectionMode.Multiple;
        //}

        //private void btn_Select_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    list_Downed.IsItemClickEnabled = true;
        //    list_Downed.SelectionMode = ListViewSelectionMode.None;
        //}

        private  void btn_Delete_Folder_Click(object sender, RoutedEventArgs e)
        {

                //foreach (DownloadedModel item in list_Downed.SelectedItems)
                //{
                //    await DownloadHelper.DeleteFolder(item);
                //}
                //GetDownOk();
          
        }

        private async void btn_Input_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("此功能能修复一些已经下载完成，但应用里不显示的问题\r\n只需要选择 下载路径\\BiliVideoDownload\\视频ID\\视频MID\\视频MID.josn，然后稍等片刻即可").ShowAsync();
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            filePicker.FileTypeFilter.Add(".json");
            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }
            if (file.FileType != ".json")
            {
                await new MessageDialog("错误的文件格式！").ShowAsync();

            }
            string json = await FileIO.ReadTextAsync(file);
            VideoListModel info = JsonConvert.DeserializeObject<VideoListModel>(json);
            info.downstatus = true;
            string jsonInfo = JsonConvert.SerializeObject(info);
            //StorageFile fileWrite = await folder.CreateFileAsync(info.Guid + ".json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, jsonInfo);
            GetDownOk();
            Utils.ShowMessageToast("导入完成", 3000);
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetDownOk();
        }
        List<VideoListModel> vlist;
        private void list_Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            cd.Hide();
            var info = e.ClickedItem as VideoListModel;
          
            List<PlayerModel> ls = new List<PlayerModel>();
            int i = 0;
            foreach (var item in vlist)
            {

                ls.Add(new PlayerModel() { Aid = item.id, Mid = item.mid, Mode = PlayMode.Local, No = i.ToString(), VideoTitle = item.partTitle, Path=item.folderPath,Title= item.title });
                i++;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls,vlist.IndexOf(info) });


        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            right_menu.ShowAt(sender as Grid, e.GetPosition(sender as Grid));
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            right_menu.ShowAt(sender as Grid, e.GetPosition(sender as Grid));
        }

        private void Grid_RightTapped_1(object sender, RightTappedRoutedEventArgs e)
        {
            _menu.ShowAt(sender as Grid, e.GetPosition(sender as Grid));
        }

        private void Grid_Holding_1(object sender, HoldingRoutedEventArgs e)
        {
            _menu.ShowAt(sender as Grid, e.GetPosition(sender as Grid));
        }

        private void menuitem_View_Click(object sender, RoutedEventArgs e)
        {
            
            var x = (sender as MenuFlyoutItem).DataContext as DMM;
            if (x.isbangumi)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), x.aid);
            }
            else
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), x.aid);
            }
        }

        private async void menuitem_Delete_F_Click(object sender, RoutedEventArgs e)
        {
            var x=   (sender as MenuFlyoutItem).DataContext as DMM;
            await DownloadHelper.DeleteFile(x);
            GetDownOk();
        }

        private async void menuitem_Dlete_Click(object sender, RoutedEventArgs e)
        {
            right_menu.Hide();
            var x = (sender as MenuFlyoutItem).DataContext as VideoListModel;
            await DownloadHelper.DeleteFile(x);
            GetDownOk();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var lview=(( sender as ListView).DataContext as DM).videos;

            var info = e.ClickedItem as DMM;
            
            List<PlayerModel> ls = new List<PlayerModel>();
            int i = 0;
            foreach (var item in lview)
            {

                ls.Add(new PlayerModel() { Aid = item.aid, Mid = item.mid, Mode = PlayMode.Local, No = i.ToString(), VideoTitle = "" ,Path = item.folderPath, Title = item.name });
                i++;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, lview.IndexOf(info) });
        }

        private async void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            var x = (sender as HyperlinkButton).DataContext as DM;
            await DownloadHelper.DeleteFolder(x);
            GetDownOk();
        }
    }

   

}
