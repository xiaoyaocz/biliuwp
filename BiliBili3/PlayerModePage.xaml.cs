using BiliBili3.Controls;
using BiliBili3.Helper;
using BiliBili3.Pages;
using BiliBili3.Views;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili3
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayerModePage : Page
    {
        public PlayerModePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            SystemNavigationManager.GetForCurrentView().BackRequested += PlayerModePage_BackRequested; ;
            DisplayInformation.GetForCurrentView().OrientationChanged += MainPage_OrientationChanged;
        }

        private async void MainPage_OrientationChanged(DisplayInformation sender, object args)
        {

            if (sender.CurrentOrientation == DisplayOrientations.Landscape || sender.CurrentOrientation == DisplayOrientations.LandscapeFlipped || sender.CurrentOrientation == (DisplayOrientations)5)
            {
                if (SettingHelper.Get_HideStatus())
                {
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                    {
                        StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                        await statusBar.HideAsync();
                    }
                }

            }
            else
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                {
                    StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                    await statusBar.ShowAsync();

                }
            }
        }
        bool IsClicks = false;
        private async void PlayerModePage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (play_frame.CanGoBack)
            {
                e.Handled = true;
                play_frame.GoBack();
            }
            else
            {

                if (e.Handled == false)
                {
                    if (IsClicks)
                    {
                        Application.Current.Exit();
                    }
                    else
                    {
                        IsClicks = true;
                        e.Handled = true;
                        Utils.ShowMessageToast("再按一次退出应用", 1500);
                        await Task.Delay(1500);
                        IsClicks = false;
                    }
                }

            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            play_frame.Navigate(typeof(BlankPage));
            MessageCenter.ChanageThemeEvent += MessageCenter_ChanageThemeEvent;
            MessageCenter.ChangeBg += MessageCenter_ChangeBg;
            ChangeTheme();
            MessageCenter_ChangeBg();

        }

        private async void MessageCenter_ChangeBg()
        {
            if (SettingHelper.Get_CustomBG() && SettingHelper.Get_BGPath().Length != 0)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(SettingHelper.Get_BGPath());
                if (file != null)
                {
                    img_bg.Stretch = (Stretch)SettingHelper.Get_BGStretch();
                    img_bg.HorizontalAlignment = (HorizontalAlignment)SettingHelper.Get__BGHor();
                    img_bg.VerticalAlignment = (VerticalAlignment)SettingHelper.Get_BGVer();
                    img_bg.Opacity = Convert.ToDouble(SettingHelper.Get_BGOpacity()) / 10;

                    if (SettingHelper.Get_BGMaxWidth() != 0)
                    {
                        img_bg.MaxWidth = SettingHelper.Get_BGMaxWidth();
                    }
                    else
                    {

                        img_bg.MaxWidth = double.PositiveInfinity;
                    }
                    if (SettingHelper.Get_BGMaxHeight() != 0)
                    {
                        img_bg.MaxHeight = SettingHelper.Get_BGMaxHeight();
                    }
                    else
                    {
                        img_bg.MaxHeight = double.PositiveInfinity;
                    }


                    var st = await file.OpenReadAsync();
                    BitmapImage bit = new BitmapImage();
                    await bit.SetSourceAsync(st);
                    img_bg.Source = bit;
                    if (SettingHelper.Get_FrostedGlass() != 0)
                    {
                        GlassHost.Visibility = Visibility.Visible;
                        InitializedFrostedGlass(GlassHost, SettingHelper.Get_FrostedGlass());
                    }
                    else
                    {
                        GlassHost.Visibility = Visibility.Collapsed;
                    }


                }
                else
                {

                }


            }
            else
            {
                img_bg.Source = null;
            }
        }


        private void InitializedFrostedGlass(UIElement glassHost, int d)
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            Compositor compositor = hostVisual.Compositor;

            // Create a glass effect, requires Win2D NuGet package
            var glassEffect = new GaussianBlurEffect
            {
                BlurAmount = d * 5.0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new ArithmeticCompositeEffect
                {
                    MultiplyAmount = 0,
                    Source1Amount = 0.5f,
                    Source2Amount = 0.5f,
                    Source1 = new CompositionEffectSourceParameter("backdropBrush"),
                    Source2 = new ColorSourceEffect
                    {
                        Color = Color.FromArgb(255, 245, 245, 245)
                    }
                }
            };

            //  Create an instance of the effect and set its source to a CompositionBackdropBrush
            var effectFactory = compositor.CreateEffectFactory(glassEffect);
            var backdropBrush = compositor.CreateBackdropBrush();
            var effectBrush = effectFactory.CreateBrush();

            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

            // Create a Visual to contain the frosted glass effect
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = effectBrush;

            // Add the blur as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);

            // Make sure size of glass host and glass visual always stay in sync
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }

        private void MessageCenter_ChanageThemeEvent(object par, params object[] par1)
        {
            ChangeTheme();
        }

        private void ChangeTheme()
        {

            string ThemeName = SettingHelper.Get_Theme();
            ResourceDictionary newDictionary = new ResourceDictionary();
            switch (ThemeName)
            {
                case "Dark":
                    RequestedTheme = ElementTheme.Dark;

                    break;
                case "Red":

                    newDictionary.Source = new Uri("ms-appx:///Theme/RedTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    RequestedTheme = ElementTheme.Dark;
                    RequestedTheme = ElementTheme.Light;
                    break;
                case "Blue":

                    newDictionary.Source = new Uri("ms-appx:///Theme/BlueTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    RequestedTheme = ElementTheme.Dark;
                    RequestedTheme = ElementTheme.Light;
                    break;
                case "Green":
                    newDictionary.Source = new Uri("ms-appx:///Theme/GreenTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    RequestedTheme = ElementTheme.Dark;
                    RequestedTheme = ElementTheme.Light;
                    break;
                case "Pink":
                    newDictionary.Source = new Uri("ms-appx:///Theme/PinkTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    RequestedTheme = ElementTheme.Dark;
                    RequestedTheme = ElementTheme.Light;
                    break;
                case "Purple":
                    newDictionary.Source = new Uri("ms-appx:///Theme/PurpleTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    RequestedTheme = ElementTheme.Dark;
                    RequestedTheme = ElementTheme.Light;
                    break;
                case "Yellow":
                    newDictionary.Source = new Uri("ms-appx:///Theme/YellowTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    RequestedTheme = ElementTheme.Dark;
                    RequestedTheme = ElementTheme.Light;
                    break;
                case "EMT":
                    newDictionary.Source = new Uri("ms-appx:///Theme/EMTTheme.xaml", UriKind.RelativeOrAbsolute);

                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    // img_Hello.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo/EMT.png"));
                    RequestedTheme = ElementTheme.Dark;
                    RequestedTheme = ElementTheme.Light;
                    break;
            }
            //tuic.To = this.ActualWidth;
            //storyboardPopOut.Begin();
            ChangeTitbarColor();
        }
        private void ChangeTitbarColor()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                // StatusBar.GetForCurrentView().HideAsync();
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Color.FromArgb(255, 254, 254, 254);
                statusBar.BackgroundColor = ((SolidColorBrush)btn_OpenFilePlay.Background).Color;
                statusBar.BackgroundOpacity = 100;
            }
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = ((SolidColorBrush)btn_OpenFilePlay.Background).Color;
            titleBar.ForegroundColor = Color.FromArgb(255, 254, 254, 254);//Colors.White纯白用不了。。。
            //titleBar.ButtonHoverBackgroundColor = ((SolidColorBrush)sp_View.PaneBackground).Color;
            titleBar.ButtonBackgroundColor = ((SolidColorBrush)btn_OpenFilePlay.Background).Color;
            titleBar.ButtonForegroundColor = Color.FromArgb(255, 254, 254, 254);
            titleBar.InactiveBackgroundColor = ((SolidColorBrush)btn_OpenFilePlay.Background).Color;
            titleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)btn_OpenFilePlay.Background).Color;
        }

        private void btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            play_frame.Navigate(typeof(SettingPage));
        }

        private async void btn_OpenFilePlay_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.CommitButtonText = "选中此文件";
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".flv");
            openPicker.FileTypeFilter.Add(".avi");
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".ogv");
            openPicker.FileTypeFilter.Add(".wkv");
            openPicker.FileTypeFilter.Add(".3gp");
            openPicker.FileTypeFilter.Add(".rmvb");
            // 弹出文件选择窗口
            var files = await openPicker.PickMultipleFilesAsync(); // 用户在“文件选择窗口”中完成操作后，会返回对应的 StorageFile 对象
            if (files==null|| files.Count==0)
            {
                return;
            }
            List<PlayerModel> ls = new List<PlayerModel>();
            foreach (StorageFile file in files)
            {

                ls.Add(new PlayerModel() { Mode = PlayMode.FormLocal, No = "1", VideoTitle = "", Title = file.DisplayName, Parameter = file, Aid = file.DisplayName, Mid = file.Path });
            }
            play_frame.Navigate( typeof(PlayerPage), new object[] { ls, 0 });


        }

        private void play_frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (play_frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            }
        }

        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }
    }
}
