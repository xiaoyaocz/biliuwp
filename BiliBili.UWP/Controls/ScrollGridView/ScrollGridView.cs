using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BiliBili.UWP.Controls.ScrollGridView
{
	public class ScrollGridView : GridView
	{
		public static readonly DependencyProperty AlwayShowButtonProperty =
			DependencyProperty.Register("AlwayShowButton", typeof(bool), typeof(ScrollGridView), new PropertyMetadata(false, OnAlwayShowButtonChanged));

		// Using a DependencyProperty as the backing store for MoveOffset. This enables animation,
		// styling, binding, etc...
		public static readonly DependencyProperty MoveOffsetProperty =
			DependencyProperty.Register("MoveOffset", typeof(double), typeof(ScrollGridView), new PropertyMetadata((double)200.0));

		public Button btnMoveLeft;

		public Button btnMoveRight;

		public Grid gridGesture;

		public ScrollViewer scrollViewer;

		public ScrollGridView()
		{
			this.DefaultStyleKey = typeof(ScrollGridView);
		}

		public bool AlwayShowButton
		{
			get { return (bool)GetValue(AlwayShowButtonProperty); }
			set { SetValue(AlwayShowButtonProperty, value); }
		}

		public double MoveOffset
		{
			get { return (double)GetValue(MoveOffsetProperty); }
			set { SetValue(MoveOffsetProperty, value); }
		}

		protected override void OnApplyTemplate()
		{
			gridGesture = GetTemplateChild("GridGesture") as Grid;
			btnMoveLeft = GetTemplateChild("moveLeft") as Button;
			btnMoveRight = GetTemplateChild("moveRight") as Button;
			scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
			scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
			gridGesture.PointerExited += GridGesture_PointerExited;
			gridGesture.PointerEntered += GridGesture_PointerEntered;
			btnMoveLeft.Click += BtnMoveLeft_Click;
			btnMoveRight.Click += BtnMoveRight_Click;
			base.OnApplyTemplate();
		}

		private static void OnAlwayShowButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var data = d as ScrollGridView;
			if ((bool)e.NewValue)
			{
				data.btnMoveLeft.Visibility = Visibility.Visible;
				data.btnMoveRight.Visibility = Visibility.Visible;
			}
		}

		private void BtnMoveLeft_Click(object sender, RoutedEventArgs e)
		{
			var move = scrollViewer.HorizontalOffset - MoveOffset;
			if (move <= 0)
			{
				move = 0;
			}
			scrollViewer.ChangeView(move, null, null);
		}

		private void BtnMoveRight_Click(object sender, RoutedEventArgs e)
		{
			var move = scrollViewer.HorizontalOffset + MoveOffset;
			if (move >= scrollViewer.ScrollableWidth)
			{
				move = scrollViewer.ScrollableWidth;
			}
			scrollViewer.ChangeView(move, null, null);
		}

		private void GridGesture_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
			{
				scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
				setButton();
			}
			else
			{
				scrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
			}
		}

		private void GridGesture_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (AlwayShowButton)
			{
				return;
			}
			btnMoveLeft.Visibility = Visibility.Collapsed;
			btnMoveRight.Visibility = Visibility.Collapsed;
		}

		private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			setButton();
		}

		private void setButton()
		{
			if (AlwayShowButton)
			{
				btnMoveLeft.Visibility = Visibility.Visible;
				btnMoveRight.Visibility = Visibility.Visible;
				return;
			}
			if (scrollViewer.HorizontalOffset > 0)
			{
				btnMoveLeft.Visibility = Visibility.Visible;
			}
			else
			{
				btnMoveLeft.Visibility = Visibility.Collapsed;
			}
			if (scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth)
			{
				btnMoveRight.Visibility = Visibility.Visible;
			}
			else
			{
				btnMoveRight.Visibility = Visibility.Collapsed;
			}
		}
	}
}