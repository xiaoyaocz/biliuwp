using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Media.MediaProperties;
using Windows.Foundation;
using Windows.Media.Editing;

namespace BiliBili.UWP.Helper
{
    public class MediaProcessing
    {
        public event EventHandler<double> ProcessingProgressChanged;
        public event EventHandler ProcessingCompleted;
        public event EventHandler ProcessingCanceled;
        public event EventHandler<string> ProcessingError;
        public string Title { get; set; }
        public string ID { get; set; }
        public MediaProcessing()
        {
        }
        public MediaProcessing(string id, string title)
        {
            Title = title;
            ID = id;
        }

        public async Task<IAsyncActionWithProgress<double>> StartTranscodeMedia(StorageFile inputFile, StorageFile outFile, MediaEncodingProfile profile)
        {

            MediaTranscoder transcoder = new MediaTranscoder();
            PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(inputFile, outFile, profile);
            if (prepareOp.CanTranscode)
            {
                var transcodeOp = prepareOp.TranscodeAsync();

                transcodeOp.Progress += new AsyncActionProgressHandler<double>((asyncInfo, e) =>
                {
                    ProcessingProgressChanged?.Invoke(this, e);
                });
                transcodeOp.Completed += new AsyncActionWithProgressCompletedHandler<double>((asyncInfo, status) =>
                {
                    asyncInfo.GetResults();
                    switch (status)
                    {
                        case AsyncStatus.Canceled:
                            ProcessingCanceled?.Invoke(this, new EventArgs());
                            break;
                        case AsyncStatus.Completed:
                            ProcessingCompleted?.Invoke(this, new EventArgs());
                            break;
                        case AsyncStatus.Started:
                            break;
                        case AsyncStatus.Error:
                        default:
                            ProcessingError?.Invoke(this, "转码失败");
                            break;
                    }
                });
                return transcodeOp;
            }
            else
            {
                switch (prepareOp.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        ProcessingError?.Invoke(this, "转码失败:找不到编解码器");
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        ProcessingError?.Invoke(this, "转码失败:配置文件无效");
                        break;
                    default:
                        ProcessingError?.Invoke(this, "转码失败:未知错误");
                        break;
                }
                return null;
            }

        }

        public async Task<IAsyncOperationWithProgress<TranscodeFailureReason, double>> StartCompositionMedia(IList<StorageFile> inputFiles, StorageFile outFile, MediaEncodingProfile profile = null)
        {
            MediaComposition composition = new MediaComposition();

            foreach (var item in inputFiles)
            {
                var clip = await MediaClip.CreateFromFileAsync(item);
                composition.Clips.Add(clip);
            }

            IAsyncOperationWithProgress<TranscodeFailureReason, double> saveOperation = null;
            if (profile != null)
            {
                saveOperation = composition.RenderToFileAsync(outFile, MediaTrimmingPreference.Fast, profile);
            }
            else
            {
                saveOperation = composition.RenderToFileAsync(outFile, MediaTrimmingPreference.Fast);
            }
            saveOperation.Progress = new AsyncOperationProgressHandler<TranscodeFailureReason, double>((info, progress) =>
            {
                ProcessingProgressChanged?.Invoke(this, progress);
            });
            saveOperation.Completed = new AsyncOperationWithProgressCompletedHandler<TranscodeFailureReason, double>((info, status) =>
            {
                if (status == AsyncStatus.Canceled)
                {
                    ProcessingCanceled?.Invoke(this, new EventArgs());
                    return;
                }
                var results = info.GetResults();
                if (results != TranscodeFailureReason.None || status != AsyncStatus.Completed)
                {
                    ProcessingError?.Invoke(this, "合并失败:未知错误");
                }
                else
                {
                    ProcessingCompleted?.Invoke(this, new EventArgs());
                }
            });

            return saveOperation;
        }

        public async Task<IAsyncOperationWithProgress<TranscodeFailureReason, double>> StartCompositionDashMedia(IList<StorageFile> inputFiles, StorageFile outFile, MediaEncodingProfile profile = null)
        {
            MediaComposition composition = new MediaComposition();

            var clip = await MediaClip.CreateFromFileAsync(inputFiles.FirstOrDefault(x => x.Name == "video.m4s"));
            composition.Clips.Add(clip);

            var backgroundTrack = await BackgroundAudioTrack.CreateFromFileAsync(inputFiles.FirstOrDefault(x => x.Name == "audio.m4s"));
            composition.BackgroundAudioTracks.Add(backgroundTrack);

            IAsyncOperationWithProgress<TranscodeFailureReason, double> saveOperation = null;
            if (profile != null)
            {
                saveOperation = composition.RenderToFileAsync(outFile, MediaTrimmingPreference.Fast, profile);
            }
            else
            {
                saveOperation = composition.RenderToFileAsync(outFile, MediaTrimmingPreference.Fast);
            }
            saveOperation.Progress = new AsyncOperationProgressHandler<TranscodeFailureReason, double>((info, progress) =>
            {
                ProcessingProgressChanged?.Invoke(this, progress);
            });
            saveOperation.Completed = new AsyncOperationWithProgressCompletedHandler<TranscodeFailureReason, double>((info, status) =>
            {
                if (status == AsyncStatus.Canceled)
                {
                    ProcessingCanceled?.Invoke(this, new EventArgs());
                    return;
                }
                var results = info.GetResults();
                if (results != TranscodeFailureReason.None || status != AsyncStatus.Completed)
                {
                    ProcessingError?.Invoke(this, "合并失败:未知错误");
                }
                else
                {
                    ProcessingCompleted?.Invoke(this, new EventArgs());
                }
            });

            return saveOperation;
        }

    }
}
