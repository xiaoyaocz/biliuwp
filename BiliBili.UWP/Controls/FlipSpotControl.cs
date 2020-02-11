using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace BiliBili.UWP.Controls
{
    public sealed class FlipSpotControl : Control
    {
        public FlipSpotControl()
        {
            this.DefaultStyleKey = typeof(FlipSpotControl);
        }
        public ItemsControl itemsControl;
        protected override void OnApplyTemplate()
        {
            itemsControl = GetTemplateChild("itemsControl") as ItemsControl;
            if (itemsControl != null)
            {
                List<Models> model = new List<Models>();
                for (int i = 0; i < ItemsCount; i++)
                {
                    model.Add(new Models()
                    {
                        color = new SolidColorBrush(Colors.White)
                    });
                }
                itemsControl.ItemsSource = model;
            }
            if (itemsControl != null && itemsControl.ItemsSource != null &&itemsControl.Items.Count != 0)
            {
                if (SelectIndex == -1)
                {
                    return;
                }
                (itemsControl.Items[SelectIndex] as Models).color = Color;
            }

            base.OnApplyTemplate();
        }


        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(FlipSpotControl), new PropertyMetadata(new SolidColorBrush(Colors.Gray), OnColorChanged));

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var data = d as FlipSpotControl;

            if (data.itemsControl != null && data.itemsControl.ItemsSource != null && data.itemsControl.Items.Count != 0)
            {
                (data.itemsControl.Items[data.SelectIndex] as Models).color =(SolidColorBrush)e.NewValue;
            }
        }


        public int SelectIndex
        {
            get { return (int)GetValue(SelectIndexProperty); }
            set { {
                    SetValue(SelectIndexProperty, value);
                    
                } }
        }

        // Using a DependencyProperty as the backing store for SelectIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectIndexProperty =
            DependencyProperty.Register("SelectIndex", typeof(int), typeof(FlipSpotControl), new PropertyMetadata(0, OnSelectIndexChanged));

      
        private static void OnSelectIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var data = d as FlipSpotControl;
            if ((int)e.NewValue==-1)
            {
                return;
            }
            if (data.itemsControl != null && data.itemsControl.ItemsSource != null && data.itemsControl.Items.Count != 0)
            {
                foreach (Models item in data.itemsControl.Items)
                {
                    item.color = new SolidColorBrush(Colors.White);
                }
                (data.itemsControl.Items[(int)e.NewValue] as Models).color = data.Color;
            }
        }



        public int ItemsCount
        {
            get { return (int)GetValue(ItemsCountProperty); }
            set { {
                    SetValue(ItemsCountProperty, value);
                    if (itemsControl!=null)
                    {
                        List<Models> model = new List<Models>();
                        for (int i = 0; i < value; i++)
                        {
                            model.Add(new Models()
                            {
                                color = new SolidColorBrush(Colors.White)
                            });
                        }
                        itemsControl.ItemsSource = model;
                    }
                } }
        }

        // Using a DependencyProperty as the backing store for ItemsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsCountProperty =
            DependencyProperty.Register("ItemsCount", typeof(int), typeof(FlipSpotControl), new PropertyMetadata(0, OnItemsCountChanged));

        private static void OnItemsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var data = d as FlipSpotControl;
            if (data.itemsControl != null)
            {
                List<Models> model = new List<Models>();
                for (int i = 0; i < data.ItemsCount; i++)
                {
                    model.Add(new Models()
                    {
                        color = new SolidColorBrush(Colors.White)
                    });
                }
                data.itemsControl.ItemsSource = model;
            }
        }


        public class Models: INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void thisPropertyChanged(string name)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
                }
            }
            private SolidColorBrush _color;

            public SolidColorBrush color
            {
                get { return _color; }
                set { _color = value; thisPropertyChanged("color"); }
            }

          
        }

    }


}
