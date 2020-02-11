using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// 显示评论框
    /// </summary>
    public sealed partial class CommentDialog : ContentDialog
    {
        Emote emote;
        public CommentDialog(int type, string oid)
        {
           
            this.InitializeComponent();
            emote = new Emote(EmoteMode.reply);
            //pivot_face.ItemsSource = ApiHelper.emoji;
            _type = type;
            _oid = oid;

            if (Window.Current.CoreWindow.Bounds.Width >= 500)
            {
                st.Width = 440;
                //commentDialog.MinWidth = 500;
            }
            else
            {
                st.Width = Window.Current.CoreWindow.Bounds.Width - 24;
            }
        }

        

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        //LoadCommentInfo _loadCommentInfo;
        int _type = 0;
        string _oid = "";

        public bool State = false;
        private async void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Comment.Text.Trim().Length == 0)
            {
                Utils.ShowMessageToast("检查下你的输入哦...");
                return;
            }
            try
            {

                var text = txt_Comment.Text;

                string url = "https://api.bilibili.com/x/v2/reply/add";

                string content =
                    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&message={5}",
                    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _oid, ApiHelper.GetTimeSpan_2, Uri.EscapeDataString(text));
                content += "&sign=" + ApiHelper.GetSign(content);
                var re = await WebClientClass.PostResults(new Uri(url), content);
                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    Utils.ShowMessageToast("发送评论成功");


                    State = true;
                    this.Hide();

                }
                else
                {
                    State = false;
                    Utils.ShowMessageToast(obj["message"].ToString());
                }

            }
            catch (Exception)
            {
                State = false;
                Utils.ShowMessageToast("发送评论失败");
                // throw;
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            State = false;
            txt_Comment.Text += (sender as Button).Content.ToString();

        }


        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            txt_Comment.Text += (e.ClickedItem as EmoteItem).text;
        }

        private void txt_Comment_TextChanged(object sender, TextChangedEventArgs e)
        {
            //txt_Length.Text =(233- txt_Comment.Text.Length).ToString();
        }
        ObservableCollection<EmotePackage> packages;
        private async void btn_F_Click(object sender, RoutedEventArgs e)
        {
            if (packages == null)
            {
                var data = await emote.LoadEmote();
                if (data.success)
                {
                    packages = data.data;
                    pivot_face.ItemsSource = packages;
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }
            }
           
        }

        private async void pivot_face_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var select = pivot_face.SelectedItem as EmotePackage;
            if (select.emote==null|| select.emote.Count==0)
            {
                var data =await emote.LoadEmote(select.id);
                if (data.success)
                {
                    packages[pivot_face.SelectedIndex] = data.data[0];
                }
                else
                {
                    Utils.ShowMessageToast(data.message);
                }
            }
        }
    }
}
