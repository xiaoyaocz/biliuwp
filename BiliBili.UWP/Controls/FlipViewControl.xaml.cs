using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliBili.UWP.Controls
{
	public sealed partial class FlipViewControl : UserControl
	{
		private DispatcherTimer timer;

		public FlipViewControl()
		{
			this.InitializeComponent();
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(3);
			timer.Tick += Timer_Tick;
			timer.Start();
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
				timer.Interval = TimeSpan.FromSeconds(value);
			}
		}

		public object ItemsSource
		{
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

		private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (flipSpotControl.ItemsCount == 0 && flipView.Items.Count != 0)
			{
				flipSpotControl.ItemsCount = flipView.Items.Count;
			}
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
	}
}