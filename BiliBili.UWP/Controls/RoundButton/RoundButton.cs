﻿using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace BiliBili.UWP.Controls
{
	public sealed class RoundButton : Button
	{
		public RoundButton()
		{
			this.DefaultStyleKey = typeof(RoundButton);
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			var size = base.MeasureOverride(availableSize);
			return new Size(size.Width, size.Width);
		}
	}
}