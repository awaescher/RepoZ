using RepoZ.Api.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoZ.UI.Win
{
	public class UIErrorHandler : IErrorHandler
	{
		public void Handle(string error)
		{
			Eto.Forms.MessageBox.Show(error, Eto.Forms.MessageBoxType.Error);
		}
	}
}
