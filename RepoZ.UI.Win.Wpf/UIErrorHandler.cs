using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RepoZ.Api.Common;

namespace RepoZ.UI.Win.Wpf
{
	public class UIErrorHandler : IErrorHandler
	{
		public void Handle(string error)
		{
			MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
