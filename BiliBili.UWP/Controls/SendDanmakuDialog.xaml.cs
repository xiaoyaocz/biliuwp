using Newtonsoft.Json.Linq;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili.UWP.Controls
{
    public sealed partial class SendDanmakuDialog : ContentDialog
    {
        public SendDanmakuDialog(string _aid,string _cid,double _position)
        {
            this.InitializeComponent();

            aid = _aid;
            cid = _cid;
            position = Convert.ToInt32((_position*1000)).ToString();

        }
        public event EventHandler<SendDanmakuModel> DanmakuSended;


        public string aid, cid, position;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {


            if (Send_text_Comment.Text.Length == 0)
            {
                Utils.ShowMessageToast("弹幕内容不能为空!", 2000);
                return;
            }
            if (!ApiHelper.IsLogin())
            {
                Utils.ShowMessageToast("请先登录!", 2000);
                return;
            }
            try
            {
                var url = $"https://api.bilibili.com/x/v2/dm/post?access_key={ApiHelper.access_key}&aid={aid}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&mobi_app=android&oid={cid}&platform=android&ts={ApiHelper.GetTimeSpan}";
                url += "&sign=" + ApiHelper.GetSign(url);

                Uri ReUri = new Uri(url);
                int modeInt = 1;
                if (Send_cb_Mode.SelectedIndex == 2)
                {
                    modeInt = 4;
                }
                if (Send_cb_Mode.SelectedIndex == 1)
                {
                    modeInt = 5;
                }
                string data = $"pool=0&rnd={ApiHelper.GetTimeSpan}&oid={cid}&fontsize=25&msg={Uri.EscapeDataString(Send_text_Comment.Text)}&mode={modeInt}&progress={position}&color={ ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag}&plat=2&screen_state=0&from=861&type=1";
                //string data = string.Format("playTime={0}&pool=0&color={1}&screen_state=1&rnd={2}&from=0&type=json&msg={3}&cid={4}&fontsize=25&mode={5}&mid={6}",
                //    position,
                //    ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag,
                //    new Random().Next(1, 99999999),
                //    Uri.EscapeDataString(Send_text_Comment.Text),
                //    cid, modeInt, ApiHelper.GetUserId()
                //    );

                //string Canshu = "message=" + Send_text_Comment.Text + "&pool=0&playTime=" + mediaElement.Position.TotalSeconds.ToString() + "&cid=" + playNow.Mid + "&date=" + DateTime.Now.ToString() + "&fontsize=25&mode=" + modeInt + "&rnd=" + new Random().Next(100000000, 999999999) + "&color=" + ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag;
                string result = await WebClientClass.PostResults(ReUri, data);
                var obj = JObject.Parse(result);

                if (Convert.ToInt32(obj["code"].ToString()) != 0)
                {
                   
                    Utils.ShowMessageToast("弹幕发送失败" + obj["message"].ToString(), 3000);
                }
                else
                {
                    if (DanmakuSended!=null)
                    {
                        DanmakuSended(this, new SendDanmakuModel() {
                            location=modeInt,
                            color= ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(),
                            text=Send_text_Comment.Text
                        });
                    }
                    Utils.ShowMessageToast("弹幕成功发射", 3000);
                   
                    Send_text_Comment.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                
                Utils.ShowMessageToast("发送弹幕发生错误！\r\n" + ex.HResult, 3000);
            }


        }
    }


    public class SendDanmakuModel
    {
        public string text { get; set; }
        public string color { get; set; }
        public int location { get; set; }
    }

}
