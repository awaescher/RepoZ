using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace RepoZ.App.Win
{
	public static class AcrylicHelper
	{
		public static void EnableBlur(Visual visual)
		{
			var hwnd = (HwndSource)PresentationSource.FromVisual(visual);
			WindowsCompositionHelper.EnableBlur(hwnd.Handle);
		}

		public static void EnableBlurForMenu(Control menu)
		{
			menu.Background = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0));
			EnableBlur(menu);
		}
	}
}
