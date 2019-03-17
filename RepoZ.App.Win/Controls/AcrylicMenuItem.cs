using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RepoZ.App.Win.Controls
{
	public class AcrylicMenuItem : MenuItem
	{
		protected override void OnSubmenuOpened(RoutedEventArgs e)
		{
			base.OnSubmenuOpened(e);

			Dispatcher.BeginInvoke((Action)BlurSubMenu);
		}

		private void BlurSubMenu()
		{
			var firstSubItem = ItemContainerGenerator.ContainerFromIndex(0);

			if (firstSubItem == null)
				return;

			var container = VisualTreeHelper.GetParent(firstSubItem) as Visual;

			if (container == null)
				return;

			AcrylicHelper.EnableBlur(container);
		}
	}
}
