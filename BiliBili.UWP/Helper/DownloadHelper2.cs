using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Newtonsoft.Json;
using Newtonsoft;
using Windows.Networking.BackgroundTransfer;
using System.IO;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using System.Diagnostics;
using BiliBili.UWP.Pages;
using NSDanmaku;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Controls;

namespace BiliBili.UWP.Helper
{
    public enum DownloadMode
    {
        Video = 0,
        Anime = 1,
        Movie = 2
    }
    /*
     * create 2018/2/13
     * 支持多段flv视频的下载
     */
    /// <summary>
    /// 新的下载帮助类
    /// </summary>
    public class DownloadHelper2
    {


        /*
         * 文件路径
         * 选择路径
         *  /AID/SID
         *   Info.json 存储任务信息
         *   /CID/EPID
         *    video 0.flv/1.flv/2.flv/3.flv
         *    CID.json
         *    CID.xml
         */
        public static Dictionary<string, string> downloadeds = new Dictionary<string, string>();
        public static BackgroundTransferGroup group = BackgroundTransferGroup.CreateGroup("BILIBILIUWP");//下载组，方便管理
        public List<string> downedList;
        public static List<VideoProcessingDialog> videoProcessingDialogs=new List<VideoProcessingDialog>();
        public static async Task CreateDownload(DownloadTaskModel m, List<DownloadUrlInfo> downloadUrls)
        {
            //var urls = await PlayurlHelper.GetVideoUrl_Download(m);
            if (downloadUrls == null || downloadUrls.Count == 0)
            {
                await new MessageDialog(m.epTitle + " 无法读取到下载地址").ShowAsync();
                return;
                //throw new Exception(m.epTitle + " 无法读取到下载地址");
            }
            try
            {
                var folder = await GetCIDFolder(m);
                await Task.Run(async () =>
                {
                    await SetVideoInfo(m, folder);
                    await SetPartInfo(m, folder);
                    await DownloadDanmu(m.cid, folder);
                    await DownThumb(m.thumb, await folder.GetParentAsync());
                });
                for (int i = 0; i < downloadUrls.Count; i++)
                {
                    CreateDown(m, i, downloadUrls[i], folder);
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

        }

        private static async void CreateDown(DownloadTaskModel m, int index, DownloadUrlInfo url, StorageFolder folder)
        {

            BackgroundDownloader downloader = new BackgroundDownloader();
            foreach (var item in url.Headers)
            {
                downloader.SetRequestHeader(item.Key, item.Value);
            }
            //downloader.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
            //if (!url.Contains("360.cn"))
            //{
            //    downloader.SetRequestHeader("Origin", "https://www.bilibili.com/");
            //    downloader.SetRequestHeader("Referer", "https://www.bilibili.com/");
            //}
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
            //创建视频文件
            var fileName = index.ToString("000");
            var filetype = ".flv";
            if (url.Url.Contains(".mp4"))
            {
                filetype = ".mp4";
            }
            if (url.Format=="dash")
            {
                filetype = ".m4s";
                fileName = url.DashFileType;
            }
         
            StorageFile file = await folder.CreateFileAsync(fileName + filetype, CreationCollisionOption.OpenIfExists);
            DownloadOperation downloadOp = downloader.CreateDownload(new Uri(url.Url), file);
            //设置下载策略
            if (SettingHelper.Get_Use4GDown())
            {
                downloadOp.CostPolicy = BackgroundTransferCostPolicy.Always;
            }
            else
            {
                downloadOp.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
            }
            SqlHelper.InsertDownload(new DownloadGuidClass()
            {
                guid = downloadOp.Guid.ToString(),
                cid = m.cid,
                index = index,
                aid = (m.downloadMode == DownloadMode.Anime) ? m.sid : m.avid,
                eptitle = m.epTitle,
                title = m.title,
                mode = (m.downloadMode == DownloadMode.Anime) ? "anime" : "video"
            });
            try
            {
                await downloadOp.StartAsync();
            }
            catch (Exception)
            {
            }


        }

        private static async Task DownloadDanmu(string cid, StorageFolder folder)
        {
            try
            {
                if (await ExistsFile(folder.Path + @"\" + cid + ".xml"))
                {
                    return;
                }
                string results = await new NSDanmaku.Helper.DanmakuParse().GetBiliBili(Convert.ToInt64(cid));
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

        public static async Task UpdateDanmu(string path, string cid)
        {
            try
            {
                string results = await new NSDanmaku.Helper.DanmakuParse().GetBiliBili(Convert.ToInt64(cid));

                StorageFile fileWrite = await StorageFile.GetFileFromPathAsync(path);
                await FileIO.WriteTextAsync(fileWrite, results);

            }
            catch (Exception ex)
            {
                await new MessageDialog(cid + "更新失败").ShowAsync();
                //return null;
            }
        }

        private static async Task DownThumb(string url, StorageFolder folder)
        {
            try
            {
                if (await ExistsFile(folder.Path + @"\thumb.jpg"))
                {
                    return;
                }
                StorageFile file = await folder.CreateFileAsync("thumb.jpg", CreationCollisionOption.OpenIfExists);
                IBuffer bu = await WebClientClass.GetBuffer(new Uri(url));
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, bu);
                await CachedFileManager.CompleteUpdatesAsync(file);

            }
            catch (Exception)
            {
                //return null;
            }

        }


        private static async Task<StorageFolder> GetCIDFolder(DownloadTaskModel m)
        {
            string setting = SettingHelper.Get_DownPath();

            StorageFolder DownFolder = null;
            try
            {
                if (SettingHelper.Get_CustomDownPath() && setting != "系统视频库")
                {

                    string mruFirstToken = StorageApplicationPermissions.MostRecentlyUsedList.Entries.FirstOrDefault(x => x.Metadata == setting).Token;
                    var settingFolder = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruFirstToken);
                    DownFolder = await settingFolder.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
                }
                else
                {
                    DownFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
                }
            }
            catch (Exception)
            {
                DownFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
            }


            StorageFolder avidFolder = null;
            if (m.downloadMode == DownloadMode.Video)
            {
                avidFolder = await DownFolder.CreateFolderAsync(m.avid, CreationCollisionOption.OpenIfExists);
            }
            else
            {
                avidFolder = await DownFolder.CreateFolderAsync("s_" + m.sid, CreationCollisionOption.OpenIfExists);
            }

            StorageFolder cidFolder = await avidFolder.CreateFolderAsync(m.cid, CreationCollisionOption.OpenIfExists);

            return cidFolder;
        }

        public static async Task<StorageFolder> GetDownloadFolder()
        {
            string setting = SettingHelper.Get_DownPath();

            StorageFolder DownFolder = null;
            try
            {
                if (SettingHelper.Get_CustomDownPath() && setting != "系统视频库")
                {

                    string mruFirstToken = StorageApplicationPermissions.MostRecentlyUsedList.Entries.First(x => x.Metadata == setting).Token;
                    var settingFolder = await StorageApplicationPermissions.MostRecentlyUsedList.GetFolderAsync(mruFirstToken);
                    DownFolder = await settingFolder.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
                }
                else
                {
                    DownFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
                }
            }
            catch (Exception)
            {
                DownFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
            }



            return DownFolder;
        }


        private static async Task SetVideoInfo(DownloadTaskModel m, StorageFolder folder)
        {
            try
            {
                var f = await folder.GetParentAsync();
                StorageFile file = null;
                if (await ExistsFile(f.Path + @"/info.json"))
                {
                    return;
                }
                else
                {
                    file = await f.CreateFileAsync("info.json", CreationCollisionOption.OpenIfExists);
                }
                DownloadVideonInfoModel downloadVideonInfoModel = new DownloadVideonInfoModel()
                {
                    thumb = m.thumb,
                    title = m.title
                };
                if (m.downloadMode == DownloadMode.Video)
                {
                    downloadVideonInfoModel.id = m.avid;
                    downloadVideonInfoModel.type = "video";
                }
                else
                {
                    downloadVideonInfoModel.id = m.sid;
                    downloadVideonInfoModel.type = "anime";
                }

                var infoJson = JsonConvert.SerializeObject(downloadVideonInfoModel);

                await FileIO.WriteTextAsync(file, infoJson);
            }
            catch (Exception)
            {
            }

        }
        private static async Task SetPartInfo(DownloadTaskModel m, StorageFolder folder)
        {
            try
            {
                StorageFile file = null;
                if (await ExistsFile(folder.Path + @"/info.json"))
                {
                    return;
                }
                else
                {
                    file = await folder.CreateFileAsync("info.json", CreationCollisionOption.OpenIfExists);
                }
                DownloadPartnInfoModel downloadVideonInfoModel = new DownloadPartnInfoModel()
                {
                    index = m.epIndex,
                    title = m.epTitle,
                    cid = m.cid,
                    path = folder.Path,
                    epid = m.epid,
                    is_dash=m.is_dash
                };

                var infoJson = JsonConvert.SerializeObject(downloadVideonInfoModel);

                await FileIO.WriteTextAsync(file, infoJson);
            }
            catch (Exception)
            {
            }

        }

        public async static Task<bool> ExistsFile(string path)
        {
            //File.Exists始终返回False。。。
            var dir = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);
            StorageFile file;
            StorageFolder folder =await StorageFolder.GetFolderFromPathAsync(dir);
            file = await folder.TryGetItemAsync(fileName) as StorageFile;
            return file != null;
        }

        public static async void UpdateDowningStatus()
        {
            try
            {
                bool use = SettingHelper.Get_Use4GDown();

                var downloading = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper2.group);
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
        public async static Task<ulong> GetFileSize(string path)
        {
            ulong re = 0;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                re = (await file.GetBasicPropertiesAsync()).Size;
            }
            catch (Exception)
            {
            }
            return re;
        }
       

        public static async Task DeleteFolder(string id, string cid, string mode)
        {
            try
            {
                if (mode == "anime")
                {
                    id = "s_" + id;
                }
                var folder = await DownloadHelper2.GetDownloadFolder();
                var avFolder = await folder.GetFolderAsync(id);
                var videoFolder = await avFolder.GetFolderAsync(cid);
                await videoFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {

            }
        }

        public static async Task DeleteFolder(string id, string mode)
        {
            try
            {
                if (mode == "anime")
                {
                    id = "s_" + id;
                }
                var folder = await DownloadHelper2.GetDownloadFolder();
                var avFolder = await folder.GetFolderAsync(id);
                await avFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception)
            {
            }


        }

        public static async Task LoadDowned()
        {
            Dictionary<string, string> valuePairs = new Dictionary<string, string>();
            var folder = await DownloadHelper2.GetDownloadFolder();
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

                                var flag = false;
                                var files = (await item1.GetFilesAsync()).Where(x => x.FileType == ".mp4" || x.FileType == ".flv" || x.FileType == ".m4s");
                                if (files.Count() != 0)
                                {
                                    //DownloadHelper2.GetFileSize(x.Path).Result
                                    foreach (var subfile in files)
                                    {
                                        if (await DownloadHelper2.GetFileSize(subfile.Path) == 0)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (flag)
                                    {
                                        break;
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
                                        title = video.title
                                    };
                                    if (!valuePairs.ContainsKey(downloadPartnInfoModel.cid))
                                    {
                                        valuePairs.Add(downloadPartnInfoModel.cid, item1.Path);
                                    }
                                    video.videolist.Add(part);
                                }
                            }
                        }

                        if (video.videolist.Count != 0)
                        {
                            video.videolist = video.videolist.OrderBy(x => x.index).ToList();

                        }
                    }
                }
                downloadeds = valuePairs;
            }
            catch (Exception ex)
            {
                await new MessageDialog("无法读取已经下载完成的视频").ShowAsync();
            }


        }


    }

    public class DownloadTaskModel
    {
        public DownloadMode downloadMode { get; set; }
        public string avid { get; set; }//视频用AV号
        public string sid { get; set; }//动漫用Sid
        public string title { get; set; }
        public string thumb { get; set; }


        public string cid { get; set; }
        public string epid { get; set; }
        public int epIndex { get; set; }
        public string epTitle { get; set; }
        public int quality { get; set; }
        public bool is_dash { get; set; }
    }
    public class DownloadVideonInfoModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string thumb { get; set; }
        public string type { get; set; }
    }
    public class DownloadPartnInfoModel
    {
        public string cid { get; set; }
        public string title { get; set; }
        public string path { get; set; }
        public string epid { get; set; }
        public int index { get; set; }
        public bool is_dash { get; set; } = false;
    }

}
