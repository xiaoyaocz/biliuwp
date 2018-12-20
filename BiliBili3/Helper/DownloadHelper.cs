using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Popups;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Storage.Provider;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BiliBili3.Helper
{

    /*  文件夹结构
        downlist.json
            title,aid,listpath,isbangumi
        aid(folder)
           thumb.jpg
           info.json
                -aid,isbangumi,list
           mid(folder)
               danmu.xml
               video.mp4
               info.json
                    -aid,mid,part,parttitle


    */

    public class DownloadHelper
    {
        /*
         * GetDownloaingList()
         * GetDownlaodedList()
         * ReplaceSymbol()
         * DownlaodDanmuku()
         * GetVideoUrl()//放到ApiHelper中，Player和Downhelper可以统一调用
         * SetDownInfo()
         * _BackgroundTransferGroup
         * StartDownload()
         * GetDownloadSetting()
         */

        public static Dictionary<string, string> downMids = new Dictionary<string, string>();
        public static BackgroundTransferGroup group = BackgroundTransferGroup.CreateGroup("BILIBILI-UWP-30");//下载组，方便管理
        public static List<FolderListModel> folderList = null;
        static StorageFolder DownFolder = null;
        static StorageFile file_downlist = null;
        /// <summary>
        /// 将特殊字符替换为_
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ReplaceSymbol(string input)
        {
            string reg = @"\:" + @"|\;" + @"|\/" + @"|\\" + @"|\|" + @"|\," + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";//特殊字符
            Regex r = new Regex(reg);
            string strFiltered = r.Replace(input, "_");//将特殊字符替换为"_"
            return strFiltered;

        }
        public static async Task DownDanMu(string cid, StorageFolder folder)
        {
            try
            {
                string results = await WebClientClass.GetResults(new Uri("http://comment.bilibili.com/" + cid + ".xml"));
                //将弹幕存在在应用文件夹
                //StorageFolder folder = ApplicationData.Current.LocalFolder;
                //StorageFolder DowFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("Bili-Download", CreationCollisionOption.OpenIfExists);
                StorageFile fileWrite = await folder.CreateFileAsync(cid + ".xml", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(fileWrite, results);
            }
            catch (Exception)
            {
                //return null;
            }

        }
        public static async Task<string> DownThumb(string url, string aid, StorageFolder folder)
        {
            try
            {
                StorageFile file = await folder.CreateFileAsync(aid + ".jpg", CreationCollisionOption.OpenIfExists);
                IBuffer bu = await WebClientClass.GetBuffer(new Uri(url));
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, bu);
                await CachedFileManager.CompleteUpdatesAsync(file);

                return file.Path;

            }
            catch (Exception)
            {
                return url;
                //return null;
            }

        }
        public static async Task GetfolderList()
        {
            try
            {
                // string mruFirstToken = StorageApplicationPermissions.MostRecentlyUsedList.Entries.First(x=>x.Metadata==f.Path).Token;
                //StorageFolder retrievedFile = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruFirstToken);



                string setting = SettingHelper.Get_DownPath();
                try
                {
                    if (SettingHelper.Get_CustomDownPath() && setting != "系统视频库")
                    {

                        string mruFirstToken = StorageApplicationPermissions.MostRecentlyUsedList.Entries.First(x => x.Metadata == setting).Token;
                        var settingFolder = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruFirstToken);
                        DownFolder = await settingFolder.CreateFolderAsync("BiliVideoDownload", CreationCollisionOption.OpenIfExists);

                    }
                    else
                    {
                        DownFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("BiliVideoDownload", CreationCollisionOption.OpenIfExists);
                    }
                }
                catch (Exception)
                {
                    DownFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("BiliVideoDownload", CreationCollisionOption.OpenIfExists);
                }
                finally
                {
                    file_downlist = await DownFolder.CreateFileAsync("downlist.json", CreationCollisionOption.OpenIfExists);

                }

                if (file_downlist == null)
                {
                    file_downlist = await DownFolder.CreateFileAsync("downlist.json", CreationCollisionOption.OpenIfExists);
                    folderList = new List<FolderListModel>();
                    await FileIO.WriteTextAsync(file_downlist, JsonConvert.SerializeObject(folderList));
                }
                else
                {
                    string a = await FileIO.ReadTextAsync(file_downlist);
                    if (a.Length != 0)
                    {
                        folderList = JsonConvert.DeserializeObject<List<FolderListModel>>(a);
                        if (folderList == null)
                        {
                            folderList = new List<FolderListModel>();
                        }
                    }
                    else
                    {
                        file_downlist = await DownFolder.CreateFileAsync("downlist.json", CreationCollisionOption.OpenIfExists);
                        folderList = new List<FolderListModel>();
                        await FileIO.WriteTextAsync(file_downlist, JsonConvert.SerializeObject(folderList));
                    }
                }
            }
            catch (Exception)
            {

                folderList = new List<FolderListModel>();

                //// MessageDialog md = new MessageDialog("GetfolderList Error!\r\n" + ex.Message);
                // await md.ShowAsync();
            }
        }
        public static async Task SetfolderList()
        {
            try
            {
                if (file_downlist == null)
                {
                    file_downlist = await DownFolder.CreateFileAsync("downlist.json", CreationCollisionOption.OpenIfExists);
                    folderList = new List<FolderListModel>();

                }
                if (folderList == null)
                {
                    await GetfolderList();
                }
                await FileIO.WriteTextAsync(file_downlist, JsonConvert.SerializeObject(folderList));
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog("SetfolderList Error!\r\n" + ex.Message);
                await md.ShowAsync();
            }





        }
        public static async void StartDownload(DownloadModel m)
        {
            try
            {
                if (folderList == null)
                {
                    await GetfolderList();
                }
               
                BackgroundDownloader downloader = new BackgroundDownloader();
                downloader.SetRequestHeader("Referer", "https://www.bilibili.com/blackboard/html5player.html?crossDomain=true");
                downloader.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                //设置下载模式
                if (SettingHelper.Get_DownMode() == 0)
                {
                    group.TransferBehavior = BackgroundTransferBehavior.Serialized;
                }
                else
                {
                    group.TransferBehavior = BackgroundTransferBehavior.Parallel;
                }
                downloader.TransferGroup = group;
                //设置视频文件夹
                StorageFolder videoFolder = null;
                var f = folderList.Find(x => x.id == m.folderinfo.id);
                if (f == null)
                {
                    videoFolder = await DownFolder.CreateFolderAsync(m.folderinfo.id, CreationCollisionOption.OpenIfExists);
                    m.folderinfo.folderPath = videoFolder.Path;
                    m.folderinfo.thumb = await DownThumb(m.folderinfo.thumb, m.folderinfo.id, videoFolder);
                    folderList.Add(m.folderinfo);
                }
                else
                {
                    try
                    {
                        videoFolder = await StorageFolder.GetFolderFromPathAsync(f.folderPath);
                    }
                    catch (Exception ex)
                    {
                        MessageDialog md = new MessageDialog("Get videoFolder Error！\r\n" + ex.Message);
                    }
                   
                }

                //读取part文件夹
                StorageFolder PartFolder = null;
                var partf = await videoFolder.CreateFolderAsync(m.videoinfo.mid, CreationCollisionOption.OpenIfExists);
                if (partf == null)
                {
                    PartFolder = await videoFolder.CreateFolderAsync(m.videoinfo.mid, CreationCollisionOption.OpenIfExists);
                }
                else
                {
                    PartFolder = partf;
                }
                //创建相关文件
                //创建配置文件

                //创建视频文件
                StorageFile file = await PartFolder.CreateFileAsync(m.videoinfo.mid + ".mp4", CreationCollisionOption.OpenIfExists);
                //下载弹幕文件
                await DownDanMu(m.videoinfo.mid, PartFolder);

                DownloadOperation downloadOp = downloader.CreateDownload(new Uri(m.videoinfo.videoUrl), file);
                //设置下载策略
                if (SettingHelper.Get_Use4GDown())
                {
                    downloadOp.CostPolicy = BackgroundTransferCostPolicy.Always;
                }
                else
                {
                    downloadOp.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                }
                BackgroundTransferStatus downloadStatus = downloadOp.Progress.Status;
                m.videoinfo.downGUID = downloadOp.Guid.ToString();
                m.videoinfo.videoPath = downloadOp.ResultFile.Path;
                m.videoinfo.folderPath = PartFolder.Path;
                m.videoinfo.downstatus = false;
                StorageFile sefile = await PartFolder.CreateFileAsync(m.videoinfo.mid + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(sefile, JsonConvert.SerializeObject(m.videoinfo));
                await SetGUIDFile(m);
                downloadOp.StartAsync();

            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog("StartDownload Eroor！\r\n" + ex.Message);
                await md.ShowAsync();
            }
            finally
            {
                await SetfolderList();
                await GetfolderList();
            }
        }
        public static async void StartDownload(DownloadModel m,int index)
        {
            try
            {
                if (folderList == null)
                {
                    await GetfolderList();
                }

                BackgroundDownloader downloader = new BackgroundDownloader();
                downloader.SetRequestHeader("Referer", "https://www.bilibili.com/blackboard/html5player.html?crossDomain=true");
                downloader.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
                //设置下载模式
                if (SettingHelper.Get_DownMode() == 0)
                {
                    group.TransferBehavior = BackgroundTransferBehavior.Serialized;
                }
                else
                {
                    group.TransferBehavior = BackgroundTransferBehavior.Parallel;
                }
                downloader.TransferGroup = group;
                //设置视频文件夹
                StorageFolder videoFolder = null;
                var f = folderList.Find(x => x.id == m.folderinfo.id);
                if (f == null)
                {
                    videoFolder = await DownFolder.CreateFolderAsync(m.folderinfo.id, CreationCollisionOption.OpenIfExists);
                    m.folderinfo.folderPath = videoFolder.Path;
                    m.folderinfo.thumb = await DownThumb(m.folderinfo.thumb, m.folderinfo.id, videoFolder);
                    folderList.Add(m.folderinfo);
                }
                else
                {
                    try
                    {
                        videoFolder = await StorageFolder.GetFolderFromPathAsync(f.folderPath);
                    }
                    catch (Exception ex)
                    {
                        MessageDialog md = new MessageDialog("Get videoFolder Error！\r\n" + ex.Message);
                    }

                }

                //读取part文件夹
                StorageFolder PartFolder = null;
                var partf = await videoFolder.CreateFolderAsync(m.videoinfo.mid, CreationCollisionOption.OpenIfExists);
                if (partf == null)
                {
                    PartFolder = await videoFolder.CreateFolderAsync(m.videoinfo.mid, CreationCollisionOption.OpenIfExists);
                }
                else
                {
                    PartFolder = partf;
                }
                //创建相关文件
                //创建配置文件

                //创建视频文件
                StorageFile file = await PartFolder.CreateFileAsync(m.videoinfo.mid+"-"+ index + ".flv", CreationCollisionOption.OpenIfExists);
                //下载弹幕文件
                await DownDanMu(m.videoinfo.mid, PartFolder);

                DownloadOperation downloadOp = downloader.CreateDownload(new Uri(m.videoinfo.videoUrl), file);
                //设置下载策略
                if (SettingHelper.Get_Use4GDown())
                {
                    downloadOp.CostPolicy = BackgroundTransferCostPolicy.Always;
                }
                else
                {
                    downloadOp.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                }
                BackgroundTransferStatus downloadStatus = downloadOp.Progress.Status;
                m.videoinfo.downGUID = downloadOp.Guid.ToString();
                m.videoinfo.videoPath = downloadOp.ResultFile.Path;
                m.videoinfo.folderPath = PartFolder.Path;
                m.videoinfo.downstatus = false;
                StorageFile sefile = await PartFolder.CreateFileAsync(m.videoinfo.mid + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(sefile, JsonConvert.SerializeObject(m.videoinfo));
                await SetGUIDFile(m);
                downloadOp.StartAsync();

            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog("StartDownload Eroor！\r\n" + ex.Message);
                await md.ShowAsync();
            }
            finally
            {
                await SetfolderList();
                await GetfolderList();
            }
        }
        /// <summary>
        /// GUID对应下载任务
        /// </summary>
        public static async Task SetGUIDFile(DownloadModel m)
        {
            try
            {
                if (DownFolder == null)
                {
                    await GetfolderList();
                }
                StorageFolder videoFolder = await DownFolder.CreateFolderAsync("GUID", CreationCollisionOption.OpenIfExists);
                StorageFile sefile = await videoFolder.CreateFileAsync(m.videoinfo.downGUID + ".json", CreationCollisionOption.OpenIfExists);
                GuidModel gm = new GuidModel() { path = m.videoinfo.folderPath, guid = m.videoinfo.downGUID, mid = m.videoinfo.mid };
                await FileIO.WriteTextAsync(sefile, JsonConvert.SerializeObject(gm));
            }
            catch (Exception)
            {
            }


        }

        public static async Task<List<DownloadModel>> GetDownList()
        {
            try
            {
                if (DownFolder == null)
                {
                    await GetfolderList();
                }
                StorageFolder videoFolder = await DownFolder.CreateFolderAsync("GUID", CreationCollisionOption.OpenIfExists);

                List<DownloadModel> ls = new List<DownloadModel>();
                var downloads = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(group);
                if (downloads.Count > 0)
                {
                    foreach (var item in downloads)
                    {
                        var file = await videoFolder.GetFileAsync(item.Guid.ToString() + ".json");
                        string filedata = await FileIO.ReadTextAsync(file);
                        GuidModel m = JsonConvert.DeserializeObject<GuidModel>(filedata);
                        StorageFile files = await StorageFile.GetFileFromPathAsync(m.path + @"\" + m.mid + ".json");
                        string json = await FileIO.ReadTextAsync(files);
                        var dm = new DownloadModel();
                        dm.handel = new HandleModel();
                        dm.videoinfo = JsonConvert.DeserializeObject<VideoListModel>(json);
                        dm.handel.downOp = item;
                        dm.handel.Size = item.Progress.BytesReceived.ToString();
                        ls.Add(dm);
                    }
                    //list.ItemsSource = downlingModel;
                }
                return ls;
            }
            catch (Exception)
            {

                return new List<DownloadModel>();
            }

        }
        public static async Task<List<DownloadedModel>> GetDownedList()
        {
            List<DownloadedModel> ls = new List<DownloadedModel>();
            try
            {
                if (DownFolder == null)
                {
                    await GetfolderList();
                }

                downMids.Clear();

                foreach (var item in folderList)
                {
                    try
                    {
                        int count = 0;
                        int edcount = 0;
                        var vls = new List<VideoListModel>();
                        StorageFolder f = await StorageFolder.GetFolderFromPathAsync(item.folderPath);
                        var fls = await f.GetFoldersAsync();
                        if (fls.Count != 0)
                        {
                            foreach (var fitem in fls)
                            {

                                var fileinfolist = await fitem.GetFilesAsync();
                                foreach (var fileitem in fileinfolist)
                                {
                                    if (fileitem.FileType == ".json")
                                    {
                                        string json = await FileIO.ReadTextAsync(fileitem);
                                        var vm = JsonConvert.DeserializeObject<VideoListModel>(json);
                                        if (vm.downstatus)
                                        {

                                            downMids.Add(vm.mid, vm.folderPath);
                                            vls.Add(vm);
                                            edcount++;
                                        }

                                    }
                                }
                            }
                        }
                        count = fls.Count;
                        ls.Add(new DownloadedModel()
                        {
                            folderinfo = item,
                            videoinfo = vls,
                            count = count,
                            downedcount = edcount
                        });
                    }
                    catch (Exception)
                    {
                    }

                }
                return ls;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return ls;
            }


        }

        public static async Task DeleteFile(DownloadModel m)
        {
            try
            {
                if (DownFolder == null)
                {
                    await GetfolderList();
                }
                StorageFolder videoFolder = await DownFolder.CreateFolderAsync("GUID", CreationCollisionOption.OpenIfExists);

                var file = await videoFolder.GetFileAsync(m.videoinfo.downGUID + ".json");
                string filedata = await FileIO.ReadTextAsync(file);
                GuidModel gm = JsonConvert.DeserializeObject<GuidModel>(filedata);
                var fo = await StorageFolder.GetFolderFromPathAsync(gm.path);
                // folderList.Remove(folderList.First(x => x.id == m.videoinfo.id));
                //    await SetfolderList();
                await fo.DeleteAsync(StorageDeleteOption.Default);
                await file.DeleteAsync(StorageDeleteOption.Default);




            }
            catch (Exception ex)
            {
                await new MessageDialog("任务取消时文件删除失败\r\n" + ex.Message).ShowAsync();
            }
        }
        public static async Task DeleteFile(VideoListModel m)
        {
            try
            {
                if (DownFolder == null)
                {
                    await GetfolderList();
                }
                StorageFolder videoFolder = await DownFolder.CreateFolderAsync("GUID", CreationCollisionOption.OpenIfExists);

                var file = await videoFolder.GetFileAsync(m.downGUID + ".json");
                string filedata = await FileIO.ReadTextAsync(file);
                GuidModel gm = JsonConvert.DeserializeObject<GuidModel>(filedata);
                var fo = await StorageFolder.GetFolderFromPathAsync(gm.path);
                // folderList.Remove(folderList.First(x => x.id == m.videoinfo.id));
                //    await SetfolderList();
                await fo.DeleteAsync(StorageDeleteOption.Default);
                await file.DeleteAsync(StorageDeleteOption.Default);




            }
            catch (Exception ex)
            {
                await new MessageDialog("任务取消时文件删除失败\r\n" + ex.Message).ShowAsync();
            }
        }


        public static async Task<List<DM>> GetDownEedList()
        {
            List<DM> ls = new List<DM>();
            try
            {
                downMids.Clear();
                StorageFolder ds;
                if (SettingHelper.Get_CustomDownPath() && SettingHelper.Get_DownPath() != "系统视频库")
                {

                    string mruFirstToken = StorageApplicationPermissions.MostRecentlyUsedList.Entries.First(x => x.Metadata == SettingHelper.Get_DownPath()).Token;
                    var settingFolder = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruFirstToken);
                    ds = await settingFolder.CreateFolderAsync("BiliVideoDownload", CreationCollisionOption.OpenIfExists);

                }
                else
                {
                    ds = await KnownFolders.VideosLibrary.CreateFolderAsync("BiliVideoDownload", CreationCollisionOption.OpenIfExists);
                }
                try
                {
                    foreach (var item in await ds.GetFoldersAsync())
                    {
                        DM dm = new DM();

                        dm.id = item.Name;
                        //BitmapImage bm = new BitmapImage();
                        //bm.SetSource((await item.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView)).CloneStream());
                       // dm.img = bm;
                        dm.path = item.Path;
                        var ll = new List<DMM>();
                        try
                        {
                            foreach (var item1 in await item.GetFoldersAsync())
                            {
                               
                                var d = new DMM();
                                var files = await item1.GetFilesAsync();
                               
                                foreach (var item2 in files)
                                {
                                   
                                    try
                                    {
                                        if (item2.FileType == ".json")
                                        {
                                            string json = await FileIO.ReadTextAsync(item2);
                                            var vm = JsonConvert.DeserializeObject<VideoListModel>(json);
                                            d.path = item2.Path;
                                            d.name = vm.title + "-" + vm.partTitle;
                                            d.aid = vm.id;
                                            d.mid = vm.mid;
                                            downMids.Add(d.mid, item1.Path);
                                            d.folderPath = item1.Path;
                                            d.guid = vm.downGUID;
                                            if (d.aid.Length<=5)
                                            {
                                                d.isbangumi = true;
                                            }
                                            else
                                            {
                                                d.isbangumi = false;

                                            }
                                        }
                                        if (item2.FileType == ".xml")
                                        {
                                            d.dmPath = item2.Path;
                                        }
                                        if (item2.FileType == ".mp4" && (await item2.GetBasicPropertiesAsync()).Size != 0)
                                        {
                                            ll.Add(d);
                                        }
                                    }
                                    catch (Exception ex)
                                    {


                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {


                        }
                       
                        dm.videos = ll;
                        if (dm.videos.Count!=0)
                        {
                            ls.Add(dm);
                        }
                        
                    }
                }
                catch (Exception ex)
                {

                }
               
                
                return ls;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return ls;
            }

        }

        public static async Task DeleteFolder(DownloadedModel m)
        {
            try
            {
               
                StorageFolder guidFolder = await DownFolder.CreateFolderAsync("GUID", CreationCollisionOption.OpenIfExists);
                foreach (var item in m.videoinfo)
                {
                    await (await guidFolder.GetFileAsync(item.downGUID + ".json")).DeleteAsync();
                }


                StorageFolder videoFolder = await DownFolder.GetFolderAsync(m.folderinfo.id);
                await videoFolder.DeleteAsync();

                folderList.Remove(folderList.First(x => x.id == m.folderinfo.id));
                await SetfolderList();
                //var file = await videoFolder.GetFileAsync(m.videoinfo.downGUID + ".json");
                //string filedata = await FileIO.ReadTextAsync(file);
                //GuidModel gm = JsonConvert.DeserializeObject<GuidModel>(filedata);
                //var fo = await StorageFolder.GetFolderFromPathAsync(gm.path);
                //await fo.DeleteAsync(StorageDeleteOption.Default);
                //await file.DeleteAsync(StorageDeleteOption.Default);

            }
            catch (Exception ex)
            {
                await new MessageDialog("删除文件失败\r\n" + ex.Message).ShowAsync();
            }


        }
        public static async Task DeleteFolder(DM m)
        {
            try
            {
                
                StorageFolder guidFolder = await DownFolder.CreateFolderAsync("GUID", CreationCollisionOption.OpenIfExists);

                // await (await guidFolder.GetFileAsync(m.guid + ".json")).DeleteAsync();

                foreach (var item in m.videos)
                {
                    await (await guidFolder.GetFileAsync(item.guid + ".json")).DeleteAsync();
                }

                StorageFolder videoFolder = await DownFolder.GetFolderAsync(m.id);

                await videoFolder.DeleteAsync();

                //folderList.Remove(folderList.First(x => x.id == m.folderinfo.id));
                //await SetfolderList();
                //var file = await videoFolder.GetFileAsync(m.videoinfo.downGUID + ".json");
                //string filedata = await FileIO.ReadTextAsync(file);
                //GuidModel gm = JsonConvert.DeserializeObject<GuidModel>(filedata);
                //var fo = await StorageFolder.GetFolderFromPathAsync(gm.path);
                //await fo.DeleteAsync(StorageDeleteOption.Default);
                //await file.DeleteAsync(StorageDeleteOption.Default);

            }
            catch (Exception ex)
            {
                await new MessageDialog("删除文件失败\r\n" + ex.Message).ShowAsync();
            }


        }
        public static async Task DeleteFile(DMM m)
        {
            try
            {
              
                StorageFolder guidFolder = await DownFolder.CreateFolderAsync("GUID", CreationCollisionOption.OpenIfExists);

                await (await guidFolder.GetFileAsync(m.guid + ".json")).DeleteAsync();



                StorageFolder videoFolder = await StorageFolder.GetFolderFromPathAsync(m.folderPath);
                await videoFolder.DeleteAsync();

                //folderList.Remove(folderList.First(x => x.id == m.folderinfo.id));
                //await SetfolderList();
                //var file = await videoFolder.GetFileAsync(m.videoinfo.downGUID + ".json");
                //string filedata = await FileIO.ReadTextAsync(file);
                //GuidModel gm = JsonConvert.DeserializeObject<GuidModel>(filedata);
                //var fo = await StorageFolder.GetFolderFromPathAsync(gm.path);
                //await fo.DeleteAsync(StorageDeleteOption.Default);
                //await file.DeleteAsync(StorageDeleteOption.Default);

            }
            catch (Exception ex)
            {
                await new MessageDialog("删除文件失败\r\n" + ex.Message).ShowAsync();
            }


        }

        public static async void UpdateDowningStatus()
        {
            try
            {
                bool use = SettingHelper.Get_Use4GDown();

                var downloading = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(group);
                if (downloading.Count == 0)
                {
                    return;
                }
                foreach (var item in downloading)
                {
                    if (use)
                    {
                        
                        item.CostPolicy = BackgroundTransferCostPolicy.Always;
                    }
                    else
                    {
                        item.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }



        }

        public static List<string> contents = new List<string>();
    }
    public class DM
    {
        public string id { get; set; }
        public BitmapImage img { get; set; }
        public List<DMM> videos { get; set; }
        public string path { get; set; }

    }
    public class DMM
    {
        public string path { get; set; }
        public string dmPath { get; set; }
        public string name { get; set; }
        public string mid { get; set; }
        public string aid { get; set; }
        public string folderPath { get; set; }
        public string guid { get; set; }
        public bool isbangumi{ get; set; }
    }

    public class GuidModel
    {
        public string path { get; set; }
        public string guid { get; set; }
        public string mid { get; set; }
    }
    //下载前创建此Model,用于读取视频
    public class FolderListModel
    {
        public string id { get; set; }
        public string thumb { get; set; }
        public string folderPath { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public bool isbangumi { get; set; }
    }
    public class VideoListModel
    {
        public string id { get; set; }
        public string mid { get; set; }
        public string videoPath { get; set; }
        public string DanmuPath { get; set; }
        public int part { get; set; }
        public string partTitle { get; set; }
        public string videoUrl { get; set; }
        public string downGUID { get; set; }
        public string folderPath { get; set; }
        public string title { get; set; }
        public bool downstatus { get; set; }
    }

    public class DownloadModel
    {
        public FolderListModel folderinfo { get; set; }
        public VideoListModel videoinfo { get; set; }
        public HandleModel handel { get; set; }

    }

    public class DownloadedModel : INotifyPropertyChanged
    {
        public FolderListModel folderinfo { get; set; }
        public List<VideoListModel> videoinfo { get; set; }
        public int count { get; set; }
        public int downedcount { get; set; }
        public BitmapImage _img;
        public BitmapImage img
        {
            get { return _img; }
            set
            {
                _img = value;
                thisPropertyChanged("img");
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


    public class HandleModel : INotifyPropertyChanged
    {

        public CancellationTokenSource cts = new CancellationTokenSource();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        private DownloadOperation _downOp;
        public DownloadOperation downOp
        {
            get { return _downOp; }
            set
            {
                _downOp = value;
            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                thisPropertyChanged("Progress");
            }
        }

        private string _Size;
        public string Size
        {
            get { return _Size; }
            set
            {
                _Size = ((Convert.ToDouble(value) / 1024 / 1024)).ToString("0.0") + "M/" + (Convert.ToDouble(downOp.Progress.TotalBytesToReceive) / 1024 / 1024).ToString("0.0") + "M";
                thisPropertyChanged("Size");
            }
        }

        public string Guid { get { return downOp.Guid.ToString(); } }

        private Visibility _PauseVis;
        public Visibility PauseVis
        {
            get { return _PauseVis; }
            set { _PauseVis = value; thisPropertyChanged("PauseVis"); }
        }
        private Visibility _DownVis;
        public Visibility DownVis
        {
            get { return _DownVis; }
            set { _DownVis = value; thisPropertyChanged("DownVis"); }
        }


        public string _Status;
        public string Status
        {
            get { thisPropertyChanged("Status"); return _Status; }
            set
            {
                switch (downOp.Progress.Status)
                {
                    case BackgroundTransferStatus.Idle:
                        _Status = "空闲中";
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.Running:
                        _Status = "下载中";
                        PauseVis = Visibility.Visible;
                        DownVis = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.PausedByApplication:
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Visible;
                        _Status = "暂停中";
                        break;
                    case BackgroundTransferStatus.PausedCostedNetwork:
                        _Status = "因网络暂停，可能正在使用数据";
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.PausedNoNetwork:
                        _Status = "挂起";
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Visible;
                        break;
                    case BackgroundTransferStatus.Completed:
                        _Status = "完成";
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.Canceled:
                        _Status = "取消";
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.Error:
                        _Status = "下载错误";
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Collapsed;
                        break;
                    case BackgroundTransferStatus.PausedSystemPolicy:
                        _Status = "因系统问题暂停";
                        PauseVis = Visibility.Collapsed;
                        DownVis = Visibility.Collapsed;
                        break;
                    default:
                        _Status = "Wait...";
                        break;
                }
                thisPropertyChanged("Status");
            }
        }
    }



}
