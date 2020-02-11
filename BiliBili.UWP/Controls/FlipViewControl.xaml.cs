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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliBili.UWP.Controls
{
    public sealed partial class FlipViewControl : UserControl
    {

        public object ItemsSource {
            get
            {
                return flipView.ItemsSource;
            }
            set
            {
                if (value is Binding)
                {
                    BindingOperations.SetBinding(flipView, FlipView.ItemsSourceProperty, value as Binding);
                }
                else
                {
                    flipView.ItemsSource = value;
                }
                
            }
        }

        public DataTemplate ItemTemplate
        {
            get
            {
                return flipView.ItemTemplate;
            }
            set
            {
                flipView.ItemTemplate = value as DataTemplate;
            }
        }



        public double FlipViewHeight
        {
            get
            {
                return flipView.Height;
            }
            set
            {
                flipView.Height = value;
            }
        }
        public double FlipViewWidth
        {
            get
            {
                return flipView.Width;
            }
            set
            {
                flipView.Width = value;
            }
        }
        public double IntervalSeconds
        {
            get
            {
                return timer.Interval.TotalSeconds;
            }
            set
            {
                timer.Interval= TimeSpan.FromSeconds(value); 
            }
        }

        DispatcherTimer timer;
        public FlipViewControl()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            if (flipView.ItemsSource == null || flipView.Items.Count <= 1)
            {
                return;
            }
            try
            {

                if (flipView.SelectedIndex == flipView.Items.Count - 1)
                {
                    flipView.SelectedIndex = 0;
                }
                else
                {
                    flipView.SelectedIndex += 1;
                }
            }
            catch (Exception)
            {
            }
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flipSpotControl.ItemsCount==0&& flipView.Items.Count!=0)
            {
                flipSpotControl.ItemsCount = flipView.Items.Count;
            }
            
        }



    }
}
