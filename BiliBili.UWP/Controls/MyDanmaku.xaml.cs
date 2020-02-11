using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Text;
using System.Diagnostics;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BiliBili.UWP.Controls
{
    public enum DanmuStyle
    {
        Boder,
        NoBorder,
        Shadow
    }
    public sealed partial class MyDanmaku : UserControl
    {

        public MyDanmaku()
        {
            
            this.InitializeComponent();
            D_Border = SettingHelper.Get_DMBorder();
            danmuStyle = (DanmuStyle)SettingHelper.Get_DMStyle();
        }
        public DanmuStyle danmuStyle  = DanmuStyle.Boder;

        /// <summary>
        /// 是否直播
        /// </summary>
        public bool isLive = false;
        /// <summary>
        /// 弹幕字体
        /// </summary>
        public string fontFamily = "默认";
        /// <summary>
        /// 弹幕字体大小
        /// </summary>
        public double fontSize = 22;
        /// <summary>
        /// 弹幕速度
        /// </summary>
        public int Speed = 12;
        /// <summary>
        /// 弹幕透明度
        /// </summary>
        public double Tran = 1;
        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying = true;
        public MediaElementState state;
        public int row = 0;//行数
        public int maxRow = 10;
        public bool D_Border = true;
        /// <summary>
        /// 添加滚动弹幕
        /// </summary>
        /// <param name="model">弹幕参数</param>
        /// <param name="Myself">是否自己发送的</param>
        public async void AddGunDanmu(DanMuModel model, bool Myself)
        {
            try
            {

                ////创建基础控件
                //TextBlock tx = new TextBlock();
                //TextBlock tx2 = new TextBlock();
                //Grid grid = new Grid();
                ////设置控件相关信息
                //grid.Margin = new Thickness(0, 0, 20, 0);
                //grid.VerticalAlignment = VerticalAlignment.Center;
                //grid.HorizontalAlignment = HorizontalAlignment.Left;
                //if (fontFamily != "默认")
                //{
                //    tx.FontFamily = new FontFamily(fontFamily);
                //    tx2.FontFamily = new FontFamily(fontFamily);
                //}
                //tx2.Text = model.DanText;
                //tx.Text = model.DanText;
                //tx2.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //tx.Foreground = model.DanColor;
                ////弹幕大小
                //double size = double.Parse(model.DanSize);
                //if (size == 25)
                //{
                //    tx2.FontSize = fontSize;
                //    tx.FontSize = fontSize;
                //}
                //else
                //{
                //    tx2.FontSize = fontSize - 2;
                //    tx.FontSize = fontSize - 2;
                //}

                ////grid包含弹幕文本信息

                //if (D_Border)
                //{
                //    grid.Children.Add(tx2);
                //}
                //grid.Children.Add(tx);
                //grid.VerticalAlignment = VerticalAlignment.Top;
                //grid.HorizontalAlignment = HorizontalAlignment.Left;
                Grid grid=null;
                switch (danmuStyle)
                {
                    case DanmuStyle.Boder:
                        grid = CreateControlBorder(model);
                        break;
                    case DanmuStyle.NoBorder:
                        grid = CreateControlNoBorder(model);
                        break;
                    case DanmuStyle.Shadow:
                        grid = CreateControlShadow(model);
                        break;
                    default:
                        grid = CreateControlBorder(model);
                        break;
                }
                 
                grid.VerticalAlignment = VerticalAlignment.Top;
                grid.HorizontalAlignment = HorizontalAlignment.Left;

                TranslateTransform moveTransform = new TranslateTransform();
                moveTransform.X = grid_Danmu.ActualWidth;
                grid.RenderTransform = moveTransform;
                //将弹幕加载入控件中,并且设置位置
                grid_Danmu.Children.Add(grid);

                Grid.SetRow(grid, row);

                if (row >= maxRow - 1)
                {
                    row = 0;
                }
                else
                {
                    row++;
                }


                if (Myself)
                {
                    grid.BorderThickness = new Thickness(2);
                    grid.BorderBrush = new SolidColorBrush(Colors.Gray);
                }
                grid.Opacity = Tran;
                grid.DataContext = model;
                //更新弹幕UI，不更新无法获得弹幕的ActualWidth
                grid.UpdateLayout();
                //D_height = grid.ActualHeight;
                //if (!wCnMdBUG)
                //{
                //    SetJJ();
                //}

                //SetJJ();
                //创建动画
                Duration duration = new Duration(TimeSpan.FromSeconds(Speed));
                DoubleAnimation myDoubleAnimationX = new DoubleAnimation();
                myDoubleAnimationX.Duration = duration;
                //创建故事版
                Storyboard justintimeStoryboard = new Storyboard();
                justintimeStoryboard.Duration = duration;
                myDoubleAnimationX.To = -(grid.ActualWidth);//到达
                justintimeStoryboard.Children.Add(myDoubleAnimationX);
                Storyboard.SetTarget(myDoubleAnimationX, moveTransform);
                //故事版加入动画
                Storyboard.SetTargetProperty(myDoubleAnimationX, "X");
                grid_Danmu.Resources.Remove("justintimeStoryboard");
                grid_Danmu.Resources.Add("justintimeStoryboard", justintimeStoryboard);
                justintimeStoryboard.Begin();
                //等待，暂停则暂停
                await Task.Run(async () =>
                {
                    int i = 0;
                    while (true)
                    {
                        if (!IsPlaying)
                        {
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                justintimeStoryboard.Pause();
                            });
                            //break;
                        }
                        else
                        {
                            if (i == Speed * 2)
                            {
                                break;
                            }
                            i++;
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                justintimeStoryboard.Resume();
                            });
                        }
                        await Task.Delay(500);
                    }
                });
                grid_Danmu.Children.Remove(grid);
            }
            catch (Exception)
            {
            }
        }



        private Grid CreateControlShadow(DanMuModel model)
        {
            //创建基础控件
            TextBlock tx = new TextBlock();
            DropShadowPanel dropShadowPanel = new DropShadowPanel()
            {
                BlurRadius = 6,
                ShadowOpacity = 1,
                OffsetX = 0,
                OffsetY = 0,
                Color= SetBorder(model.DanColor.Color)
            };


            Grid grid = new Grid();
            //设置控件相关信息
            grid.Margin = new Thickness(0, 0, 20, 0);
            grid.VerticalAlignment = VerticalAlignment.Center;
            grid.HorizontalAlignment = HorizontalAlignment.Left;

            if (fontFamily != "默认")
            {
                tx.FontFamily = new FontFamily(fontFamily);

            }

            tx.Text = model.DanText;

            tx.FontWeight = FontWeights.Bold;
            tx.Foreground = model.DanColor;
            //弹幕大小
            double size = double.Parse(model.DanSize);
            if (size == 25)
            {

                tx.FontSize = fontSize;
            }
            else
            {

                tx.FontSize = fontSize - 2;
            }
            dropShadowPanel.Content = tx;

            grid.Children.Add(dropShadowPanel);
            
            return grid;
        }
        private Grid CreateControlBorder(DanMuModel model)
        {
            //创建基础控件
            TextBlock tx = new TextBlock();
            TextBlock tx2 = new TextBlock();
            Grid grid = new Grid();
            //设置控件相关信息
            grid.Margin = new Thickness(0, 0, 20, 0);
            grid.VerticalAlignment = VerticalAlignment.Center;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            if (fontFamily != "默认")
            {
                tx.FontFamily = new FontFamily(fontFamily);
                tx2.FontFamily = new FontFamily(fontFamily);
            }
            tx2.Text = model.DanText;
            tx.Text = model.DanText;
            tx.FontWeight = FontWeights.Bold;
            tx2.FontWeight = FontWeights.Bold;
            tx2.Foreground = new SolidColorBrush(SetBorder(model.DanColor.Color));
            tx.Foreground = model.DanColor;
            //弹幕大小
            double size = double.Parse(model.DanSize);
            if (size == 25)
            {
                tx2.FontSize = fontSize;
                tx.FontSize = fontSize;
            }
            else
            {
                tx2.FontSize = fontSize - 2;
                tx.FontSize = fontSize - 2;
            }
            tx2.Margin = new Thickness(1);
            //grid包含弹幕文本信息

            if (D_Border)
            {
                grid.Children.Add(tx2);
            }
            
            grid.Children.Add(tx);
            return grid;
        }
        private Grid CreateControlNoBorder(DanMuModel model)
        {
            //创建基础控件
            TextBlock tx = new TextBlock();
          
            Grid grid = new Grid();
            //设置控件相关信息
            grid.Margin = new Thickness(0, 0, 20, 0);
            grid.VerticalAlignment = VerticalAlignment.Center;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            if (fontFamily != "默认")
            {
                tx.FontFamily = new FontFamily(fontFamily);
             
            }
         
            tx.Text = model.DanText;
            tx.FontWeight = FontWeights.Bold;
          
            tx.Foreground = model.DanColor;
            //弹幕大小
            double size = double.Parse(model.DanSize);
            if (size == 25)
            {
                tx.FontSize = fontSize;
            }
            else
            {
               
                tx.FontSize = fontSize - 2;
            }
            grid.Children.Add(tx);
            return grid;
        }


        private Color SetBorder(Color textColor)
        {
            if (textColor.R < 100 && textColor.G < 100 && textColor.B < 100)
            {
                return Colors.White;
            }
            else
            {
                return Colors.Black;
            }
           
        }



        /// <summary>
        /// 加载图片礼物弹幕
        /// </summary>
        public async void AddGiftDanmu(BitmapImage img)
        {
            try
            {



                //创建基础控件
                Image tx = new Image();
                tx.Source = img;

                Grid grid = new Grid();
                //设置控件相关信息
                grid.Margin = new Thickness(0, 0, 10, 0);
                grid.VerticalAlignment = VerticalAlignment.Center;
                grid.HorizontalAlignment = HorizontalAlignment.Left;

                tx.MaxHeight = D_height;

                grid.Children.Add(tx);
                grid.VerticalAlignment = VerticalAlignment.Top;
                grid.HorizontalAlignment = HorizontalAlignment.Left;

                TranslateTransform moveTransform = new TranslateTransform();
                moveTransform.X = grid_Danmu.ActualWidth;
                grid.RenderTransform = moveTransform;
                //将弹幕加载入控件中,并且设置位置
                grid_Danmu.Children.Add(grid);

                Grid.SetRow(grid, row);

                if (row >= maxRow - 1)
                {
                    row = 0;
                }
                else
                {
                    row++;
                }


                grid.Opacity = Tran;
                //更新弹幕UI，不更新无法获得弹幕的ActualWidth
                grid.UpdateLayout();
                //D_height = grid.ActualHeight;
                //if (!wCnMdBUG)
                //{
                //    SetJJ();
                //}

                //SetJJ();
                //创建动画
                Duration duration = new Duration(TimeSpan.FromSeconds(Speed));
                DoubleAnimation myDoubleAnimationX = new DoubleAnimation();
                myDoubleAnimationX.Duration = duration;
                //创建故事版
                Storyboard justintimeStoryboard = new Storyboard();
                justintimeStoryboard.Duration = duration;
                myDoubleAnimationX.To = -(grid.ActualWidth + 60);//到达
                justintimeStoryboard.Children.Add(myDoubleAnimationX);
                Storyboard.SetTarget(myDoubleAnimationX, moveTransform);
                //故事版加入动画
                Storyboard.SetTargetProperty(myDoubleAnimationX, "X");
                grid_Danmu.Resources.Remove("justintimeStoryboard");
                grid_Danmu.Resources.Add("justintimeStoryboard", justintimeStoryboard);
                justintimeStoryboard.Begin();
                await Task.Run(async () =>
                {
                    int i = 0;
                    while (true)
                    {
                        if (!IsPlaying)
                        {
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                justintimeStoryboard.Pause();
                            });
                            //break;
                        }
                        else
                        {
                            if (i == Speed * 2)
                            {
                                break;
                            }
                            i++;
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                justintimeStoryboard.Resume();
                            });
                        }
                        await Task.Delay(500);
                    }
                });
                grid_Danmu.Children.Remove(grid);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 清除全部弹幕
        /// </summary>
        public void ClearDanmu()
        {
            row = 0;
            grid_Danmu.Children.Clear();
            D_Top.Children.Clear();
            D_Bottom.Children.Clear();
        }

        private bool Handling = false;//是否正在监听

        private double D_height = 40;
        /// <summary>
        /// 添加顶部及底部弹幕
        /// </summary>
        /// <param name="model">弹幕参数</param>
        /// <param name="istop">是否顶部</param>
        /// <param name="Myself">是否自己发送的</param>
        public async void AddTopButtomDanmu(DanMuModel model, bool istop, bool Myself)
        {
            //TextBlock tx = new TextBlock();
            //TextBlock tx2 = new TextBlock();
            //Grid grid = new Grid();
            //if (fontFamily != "默认")
            //{
            //    tx.FontFamily = new FontFamily(fontFamily);
            //    tx2.FontFamily = new FontFamily(fontFamily);
            //}

            //tx2.Text = model.DanText;
            //tx.Text = model.DanText;
            //tx2.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            //tx.Foreground = model.DanColor;//new SolidColorBrush(co[rd.Next(0, 7)]);
            //double size = double.Parse(model.DanSize);
            //if (size == 25)
            //{
            //    tx2.FontSize = fontSize;
            //    tx.FontSize = fontSize;
            //}
            //else
            //{
            //    tx2.FontSize = fontSize - 2;
            //    tx.FontSize = fontSize - 2;
            //}
            ////grid包含弹幕文本信息
            //if (D_Border)
            //{
            //    grid.Children.Add(tx2);
            //}
            //grid.Children.Add(tx);

            //// tx.FontSize = Double.Parse(model.DanSize) - fontSize;
            //grid.HorizontalAlignment = HorizontalAlignment.Center;
            //grid.VerticalAlignment = VerticalAlignment.Top;
            //tx2.Margin = new Thickness(1);
            //Grid grid = CreateControlV1(model);
            Grid grid = null;
            switch (danmuStyle)
            {
                case DanmuStyle.Boder:
                    grid = CreateControlBorder(model);
                    break;
                case DanmuStyle.NoBorder:
                    grid = CreateControlNoBorder(model);
                    break;
                case DanmuStyle.Shadow:
                    grid = CreateControlShadow(model);
                    break;
                default:
                    grid = CreateControlBorder(model);
                    break;
            }

            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.HorizontalAlignment = HorizontalAlignment.Center;

            if (Myself)
            {
                grid.BorderThickness = new Thickness(2);
                grid.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            grid.Opacity = Tran;
            grid.DataContext = model;
            grid.UpdateLayout();
            //D_height = grid.ActualHeight;
            if (istop)
            {
                D_Top.Children.Add(grid);
                await Task.Delay(5000);
                if (state == MediaElementState.Paused)
                {
                    ClearTopButtomDanmuku();
                }
                else
                {
                    D_Top.Children.Remove(grid);
                }
            }
            else
            {
                D_Bottom.Children.Add(grid);
                await Task.Delay(5000);
                if (state == MediaElementState.Paused)
                {
                    ClearTopButtomDanmuku();
                }
                else
                {
                    D_Bottom.Children.Remove(grid);
                }
            }
        }
        /// <summary>
        /// 清除顶部及底部弹幕
        /// </summary>
        private async void ClearTopButtomDanmuku()
        {
            //一定要检查是否正在循环，多个while死循环会爆CPU
            if (!Handling)
            {
                Handling = true;
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsPlaying)
                        {
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                D_Bottom.Children.Clear();
                                D_Top.Children.Clear();
                            });
                            break;
                        }
                        //循环速度不要太快，否则也会爆CPU
                        await Task.Delay(200);
                    }
                });
                Handling = false;
            }
        }
        /// <summary>
        /// 设置弹幕上下间距
        /// </summary>
        /// <param name="value"></param>
        public void SetSpacing(double value)
        {
            Jianju.Height = new GridLength(value, GridUnitType.Pixel);
        }
        /// <summary>
        /// 读取弹幕屏幕中的弹幕
        /// </summary>
        /// <returns></returns>
        public List<DanMuModel> GetScreenDanmu()
        {
            List<DanMuModel> list = new List<DanMuModel>();
            foreach (Grid item in D_Top.Children)
            {
                list.Add(item.DataContext as DanMuModel);
            }
            foreach (Grid item in D_Bottom.Children)
            {
                list.Add(item.DataContext as DanMuModel);
            }
            foreach (Grid item in grid_Danmu.Children)
            {
                list.Add(item.DataContext as DanMuModel);
            }
            return list;
        }
        /// <summary>
        /// 移除当前屏幕中的弹幕
        /// </summary>
        public void RemoveDanmu(DanMuModel model)
        {
            foreach (Grid item in grid_Danmu.Children)
            {
                if (item.DataContext == model)
                {
                    grid_Danmu.Children.Remove(item);
                }
            }
            foreach (Grid item in D_Bottom.Children)
            {
                if (item.DataContext == model)
                {
                    D_Bottom.Children.Remove(item);
                }
            }
            foreach (Grid item in D_Top.Children)
            {
                if (item.DataContext == model)
                {
                    D_Top.Children.Remove(item);
                }
            }

        }
        /// <summary>
        /// 弹幕可见
        /// </summary>
        /// <param name="IsVisible">是否可见</param>
        /// <param name="mode">模式</param>
        public void SetDanmuVisibility(bool IsVisible, DanmuMode mode)
        {
            if (IsVisible)
            {
                switch (mode)
                {
                    case DanmuMode.Roll:
                        grid_Danmu.Visibility = Visibility.Visible;
                        break;
                    case DanmuMode.Top:
                        D_Top.Visibility = Visibility.Visible;
                        break;
                    case DanmuMode.Buttom:
                        D_Bottom.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (mode)
                {
                    case DanmuMode.Roll:
                        grid_Danmu.Visibility = Visibility.Collapsed;
                        break;
                    case DanmuMode.Top:
                        D_Top.Visibility = Visibility.Collapsed;
                        break;
                    case DanmuMode.Buttom:
                        D_Bottom.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum DanmuMode
        {
            Roll = 0,
            Top = 1,
            Buttom = 2
        }

        public class DanMuModel
        {
            public static double Tran = 255;//弹幕透明度
            //头声明
            public string chatserver { get; set; }//弹幕服务器
            public string chatid { get; set; }//弹幕ID
            public string mission { get; set; }//任务？
            public string maxlimit { get; set; }//弹幕池上限
            public string source { get; set; }//来源？
            //弹幕信息
            //<d p = "1355.2700195312,5,25,16776960,1447587837,0,222d0737,1347973259" > やがて巡り巡る季節に僕らは息をする</d>
            private decimal _DanTime;//弹幕出现时间
            public decimal DanTime
            {
                get { return _DanTime; }
                set { _DanTime = value; }
            }
            public string DanMode { get; set; }//弹幕模式 1..3 滚动弹幕 4底端弹幕 5顶端弹幕 6.逆向弹幕 7精准定位 8高级弹幕
            public string DanSize { get; set; }//弹幕大小 12非常小,16特小,18小,25中,36大,45很大,64特别大
            public string _DanColor { get; set; }//弹幕颜色，十进制
            public string DanModeStr
            {
                get
                {
                    switch (DanMode)
                    {
                        case "4":
                            return "底端";
                        case "5":
                            return "顶端";
                        default:
                            return "滚动";
                    }
                }
            }
            public string DanTimeStr
            {
                get
                {
                    TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(DanTime));
                    return ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                }
            }
            public SolidColorBrush color
            {
                get; set;
            }
            public SolidColorBrush DanColor
            {
                get
                {
                    try
                    {
                        var color = Convert.ToInt32(_DanColor).ToString("X2").ToColor();
                        return new SolidColorBrush(color); 
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        SolidColorBrush solid = new SolidColorBrush(new Color()
                        {
                            A = 255,
                            R = 255,
                            G = 255,
                            B = 255
                        });
                        color = solid;
                        return solid;
                    }

                }
            }
            public string DanSendTime { get; set; }//弹幕发送时间
            public string DanPool { get; set; }//弹幕池，0普通池 1字幕池 2特殊池 【目前特殊池为高级弹幕专用】
            public string DanID { get; set; }//弹幕发送人ID
            public string DanRowID { get; set; }
            public string DanText { get; set; }//信息
        }
        // public bool wCnMdBUG = false;




        public void SetJJ()
        {
            try
            {
                //wCnMdBUG = false;
                // if (D_height == 0)
                // {
                D_height = GetTestHeight();
                // }
                var d = this.ActualHeight % D_height;

                if (d != 0)
                {
                    maxRow = Convert.ToInt32(this.ActualHeight / D_height);
                }
                else
                {
                    maxRow = Convert.ToInt32(this.ActualHeight / D_height) - 1;
                }
                if (grid_Danmu.RowDefinitions.Count + 1 < maxRow)
                {
                    for (int i = 0; i < maxRow - grid_Danmu.RowDefinitions.Count + 1; i++)
                    {
                        grid_Danmu.RowDefinitions.Insert(grid_Danmu.RowDefinitions.Count - 1, new RowDefinition());
                    }
                }
                else
                {
                    for (int i = 0; i < grid_Danmu.RowDefinitions.Count - maxRow; i++)
                    {
                        grid_Danmu.RowDefinitions.RemoveAt(grid_Danmu.RowDefinitions.Count - 1);
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        public double GetTestHeight()
        {
            return fontSize / 0.6875;

            //TextBlock tx = new TextBlock() {
            //    Text="Test",
            //    FontSize=fontSize,

            //};
            //tx.Margin = new Thickness(1);

            //Grid grid = new Grid();
            //grid.Children.Add(tx);
            //grid.VerticalAlignment = VerticalAlignment.Top;
            //grid.HorizontalAlignment = HorizontalAlignment.Left;

            ////设置控件相关信息
            //grid.Margin = new Thickness(0, 0, 20, 0);
            //d.Children.Add(grid);
            //tx.UpdateLayout();
            //grid.UpdateLayout();
            //return grid.ActualHeight;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (!isLive)
            {
                SetJJ();
            }

            return base.MeasureOverride(availableSize);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //SetJJ();
            // wCnMdBUG = false;

        }
    }
}
