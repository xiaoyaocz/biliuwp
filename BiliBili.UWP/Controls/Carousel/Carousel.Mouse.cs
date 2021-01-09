using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace BiliBili.UWP.Controls
{
	partial class Carousel
	{
		private DispatcherTimer _fadeTimer = null;
		private bool _isArrowOver = false;
		private bool _isArrowVisible = false;

		#region Create/Dispose FadeTimer

		private void CreateFadeTimer()
		{
			_fadeTimer = new DispatcherTimer();
			_fadeTimer.Interval = TimeSpan.FromMilliseconds(1500);
			_fadeTimer.Tick += OnFadeTimerTick;
		}

		private void DisposeFadeTimer()
		{
			var fadeTimer = _fadeTimer;
			_fadeTimer = null;
			if (fadeTimer != null)
			{
				fadeTimer.Stop();
			}
		}

		#endregion Create/Dispose FadeTimer

		#region ArrowPointerEntered/ArrowPointerExited

		private void OnArrowPointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_isArrowOver = true;
		}

		private void OnArrowPointerExited(object sender, PointerRoutedEventArgs e)
		{
			_isArrowOver = false;
		}

		#endregion ArrowPointerEntered/ArrowPointerExited

		#region LeftClick/RightClick

		private void OnLeftClick(object sender, RoutedEventArgs e)
		{
			MoveBack();
		}

		private void OnRightClick(object sender, RoutedEventArgs e)
		{
			MoveForward();
		}

		#endregion LeftClick/RightClick

		private void OnFadeTimerTick(object sender, object e)
		{
			if (_isArrowVisible && !_isArrowOver)
			{
				_isArrowVisible = false;
				//_arrows.Fade(easingMode: Windows.UI.Xaml.Media.Animation.EasingMode.EaseOut).Start();
				_arrows.FadeOut();
			}
		}

		private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isArrowVisible)
			{
				// _arrows.Fade(easingMode: Windows.UI.Xaml.Media.Animation.EasingMode.EaseIn).Start();
				_arrows.FadeIn();
				_isArrowVisible = true;
			}
			this._fadeTimer?.Start();
		}
	}
}