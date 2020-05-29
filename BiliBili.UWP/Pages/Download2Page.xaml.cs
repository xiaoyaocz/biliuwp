using BiliBili.UWP.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using BiliBili.UWP.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Download2Page : Page
    {
        public Download2Page()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetDowned();
            await LoadDowning();
            
            //await Task.Delay(200);
        }
        private IDictionary<string, CancellationTokenSource> cts;
        List<DownloadOperation> downloadOperations;
        List<Task> handelList;
        ObservableCollection<DownloadDisplayInfo> downloadDisplayInfos;
        private async Task LoadDowning()
        {
            pr_loading.Visibility = Visibility.Visible;

            cts = new Dictionary<string, CancellationTokenSource>();
            if (handelList == null)
            {
                handelList = new List<Task>();
            }
            if (downloadOperations == null)
            {
                downloadOperations = new List<DownloadOperation>();
            }
            downloadDisplayInfos = new ObservableCollection<DownloadDisplayInfo>();
            List<DisplayModel> list = new List<DisplayModel>();
            var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
            foreach (var item in ls)
            {
              
                CancellationTokenSource cancellationTokenSource = null;

                var data = SqlHelper.GetDownload(item.Guid.ToString());
                if (cts.ContainsKey(data.cid))
                {
                    cancellationTokenSource = cts[data.cid];
                }
                else
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    cts.Add(data.cid, cancellationTokenSource);
                }

                //cancellationTokenSource.Token.Register(Handel(item, cancellationTokenSource));

                if (!downloadOperations.Contains(item))
                {
                    downloadOperations.Add(item);
                    handelList.Add(Handel(item, cancellationTokenSource));
                }
                
                list.Add(new DisplayModel()
                {
                    cid = data.cid,
                    title = data.title + " " + data.eptitle,
                    backgroundTransferStatus = item.Progress.Status,
                    index = (data.index + 1).ToString("00"),
                    progress = GetProgress(item.Progress.BytesReceived, GetTotalBytesToReceive(item)),
                    guid = item.Guid.ToString(),
                    size = GetSize(item.Progress.BytesReceived, GetTotalBytesToReceive(item)),
                    id = data.aid,
                    mode = data.mode
                });
            }
            foreach (var item in list.GroupBy(x => x.cid))
            {
                ObservableCollection<DisplayModel> displays = new ObservableCollection<DisplayModel>();
                foreach (var item2 in item.OrderBy(x => x.index))
                {
                    displays.Add(item2);
                }
                downloadDisplayInfos.Add(new DownloadDisplayInfo()
                {
                    title = displays[0].title + " " + displays[0].eptitle,
                    cid = item.Key,
                    id = displays[0].id,
                    mode = displays[0].mode,
                    items = displays
                });
            }
            listDown.ItemsSource = downloadDisplayInfos;
            downCount.Text = downloadDisplayInfos.Count.ToString();
            //list_Downing.ItemsSource = list;
            pr_loading.Visibility = Visibility.Collapsed;
           
            await Task.WhenAll(handelList);

        }

        private ulong GetTotalBytesToReceive(DownloadOperation downloadOperation)
        {
            try
            {
                //TotalBytesToReceive有时会抽风返回0
                if (downloadOperation.Progress.TotalBytesToReceive != 0)
                {
                    return downloadOperation.Progress.TotalBytesToReceive;
                }
                else
                {
                    var response = downloadOperation.GetResponseInformation();

                    if (response != null && response.Headers.ContainsKey("Content-Length") && ulong.TryParse(response.Headers["Content-Length"], out var total))
                    {
                        return total;
                    }
                    return 0;
                }
            }
            catch (Exception)
            {

                return 0;
            }

        }
        private async void GetDowned()
        {
            loading.Visibility = Visibility.Visible;
            Dictionary<string, string> valuePairs = new Dictionary<string, string>();
            var folder = await DownloadHelper2.GetDownloadFolder();
            List<VideoDiaplayModel> list = new List<VideoDiaplayModel>();
            try
            {

                foreach (var item in await folder.GetFoldersAsync())
                {
                  
                    if (await DownloadHelper2.ExistsFile(item.Path + @"\info.json"))
                    {

                        var file = await item.GetFileAsync("info.json");
                        var data = await FileIO.ReadTextAsync(file);
                        DownloadVideonInfoModel downloadVideonInfoModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DownloadVideonInfoModel>(data);
                        var video = new VideoDiaplayModel()
                        {
                            id = downloadVideonInfoModel.id,
                            mode = downloadVideonInfoModel.type,
                            title = downloadVideonInfoModel.title,
                            videolist = new List<PartDiaplayModel>()
                        };
                        if (await DownloadHelper2.ExistsFile(item.Path + @"\thumb.jpg"))
                        {
                            video.thumb = item.Path + @"\thumb.jpg";
                            var thumbFile =await StorageFile.GetFileFromPathAsync(video.thumb);
                            var thumb=await thumbFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.VideosView);
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(thumb);
                            video.thumb_img = bitmapImage;
                        }
                        else
                        {
                            video.thumb = downloadVideonInfoModel.thumb;
                            video.thumb_img = new BitmapImage();
                        }
                        foreach (var item1 in await item.GetFoldersAsync())
                        {
                            if (await DownloadHelper2.ExistsFile(item1.Path + @"\info.json"))
                            {
                                var flag = false;
                                var files = (await item1.GetFilesAsync()).Where(x => x.FileType == ".mp4" || x.FileType == ".flv" || x.FileType == ".m4s");
                                if (files.Count()!=0)
                                {
                                    //DownloadHelper2.GetFileSize(x.Path).Result
                                    foreach (var subfile in files)
                                    {
                                        if (await DownloadHelper2.GetFileSize(subfile.Path)==0)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (flag)
                                    {
                                        continue;
                                    }
                                    var file1 = await item1.GetFileAsync("info.json");
                                    var data1 = await FileIO.ReadTextAsync(file1);
                                    DownloadPartnInfoModel downloadPartnInfoModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DownloadPartnInfoModel>(data1);
                                    var part = new PartDiaplayModel()
                                    {
                                        cid = downloadPartnInfoModel.cid,
                                        eptitle = downloadPartnInfoModel.title,
                                        path = item1.Path,
                                        index = downloadPartnInfoModel.index,
                                        id = video.id,
                                        mode = video.mode,
                                        title = video.title,
                                        is_dash= downloadPartnInfoModel.is_dash
                                    };
                                    if (!valuePairs.ContainsKey(downloadPartnInfoModel.cid))
                                    {

                                        valuePairs.Add(downloadPartnInfoModel.cid, item1.Path);
                                    }
                                    video.videolist.Add(part);
                                }
                                //foreach (var item2 in files)
                                //{
                                //    if (item2.FileType == ".mp4" || item2.FileType == ".flv")

                                //        check.Add(await DownloadHelper2.GetFileSize(item2.Path) == 0);
  
                                //    }
                                //}
                                //if (!check.Contains(false))
                                //{
                                    
                               // }

                            }
                        }

                        if (video.videolist.Count != 0)
                        {
                            video.videolist = video.videolist.OrderBy(x => x.index).ToList();
                            list.Add(video);
                        }
                    }
                }
                DownloadHelper2.downloadeds = valuePairs;
            }
            catch (Exception ex)
            {
                await new MessageDialog("无法读取已经下载完成的视频").ShowAsync();
            }
            list_Downed.ItemsSource = list;
            loading.Visibility = Visibility.Collapsed;
        }


        private double GetProgress(ulong s, ulong e)
        {
            if (e != 0)
            {
                return ((double)s / e) * 100;
            }
            else
            {
                return 0;
            }
        }
        private string GetSize(ulong s, ulong e)
        {
            if (e != 0)
            {
                return ((Convert.ToDouble(s) / 1024 / 1024)).ToString("0.0") + "M/" + (Convert.ToDouble(e) / 1024 / 1024).ToString("0.0") + "M";
            }
            else
            {
                return $"{(Convert.ToDouble(s) / 1024 / 1024).ToString("0.0")}M/未知";
            }
        }
        private async Task Handel(DownloadOperation downloadOperation, CancellationTokenSource cancellationTokenSource)
        {
            try
            {

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (cancellationTokenSource != null)
                {
                    await downloadOperation.AttachAsync().AsTask(cancellationTokenSource.Token, progressCallback);
                }
                else
                {
                    await downloadOperation.AttachAsync().AsTask(progressCallback);
                }

                //var ls = list_Downing.ItemsSource as ObservableCollection<DisplayModel>;
                GetDowned();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("0x80072EF1") || ex.Message.Contains("0x80070002")||ex.Message.Contains("0x80004004"))
                {
                    return;
                }
                await new MessageDialog(ex.Message).ShowAsync();
            }
            finally
            {
                RemoveItem(downloadOperation.Guid.ToString());
            }
        }

        private void RemoveItem(string guid)
        {
            try
            {
                if (downloadDisplayInfos == null)
                {
                    return;
                }
                var item = downloadDisplayInfos.FirstOrDefault(x => x.items.Count(y => y.guid == guid) != 0);
                if (item != null)
                {
                    if (item.items.Count > 1)
                    {
                        var subItem = item.items.FirstOrDefault(x => x.guid == guid);
                        if (subItem != null)
                        {
                            item.items.Remove(subItem);

                        }
                    }
                    else
                    {
                        downloadDisplayInfos.Remove(item);
                        //await DownloadHelper2.DeleteFolder(item.id, item.cid, item.mode);
                        //GetDowned();
                    }
                }
                downCount.Text = downloadDisplayInfos.Count.ToString();
            }
            catch (Exception ex)
            {
            }

        }


        private void DownloadProgress(DownloadOperation op)
        {
            try
            {
                if (downloadDisplayInfos == null)
                {
                    return;
                }
                var guid = op.Guid.ToString();

                var item = downloadDisplayInfos.FirstOrDefault(x => x.items.Count(y => y.guid == guid) != 0);
                if (item != null)
                {
                    var subItem = item.items.FirstOrDefault(x => x.guid == guid);
                    if (subItem != null)
                    {
                        subItem.progress = GetProgress(op.Progress.BytesReceived, GetTotalBytesToReceive(op));
                        subItem.backgroundTransferStatus = op.Progress.Status;
                        subItem.size = GetSize(op.Progress.BytesReceived, GetTotalBytesToReceive(op));
                    }
                }

                //var ls = list_Downing.ItemsSource as ObservableCollection<DisplayModel>;
                //var item = ls.FirstOrDefault(x => x.guid == op.Guid.ToString());
                //if (item != null)
                //{
                //    item.progress = GetProgress(op.Progress.BytesReceived, GetTotalBytesToReceive(op));
                //    item.backgroundTransferStatus = op.Progress.Status;
                //    item.size = GetSize(op.Progress.BytesReceived, GetTotalBytesToReceive(op));
                //}
            }
            catch (Exception)
            {
                return;
            }

        }

        private async void btn_D_Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
                var data = (sender as Button).DataContext as DisplayModel;
                var item = ls.FirstOrDefault(x => x.Guid.ToString() == data.guid);
                if (item != null)
                {
                    item.Resume();
                }

                data.backgroundTransferStatus = item.Progress.Status;
            }
            catch (Exception)
            {
            }

        }

        private void btn_D_Refresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btn_D_Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageDialog md = new MessageDialog("确定要取消任务吗?\r\n关联的分段任务也会被取消");
                md.Commands.Add(new UICommand("确定")
                {
                    Id = 0
                });
                md.Commands.Add(new UICommand("取消") { Id = 1 });
                if (Convert.ToInt32((await md.ShowAsync()).Id) == 0)
                {
                    var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
                    var data = (sender as Button).DataContext as DisplayModel;
                    //var down_tasks = (list_Downing.ItemsSource as ObservableCollection<DisplayModel>).Where(x => x.cid == data.cid).ToList();
                    cts[data.cid].Cancel();

                    //foreach (var item in down_tasks)
                    //{
                    //    var task_item = ls.FirstOrDefault(x => x.Guid.ToString() == item.guid);
                    //    if (task_item.Progress.Status == BackgroundTransferStatus.Running)
                    //    {
                    //        task_item.Pause();
                    //    }
                    //}
                    //await Task.Delay(1000);
                    //for (int i = 0; i < down_tasks.Count; i++)
                    //{
                    //    var task_item = ls.FirstOrDefault(x => x.Guid.ToString() == down_tasks[0].guid);
                    //    if (task_item != null)
                    //    {
                    //        cts[task_item.Guid].Cancel();
                    //    }
                    //    await Task.Delay(1000);
                    //}

                    var d = SqlHelper.GetDownload(data.guid);
                    await DownloadHelper2.DeleteFolder(d.aid, d.cid, d.mode);

                    LoadDowning();
                }

            }
            catch (Exception ex)
            {

            }

        }

        private async void btn_D_Pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
                var data = (sender as Button).DataContext as DisplayModel;
                var item = ls.FirstOrDefault(x => x.Guid.ToString() == data.guid);
                if (item != null)
                {
                    item.Pause();
                }

                data.backgroundTransferStatus = item.Progress.Status;
            }
            catch (Exception)
            {
            }

        }

        private async void btn_StartAll_Click(object sender, RoutedEventArgs e)
        {
            var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
            foreach (var item in ls)
            {
                try
                {
                    item.Resume();
                }
                catch (Exception)
                {
                }
            }
            //LoadDowning();
        }

        private async void btn_PauseAll_Click(object sender, RoutedEventArgs e)
        {
            var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
            foreach (var item in ls)
            {
                try
                {
                    item.Pause();
                }
                catch (Exception)
                {
                }
            }
            // LoadDowning();
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var l = (sender as ListView).ItemsSource as List<PartDiaplayModel>;

            var data = e.ClickedItem as PartDiaplayModel;
          
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(data.path);
            if ((await storageFolder.GetFilesAsync()).Count(x => x.FileType == ".flv")>1)
            {

                MessageDialog md = new MessageDialog("该视频由多段FLV文件组成，建议合并后观看\r\n是否要合并文件？");
                md.Commands.Add(new UICommand("合并"){Id = 0});
                md.Commands.Add(new UICommand("取消") { Id = 1 });
                if (Convert.ToInt32((await md.ShowAsync()).Id) == 0)
                {
                    var dialog = DownloadHelper2.videoProcessingDialogs.FirstOrDefault(x => x.ID == data.cid);
                    if (dialog == null)
                    {
                        dialog = new VideoProcessingDialog(data.cid, data.title + " " + data.eptitle, storageFolder);
                        DownloadHelper2.videoProcessingDialogs.Add(dialog);
                    }
                    await dialog.ShowAsync();
                    return;
                }
            }


            List<PlayerModel> ls = new List<PlayerModel>();
            int i = 0;
            foreach (var item in l)
            {

                ls.Add(new PlayerModel() { Aid = item.id, is_dash= data.is_dash, Mid = item.cid, Mode = PlayMode.Local, No = "", VideoTitle = item.eptitle, Path = item.path, Title = item.title, episode_id = item.epid });
                i++;
            }


            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, l.IndexOf(data) });


        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetDowned();
        }

        private async void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            var x = (sender as MenuFlyoutItem).DataContext as PartDiaplayModel;

            await DownloadHelper2.DeleteFolder(x.id, x.cid, x.mode);
            GetDowned();
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            menu.ShowAt(sender as Grid, e.GetPosition(sender as Grid));
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            menu.ShowAt(sender as Grid, e.GetPosition(sender as Grid));
        }

        private void hy_View_Click(object sender, RoutedEventArgs e)
        {

            var m = (sender as MenuFlyoutItem).DataContext as VideoDiaplayModel;
            if (m.mode == "video")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), m.id);
            }
            else
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), m.id);
            }

        }

        private async void hy_Delete_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as MenuFlyoutItem).DataContext as VideoDiaplayModel;

            MessageDialog md = new MessageDialog("确定要删除全部视频吗?");
            md.Commands.Add(new UICommand("确定")
            {
                Id = 0
            });
            md.Commands.Add(new UICommand("取消") { Id = 1 });
            if (Convert.ToInt32((await md.ShowAsync()).Id) == 0)
            {
                await DownloadHelper2.DeleteFolder(m.id, m.mode);
            }
            GetDowned();
        }

        private async void btn_SendToPhone_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("该功能还未完成").ShowAsync();
        }


        private async void hy_UpdateDM_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as MenuFlyoutItem).DataContext as VideoDiaplayModel;
            foreach (var item in m.videolist)
            {
                var path = item.path + @"\" + item.cid + ".xml";
                await DownloadHelper2.UpdateDanmu(path, item.cid);
            }
            await new MessageDialog("更新完成").ShowAsync();
        }

        private async void Btn_SubItem_Pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
                var data = (sender as Button).DataContext as DisplayModel;
                var item = ls.FirstOrDefault(x => x.Guid.ToString() == data.guid);
                if (item != null)
                {
                    item.Pause();
                }

                data.backgroundTransferStatus = item.Progress.Status;
            }
            catch (Exception)
            {
            }
        }

        private async void Btn_SubItem_Resume_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
                var data = (sender as Button).DataContext as DisplayModel;
                var item = ls.FirstOrDefault(x => x.Guid.ToString() == data.guid);
                if (item != null)
                {
                    item.Resume();
                }

                data.backgroundTransferStatus = item.Progress.Status;
            }
            catch (Exception)
            {
            }
        }

        private async void Btn_Cancel_Items_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageDialog md = new MessageDialog("确定要取消任务吗?");
                md.Commands.Add(new UICommand("确定")
                {
                    Id = 0
                });
                md.Commands.Add(new UICommand("取消") { Id = 1 });
                if (Convert.ToInt32((await md.ShowAsync()).Id) == 0)
                {
                    var data = (sender as Button).DataContext as DownloadDisplayInfo;
                    var guid = data.items.FirstOrDefault().guid;
                    var id = data.id;
                    var cid = data.cid;
                    var mode = data.mode;
                    cts[data.cid].Cancel();

                    RemoveItem(guid);
                    await DownloadHelper2.DeleteFolder(id, cid, mode);

                    //LoadDowning();
                }

            }
            catch (Exception ex)
            {

            }

        }

        private async void Btn_Resume_Items_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as DownloadDisplayInfo;
            var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
            foreach (var item in ls.Where(x => data.items.Count(y => y.guid == x.Guid.ToString()) != 0))
            {
                try
                {

                    item.Resume();
                }
                catch (Exception)
                {
                }
            }
            //LoadDowning();
        }

        private async void Btn_Pause_Items_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as DownloadDisplayInfo;
            var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
            foreach (var item in ls.Where(x => data.items.Count(y => y.guid == x.Guid.ToString()) != 0))
            {
                try
                {
                    if (item.Progress.Status == BackgroundTransferStatus.Running)
                    {
                        item.Pause();
                    }
                }
                catch (Exception)
                {
                }
            }
            //LoadDowning();


        }

        private async void Btn_UpdateDanmuSubItem_Click(object sender, RoutedEventArgs e)
        {
            var x = (sender as MenuFlyoutItem).DataContext as PartDiaplayModel;
            var path = x.path + @"\" + x.cid + ".xml";
            await DownloadHelper2.UpdateDanmu(path, x.cid);
            Utils.ShowMessageToast("更新弹幕完成");
        }
        private async void btn_ToMp4_Click(object sender, RoutedEventArgs e)
        {
            var model = (sender as MenuFlyoutItem).DataContext as PartDiaplayModel;
            StorageFolder storageFolder =await StorageFolder.GetFolderFromPathAsync(model.path);

            if ((await storageFolder.GetFilesAsync()).Any(x=>x.FileType==".flv"|| x.FileType == ".m4s"))
            {
                var dialog = DownloadHelper2.videoProcessingDialogs.FirstOrDefault(x => x.ID == model.cid);
                if (dialog==null)
                {
                    dialog = new VideoProcessingDialog(model.cid, model.title + " " + model.eptitle, storageFolder);
                    DownloadHelper2.videoProcessingDialogs.Add(dialog);
                }
                await dialog.ShowAsync();
            }
            else
            {
                Utils.ShowMessageToast("该视频已经是MP4格式了");
            }
        }

        private async void Btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var model = (sender as MenuFlyoutItem).DataContext as PartDiaplayModel;
            await Windows.System.Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(model.path));
        }

        private void btnExpand_Click(object sender, RoutedEventArgs e)
        {
            var data=(sender as AppBarButton).DataContext as VideoDiaplayModel;
            if (data.expand)
            {
                data.expand = false;
                data.maxheight = 88;
            }
            else
            {
                data.expand = true;
                data.maxheight = double.PositiveInfinity;
            }
        }
    }
    public class DownloadDisplayInfo
    {
       
        public string id { get; set; }
        public string mode { get; set; }
        public string cid { get; set; }
        public ObservableCollection<DisplayModel> items { get; set; }
        public string title { get; set; }


    }


    public class DisplayModel : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string mode { get; set; }
        public string cid { get; set; }
        public string guid { get; set; }
        public string title { get; set; }
        public string eptitle { get; set; }

        public string index { get; set; }


        private double _progress;
        public double progress
        {

            get { return _progress; }
            set
            {
                _progress = value;
                thisPropertyChanged("progress");
            }
        }

        private string _size;
        public string size
        {
            get { return _size; }
            set
            {
                _size = value;
                thisPropertyChanged("size");
            }
        }


        private BackgroundTransferStatus _backgroundTransferStatus;
        public BackgroundTransferStatus backgroundTransferStatus
        {

            get { return _backgroundTransferStatus; }
            set
            {
                _backgroundTransferStatus = value;
                thisPropertyChanged("backgroundTransferStatus");
                Status = "";
            }

        }




        public string _Status;
        public string Status
        {
            get { return _Status; }
            set
            {
                switch (backgroundTransferStatus)
                {
                    case BackgroundTransferStatus.Idle:
                        _Status = "空闲中";
                        btnDownload = Visibility.Visible;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.Running:
                        _Status = "下载中";
                        btnDownload = Visibility.Collapsed;
                        btnPause = Visibility.Visible;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.PausedByApplication:
                        _Status = "暂停中";
                        btnDownload = Visibility.Visible;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.PausedCostedNetwork:
                        _Status = "已暂停，因为正在使用数据";
                        btnDownload = Visibility.Collapsed;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.PausedNoNetwork:
                        _Status = "挂起";
                        btnDownload = Visibility.Collapsed;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.Completed:
                        _Status = "完成";
                        btnDownload = Visibility.Collapsed;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.Canceled:
                        _Status = "取消";
                        btnDownload = Visibility.Collapsed;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.Error:
                        _Status = "下载错误";
                        btnDownload = Visibility.Collapsed;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Visible;
                        break;
                    case BackgroundTransferStatus.PausedSystemPolicy:
                        _Status = "因系统问题暂停";
                        btnDownload = Visibility.Collapsed;
                        btnPause = Visibility.Collapsed;
                        btnRefresh = Visibility.Collapsed;
                        break;
                    default:
                        _Status = "Wait...";
                        break;
                }
                thisPropertyChanged("Status");
            }
        }




        private Visibility _btnDownload;
        public Visibility btnDownload
        {

            get { return _btnDownload; }
            set
            {
                _btnDownload = value;
                thisPropertyChanged("btnDownload");
            }
        }

        private Visibility _btnPause;
        public Visibility btnPause
        {

            get { return _btnPause; }
            set
            {
                _btnPause = value;
                thisPropertyChanged("btnPause");
            }
        }

        private Visibility _btnRefresh;
        public Visibility btnRefresh
        {

            get { return _btnRefresh; }
            set
            {
                _btnRefresh = value;
                thisPropertyChanged("btnRefresh");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }

    public class VideoDiaplayModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public string id { get; set; }
        public string title { get; set; }
        public string thumb { get; set; }
        public string mode { get; set; }
        public BitmapImage thumb_img { get; set; }
        public List<PartDiaplayModel> videolist { get; set; }
        private bool _expand = false;
        public bool expand
        {
            get { return _expand; }
            set { _expand = value; PropertyChanged(this, new PropertyChangedEventArgs("expand")); }
        }

        private double _maxheight = 88;
        public double maxheight
        {
            get { return _maxheight; }
            set { _maxheight = value; PropertyChanged(this, new PropertyChangedEventArgs("maxheight")); }
        }
    }

    public class PartDiaplayModel
    {
        public string id { get; set; }

        public string cid { get; set; }
        public string eptitle { get; set; }
        public string title { get; set; }
        public int index { get; set; }
        public string path { get; set; }
        public string epid { get; set; }
        public string mode { get; set; }
        public bool is_dash { get; set; }
    }


}
