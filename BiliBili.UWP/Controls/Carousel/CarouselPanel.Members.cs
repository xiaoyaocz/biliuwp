using System;
using System.Windows.Input;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BiliBili.UWP.Controls
{
	public class IntEventArgs : EventArgs
	{
		public IntEventArgs(int value)
		{
			this.Value = value;
		}

		public int Value { get; private set; }
	}

	partial class CarouselPanel
	{
		public event EventHandler<IntEventArgs> SelectedIndexChanged;

		#region ItemTemplate

		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CarouselPanel), new PropertyMetadata(null, ItemTemplateChanged));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		private static void ItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as CarouselPanel;
			control.InvalidateMeasure();
		}

		#endregion ItemTemplate

		#region ItemWidth

		public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(CarouselPanel), new PropertyMetadata(400.0, ItemWidthChanged));

		public double ItemWidth
		{
			get { return (double)GetValue(ItemWidthProperty); }
			set { SetValue(ItemWidthProperty, value); }
		}

		private static void ItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as CarouselPanel;
			control.InvalidateMeasure();
		}

		#endregion ItemWidth

		#region ItemHeight

		public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(CarouselPanel), new PropertyMetadata(300.0, ItemHeightChanged));

		public double ItemHeight
		{
			get { return (double)GetValue(ItemHeightProperty); }
			set { SetValue(ItemHeightProperty, value); }
		}

		private static void ItemHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as CarouselPanel;
			control.InvalidateMeasure();
		}

		#endregion ItemHeight

		#region ItemClickCommand

		public static readonly DependencyProperty ItemClickCommandProperty = DependencyProperty.Register("ItemClickCommand", typeof(ICommand), typeof(CarouselPanel), new PropertyMetadata(null));

		public ICommand ItemClickCommand
		{
			get { return (ICommand)GetValue(ItemClickCommandProperty); }
			set { SetValue(ItemClickCommandProperty, value); }
		}

		#endregion ItemClickCommand

		private void OnPaneTapped(object sender, TappedRoutedEventArgs e)
		{
			var contentControl = sender as ContentControl;
			if (contentControl != null)
			{
				if (SelectedIndexChanged != null)
				{
					if (contentControl.Tag != null)
					{
						SelectedIndexChanged(this, new IntEventArgs((int)contentControl.Tag));
					}
				}

				if (ItemClickCommand != null)
				{
					if (ItemClickCommand.CanExecute(contentControl.Content))
					{
						ItemClickCommand.Execute(contentControl.Content);
					}
				}
			}
		}
	}
}