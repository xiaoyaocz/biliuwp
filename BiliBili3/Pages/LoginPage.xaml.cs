using BiliBili3.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili3.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            UserManage.Logined += UserManage_Logined;
        }

        private void UserManage_Logined(object sender, EventArgs e)
        {
            GetFilter();
            MessageCenter.SendLogined();
            this.Frame.GoBack();
        }

        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            //GetCaptcha();
            txt_User.Text = SettingHelper.Get_UserName();
            txt_Pass.Password = SettingHelper.Get_Password();

         
        }
        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        private void Login_Pass_LostFocus(object sender, RoutedEventArgs e)
        {
            KanZheNi.Visibility = Visibility.Visible;
            BuKanZheNi.Visibility = Visibility.Collapsed;
        }

        private void Login_Pass_GotFocus(object sender, RoutedEventArgs e)
        {
            KanZheNi.Visibility = Visibility.Collapsed;
            BuKanZheNi.Visibility = Visibility.Visible;
        }
        private async void Login()
        {
            if (txt_User.Text.Length == 0||txt_Pass.Password.Length==0)
            {
                MessageDialog md = new MessageDialog("账号或密码不能为空！");
                await md.ShowAsync();
            }
            else
            {
               
                sc.IsEnabled = false;
                btn_Login.Content = "正在登录";
                pr_Load.Visibility = Visibility.Visible;
                var result = await ApiHelper.LoginBilibili(txt_User.Text, txt_Pass.Password,txt_Captcha.Text);
                if (result.code ==0)
                {
                    SettingHelper.Set_UserName(txt_User.Text);
                    SettingHelper.Set_Password(txt_Pass.Password);
                    MessageCenter.SendLogined();
                    GetFilter();
                    this.Frame.GoBack();
                }
                else
                {
                    if (result.code==-105)
                    {
                        await new MessageDialog("请输入验证码后再试").ShowAsync();
                        GetCaptcha();
                    }
                    else
                    {
                        await new MessageDialog(result.code+ result.message).ShowAsync();
                        if (result.code==-2100)
                        {
                            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage),result.url);
                        }
                    }
                }
                pr_Load.Visibility = Visibility.Collapsed;
                btn_Login.Content = "登录";
                sc.IsEnabled = true;
            }
        }

        private async void GetCaptcha()
        {
            try
            {
                needCaptcha.Visibility = Visibility.Visible;
                var m = await WebClientClass.GetBuffer(new Uri("https://passport.bilibili.com/captcha"));
                var steam = m.AsStream();
                var img = new BitmapImage();
                await img.SetSourceAsync(steam.AsRandomAccessStream());
                img_Captcha.Source = img;
            }
            catch (Exception)
            {
                await new MessageDialog("无法显示验证码").ShowAsync();
            }
            

        }

        private async void GetFilter()
        {

            try
            {
                string results = await WebClientClass.GetResults(new Uri("http://api.bilibili.com/x/dm/filter/user?jsonp=jsonp"));
                var ls = SettingHelper.Get_Guanjianzi().Split('|').ToList();
                ls.Remove(string.Empty);
                var ls2 = SettingHelper.Get_Yonghu().Split('|').ToList();
                ls2.Remove(string.Empty);

                DMFilterModel fm = JsonConvert.DeserializeObject<DMFilterModel>(results);
                if (fm.code == 0)
                {
                    foreach (var item in fm.data.rule)
                    {
                        if (item.type == 0)
                        {
                            if (!ls.Contains(item.filter))
                            {
                                SettingHelper.Set_Guanjianzi(SettingHelper.Get_Guanjianzi() + "|" + item.filter);
                            }
                        }
                        if (item.type == 2)
                        {
                            if (!ls2.Contains(item.filter))
                            {
                                SettingHelper.Set_Yonghu(SettingHelper.Get_Yonghu() + "|" + item.filter);
                            }
                        }
                    }
                    List<string> s = new List<string>();
                    List<string> s2 = new List<string>();
                    fm.data.rule.ForEach(x => {
                        if (x.type == 0)
                        {
                            s.Add(x.filter);
                        }
                        if (x.type == 2)
                        {
                            s2.Add(x.filter);
                        }
                    });
                  
                }
                else
                {

                   
                }


            }
            catch (Exception)
            {

            }
        }


        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private async void btn_SignIn_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://passport.bilibili.com/register/phone"));
        }

        private void txt_Pass_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Login();
            }
         }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //https://passport.bilibili.com/resetpwd
            await Launcher.LaunchUriAsync(new Uri("https://passport.bilibili.com/resetpwd"));
        }

        private void web_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.Uri.AbsoluteUri== "https://passport.bilibili.com/ajax/miniLogin/redirect")
            {
                HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

                
                foreach (HttpCookie item in cookieCollection)
                {
                    item.Expires = new DateTimeOffset(DateTime.Now.AddDays(365));
                    if (item.Name == "bili_jct")
                    {
                       
                        SettingHelper.Set_Access_key(item.Value);
                        // txt.Text = item.Value;
                        MessageCenter.SendLogined();
                        GetFilter();
                        this.Frame.GoBack();
                    }
                   
                }
            }
        }

        private void img_Captcha_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GetCaptcha();
        }

        private  void btn_WebLogin_Click(object sender, RoutedEventArgs e)
        {
            //https://passport.bilibili.com/register/third.html?&appkey=27eb53fc9058f8c3&api=https://www.biliplus.com/login&sign=5c72fad931466d6d47ecb14ed9d5ea54#/
            //https://www.biliplus.com/login?access_key=c0aba73afbb63d57a9fb9a596d031edc&mid=7251681&uname=xiaoyaocz&sign=3cdf6fb8e6e1e5dd43a67f3b4c98ca42


           

        }
    }
}
