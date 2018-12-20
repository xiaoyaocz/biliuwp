using BiliBili3.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media.Imaging;
using BiliBili3.Helper;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili3.Controls
{
    public sealed partial class LoginDialog : ContentDialog
    {
        Account account;
        public LoginDialog()
        {
            this.InitializeComponent();
            account = new Account();
            _biliapp.CloseBrowserEvent += _biliapp_CloseBrowserEvent;
            _biliapp.ValidateLoginEvent += _biliapp_ValidateLoginEvent;
            _secure.CloseCaptchaEvent += _biliapp_CloseCaptchaEvent;
            _secure.CaptchaEvent += _biliapp_CaptchaEvent;
        }

        private void _biliapp_CaptchaEvent(object sender, string e)
        {
            //throw new NotImplementedException();
        }

        private void _biliapp_CloseCaptchaEvent(object sender, string e)
        {
            //throw new NotImplementedException();
        }

        BiliBiliJS.biliapp _biliapp = new BiliBiliJS.biliapp();
        BiliBiliJS.secure _secure = new BiliBiliJS.secure();
        private void _biliapp_CloseBrowserEvent(object sender, string e)
        {
            this.Hide();
        }

        private async void _biliapp_ValidateLoginEvent(object sender, string e)
        {
            try
            {
                JObject jObject = JObject.Parse(e);
                if (jObject["access_token"] != null)
                {
                    var m = await account.CheckAgainLogin(jObject["access_token"].ToString(), jObject["refresh_token"].ToString(), jObject["expires_in"].ToInt32(), Convert.ToInt64(jObject["mid"]));
                    if (m.success)
                    {
                        this.Hide();
                        Utils.ShowMessageToast("登录成功");
                    }
                    else
                    {
                        Title = "登录";
                        IsPrimaryButtonEnabled = true;
                        webView.Visibility = Visibility.Collapsed;
                        Utils.ShowMessageToast("登录失败,请重试");
                    }
                    //await UserManage.LoginSucess(jObject["access_token"].ToString());
                }
                else
                {
                    Title = "登录";
                    IsPrimaryButtonEnabled = true;
                    webView.Visibility = Visibility.Collapsed;
                    Utils.ShowMessageToast("登录失败,请重试");
                }

            }
            catch (Exception)
            {
            }

        }
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (txt_Username.Text.Length == 0)
            {
                txt_Username.Focus(FocusState.Pointer);
                Utils.ShowMessageToast("请输入用户名");
                return;
            }
            if (txt_Password.Password.Length == 0)
            {
                txt_Password.Focus(FocusState.Pointer);
                Utils.ShowMessageToast("请输入密码");
                return;
            }
            if (chatcha.Visibility == Visibility.Visible && txt_captcha.Text.Length == 0)
            {
                txt_Password.Focus(FocusState.Pointer);
                Utils.ShowMessageToast("请输入验证码");
                return;
            }
            IsPrimaryButtonEnabled = false;
            var results = await account.LoginV2(txt_Username.Text, txt_Password.Password, txt_captcha.Text);
            switch (results.status)
            {
                case Modules.LoginStatus.Success:
                    this.Hide();
                    break;
                case Modules.LoginStatus.Fail:
                case Modules.LoginStatus.Error:
                    IsPrimaryButtonEnabled = true;
                    break;
                case Modules.LoginStatus.NeedCaptcha:
                    chatcha.Visibility = Visibility.Visible;
                    IsPrimaryButtonEnabled = true;
                    GetCaptcha();
                    //webView.Visibility = Visibility.Visible;
                    //webView.Source = new Uri(results.url);
                    break;
                case Modules.LoginStatus.NeedValidate:
                    Title = "安全验证";
                    webView.Visibility = Visibility.Visible;
                    webView.Source = new Uri(results.url.Replace("&ticket=1",""));
                    break;
                default:
                    break;
            }
            Utils.ShowMessageToast(results.message);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void txt_Password_GotFocus(object sender, RoutedEventArgs e)
        {
            hide.Visibility = Visibility.Visible;
        }
        private void txt_Password_LostFocus(object sender, RoutedEventArgs e)
        {
            hide.Visibility = Visibility.Collapsed;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GetCaptcha();
        }
        private async void GetCaptcha()
        {
            try
            {
                var m = await WebClientClass.GetBuffer(new Uri("https://passport.bilibili.com/captcha?ts=" + ApiHelper.GetTimeSpan));
                var steam = m.AsStream();
                var img = new BitmapImage();
                await img.SetSourceAsync(steam.AsRandomAccessStream());
                img_Captcha.Source = img;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("无法加载验证码");
            }


        }

        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                this.webView.AddWebAllowedObject("biliapp", _biliapp);
                this.webView.AddWebAllowedObject("secure", _secure);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }

        }

        private void webView_ScriptNotify(object sender, NotifyEventArgs e)
        {

        }
    }
}
