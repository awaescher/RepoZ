using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RepoZ.App.Win.Controls
{
	public class AcrylicContextMenu : ContextMenu
	{
		protected override void OnOpened(RoutedEventArgs e)
		{
			base.OnOpened(e);

			AcrylicHelper.EnableBlur(this);
		}

	}
}
