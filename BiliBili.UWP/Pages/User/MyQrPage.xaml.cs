using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyQrPage : Page
    {
        public MyQrPage()
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var par = (e.Parameter as object[])[0] as MyqrModel;
            this.DataContext = par;
            if (par.sex=="保密")
            {
                male.Visibility = Visibility.Collapsed;
                female.Visibility = Visibility.Collapsed;
                gay.Visibility = Visibility.Visible;
            }
            if (par.sex=="男")
            {
                male.Visibility = Visibility.Visible;
                female.Visibility = Visibility.Collapsed;
                gay.Visibility = Visibility.Collapsed;
            }
            if (par.sex == "女")
            {
                male.Visibility = Visibility.Collapsed;
                female.Visibility = Visibility.Visible;
                gay.Visibility = Visibility.Collapsed;
            }
            
        }

        private async void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileSavePicker save = new FileSavePicker();
                save.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                save.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
                save.SuggestedFileName = (this.DataContext as MyqrModel).name + "の二维码";
                StorageFile file = await save.PickSaveFileAsync();
                if (file != null)
                {
                    //img_Image
                    RenderTargetBitmap bitmap = new RenderTargetBitmap();
                    await bitmap.RenderAsync(qr);
                    var pixelBuffer = await bitmap.GetPixelsAsync();
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Ignore,
                             (uint)bitmap.PixelWidth,
                             (uint)bitmap.PixelHeight,
                             DisplayInformation.GetForCurrentView().LogicalDpi,
                             DisplayInformation.GetForCurrentView().LogicalDpi,
                             pixelBuffer.ToArray());
                        await encoder.FlushAsync();
                        Utils.ShowMessageToast("保存成功", 3000);
                    }

                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("保存失败", 3000);
            }
           



        }
    }
    public class MyqrModel
    {
        public string qr { get; set; }
        public string name { get; set; }
        public string photo { get; set; }
        public string sex { get; set; }
    }
}
