using BiliBili.UWP.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili.UWP.Controls
{

    public sealed partial class VideoProcessingDialog : ContentDialog
    {
        MediaProcessing mediaProcessing;
        public StorageFolder storageFolder { get; set; }
        public StorageFile outFile { get; set; }
        public string ID { get; set; }
        public VideoProcessingDialog(string id, string title,Windows.Storage.StorageFolder folder)
        {
            this.InitializeComponent();
            ID = id;
            mediaProcessing = new MediaProcessing(id, title);
            Title = "合并:" + title;
            storageFolder = folder;
            mediaProcessing.ProcessingCanceled += MediaProcessing_ProcessingCanceled;
            mediaProcessing.ProcessingCompleted += MediaProcessing_ProcessingCompleted;
            mediaProcessing.ProcessingProgressChanged += MediaProcessing_ProcessingProgressChanged;
            mediaProcessing.ProcessingError += MediaProcessing_ProcessingError;
        }

        private async void MediaProcessing_ProcessingError(object sender, string e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                statusText.Text = e;
                prBar.Value = 0;
                prText.Text = "进度:0%";
                PrimaryButtonText = "开始";
            });
        }

        private async void MediaProcessing_ProcessingProgressChanged(object sender, double e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                prBar.Value = e;
                prText.Text = $"进度:{e.ToString("0.00")}%";
            });
        }

        private async void MediaProcessing_ProcessingCompleted(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,async () =>
            {
                statusText.Text = "开始删除文件";
                try
                {
                    var files = await storageFolder.GetFilesAsync();
                    foreach (var item in files.Where(x => x.FileType == ".flv"||x.FileType==".m4s"))
                    {
                        try
                        {
                            await item.DeleteAsync();
                        }
                        catch (Exception)
                        {
                        }
                        
                    }
                }
                catch (Exception)
                {
                }
                statusText.Text = "视频处理完成";
                PrimaryButtonText = "开始";
            });
        }

        private async void MediaProcessing_ProcessingCanceled(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,async () =>
            {
                statusText.Text = "任务已经取消";
                prBar.Value = 0;
                prText.Text = "进度:0%";
                PrimaryButtonText = "开始";
                try
                {
                    await outFile.DeleteAsync();
                }
                catch (Exception)
                {
                }
                
            });
        }

        IAsyncOperationWithProgress<TranscodeFailureReason, double> operationWithProgress;
        public async void Start()
        {
            PrimaryButtonText = "取消";
            statusText.Text = "开始读取文件";
            var files = await storageFolder.GetFilesAsync();
            outFile = await storageFolder.CreateFileAsync("000.mp4", CreationCollisionOption.ReplaceExisting);
            statusText.Text = "正在合并视频...";
            if (files.FirstOrDefault(x=>x.FileType==".m4s")!=null)
            {
                operationWithProgress = await mediaProcessing.StartCompositionDashMedia(files.Where(x => x.FileType == ".m4s").OrderBy(x => x.DisplayName).ToList(), outFile);
            }
            else
            {
                operationWithProgress = await mediaProcessing.StartCompositionMedia(files.Where(x => x.FileType == ".flv").OrderBy(x => x.DisplayName).ToList(), outFile);
            }

        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (PrimaryButtonText == "开始")
            {
                Start();
            }
            else
            {
                operationWithProgress.Cancel();
                PrimaryButtonText = "开始";
            }

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
            //if (PrimaryButtonText == "取消")
            //{
            //    var dialog = new MessageDialog("任务正在进行，确定要取消吗");
            //    dialog.Commands.Add(new UICommand("确定", cmd => {
            //        operationWithProgress.Cancel();
            //    }, commandId: 0));
            //    dialog.Commands.Add(new UICommand("取消", cmd =>
            //    {
            //        args.Cancel = true;
            //    }, commandId: 1));
            //    await dialog.ShowAsync();
            //    return;
            //}

        }
    }
}
