using BiliBili.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimelinePage : Page
    {
        public TimelinePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.Frame.Name == "bg_Frame")
            {
                g.Background = null;
            }
            if (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())
            {
                b_btn_Refresh.Visibility = Visibility.Visible;
            }
            else
            {
                b_btn_Refresh.Visibility = Visibility.Collapsed;
            }
            if (e.NavigationMode== NavigationMode.New)
            {
                await Task.Delay(200);
                if (e.Parameter!=null)
                {
                    if ((e.Parameter as object[]).Length!=0)
                    {
                        cb_OrderBy.SelectedIndex = (int)(e.Parameter as object[])[0];
                    }
                    else
                    {
                        cb_OrderBy.SelectedIndex = 0;
                    }
                }
                else
                {
                    cb_OrderBy.SelectedIndex = 0;
                }
                if (this.DataContext!=null)
                {
                    GeneralTransform generalTransform = st.TransformToVisual(gv_today);
                    Point point = generalTransform.TransformPoint(new Point(0, 0));

                    sv.ChangeView(null, Math.Abs(point.Y), null);
                }
            }
        }
        
        private async void LaodTimeline()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string url = "";
                switch (cb_OrderBy.SelectedIndex)
                {
                    case 0:
                        url = string.Format("https://bangumi.bilibili.com/api/timeline_v4?access_key={0}&appkey={1}&area_id=1%2C2%2C-1&build=5250000&date_after=6&date_before=6&mobi_app=android&platform=wp&see_mine=0&ts={2}000", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                        break;
                    case 1:
                        url = string.Format("https://bangumi.bilibili.com/api/timeline_v4?access_key={0}&appkey={1}&area_id=1%2C2%2C-1&build=5250000&date_after=6&date_before=6&mobi_app=android&platform=wp&see_mine=1&ts={2}000", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                        break;
                    case 2:
                        url = string.Format("https://bangumi.bilibili.com/api/timeline_v4?access_key={0}&appkey={1}&area_id=2%2C-1&build=5250000&date_after=6&date_before=6&mobi_app=android&platform=wp&see_mine=0&ts={2}000", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                        break;
                    case 3:
                        url = string.Format("https://bangumi.bilibili.com/api/timeline_v4?access_key={0}&appkey={1}&area_id=1%2C-1&build=5250000&date_after=6&date_before=6&mobi_app=android&platform=wp&see_mine=0&ts={2}000", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
                        break;
                    default:
                        break;
                }
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                TimeLineModel m = JsonConvert.DeserializeObject<TimeLineModel>(results);
                if (m.code == 0)
                {
                    AllTimeLineModel model = new AllTimeLineModel();
                    var t0 = GetWeek(DateTime.Now);
                    model.today = new AllTimeLineModel() { Week= t0[0],WeekStr=t0[1],Date=t0[2],ls=new List<TimeLineModel>() };
                    var n0 = GetWeek(DateTime.Now.AddDays(1));
                    model.next1 = new AllTimeLineModel() { Week = n0[0], WeekStr = n0[1], Date = n0[2], ls = new List<TimeLineModel>() };
                    var n1 = GetWeek(DateTime.Now.AddDays(2));
                    model.next2 = new AllTimeLineModel() { Week = n1[0], WeekStr = n1[1], Date = n1[2], ls = new List<TimeLineModel>() };
                    var n2 = GetWeek(DateTime.Now.AddDays(3));
                    model.next3 = new AllTimeLineModel() { Week = n2[0], WeekStr = n2[1], Date = n2[2], ls = new List<TimeLineModel>() };
                    var n3 = GetWeek(DateTime.Now.AddDays(4));
                    model.next4 = new AllTimeLineModel() { Week = n3[0], WeekStr = n3[1], Date = n3[2], ls = new List<TimeLineModel>() };
                    var n4 = GetWeek(DateTime.Now.AddDays(5));
                    model.next5 = new AllTimeLineModel() { Week = n4[0], WeekStr = n4[1], Date = n4[2], ls = new List<TimeLineModel>() };
                    var n5 = GetWeek(DateTime.Now.AddDays(6));
                    model.next6 = new AllTimeLineModel() { Week = n5[0], WeekStr = n5[1], Date = n5[2], ls = new List<TimeLineModel>() };
                    var p0 = GetWeek(DateTime.Now.AddDays(-6));
                    model.per1 = new AllTimeLineModel() { Week = p0[0], WeekStr = p0[1], Date = p0[2], ls = new List<TimeLineModel>() };
                    var p1 = GetWeek(DateTime.Now.AddDays(-5));
                    model.per2 = new AllTimeLineModel() { Week = p1[0], WeekStr = p1[1], Date = p1[2], ls = new List<TimeLineModel>() };
                    var p2 = GetWeek(DateTime.Now.AddDays(-4));
                    model.per3 = new AllTimeLineModel() { Week = p2[0], WeekStr = p2[1], Date = p2[2], ls = new List<TimeLineModel>() };
                    var p3 = GetWeek(DateTime.Now.AddDays(-3));
                    model.per4 = new AllTimeLineModel() { Week = p3[0], WeekStr = p3[1], Date = p3[2], ls = new List<TimeLineModel>() };
                    var p4 = GetWeek(DateTime.Now.AddDays(-2));
                    model.per5 = new AllTimeLineModel() { Week = p4[0], WeekStr = p4[1], Date = p4[2], ls = new List<TimeLineModel>() };
                    var p5 = GetWeek(DateTime.Now.AddDays(-1));
                    model.per6 = new AllTimeLineModel() { Week = p5[0], WeekStr = p5[1], Date = p5[2], ls = new List<TimeLineModel>() };
                    foreach (var item in m.result)
                    {
                        DateTime dt = DateTime.Parse(item.pub_date).Date;
                        if (dt == DateTime.Now.Date)
                        {
                            model.today.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(-6).Date)
                        {
                            model.per1.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(-5).Date)
                        {
                            model.per2.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(-4).Date)
                        {
                            model.per3.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(-3).Date)
                        {
                            model.per4.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(-2).Date)
                        {
                            model.per5.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(-1).Date)
                        {
                            model.per6.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(1).Date)
                        {
                            model.next1.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(2).Date)
                        {
                            model.next2.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(3).Date)
                        {
                            model.next3.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(4).Date)
                        {
                            model.next4.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(5).Date)
                        {
                            model.next5.ls.Add(item);
                        }
                        if (dt == DateTime.Now.AddDays(6).Date)
                        {
                            model.next6.ls.Add(item);
                        }
                    }


                    this.DataContext = model;

                   
                }
                else
                {
                    Utils.ShowMessageToast(m.message, 3000);
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867 || ex.HResult == -2147012889)
                {
                    Utils.ShowMessageToast("无法连接服务器，请检查你的网络连接", 3000);
                }
                else
                {

                    Utils.ShowMessageToast("读取放送表失败了", 3000);
                }
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                await Task.Delay(1000);
                GeneralTransform generalTransform= st.TransformToVisual(gv_today);
                Point point = generalTransform.TransformPoint(new Point(0, 0));
               
                sv.ChangeView(null,Math.Abs( point.Y), null);
            }
        }
        private string[] GetWeek(DateTime dt)
        {
            string[] str = new string[3];
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    str[0] = "周一";
                    str[1] = "月";
                    break;
                case DayOfWeek.Tuesday:
                    str[0] = "周二";
                    str[1] = "火";
                    
                    break;
                case DayOfWeek.Wednesday:
                    str[0] = "周三";
                    str[1] = "水";
                    break;
                case DayOfWeek.Thursday:
                    str[0] = "周四";
                    str[1] = "木";
                    break;
                case DayOfWeek.Friday:
                    str[0] = "周五";
                    str[1] = "金";
                    break;
                case DayOfWeek.Saturday:
                    str[0] = "周六";
                    str[1] = "土";
                    break;
                case DayOfWeek.Sunday:
                    str[0] = "周日";
                    str[1] = "日";
                    break;
                default:
                    break;
            }
            str[2] = dt.Month + "月" + dt.Day + "日";
            if (dt.Date==DateTime.Now.Date)
            {
                str[2] = "今天";
            }
            if (dt.Date == DateTime.Now.AddDays(-1).Date)
            {
                str[2] = "昨天";
            }
            if (dt.Date == DateTime.Now.AddDays(1).Date)
            {
                str[2] = "明天";
            }
            return str;
        }


        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void cb_OrderBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_OrderBy.SelectedIndex!=-1)
            {
                LaodTimeline();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            int w = Convert.ToInt32(this.ActualWidth/ 120);

            if (this.ActualWidth <= 500)
            {
               
                ViewBox2_num.Width = ActualWidth / 3 - 15;
            }
            else
            {

                ViewBox2_num.Width = ActualWidth / w - 13;
            }



        }

        private void gv1_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), (e.ClickedItem as TimeLineModel).season_id);
            
        }

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            cb_OrderBy.SelectedIndex = -1;
            cb_OrderBy.SelectedIndex = 0;
        }
    }
}
