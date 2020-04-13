using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Windows.Graphics.Display;
using Windows.Media.Devices;
using BiliBili.UWP.Helper;
using Windows.Devices.Sensors;
using Windows.UI.Popups;
using Windows.Media;
using BiliBili.UWP.Pages.User;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class QRPage : Page
    {

        private Result _result;
        private readonly MediaCapture _mediaCapture = new MediaCapture();
        private DispatcherTimer _timer;
        private bool IsBusy;
        public QRPage()
        {
            this.InitializeComponent();
            LineStoryboard.Begin();



        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            InitVideoCapture();
            InitVideoTimer();
        }
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _timer.Stop();
          
            if (IsBusy)
            {
                await _mediaCapture.StopPreviewAsync();
            }
            IsBusy = false;


        }

        private void InitVideoTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private async void _timer_Tick(object sender, object e)
        {
            try
            {
                Debug.WriteLine(@"[INFO]开始扫描 -> " + DateTime.Now.ToString());
                if (!IsBusy)
                {
                    IsBusy = true;
                    IRandomAccessStream stream = new InMemoryRandomAccessStream();
                    await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

                    var writeableBmp = await ReadBitmap(stream, ".jpg");

                    await Task.Factory.StartNew(async () => { await ScanBitmap(writeableBmp); });
                }
                IsBusy = false;
                await Task.Delay(100);
            }
            catch (Exception)
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 保存照片
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="photoOrientation"></param>
        /// <returns></returns>
        private static async Task ReencodeAndSavePhotoAsync(IRandomAccessStream stream, string fileName, PhotoOrientation photoOrientation = PhotoOrientation.Normal)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                var file = await KnownFolders.PicturesLibrary.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    // Set the orientation of the capture
                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };
                    await encoder.BitmapProperties.SetPropertiesAsync(properties);

                    await encoder.FlushAsync();
                }
            }
        }

        static Guid DecoderIDFromFileExtension(string strExtension)
        {
            Guid encoderId;
            switch (strExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    encoderId = BitmapDecoder.JpegDecoderId;
                    break;
                case ".bmp":
                    encoderId = BitmapDecoder.BmpDecoderId;
                    break;
                case ".png":
                default:
                    encoderId = BitmapDecoder.PngDecoderId;
                    break;
            }
            return encoderId;
        }

        public static Size MaxSizeSupported = new Size(4000, 3000);

        /// <summary>
        /// 读取照片流 转为WriteableBitmap给二维码解码器
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async static Task<WriteableBitmap> ReadBitmap(IRandomAccessStream fileStream, string type)
        {
            WriteableBitmap bitmap = null;
            try
            {
                Guid decoderId = DecoderIDFromFileExtension(type);

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(decoderId, fileStream);
                BitmapTransform tf = new BitmapTransform();

                uint width = decoder.OrientedPixelWidth;
                uint height = decoder.OrientedPixelHeight;
                double dScale = 1;

                if (decoder.OrientedPixelWidth > MaxSizeSupported.Width || decoder.OrientedPixelHeight > MaxSizeSupported.Height)
                {
                    dScale = Math.Min(MaxSizeSupported.Width / decoder.OrientedPixelWidth, MaxSizeSupported.Height / decoder.OrientedPixelHeight);
                    width = (uint)(decoder.OrientedPixelWidth * dScale);
                    height = (uint)(decoder.OrientedPixelHeight * dScale);

                    tf.ScaledWidth = (uint)(decoder.PixelWidth * dScale);
                    tf.ScaledHeight = (uint)(decoder.PixelHeight * dScale);
                }


                bitmap = new WriteableBitmap((int)width, (int)height);



                PixelDataProvider dataprovider = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, tf,
                    ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
                byte[] pixels = dataprovider.DetachPixelData();

                Stream pixelStream2 = bitmap.PixelBuffer.AsStream();

                pixelStream2.Write(pixels, 0, pixels.Length);
                //bitmap.SetSource(fileStream);
            }
            catch
            {
            }

            return bitmap;
        }

        /// <summary>
        /// 解析二维码图片
        /// </summary>
        /// <param name="writeableBmp">图片</param>
        /// <returns></returns>
        private async Task ScanBitmap(WriteableBitmap writeableBmp)
        {
            try
            {
                var barcodeReader = new BarcodeReader
                {
                    AutoRotate = true,
                    Options = new ZXing.Common.DecodingOptions { TryHarder = true }
                };
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _result = barcodeReader.Decode(writeableBmp);
                });



                if (_result != null)
                {

                    Debug.WriteLine(@"[INFO]扫描到二维码:{result}   ->" + _result.Text);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        string ban = Regex.Match(_result.Text, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
                        if (ban.Length != 0)
                        {
                            //args.Handled = true;
                            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban.Replace("/", ""));
                            this.Frame.GoBack();
                            return;
                        }
                        string ban2 = Regex.Match(_result.Text, @"^http://www.bilibili.com/bangumi/i/(.*?)$").Groups[1].Value;
                        if (ban2.Length != 0)
                        {
                            //args.Handled = true;
                            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), ban2.Replace("/", ""));
                            this.Frame.GoBack();
                            // this.Frame.Navigate(typeof(BanInfoPage), ban2.Replace("/", ""));
                            return;
                        }
                        //bilibili://?av=4284663
                        string ban3 = Regex.Match(_result.Text, @"^bilibili://?av=(.*?)$").Groups[1].Value;
                        if (ban3.Length != 0)
                        {
                            //args.Handled = true;
                            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), ban3.Replace("/", ""));
                            this.Frame.GoBack();
                            //this.Frame.Navigate(typeof(VideoViewPage), ban3.Replace("/", ""));
                            return;
                        }


                        string ban4 = Regex.Match(_result.Text + "/", @"space.bilibili.com/(.*?)/").Groups[1].Value;
                        if (ban4.Length != 0)
                        {
                            //args.Handled = true;
                            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), ban4.Replace("/", ""));
                            this.Frame.GoBack();
                            //this.Frame.Navigate(typeof(VideoViewPage), ban3.Replace("/", ""));
                            return;
                        }

                        string ban5 = Regex.Match(_result.Text + "/", @"oauthKey=(.*?)/").Groups[1].Value;
                        if (ban5.Length != 0)
                        {
                            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "https://passport.bilibili.com/qrcode/h5/login?oauthKey=" + ban5.Replace("/", ""));
                            this.Frame.GoBack();
                            //if (!ApiHelper.IsLogin())
                            //{
                            //    await new MessageDialog("手机需先登录！").ShowAsync();
                            //    return;
                            //}
                            ////args.Handled = true;
                            //pr_Laod.Visibility = Visibility.Visible;
                            //LoginStatus dts = await ApiHelper.QRLogin(ban5.Replace("/", ""));
                            //if (dts==LoginStatus.Succeed)
                            //{
                            //    this.Frame.GoBack();
                            //}
                            //else
                            //{
                            //    await new MessageDialog("登录失败").ShowAsync();
                            //}
                            //pr_Laod.Visibility = Visibility.Collapsed;
                            //MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(UserInfoPage), ban5.Replace("/", ""));
                            //this.Frame.Navigate(typeof(VideoViewPage), ban3.Replace("/", ""));
                            return;
                        }
                        //text .Text= args.Uri.AbsoluteUri;

                        string live = Regex.Match(_result.Text, @"^bilibili://live/(.*?)$").Groups[1].Value;
                        if (live.Length != 0)
                        {
                            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), live);

                            return;
                        }

                        string live2 = Regex.Match(_result.Text + "a", @"live.bilibili.com/(.*?)a").Groups[1].Value;
                        if (live2.Length != 0)
                        {
                            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), live2.Replace("a", ""));

                            return;
                        }

                        if (Regex.IsMatch(_result.Text, "/video/av(.*)?[/|+](.*)?"))
                        {

                            string a = Regex.Match(_result.Text, "/video/av(.*)?[/|+](.*)?").Groups[1].Value;
                            //this.Frame.Navigate(typeof(VideoViewPage), a);
                            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), a);
                            this.Frame.GoBack();
                            return;
                        }


                        tbkResult.Text = "无法识别的类型";
                    });
                }
            }
            catch (Exception)
            {
            }
        }


        private async void InitVideoCapture()
        {

            ///摄像头的检测
            var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);
            if (cameraDevice == null)
            {
                await new MessageDialog("没有找到摄像头！").ShowAsync();
                Debug.WriteLine("No camera device found!");
                return;
            }


            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MediaCategory = MediaCategory.Other,
                AudioProcessing = Windows.Media.AudioProcessing.Default,
                PhotoCaptureSource = PhotoCaptureSource.VideoPreview,
                VideoDeviceId = cameraDevice.Id
            };
            await _mediaCapture.InitializeAsync(settings);

            var focusControl = _mediaCapture.VideoDeviceController.FocusControl;
            if (focusControl.Supported)
            {
                var focusSettings = new FocusSettings()
                {
                    Mode = focusControl.SupportedFocusModes.FirstOrDefault(f => f == FocusMode.Continuous),
                    DisableDriverFallback = true,
                    AutoFocusRange = focusControl.SupportedFocusRanges.FirstOrDefault(f => f == AutoFocusRange.FullRange),
                    Distance = focusControl.SupportedFocusDistances.FirstOrDefault(f => f == ManualFocusDistance.Nearest)
                };

                //设置聚焦，最好使用FocusMode.Continuous，否则影响截图会很模糊，不利于识别
                focusControl.Configure(focusSettings);
            }
            if (!SettingHelper.IsPc())
            {
                _mediaCapture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);

            }


            VideoCapture.Source = _mediaCapture;
            VideoCapture.FlowDirection = FlowDirection.LeftToRight;
            await _mediaCapture.StartPreviewAsync();
            //SimpleOrientationSensor sensor = SimpleOrientationSensor.GetDefault();
            //sensor.OrientationChanged += (s, arg) =>
            //{
            //    switch (arg.Orientation)
            //    {
            //        case SimpleOrientation.Rotated90DegreesCounterclockwise:
            //            _mediaCapture.SetPreviewRotation(VideoRotation.None);
            //            break;
            //        case SimpleOrientation.Rotated180DegreesCounterclockwise:
            //        case SimpleOrientation.Rotated270DegreesCounterclockwise:
            //            _mediaCapture.SetPreviewRotation(VideoRotation.Clockwise180Degrees);
            //            break;
            //        default:
            //            _mediaCapture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
            //            break;
            //    }
            //};

            try
            {
                if (_mediaCapture.VideoDeviceController.FlashControl.Supported)
                {
                    //关闭闪光灯
                    _mediaCapture.VideoDeviceController.FlashControl.Enabled = false;
                }
            }
            catch
            {
            }

            if (focusControl.Supported)
            {
                //开始聚焦
                await focusControl.FocusAsync();
            }


            //var angle = CameraRotationHelper.ConvertSimpleOrientationToClockwiseDegrees(_rotationHelper.GetUIOrientation());
            // var transform = new RotateTransform { Angle = 90 };
            // VideoCapture.RenderTransform = transform;
        }

        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }
    }
}
