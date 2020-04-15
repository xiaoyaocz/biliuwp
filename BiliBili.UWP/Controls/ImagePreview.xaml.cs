using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BiliBili.UWP.Controls
{
    public sealed partial class ImagePreview : UserControl
    {
        private Popup m_Popup;

        private List<string> _ImgUrl;
        private int _index;
        public ImagePreview()
        {
            this.InitializeComponent();
            m_Popup = new Popup();
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            m_Popup.Child = this;
            this.Loaded += NotifyPopup_Loaded;
            this.Unloaded += NotifyPopup_Unloaded;

        }
       
        private void NotifyPopup_Loaded(object sender, RoutedEventArgs e)
        {

            LoadImage(_ImgUrl, _index);

            Window.Current.SizeChanged += Current_SizeChanged; ;
        }
        private void LoadImage(List<string> img,int index)
        {
            List<ImageModel> ls = new List<ImageModel>();

            foreach (var item in img)
            {
                Image image = new Image() {
                    Source=new BitmapImage(new Uri(item.Replace("@300w_300h_1e_1c.jpg", "").Replace("@300w.jpg",""))),
                    HorizontalAlignment= HorizontalAlignment.Center,
                    VerticalAlignment= VerticalAlignment.Center
                };
            
                ls.Add(new ImageModel() {
                     url=item,
                    image=image
                });
            }

            imgs.ItemsSource = ls;
            imgs.SelectedIndex = index;


        }
        private void SbOut_Completed(object sender, object e)
        {
            this.m_Popup.IsOpen = false;
        }
        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.Width = e.Size.Width;
            this.Height = e.Size.Height;
        }

        private void NotifyPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }


        public ImagePreview(List<string> url,int index) : this()
        {
            this._ImgUrl = url;
            _index = index;
            txt_Load.Text = "图片加载中...";
            imgs.ItemsSource = null;
        }



        public void Show()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                mainGrid.Margin = new Thickness(0, 24, 0, 0);
            }

            this.m_Popup.IsOpen = true;
            this.sbIn.Begin();
        }
        private void Hide()
        {

            this.sbOut.Begin();
            this.sbOut.Completed += SbOut_Completed1;


        }

        private void SbOut_Completed1(object sender, object e)
        {
            this.m_Popup.IsOpen = false;
        }


    
     
        private void sv1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private async void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileSavePicker save = new FileSavePicker();
                save.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                save.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
                save.SuggestedFileName = "bilibili_img_" + DateTime.Now.ToString();
                StorageFile file = await save.PickSaveFileAsync();
                if (file != null)
                {
                    //img_Image
                    var u = (imgs.SelectedItem as ImageModel).url.Replace("@300w_300h_1e_1c.jpg", "").Replace("@300w.jpg", "");
                    IBuffer bu = await WebClientClass.GetBuffer(new Uri(u));
                    CachedFileManager.DeferUpdates(file);
                    await FileIO.WriteBufferAsync(file, bu);
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    Utils.ShowMessageToast("保存成功");
                }

            }
            catch (Exception ex)
            {
               
                Utils.ShowMessageToast("保存失败");
            }
        }
        int RotateNum = 1;
        private void btn_Rotate_Click(object sender, RoutedEventArgs e)
        {
            if (RotateNum == 4)
            {
                RotateNum = 0;
            }
            CompositeTransform compositeTransform = new CompositeTransform()
            {
                Rotation = 90 * RotateNum
            };

            var imageViews=(imgs.SelectedItem as ImageModel).image;

            imageViews.RenderTransformOrigin = new Point(0.5, 0.5);
            imageViews.RenderTransform = compositeTransform;
            RotateNum++;
        }
        float ZoomFactor = (float)1.0;
        private void btn_ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomFactor += (float)0.2;
          
           //sv1.ChangeView(null, null, ZoomFactor);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomFactor -= (float)0.2;
            //sv1.ChangeView(null, null, ZoomFactor);
        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            //DataPackage dataPackage = new DataPackage();
            //RandomAccessStreamReference randomAccessStreamReference =
            //    RandomAccessStreamReference.CreateFromStream(_bitimg);
            //dataPackage.SetBitmap(randomAccessStreamReference);

            //Clipboard.SetContent(dataPackage);
            //Utils.ShowMessageToast("已将图片复制到剪贴板");
            //DataTransferManager.ShowShareUI();
        }


        public class ImageModel
        {
            public string url { get; set; }
            public Image image { get; set; }
        }

        private void imgs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            txt_Count.Text = (imgs.SelectedIndex + 1) + "/" + imgs.Items.Count;
        }
    }
}
