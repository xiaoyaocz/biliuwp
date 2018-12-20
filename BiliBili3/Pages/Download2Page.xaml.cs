using BiliBili3.Helper;
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
using BiliBili3.Controls;
using System.Collections.ObjectModel;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili3.Pages
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetDowned();
            LoadDowning();

        }
        List<Task> handelList = new List<Task>();
        private async void LoadDowning()
        {
            handelList.Clear();
            ObservableCollection<DisplayModel> list = new ObservableCollection<DisplayModel>();
            var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
            foreach (var item in ls)
            {
                var data = SqlHelper.GetDownload(item.Guid.ToString());
                handelList.Add(Handel(item));
                list.Add(new DisplayModel()
                {
                    title = data.title + " " + data.eptitle,
                    backgroundTransferStatus = item.Progress.Status,
                    index = (data.index + 1).ToString(),
                    progress = GetProgress(item.Progress.BytesReceived, item.Progress.TotalBytesToReceive),
                    guid = item.Guid.ToString(),
                    size = GetSize(item.Progress.BytesReceived, item.Progress.TotalBytesToReceive),
                });
            }
            list_Downing.ItemsSource = list;
            await Task.WhenAll(handelList);
        }

        private async void GetDowned()
        {
            loading.Visibility = Visibility.Visible;
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
                            video.thumb = item.Path + "/thumb.jpg";
                        }
                        else
                        {
                            video.thumb = downloadVideonInfoModel.thumb;
                        }
                        foreach (var item1 in await item.GetFoldersAsync())
                        {
                            if (await DownloadHelper2.ExistsFile(item1.Path + @"\info.json"))
                            {

                                List<bool> check = new List<bool>();
                                var files = await item1.GetFilesAsync();
                                foreach (var item2 in files)
                                {
                                    if (item2.FileType == ".mp4" || item2.FileType == ".flv")
                                    {
                                        if (await DownloadHelper2.GetFileSize(item2.Path) == 0)
                                        {
                                            check.Add(false);
                                        }
                                        else
                                        {
                                            check.Add(true);
                                        }

                                    }
                                }
                                if (!check.Contains(false))
                                {
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
                                        title = video.title
                                    };
                                    if (!DownloadHelper2.downloadeds.ContainsKey(downloadPartnInfoModel.cid))
                                    {
                                        DownloadHelper2.downloadeds.Add(downloadPartnInfoModel.cid, item1.Path);
                                    }
                                    video.videolist.Add(part);
                                }

                            }
                        }

                        if (video.videolist.Count != 0)
                        {
                            video.videolist = video.videolist.OrderBy(x => x.eptitle).ToList();
                            list.Add(video);
                        }
                    }
                }

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
                return "0/0";
            }
        }
        private async Task Handel(DownloadOperation downloadOperation)
        {
            try
            {
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                await downloadOperation.AttachAsync().AsTask(progressCallback);

                var ls = list_Downing.ItemsSource as ObservableCollection<DisplayModel>;
                var item = ls.First(x => x.guid == downloadOperation.Guid.ToString());
                ls.Remove(item);
                //LoadDowning();
                GetDowned();

            }
            catch (TaskCanceledException)
            {
                //SqlHelper.GetDownload(downloadOperation.Guid.ToString());
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("0x80072EF1") || ex.Message.Contains("0x80070002"))
                {
                    return;
                }
                await new MessageDialog(ex.Message).ShowAsync();
            }


        }

        private void DownloadProgress(DownloadOperation op)
        {
            try
            {
                var ls = list_Downing.ItemsSource as ObservableCollection<DisplayModel>;
                var item = ls.First(x => x.guid == op.Guid.ToString());
                // var d = (list_Downing.Items[list_Downing.Items.IndexOf(item)] as DisplayModel);
                item.progress = GetProgress(op.Progress.BytesReceived, op.Progress.TotalBytesToReceive);
                item.backgroundTransferStatus = op.Progress.Status;
                item.size = GetSize(op.Progress.BytesReceived, op.Progress.TotalBytesToReceive);

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
                var item = ls.First(x => x.Guid.ToString() == data.guid);
                item.Resume();
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
                    var item = ls.First(x => x.Guid.ToString() == data.guid);
                    item.AttachAsync().Cancel();
                    var d = SqlHelper.GetDownload(data.guid);
                    await DownloadHelper2.DeleteFolder(d.aid, d.cid, d.mode);
                }
                LoadDowning();
            }
            catch (Exception)
            {
            }

        }

        private async void btn_D_Pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
                var data = (sender as Button).DataContext as DisplayModel;
                var item = ls.First(x => x.Guid.ToString() == data.guid);

                item.Pause();
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
            LoadDowning();
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
            LoadDowning();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var l = (sender as ListView).ItemsSource as List<PartDiaplayModel>;

            var data = e.ClickedItem as PartDiaplayModel;

            List<PlayerModel> ls = new List<PlayerModel>();
            int i = 0;
            foreach (var item in l)
            {

                ls.Add(new PlayerModel() { Aid = item.id, Mid = item.cid, Mode = PlayMode.Local, No = "", VideoTitle = item.eptitle, Path = item.path, Title = item.title, episode_id = item.epid });
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

            var m = (sender as HyperlinkButton).DataContext as VideoDiaplayModel;
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
            var m = (sender as HyperlinkButton).DataContext as VideoDiaplayModel;

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
        }

        private async void btn_SendToPhone_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("该功能还未完成").ShowAsync();
        }

        private async void btn_ToMp4_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("该功能还未完成").ShowAsync();
        }

        private void btn_old_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DownloadPage));
        }

        private async void hy_UpdateDM_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as VideoDiaplayModel;
            foreach (var item in m.videolist)
            {
                var path = item.path + @"\" + item.cid + ".xml";
                await DownloadHelper2.UpdateDanmu(path,item.cid);
            }
            await new MessageDialog("更新完成").ShowAsync();
        }
    }
    public class DisplayModel : INotifyPropertyChanged
    {

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

    public class VideoDiaplayModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string thumb { get; set; }
        public string mode { get; set; }
        public List<PartDiaplayModel> videolist { get; set; }
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
    }


}
